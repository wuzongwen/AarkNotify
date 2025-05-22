using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AarkNotify.Middleware
{
    public class IpRateLimitMiddleware
    {
        private static readonly Dictionary<string, List<DateTime>> _ipAccessLog = new();
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        private const int INTERVAL = 60; // 秒

        public IpRateLimitMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrEmpty(ip))
                {
                    await _next(context);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var systemSettings = await dbContext.SystemSettings.FirstOrDefaultAsync();
                if (systemSettings == null)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 400, message = "系统错误，请稍后再试", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }));
                    return;
                }

                var _blackList = systemSettings.BlackList.Split(",");
                if (_blackList.Contains(ip))
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 400, message = "禁止访问", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }));
                    return;
                }

                var _whiteList = systemSettings.WhiteList.Split(",");
                if (_whiteList.Contains(ip))
                {
                    await _next(context);
                    return;
                }

                lock (_ipAccessLog)
                {
                    if (!_ipAccessLog.ContainsKey(ip))
                    {
                        _ipAccessLog[ip] = new List<DateTime>();
                    }

                    _ipAccessLog[ip].Add(DateTime.UtcNow);
                    _ipAccessLog[ip] = _ipAccessLog[ip]
                        .Where(t => t > DateTime.UtcNow.AddSeconds(-INTERVAL))
                        .ToList();

                    if (_ipAccessLog[ip].Count > systemSettings.Limit)
                    {
                        if (_ipAccessLog[ip].Count >= 15) 
                        {
                            systemSettings.BlackList += $",{ip}";
                            dbContext.SystemSettings.Update(systemSettings);
                            SystemLogs systemLogs = new SystemLogs();
                            systemLogs.Content = $"IP: {ip} 访问过于频繁，已加入黑名单";
                            systemLogs.Time = DateTime.Now;
                            dbContext.SystemLogs.Add(systemLogs);
                            dbContext.SaveChangesAsync();
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "text/plain; charset=utf-8";
                            context.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 400, message = "禁止访问", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }));
                            return;
                        }

                        context.Response.StatusCode = 429;
                        context.Response.Headers["Retry-After"] = "60";
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 400, message = "请求过于频繁，请一分钟后再请求，否则IP将被拉黑", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }));
                        return;
                    }
                }
            }
            catch
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.WriteAsync("系统错误，请稍后再试").Wait();
                return;
            }

            await _next(context);
        }
    }

}
