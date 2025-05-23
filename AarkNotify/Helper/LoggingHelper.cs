using Microsoft.Extensions.Logging;

namespace AarkNotify.Helper
{
    public static class LoggingHelper
    {
        private static ILoggerFactory _loggerFactory;

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// 常规日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            var logger = _loggerFactory.CreateLogger("General");
            logger.LogInformation(message);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(string message)
        {
            var logger = _loggerFactory.CreateLogger("General");
            logger.LogWarning(message);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Error(string message, Exception ex = null)
        {
            var logger = _loggerFactory.CreateLogger("General");
            logger.LogError(ex, message);
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            var logger = _loggerFactory.CreateLogger("General");
            logger.LogDebug(message);
        }
    }
}
