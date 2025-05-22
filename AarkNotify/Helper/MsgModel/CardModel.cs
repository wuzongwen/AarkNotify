using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AarkNotify.Helper.MsgModel
{
    public class LarkMd
    {
        public string tag { get; set; }
        public string content { get; set; }
    }

    public class Text
    {
        public string tag { get; set; }
        public string content { get; set; }
    }

    public class ButtonAction
    {
        public string tag { get; set; }
        public Text text { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public object value { get; set; }
    }

    public class ActionElement
    {
        public string tag { get; set; }
        public List<ButtonAction> actions { get; set; }
    }

    public class DivElement
    {
        public string tag { get; set; }
        public LarkMd text { get; set; }
    }

    public class Header
    {
        public Title title { get; set; }
        public Title subtitle { get; set; }
        public string template { get; set; }
        public Icon ud_icon { get; set; }
        public List<TabListItem> text_tag_list { get; set; }
    }

    public class Title
    {
        public string tag { get; set; }
        public string content { get; set; }
    }

    public class Card
    {
        public Header header { get; set; }
        public List<object> elements { get; set; } // This list can contain both DivElement and ActionElement, so we use object type.
    }

    public class CardModel
    {
        public string timestamp { get; set; }
        public string sign { get; set; }
        public string msg_type { get; set; }
        public Card card { get; set; }
    }

    public class Icon
    {
        public string token { get; set; }

        public IconColor style { get; set; }
    }

    public class IconColor
    {
        public string color { get; set; }
    }

    public class TabListItem
    {
        public string tag { get; set; }

        public string color { get; set; }

        public Text text { get; set; }
    }
}
