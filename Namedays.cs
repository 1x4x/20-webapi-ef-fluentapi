using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class Nameday
{
    public string Name { get; set; } = "";
    public int Day { get; set; }
    public int Month { get; set; }
}
public sealed class NamedayMap : IEntityTypeConfiguration<Nameday>
{
    public void Configure(EntityTypeBuilder<Nameday> b)
    {
        b.HasKey(x => x.Name);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Day).IsRequired();
        b.Property(x => x.Month).IsRequired();
    }
}
public sealed class NamedayStore
{
    private readonly object gate = new();
    private readonly string path = Path.Combine(AppContext.BaseDirectory, "App_Data", "namedays.json");
    private readonly string seed = Path.Combine(AppContext.BaseDirectory, "Data", "namedays.seed.json");
    private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
    public static bool Valid(Nameday n) => !string.IsNullOrWhiteSpace(n.Name) && n.Name.Length <= 120 &&
        DateOnly.TryParseExact($"2000-{n.Month:D2}-{n.Day:D2}", "yyyy-MM-dd", out _);
    private List<Nameday> Read()
    {
        var source = File.Exists(path) ? path : seed;
        if (!File.Exists(source)) throw new FileNotFoundException("Nameday seed data missing", source);
        return JsonSerializer.Deserialize<List<Nameday>>(File.ReadAllText(source), options) ?? new();
    }
    private void Write(List<Nameday> data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(tmp, JsonSerializer.Serialize(data, options)); File.Move(tmp, path, true); }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }
    public List<Nameday> All(string? search = null)
    {
        lock(gate) return Read().Where(n => string.IsNullOrWhiteSpace(search) || n.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.Name, StringComparer.Create(new System.Globalization.CultureInfo("sv-SE"), true)).ToList();
    }
    public Nameday? Find(string name) { lock(gate) return Read().FirstOrDefault(n => comparer.Equals(n.Name, name)); }
    public List<Nameday> ByDate(int month,int day) { lock(gate) return Read().Where(n => n.Month == month && n.Day == day).OrderBy(n=>n.Name).ToList(); }
    public bool Add(Nameday n)
    {
        lock(gate) { var rows=Read();if(rows.Any(x=>comparer.Equals(x.Name,n.Name)))return false;
            n.Name=n.Name.Trim();rows.Add(n);Write(rows);return true; }
    }
    // 0: missing, 1: success, 2: duplicate name
    public int Update(string name,Nameday n)
    {
        lock(gate) { var rows=Read();var i=rows.FindIndex(x=>comparer.Equals(x.Name,name));if(i<0)return 0;
            n.Name=n.Name.Trim();if(rows.Where((_,j)=>j!=i).Any(x=>comparer.Equals(x.Name,n.Name)))return 2;
            rows[i]=n;Write(rows);return 1; }
    }
    public bool Delete(string name)
    {
        lock(gate) { var rows=Read();if(rows.RemoveAll(x=>comparer.Equals(x.Name,name))==0)return false;Write(rows);return true; }
    }
}
