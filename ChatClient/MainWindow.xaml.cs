using ChatClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace ChatClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string _ClientID = "Chưa kết nối";

    public string ClientID
    {
        get { return _ClientID; }
        set
        {
            if (_ClientID != value)
            {
                _ClientID = value;
                OnPropertyChanged(nameof(ClientID));
            }
        }
    }
    HubConnection connection;
    //Managing connected users and created groups
    public ObservableCollection<string> UserList { get; set; } = new();
    //Managing chat data
    public ObservableCollection<MessageStyle> CurrentChat { get; set; } = new();
    public ObservableCollection<MessageStyle> AllChat { get; set; } = new();

    private string? SelectedClientID = null;
    private Button? LastButtonClicked;
    public Dictionary<string, ObservableCollection<MessageStyle>> PrivateChat { get; set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        messages.ItemsSource = AllChat;
        connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithUrl("https://signalr-chat-demo.azurewebsites.net/chatHub")
            .Build();

        //Attempt to reconnect when lost connection to server
        connection.Reconnecting += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Đang kết nối lại...";
                AllChat.Add(new SystemMessage { Content = newMessage });
            });
            return Task.CompletedTask;
        };

        connection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Đã kết nối lại với server.";
                ClientID = connection.ConnectionId;
                AllChat.Clear();
                AllChat.Add(new SystemMessage { Content = newMessage });
            });
            return Task.CompletedTask;
        };

        connection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Đã ngắt kết nối.";
                ClientID = "Chưa kết nối";
                AllChat.Clear();
                AllChat.Add(new SystemMessage { Content = newMessage });
                openConnection.IsEnabled = true;
                sendMessage.IsEnabled = false;
                userNameInput.IsEnabled = true;
            });
            return Task.CompletedTask;
        };
    }

    private async void openConnection_Click(object sender, RoutedEventArgs e)
    {
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                MessageStyle newMessage;
                if (user == userNameInput.Text)
                {
                    newMessage = new SentMessage { Content = message };
                }
                else
                {
                    newMessage = new ReceivedMessage { Content = $"{user}: {message}" };
                }
                AllChat.Add(newMessage);
            });
        });
        connection.On<string, string, string, string>("ReceivePrivateMessage", (user, SenderID, ReceiverID, message) =>
                {
                    ObservableCollection<MessageStyle>? ChatLog;
                    if (SenderID == _ClientID)
                    {
                        if (!PrivateChat.TryGetValue(ReceiverID, out ChatLog))
                            ChatLog = new();
                        ChatLog.Add(new SentMessage { Content = message });
                    }
                    else
                    {
                        if (!PrivateChat.TryGetValue(SenderID, out ChatLog))
                        {
                            ChatLog = new();
                            PrivateChat.Add(SenderID, ChatLog);
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            ChatLog.Add(new ReceivedMessage { Content = $"{user}: {message}" });
                        });
                       
                    }
                });
        connection.On<string>("Broadcast", (message) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AllChat.Add(new SystemMessage { Content = message });
            });
        });

        connection.On<List<string>>("ReceiveInitializeUserList", (list) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UserList.Clear();
                foreach (var user in list)
                {
                    if (user != _ClientID)
                        UserList.Add(user);
                }
            });
        });

        try
        {
            AllChat.Add(new SystemMessage { Content = $"Bắt đầu kết nối" });
            await connection.StartAsync();
            ClientID = connection.ConnectionId ?? "Lỗi...";
            await connection.InvokeAsync("RequestInitializeUserList");
            LastButtonClicked = AllChatButton;
            openConnection.IsEnabled = false;
            sendMessage.IsEnabled = true;
        }
        catch (Exception ex)
        {
            AllChat.Add(new SystemMessage { Content = ex.Message });
        }
    }
    private async void sendMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await connection.InvokeAsync("SendMessage", userNameInput.Text, messageInput.Text);
        }
        catch (Exception ex)
        {
            AllChat.Add(new SystemMessage { Content = ex.Message });
        }
    }

    private async void sendPrivateMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PrivateChat[SelectedClientID!].Add(new SentMessage { Content = messageInput.Text });
            await connection.InvokeAsync("SendPrivateMessage", userNameInput.Text, SelectedClientID, messageInput.Text);
        }
        catch (Exception ex)
        {
            PrivateChat[SelectedClientID!].Add(new SystemMessage { Content = ex.Message });
        }
    }
    private void NewMessageNotification(ContentControl button)
    {
        //If button is not currently selected
        this.Dispatcher.Invoke(() =>
        {
            if (button.Background != Brushes.LightBlue)
            {
                button.Background = Brushes.Red;
            }
        });
    }

    private void ClickedButtonBackgroundBehavior(Button sender)
    {
        // Deselect previous button
        if (LastButtonClicked != null)
        {
            LastButtonClicked.Background = Brushes.White;
        }
        //Set Clickedbutton and change its color
        var ClickedButton = sender;
        ClickedButton.Background = Brushes.LightBlue;
        LastButtonClicked = ClickedButton;
    }
    //Switch to All Chat 
    private void AllChatButton_Click(object sender, RoutedEventArgs e)
    {
        ClickedButtonBackgroundBehavior((Button)sender);
        messages.ItemsSource = AllChat;
        RemoveRoutedEventHandlers(sendMessage, Button.ClickEvent);
        sendMessage.Click += sendMessage_Click;
    }
    private void PrivateChatButton_Click(object sender, RoutedEventArgs e)
    {
        ClickedButtonBackgroundBehavior((Button)sender);
        //Set chat window to private chat
        SelectedClientID = (sender as Button)!.Content.ToString()!;
        ObservableCollection<MessageStyle>? ChatLog;
        if (!PrivateChat.TryGetValue(SelectedClientID, out ChatLog))
        {
            ChatLog = new();
            PrivateChat.Add(SelectedClientID, ChatLog);
        }
        RemoveRoutedEventHandlers(sendMessage, Button.ClickEvent);
        sendMessage.Click += new RoutedEventHandler(sendPrivateMessage_Click);
        messages.ItemsSource = PrivateChat[SelectedClientID];
    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
    {
        // Get the EventHandlersStore instance which holds event handlers for the specified element.
        // The EventHandlersStore class is declared as internal.
        var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
            "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
        object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

        // If no event handlers are subscribed, eventHandlersStore will be null.
        // Credit: https://stackoverflow.com/a/16392387/1149773
        if (eventHandlersStore == null)
            return;

        // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
        // for getting an array of the subscribed event handlers.
        var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
            "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
            eventHandlersStore, new object[] { routedEvent });

        // Iteratively remove all routed event handlers from the element.
        foreach (var routedEventHandler in routedEventHandlers)
            element.RemoveHandler(routedEvent, routedEventHandler.Handler);
    }
}