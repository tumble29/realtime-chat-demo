using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatClient.Components
{
    /// <summary>
    /// Interaction logic for ChatTab.xaml
    /// </summary>
    public partial class ChatTab : UserControl
    {

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ChatTab), new PropertyMetadata(string.Empty));




        public ObservableCollection<string> ContentList
        {
            get { return (ObservableCollection<string>)GetValue(ContentListProperty); }
            set { SetValue(ContentListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentListProperty =
            DependencyProperty.Register("ContentList", typeof(ObservableCollection<string>), typeof(ChatTab), new PropertyMetadata(new ObservableCollection<string>()));



        public ChatTab()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (contentList.Visibility == Visibility.Visible)
            {
                contentList.Visibility = Visibility.Collapsed;
            }
            else
            {
                contentList.Visibility = Visibility.Visible;
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedItem = (ListBoxItem)sender;
            MessageBox.Show($"Clicked: {clickedItem.Content}");
        }
        private void ListBoxButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = (Button)sender;
            MessageBox.Show($"Clicked: {clickedItem.Content}");
        }
    }
}
