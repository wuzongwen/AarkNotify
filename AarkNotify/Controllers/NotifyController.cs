using AarkNotify.Helper;
using AarkNotify.Helper.MsgModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AarkNotify.Controllers
{
    [ApiController]
    [Route("")]
    public class NotifyController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public NotifyController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 有标题
        [HttpGet]
        [Route("{key}/{title}/{body}")]
        public async Task<IActionResult> NotifyWithTitle(string key, string title, string body)
        {
            MsgModel msgModel = new MsgModel();
            msgModel.Title = title;
            msgModel.Content = body;
            if (await Notify(msgModel, key))
            {
                return Ok(new { code = 200, message = "success", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
            }
            return Ok(new { code = 400, message = "消息发送失败", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
        }

        // 无标题（只有内容）
        [HttpGet]
        [Route("{key}/{body}")]
        public async Task<IActionResult> NotifyNoTitle(string key, string body)
        {
            MsgModel msgModel = new MsgModel();
            msgModel.Title = body;
            msgModel.Content = body;
            if (await Notify(msgModel, key))
            {
                return Ok(new { code = 200, message = "success", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
            }
            return Ok(new { code = 400, message = "消息发送失败", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
        }

        [HttpPost("{deviceKey}")]
        public async Task<IActionResult> Notify([FromBody] BarkMessage message, string deviceKey)
        {
            var realDeviceKey = deviceKey;

            if (string.IsNullOrWhiteSpace(message.body) || string.IsNullOrWhiteSpace(realDeviceKey))
                return Ok(new { code = 400, message = "消息发送失败，缺少必须参数", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });

            MsgModel msgModel = new MsgModel();
            msgModel.Title = message.title;
            msgModel.Content = message.body;
            if (await Notify(msgModel, realDeviceKey))
            {
                return Ok(new { code = 200, message = "success", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
            }
            return Ok(new { code = 400, message = "消息发送失败", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
        }

        [HttpPost("/push")]
        public async Task<IActionResult> PushNotify([FromBody] BarkMessage message)
        {
            var realDeviceKey = message.device_key;

            if (string.IsNullOrWhiteSpace(message.body) || string.IsNullOrWhiteSpace(realDeviceKey))
                return Ok(new { code = 400, message = "消息发送失败，缺少必须参数", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });

            MsgModel msgModel = new MsgModel();
            msgModel.Title = message.title;
            msgModel.Content = message.body;
            if (await Notify(msgModel, realDeviceKey))
            {
                return Ok(new { code = 200, message = "success", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
            }
            return Ok(new { code = 400, message = "消息发送失败", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() });
        }

        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="msgModel"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> Notify(MsgModel msgModel,string key) 
        {
            var nofitySetting = await _dbContext.NotifySettings.Where(o => o.NotifyKey == key).FirstOrDefaultAsync();
            if (nofitySetting != null)
            {
                var msg = FeiShuHelper.CreateFeishuTextMessage(msgModel, nofitySetting.Secret);
                return await FeiShuHelper.SendToFeishu(nofitySetting.NotifyPath, msg);
            }
            return false;
        }

        public class BarkMessage
        {
            public string? title { get; set; }
            public string? body { get; set; }
            public string? device_key { get; set; }  // 可选
        }
    }
}
