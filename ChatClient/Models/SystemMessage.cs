using System.Windows;

namespace ChatClient.Models
{
    internal class SystemMessage:MessageStyle
    {
        public string Content { get; set; }
        public HorizontalAlignment Alignment { get; set; }= HorizontalAlignment.Center;
    }
}
