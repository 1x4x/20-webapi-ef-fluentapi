using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class FlagDay
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int Month { get; set; }
    public int Day { get; set; }
    public string CountryCode { get; set; } = "SE";
    public string? Description { get; set; }
    public bool IsOfficial { get; set; }
}
public sealed record FlagDayInput(string Name, int Month, int Day, string CountryCode = "SE",
    string? Description = null, bool IsOfficial = false);
public sealed record FlagDayView(Guid Id, string Name, int Month, int Day, string CountryCode,
    string? Description, bool IsOfficial, DateOnly NextOccurrence, int DaysUntil);
public sealed class FlagDayConfiguration : IEntityTypeConfiguration<FlagDay>
{
    public void Configure(EntityTypeBuilder<FlagDay> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(160);
        b.Property(x => x.Month).IsRequired();
        b.Property(x => x.Day).IsRequired();
        b.Property(x => x.CountryCode).IsRequired().HasMaxLength(2);
        b.Property(x => x.Description).HasMaxLength(2000);
    }
}
public sealed class FlagDayStore
{
    private readonly object gate = new();
    private readonly string path = Path.Combine(AppContext.BaseDirectory, "App_Data", "flagdays.json");
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public static bool Valid(FlagDayInput? x) => x is not null &&
        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Trim().Length <= 160 &&
        x.CountryCode is { Length: 2 } && x.CountryCode.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z') &&
        (x.Description?.Length ?? 0) <= 2000 &&
        DateOnly.TryParseExact($"2000-{x.Month:D2}-{x.Day:D2}", "yyyy-MM-dd", out _);

    private List<FlagDay> Read() => File.Exists(path)
        ? JsonSerializer.Deserialize<List<FlagDay>>(File.ReadAllText(path), options) ?? new()
        : new();

    private void Write(List<FlagDay> data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(tmp, JsonSerializer.Serialize(data, options)); File.Move(tmp, path, true); }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }
    private static DateOnly Occurrence(FlagDay x, int year) =>
        new(year, x.Month, Math.Min(x.Day, DateTime.DaysInMonth(year, x.Month)));

    private static FlagDayView Project(FlagDay x, DateOnly today)
    {
        var thisYear = Occurrence(x, today.Year);
        var next = thisYear >= today ? thisYear : Occurrence(x, today.Year + 1);
        return new(x.Id, x.Name, x.Month, x.Day, x.CountryCode, x.Description,
            x.IsOfficial, next, next.DayNumber - today.DayNumber);
    }
    public List<FlagDayView> All(string? search = null, string? country = null)
    {
        lock (gate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return Read().Where(x => (string.IsNullOrWhiteSpace(search) ||
                    x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(country) ||
                    x.CountryCode.Equals(country, StringComparison.OrdinalIgnoreCase)))
                .Select(x => Project(x, today)).OrderBy(x => x.DaysUntil)
                .ThenBy(x => x.Name).ToList();
        }
    }
    public FlagDayView? Find(Guid id)
    {
        lock (gate)
        {
            var x = Read().FirstOrDefault(x => x.Id == id);
            return x is null ? null : Project(x, DateOnly.FromDateTime(DateTime.Today));
        }
    }
    public List<FlagDayView> ByDate(int month, int day, string? country = null) =>
        All(country: country).Where(x => x.Month == month && x.Day == day).ToList();
    public List<FlagDayView> Today(string? country = null)
    {
        var now = DateOnly.FromDateTime(DateTime.Today);
        return ByDate(now.Month, now.Day, country);
    }
    public List<FlagDayView> Upcoming(int days, string? country = null) =>
        All(country: country).Where(x => x.DaysUntil <= days).ToList();

    private static FlagDay Build(FlagDayInput x, Guid id) => new()
    {
        Id = id, Name = x.Name.Trim(), Month = x.Month, Day = x.Day,
        CountryCode = x.CountryCode.ToUpperInvariant(), Description = x.Description, IsOfficial = x.IsOfficial
    };
    public FlagDayView Create(FlagDayInput input)
    {
        lock (gate)
        {
            var x = Build(input, Guid.NewGuid());
            var data = Read(); data.Add(x); Write(data);
            return Project(x, DateOnly.FromDateTime(DateTime.Today));
        }
    }
    public FlagDayView? Update(Guid id, FlagDayInput input)
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
