using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ThemeDay
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int Month { get; set; }
    public int Day { get; set; }
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
}
public sealed record ThemeDayInput(string Name, int Month, int Day, string Category = "General",
    string? Description = null);
public sealed record ThemeDayView(Guid Id, string Name, int Month, int Day, string Category,
    string? Description, DateOnly NextOccurrence, int DaysUntil);
public sealed class ThemeDayConfiguration : IEntityTypeConfiguration<ThemeDay>
{
    public void Configure(EntityTypeBuilder<ThemeDay> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(160);
        b.Property(x => x.Month).IsRequired();
        b.Property(x => x.Day).IsRequired();
        b.Property(x => x.Category).IsRequired().HasMaxLength(80);
        b.Property(x => x.Description).HasMaxLength(2000);
    }
}
public sealed class ThemeDayStore
{
    private readonly object gate = new();
    private readonly string path = Path.Combine(AppContext.BaseDirectory, "App_Data", "themedays.json");
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public static bool Valid(ThemeDayInput? x) => x is not null &&
        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Trim().Length <= 160 &&
        !string.IsNullOrWhiteSpace(x.Category) && x.Category.Trim().Length <= 80 &&
        (x.Description?.Length ?? 0) <= 2000 &&
        DateOnly.TryParseExact($"2000-{x.Month:D2}-{x.Day:D2}", "yyyy-MM-dd", out _);

    private List<ThemeDay> Read() => File.Exists(path)
        ? JsonSerializer.Deserialize<List<ThemeDay>>(File.ReadAllText(path), options) ?? new()
        : new();

    private void Write(List<ThemeDay> data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(tmp, JsonSerializer.Serialize(data, options)); File.Move(tmp, path, true); }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }

    // Feb 29 entries are observed on Feb 28 in non-leap years.
    private static DateOnly Occurrence(ThemeDay x, int year) =>
        new(year, x.Month, Math.Min(x.Day, DateTime.DaysInMonth(year, x.Month)));

    private static ThemeDayView Project(ThemeDay x, DateOnly today)
    {
        var thisYear = Occurrence(x, today.Year);
        var next = thisYear >= today ? thisYear : Occurrence(x, today.Year + 1);
        return new(x.Id, x.Name, x.Month, x.Day, x.Category, x.Description,
            next, next.DayNumber - today.DayNumber);
    }

    public List<ThemeDayView> All(string? search = null, string? category = null)
    {
        lock (gate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return Read().Where(x =>
                    (string.IsNullOrWhiteSpace(search) ||
                     x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(category) ||
                     x.Category.Equals(category, StringComparison.OrdinalIgnoreCase)))
                .Select(x => Project(x, today)).OrderBy(x => x.DaysUntil)
                .ThenBy(x => x.Name).ToList();
        }
    }

    public ThemeDayView? Find(Guid id)
    {
        lock (gate)
        {
            var x = Read().FirstOrDefault(x => x.Id == id);
            return x is null ? null : Project(x, DateOnly.FromDateTime(DateTime.Today));
        }
    }

    public List<ThemeDayView> ByDate(int month, int day, string? category = null) =>
        All(category: category).Where(x => x.Month == month && x.Day == day).ToList();

    public List<ThemeDayView> Today(string? category = null)
    {
        var now = DateOnly.FromDateTime(DateTime.Today);
        return ByDate(now.Month, now.Day, category);
    }

    public List<ThemeDayView> Upcoming(int days, string? category = null) =>
        All(category: category).Where(x => x.DaysUntil <= days).ToList();

    private static ThemeDay Build(ThemeDayInput x, Guid id) => new()
    {
        Id = id, Name = x.Name.Trim(), Month = x.Month, Day = x.Day,
        Category = x.Category.Trim(), Description = x.Description
    };

    public ThemeDayView Create(ThemeDayInput input)
    {
        lock (gate)
        {
            var x = Build(input, Guid.NewGuid());
            var data = Read(); data.Add(x); Write(data);
            return Project(x, DateOnly.FromDateTime(DateTime.Today));
        }
    }

    public ThemeDayView? Update(Guid id, ThemeDayInput input)
    {
        lock (gate)
        {
            var data = Read();
            var index = data.FindIndex(x => x.Id == id);
            if (index < 0) return null;
            var x = Build(input, id);
            data[index] = x; Write(data);
            return Project(x, DateOnly.FromDateTime(DateTime.Today));
        }
    }

    public bool Delete(Guid id)
    {
        lock (gate)
        {
            var data = Read();
            if (data.RemoveAll(x => x.Id == id) == 0) return false;
            Write(data); return true;
        }
    }
}
