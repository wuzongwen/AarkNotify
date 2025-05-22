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
// ��� SQLite ���ݿ�֧��
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

builder.Services.AddControllers();

var app = builder.Build();

// ȷ�����ݿ��Ѵ���
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseMiddleware<IpRateLimitMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
