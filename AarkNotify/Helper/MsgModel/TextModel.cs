using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XianBao.Helper.MsgModel
{
    public class TextModel
    {
        public string timestamp { get; set; }
        public string sign { get; set; }
        public string msg_type { get; set; }

        public TextContent content { get; set; }
    }

    public class TextContent 
    {
        public string text { get; set; }
    }
}
