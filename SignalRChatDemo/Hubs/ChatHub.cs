using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRChatDemo.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SignalRChatDemo.Hubs;

public class ChatHub : Hub
{
    private readonly static ConnectionMapping<string> _connections = new();

    public async Task RequestInitializeUserList()
    {
        var list = _connections.GetUsers();
        await Clients.All.SendAsync("ReceiveInitializeUserList", list);
    }

    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    public async Task SendPrivateMessage(string userID, string message)
    {
        string? name = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(userID))
        {
            var users = _connections.GetUsers();
            foreach(var user in users)
            {
                foreach(var connectionId in _connections.GetConnections(user)) 
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", name ?? "anonymous", message);
                }
            }
        }
        else
        {
            foreach (var connectionId in _connections.GetConnections(userID))
            {
                await Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", name ?? "anonymous", message);
            }
        }
    }
    public override async Task OnConnectedAsync()
    {
        string name = Context.User?.Identity?.Name ?? Context.ConnectionId;
        _connections.Add(name, Context.ConnectionId);

        //Broadcast a message announcing user has joiend
        await Clients.All.SendAsync("Broadcast", $"{name} đã tham gia chat!");
        await Clients.All.SendAsync("ReceiveUsername", name);

        await Task.CompletedTask;
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? name = Context.User?.Identity?.Name??Context.ConnectionId;
        _connections.Remove(name, Context.ConnectionId);
        await Clients.All.SendAsync("Broadcast", $"{name} đã rời chat!");
        await Task.CompletedTask;
    }

    public int GetConnectedClientsCount()
    {
        return _connections.Count;
    }
}