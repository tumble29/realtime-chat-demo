using System.Windows;

namespace ChatClient.Models
{
    internal class ReceivedMessage : MessageStyle
    {
        public string Content { get; set; }
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
    }
}
