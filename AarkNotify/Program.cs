using AarkNotify;
using AarkNotify.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
if (string.IsNullOrWhiteSpace(dbPath))
{
    dbPath = builder.Configuration.GetConnectionString("Sqlite");
}
else
{
    dbPath = $"Data Source={dbPath}";
}
// 添加 SQLite 数据库支持
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

builder.Services.AddControllers();

var app = builder.Build();

// 确保数据库已创建
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseMiddleware<IpRateLimitMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
