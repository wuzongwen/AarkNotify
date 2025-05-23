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

// ��� SQLite ���ݿ�֧��
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

if (isDebug)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.Extensions.Http.Resilience.IHttpStandardResiliencePipelineBuilder", LogEventLevel.Fatal)
                    //.MinimumLevel.Override("MyApp.Services", LogEventLevel.Verbose) // �����ض�ģ������ Verbose
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
    // ע�� Serilog �� DI ϵͳ
    builder.Host.UseSerilog();
    builder.Services.AddControllers(options =>
    {
        // ������󷽷�ʱ����־��¼������
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
    // ��ʼ�� LoggingHelper
    LoggingHelper.Initialize(app.Services.GetRequiredService<ILoggerFactory>());
}

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
