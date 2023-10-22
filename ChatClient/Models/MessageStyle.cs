using System.Windows;

namespace ChatClient.Models;

public interface MessageStyle
{
    public string Content { get; set; }
    public HorizontalAlignment Alignment { get; set; }
}