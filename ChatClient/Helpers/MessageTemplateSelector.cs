using ChatClient.Models;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient.Helpers;

internal class MessageTemplateSelector:DataTemplateSelector
{
    public DataTemplate LeftAlignedTemplate { get; set; }
    public DataTemplate RightAlignedTemplate { get;set; }
    public DataTemplate CenteredTemplate { get; set; }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is MessageStyle message)
        {
            switch (message.Alignment)
            {
                case HorizontalAlignment.Left: return LeftAlignedTemplate;
                case HorizontalAlignment.Right: return RightAlignedTemplate;
                case HorizontalAlignment.Center: return CenteredTemplate;
            }
        }
        return base.SelectTemplate(item, container);
    }
}
