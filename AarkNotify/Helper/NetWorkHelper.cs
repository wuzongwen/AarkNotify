namespace AarkNotify.Helper
{
    public class NetWorkHelper
    {
        // 新增方法：获取真实 IP
        public static string GetClientIp(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ip))
            {
                // 如果有多个 IP，取第一个
                ip = ip.Split(',').FirstOrDefault()?.Trim();
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }

            // 去除 IPv6 映射前缀 ::ffff:
            if (ip != null && ip.StartsWith("::ffff:"))
            {
                ip = ip.Substring(7);
            }

            return ip ?? "unknown";
        }
    }
}
