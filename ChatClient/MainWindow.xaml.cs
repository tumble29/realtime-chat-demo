using ChatClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace ChatClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    HubConnection connection;
    public ObservableCollection<string> UserList { get; set; } = new();
    public ObservableCollection<string> GroupList { get; set; } = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithUrl("https://localhost:7055/chatHub")
            .Build();

        //Attempt to reconnect when lost connection to server
        connection.Reconnecting += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Đang kết nối lại...";
                messages.Items.Add(new SystemMessage { Content = newMessage });
            });
            return Task.CompletedTask;
        };

        connection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Đã kết nối lại với server.";
                messages.Items.Clear();
                messages.Items.Add(new SystemMessage { Content = newMessage });
            });
            return Task.CompletedTask;
        };

        connection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Đã ngắt kết nối.";
                messages.Items.Clear();
                messages.Items.Add(new SystemMessage { Content = newMessage });
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
                messages.Items.Add(newMessage);
            });
        });

        connection.On<string>("Broadcast", (message) =>
        {
            Application.Current.Dispatcher.Invoke(() => {
                messages.Items.Add(message);
            });
        });

        connection.On<List<string>>("ReceiveInitializeUserList", (list) =>
        {
            Application.Current.Dispatcher.Invoke(() => {
                UserList.Clear();
                foreach (var user in list)
                {
                    UserList.Add(user);
                }
            });
        });


        try
        {
            messages.Items.Add(new SystemMessage { Content = $"Bắt đầu kết nối" });
            await connection.StartAsync();
            await connection.InvokeAsync("RequestInitializeUserList");
            openConnection.IsEnabled = false;
            sendMessage.IsEnabled = true;
            userNameInput.IsEnabled = false;
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private async void sendMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MessageContent messageContent = new MessageContent(userNameInput.Text, MessageType.All, messageInput.Text);

            await connection.InvokeAsync("SendMessage", userNameInput.Text, messageInput.Text);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

}
