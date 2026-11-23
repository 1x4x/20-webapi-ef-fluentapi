using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(o=>o.UseInMemoryDatabase("fluent-model"));
builder.Services.AddSingleton<JsonStore>();
builder.Services.AddSingleton<NamedayStore>();
builder.Services.AddSingleton<BirthdayStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app=builder.Build();
app.UseDefaultFiles(); app.UseStaticFiles(); app.UseSwagger(); app.UseSwaggerUI();
app.MapGet("/api/health",()=>Results.Ok(new {status="ok",storage="JSON files",model="EF Core Fluent API"}));
app.MapGet("/api/boards",(JsonStore s)=>s.All());
app.MapGet("/api/boards/{id:guid}",(Guid id,JsonStore s)=>s.Find(id) is {} b?Results.Ok(b):Results.NotFound());
app.MapPost("/api/boards",(Board b,JsonStore s)=>{b.Id=Guid.NewGuid();s.Save(b);return Results.Created($"/api/boards/{b.Id}",b);});
app.MapPut("/api/boards/{id:guid}",(Guid id,Board b,JsonStore s)=>s.Replace(id,b)?Results.NoContent():Results.NotFound());
app.MapDelete("/api/boards/{id:guid}",(Guid id,JsonStore s)=>s.Delete(id)?Results.NoContent():Results.NotFound());

var namedays=app.MapGroup("/api/namedays").WithTags("Namedays");
namedays.MapGet("/",(NamedayStore s,string? search)=>Results.Ok(s.All(search)));
namedays.MapGet("/today",(NamedayStore s)=>Results.Ok(s.ByDate(DateTime.Now.Month,DateTime.Now.Day)));
namedays.MapGet("/date/{month:int}/{day:int}",(int month,int day,NamedayStore s)=>DateOnly.TryParseExact($"2000-{month:D2}-{day:D2}","yyyy-MM-dd",out _)?Results.Ok(s.ByDate(month,day)):Results.BadRequest(new {error="Invalid month/day"}));
namedays.MapGet("/name/{name}",(string name,NamedayStore s)=>s.Find(name) is {} n?Results.Ok(n):Results.NotFound());
namedays.MapPost("/",(Nameday n,NamedayStore s)=>{if(!NamedayStore.Valid(n))return Results.BadRequest(new {error="Name and valid month/day are required"});return s.Add(n)?Results.Created($"/api/namedays/name/{Uri.EscapeDataString(n.Name)}",n):Results.Conflict(new {error="Name already exists"});});
namedays.MapPut("/name/{name}",(string name,Nameday n,NamedayStore s)=>{if(!NamedayStore.Valid(n))return Results.BadRequest(new {error="Name and valid month/day are required"});return s.Update(name,n) switch {1=>Results.NoContent(),2=>Results.Conflict(new {error="Target name already exists"}),_=>Results.NotFound()};});
namedays.MapDelete("/name/{name}",(string name,NamedayStore s)=>s.Delete(name)?Results.NoContent():Results.NotFound());

// Birthday endpoints use server-local dates; writes require authentication before public deployment.
var birthdays = app.MapGroup("/api/birthdays").WithTags("Birthdays");
birthdays.MapGet("/", (BirthdayStore store, string? search) => Results.Ok(store.All(search)));
birthdays.MapGet("/today", (BirthdayStore store) => Results.Ok(store.Today()));
birthdays.MapGet("/upcoming", (BirthdayStore store, int? days) =>
    days is < 0 or > 366 ? Results.BadRequest(new { error = "days must be between 0 and 366" })
    : Results.Ok(store.Upcoming(days ?? 30)));
birthdays.MapGet("/{id:guid}", (Guid id, BirthdayStore store) =>
    store.Find(id) is { } b ? Results.Ok(b) : Results.NotFound());
birthdays.MapPost("/", (BirthdayInput input, BirthdayStore store) =>
{
    if (!BirthdayStore.Valid(input)) return Results.BadRequest(new { error = "Valid name, dateOfBirth (not in the future), and optional notes are required" });
    var created = store.Create(input);
    return Results.Created($"/api/birthdays/{created.Id}", created);
});
birthdays.MapPut("/{id:guid}", (Guid id, BirthdayInput input, BirthdayStore store) =>
{
    if (!BirthdayStore.Valid(input)) return Results.BadRequest(new { error = "Invalid birthday details" });
    return store.Update(id, input) is { } updated ? Results.Ok(updated) : Results.NotFound();
});
birthdays.MapDelete("/{id:guid}", (Guid id, BirthdayStore store) =>
    store.Delete(id) ? Results.NoContent() : Results.NotFound());
app.Run();
public class Board {public Guid Id {get;set;} public string Name {get;set;}="New board";public List<BoardItem> Items {get;set;}=new();}
public class BoardItem {public Guid Id {get;set;}=Guid.NewGuid();public Guid BoardId {get;set;}public string Text {get;set;}="";public bool Done {get;set;}}
public class AppDb:DbContext {public AppDb(DbContextOptions<AppDb> options):base(options){} public DbSet<Board> Boards=>Set<Board>(); public DbSet<Nameday> Namedays=>Set<Nameday>(); public DbSet<Birthday> Birthdays=>Set<Birthday>(); public DbSet<BoardItem> Items=>Set<BoardItem>();protected override void OnModelCreating(ModelBuilder b){b.ApplyConfiguration(new BoardMap());b.ApplyConfiguration(new ItemMap());b.ApplyConfiguration(new NamedayMap());b.ApplyConfiguration(new BirthdayConfiguration());}}
public class BoardMap:IEntityTypeConfiguration<Board>{public void Configure(EntityTypeBuilder<Board> b){b.HasKey(x=>x.Id);b.Property(x=>x.Name).IsRequired().HasMaxLength(120);b.HasMany(x=>x.Items).WithOne().HasForeignKey(x=>x.BoardId).OnDelete(DeleteBehavior.Cascade);}}
public class ItemMap:IEntityTypeConfiguration<BoardItem>{public void Configure(EntityTypeBuilder<BoardItem> b){b.HasKey(x=>x.Id);b.Property(x=>x.Text).IsRequired().HasMaxLength(500);}}
public class JsonStore {readonly string path=Path.Combine(AppContext.BaseDirectory,"App_Data","boards.json");readonly object gate=new();List<Board> Read(){if(!File.Exists(path))return new();return JsonSerializer.Deserialize<List<Board>>(File.ReadAllText(path))??new();}void Write(List<Board> data){Directory.CreateDirectory(Path.GetDirectoryName(path)!);var tmp=path+".tmp";File.WriteAllText(tmp,JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));File.Move(tmp,path,true);}public List<Board> All(){lock(gate)return Read();}public Board? Find(Guid id){lock(gate)return Read().FirstOrDefault(x=>x.Id==id);}public void Save(Board b){lock(gate){var d=Read();d.Add(b);Write(d);}}public bool Replace(Guid id,Board b){lock(gate){var d=Read();var i=d.FindIndex(x=>x.Id==id);if(i<0)return false;b.Id=id;d[i]=b;Write(d);return true;}}public bool Delete(Guid id){lock(gate){var d=Read();var ok=d.RemoveAll(x=>x.Id==id)>0;if(ok)Write(d);return ok;}}}