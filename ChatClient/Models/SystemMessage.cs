using System.Windows;

namespace ChatClient.Models
{
    internal class SystemMessage:MessageStyle
    {
        public string Content { get; set; }

        public override string ToString() => Content ?? string.Empty;
    }
}
