using Flurl.Http;
using AarkNotify.Helper.MsgModel;
using XianBao.Helper.MsgModel;
using System.Security.Cryptography;
using System.Text;

namespace AarkNotify.Helper
{
    public class FeiShuHelper
    {
        /// <summary>
        /// 创建飞书卡片消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static CardModel CreateFeishuCardMessage(MsgModel.MsgModel msg, List<TabListItem> tabLists, string secret)
        {
            CardModel cardModel = new CardModel();
            Card card = new Card();
            Header header = new Header();
            header.title = new Title()
            {
                content = $"{msg.Title}",
                tag = "plain_text"
            };
            header.subtitle = new Title()
            {
                content = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}",
                tag = "plain_text"
            };
            header.template = "blue";
            header.ud_icon = new Icon()
            {
                token = "building_outlined",
                style = new IconColor()
                {
                    color = "blue"
                }
            };

            header.text_tag_list = tabLists;
            List<object> Elements = new List<object>();
            DivElement divElement = new DivElement();
            divElement.tag = "div";
            divElement.text = new LarkMd()
            {
                tag = "lark_md",
                content = msg.Content,
            };
            Elements.Add(divElement);
            card.elements = Elements;
            card.header = header;

            //签名
            var sign = GenSign(secret);
            cardModel.timestamp = sign.Timestamp;
            cardModel.sign = sign.Sign;
            cardModel.card = card;
            cardModel.msg_type = "interactive";

            return cardModel;
        }

        /// <summary>
        /// 创建飞书文本消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TextModel CreateFeishuTextMessage(MsgModel.MsgModel msg, string secret)
        {
            TextModel textModel = new TextModel();
            //签名
            var sign = GenSign(secret);
            textModel.timestamp = sign.Timestamp;
            textModel.sign = sign.Sign;
            textModel.content = new TextContent
            {
                text = msg.Title + "\r\n" + msg.Content
            };
            textModel.msg_type = "text";
            return textModel;
        }

        /// <summary>
        /// 发送消息到飞书自定义机器人
        /// </summary>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<bool> SendToFeishu(string url, object message)
        {
            string feishuWebhookUrl = url; // 替换为你的飞书 Webhook URL

            try
            {
                var response = await feishuWebhookUrl
                    .PostJsonAsync(message);

                if (response.StatusCode == 200)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static (string Timestamp, string Sign) GenSign(string secret)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signContent = $"{timestamp}\n{secret}";

            byte[] contentByte = Encoding.UTF8.GetBytes(signContent);

            using var hmacsha256 = new HMACSHA256Final(contentByte);
            return (timestamp, Convert.ToBase64String(hmacsha256.HashFinal()));
        }

        public class HMACSHA256Final : HMACSHA256
        {
            public HMACSHA256Final(byte[] bytes) : base(bytes)
            {

            }

            public new byte[] HashFinal()
            {
                return base.HashFinal();
            }
        }
    }
}
