using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(o=>o.UseInMemoryDatabase("fluent-model"));
builder.Services.AddSingleton<JsonStore>();
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
app.Run();
public class Board {public Guid Id {get;set;} public string Name {get;set;}="New board";public List<BoardItem> Items {get;set;}=new();}
public class BoardItem {public Guid Id {get;set;}=Guid.NewGuid();public Guid BoardId {get;set;}public string Text {get;set;}="";public bool Done {get;set;}}
public class AppDb:DbContext {public AppDb(DbContextOptions<AppDb> options):base(options){} public DbSet<Board> Boards=>Set<Board>(); public DbSet<BoardItem> Items=>Set<BoardItem>();protected override void OnModelCreating(ModelBuilder b){b.ApplyConfiguration(new BoardMap());b.ApplyConfiguration(new ItemMap());}}
public class BoardMap:IEntityTypeConfiguration<Board>{public void Configure(EntityTypeBuilder<Board> b){b.HasKey(x=>x.Id);b.Property(x=>x.Name).IsRequired().HasMaxLength(120);b.HasMany(x=>x.Items).WithOne().HasForeignKey(x=>x.BoardId).OnDelete(DeleteBehavior.Cascade);}}
public class ItemMap:IEntityTypeConfiguration<BoardItem>{public void Configure(EntityTypeBuilder<BoardItem> b){b.HasKey(x=>x.Id);b.Property(x=>x.Text).IsRequired().HasMaxLength(500);}}
public class JsonStore {readonly string path=Path.Combine(AppContext.BaseDirectory,"App_Data","boards.json");readonly object gate=new();List<Board> Read(){if(!File.Exists(path))return new();return JsonSerializer.Deserialize<List<Board>>(File.ReadAllText(path))??new();}void Write(List<Board> data){Directory.CreateDirectory(Path.GetDirectoryName(path)!);var tmp=path+".tmp";File.WriteAllText(tmp,JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));File.Move(tmp,path,true);}public List<Board> All(){lock(gate)return Read();}public Board? Find(Guid id){lock(gate)return Read().FirstOrDefault(x=>x.Id==id);}public void Save(Board b){lock(gate){var d=Read();d.Add(b);Write(d);}}public bool Replace(Guid id,Board b){lock(gate){var d=Read();var i=d.FindIndex(x=>x.Id==id);if(i<0)return false;b.Id=id;d[i]=b;Write(d);return true;}}public bool Delete(Guid id){lock(gate){var d=Read();var ok=d.RemoveAll(x=>x.Id==id)>0;if(ok)Write(d);return ok;}}}