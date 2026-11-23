using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class Birthday
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string? Notes { get; set; }
}
public sealed class BirthdayConfiguration : IEntityTypeConfiguration<Birthday>
{
    public void Configure(EntityTypeBuilder<Birthday> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(120);
        b.Property(x => x.DateOfBirth).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(2000);
    }
}
public sealed record BirthdayInput(string Name, DateOnly DateOfBirth, string? Notes);
public sealed record BirthdayView(Guid Id, string Name, DateOnly DateOfBirth, string? Notes,
    int Age, DateOnly NextBirthday, int TurningAge, int DaysUntilBirthday);
public sealed class BirthdayStore
{
    private readonly object gate = new();
    private readonly string path = Path.Combine(AppContext.BaseDirectory, "App_Data", "birthdays.json");
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public static bool Valid(BirthdayInput? input) =>
        input is not null && !string.IsNullOrWhiteSpace(input.Name) &&
        input.Name.Trim().Length <= 120 && input.DateOfBirth != default &&
        input.DateOfBirth <= DateOnly.FromDateTime(DateTime.Today) &&
        (input.Notes?.Length ?? 0) <= 2000;

    private List<Birthday> Read() =>
        File.Exists(path) ? JsonSerializer.Deserialize<List<Birthday>>(File.ReadAllText(path), options) ?? new() : new();

    private void Write(List<Birthday> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(items, options));
            File.Move(temp, path, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    // Feb 29 birthdays are observed on Feb 28 in non-leap years.
    private static DateOnly Anniversary(DateOnly birth, int year) =>
        new(year, birth.Month, Math.Min(birth.Day, DateTime.DaysInMonth(year, birth.Month)));

    public static BirthdayView Project(Birthday b, DateOnly today)
    {
        var thisYear = Anniversary(b.DateOfBirth, today.Year);
        var age = today.Year - b.DateOfBirth.Year - (today < thisYear ? 1 : 0);
        var next = today <= thisYear ? thisYear : Anniversary(b.DateOfBirth, today.Year + 1);
        return new(b.Id, b.Name, b.DateOfBirth, b.Notes, age, next,
            next.Year - b.DateOfBirth.Year, next.DayNumber - today.DayNumber);
    }
    public List<BirthdayView> All(string? search = null)
    {
        lock (gate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return Read().Where(x => string.IsNullOrWhiteSpace(search) ||
                x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Select(x => Project(x, today)).OrderBy(x => x.DaysUntilBirthday)
                .ThenBy(x => x.Name).ToList();
        }
    }
    public BirthdayView? Find(Guid id)
    {
        lock (gate)
        {
            var b = Read().FirstOrDefault(x => x.Id == id);
            return b is null ? null : Project(b, DateOnly.FromDateTime(DateTime.Today));
        }
    }
    public List<BirthdayView> Today() => All().Where(x => x.DaysUntilBirthday == 0).ToList();
    public List<BirthdayView> Upcoming(int days) => All().Where(x => x.DaysUntilBirthday <= days).ToList();
    public BirthdayView Create(BirthdayInput input)
    {
        lock (gate)
        {
            var b = new Birthday { Id = Guid.NewGuid(), Name = input.Name.Trim(),
                DateOfBirth = input.DateOfBirth, Notes = input.Notes };
            var all = Read(); all.Add(b); Write(all);
            return Project(b, DateOnly.FromDateTime(DateTime.Today));
        }
    }
    public BirthdayView? Update(Guid id, BirthdayInput input)
    {
        lock (gate)
        {
            var all = Read();
            var b = all.FirstOrDefault(x => x.Id == id);
            if (b is null) return null;
            b.Name = input.Name.Trim(); b.DateOfBirth = input.DateOfBirth; b.Notes = input.Notes;
            Write(all);
            return Project(b, DateOnly.FromDateTime(DateTime.Today));
        }
    }
    public bool Delete(Guid id)
    {
        lock (gate)
        {
            var all = Read();
            if (all.RemoveAll(x => x.Id == id) == 0) return false;
            Write(all);
            return true;
        }
    }
}
