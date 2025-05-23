using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Options;
using AarkNotify.Helper;

namespace AarkNotify.Filters
{
    /// <summary>
    /// 执行操作前的过滤器（可以在方法、类、派生类使用）
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class LogFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 服务容器的对象
        /// </summary>
        public static class MyServiceProvider
        {
            /// <summary>
            /// 服务
            /// </summary>
            public static IServiceProvider ServiceProvider
            {
                get; set;
            }
        }

        private string ActionArguments { get; set; }

        private string Result { get; set; }

        private string ApiPath { get; set; }

        private string IP { get; set; }

        private string Headers { get; set; }

        private string ConnectionId { get; set; }

        private int HttpCode { get; set; }

        private Stopwatch Stopwatch { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogFilterAttribute()
        {
        }


        /// <summary>
        /// 操作筛选器之前
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            var apiName = actionContext.HttpContext.Request.Path;
            ActionArguments = JsonConvert.SerializeObject(actionContext.ActionArguments);

            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// 操作筛选器之后
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Stopwatch.Stop();
            base.OnActionExecuted(context);
        }

        /// <summary>
        /// 执行完毕后
        /// </summary>
        /// <param name="context"></param>
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            ApiPath = context.HttpContext.Request.Path.ToString()?.ToLower();
            IP = NetWorkHelper.GetClientIp(context.HttpContext);
            ConnectionId = context.HttpContext.Connection.Id;
            Headers = JsonConvert.SerializeObject(context.HttpContext.Request.Headers);
            HttpCode = context.HttpContext.Response.StatusCode;

            var jsonResult = context.Result as JsonResult;

            if (jsonResult != null)
            {
                Result = JsonConvert.SerializeObject(jsonResult.Value);
            }
            else
            {
                var contentResult = context.Result as ContentResult;
                if (contentResult != null)
                {
                    Result = JsonConvert.SerializeObject(contentResult.Content);
                }
                else
                {
                    Result = JsonConvert.SerializeObject((context.Result));
                }
            }

            Task.Run(() =>
            {
                //记录日志
                LogHandle(context);
            });

            base.OnResultExecuted(context);
        }

        /// <summary>
        /// 接口日志
        /// </summary>
        /// <param name="context"></param>
        private void LogHandle(ResultExecutedContext context)
        {
            LoggingHelper.Info("【ID】：" + ConnectionId +
                    "\r\n【IP】：" + IP +
                    "\r\n【接口】：" + ApiPath +
                    "\r\n【请求头】：" + Headers +
                    "\r\n【请求】：" + ActionArguments +
                    "\r\n【返回】：" + Result +
                    "\r\n【耗时】：" + Stopwatch?.Elapsed.TotalMilliseconds + "毫秒");
        }
    }
}
