using System.Windows;

namespace ChatClient.Models
{
    internal class SentMessage : MessageStyle
    {
        public string Content { get; set; }
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Right;
    }
}
