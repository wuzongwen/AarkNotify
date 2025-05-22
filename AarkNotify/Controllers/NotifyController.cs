using AarkNotify.Helper;
using AarkNotify.Helper.MsgModel;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("{key}/{title}/{body}")]
        public async Task<IActionResult> GetWithTitle(string key, string title, string body)
        {
            MsgModel msgModel = new MsgModel();
            msgModel.Title = title;
            msgModel.Content = body;
            if (await Notify(msgModel, key))
            {
                return Ok(new { code = 200, message = "消息发送成功", title, body });
            }
            return Ok(new { code = -1, message = "消息发送失败", title, body });
        }

        // 无标题（只有内容）
        [HttpGet("{key}/{body}")]
        public async Task<IActionResult> GetWithoutTitle(string key, string body)
        {
            MsgModel msgModel = new MsgModel();
            msgModel.Title = body;
            msgModel.Content = body;
            if (await Notify(msgModel, key))
            {
                return Ok(new { code = 200, message = "消息发送成功", body });
            }
            return Ok(new { code = -1, message = "消息发送失败", body });
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
    }
}
