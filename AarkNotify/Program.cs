using AarkNotify;
using AarkNotify.Filters;
using AarkNotify.Helper;
using AarkNotify.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
if (string.IsNullOrWhiteSpace(dbPath)) 
{
    dbPath = $"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "aarknotify.db")}";
}
else
{
    dbPath = $"Data Source={dbPath}";
}
var logPath = Environment.GetEnvironmentVariable("LOG_PATH");
if (string.IsNullOrWhiteSpace(logPath))
{
    logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
}
var debug = Environment.GetEnvironmentVariable("DEBUG");
bool isDebug = false;
if (!string.IsNullOrWhiteSpace(debug))
{
    isDebug = Convert.ToBoolean(debug); 
}
else 
{
    isDebug = builder.Configuration.GetValue<bool>("SystemConfig:Debug");
}

// 添加 SQLite 数据库支持
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

if (isDebug)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.Extensions.Http.Resilience.IHttpStandardResiliencePipelineBuilder", LogEventLevel.Fatal)
                    //.MinimumLevel.Override("MyApp.Services", LogEventLevel.Verbose) // 仅对特定模块启用 Verbose
                    .WriteTo.Logger(lc =>
                    lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Information).WriteTo.File($"{logPath}/Info/.log", rollingInterval: RollingInterval.Hour))
                    .WriteTo.Logger(lc
                    => lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Error).WriteTo.File($"{logPath}/Error/.log", rollingInterval: RollingInterval.Hour))
                    .WriteTo.Logger(lc
                    => lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Warning).WriteTo.File($"{logPath}/Warning/.log", rollingInterval: RollingInterval.Hour))
                    .WriteTo.Logger(lc
                    => lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Debug).WriteTo.File($"{logPath}/Debug/.log", rollingInterval: RollingInterval.Hour))
                    .WriteTo.Logger(lc
                    => lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Verbose).WriteTo.File($"{logPath}/Trace/.log", rollingInterval: RollingInterval.Hour))
        .CreateLogger();
    // 注册 Serilog 到 DI 系统
    builder.Host.UseSerilog();
    builder.Services.AddControllers(options =>
    {
        // 添加请求方法时的日志记录过滤器
        options.Filters.Add<LogFilterAttribute>();
    });
}
else 
{
    builder.Services.AddControllers();
}

var app = builder.Build();

if (isDebug) 
{
    // 初始化 LoggingHelper
    LoggingHelper.Initialize(app.Services.GetRequiredService<ILoggerFactory>());
}

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
