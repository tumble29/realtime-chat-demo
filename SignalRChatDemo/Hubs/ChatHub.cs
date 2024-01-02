using Microsoft.AspNetCore.SignalR;
using SignalRChatDemo.Models;

namespace SignalRChatDemo.Hubs;

public class ChatHub : Hub
{
    //Map kết nối với user
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
    public async Task SendPrivateMessage(string UserName, string ReceiverID, string message)
    {
        //string? name = Context.User?.Identity?.Name;

        await Clients.Client(ReceiverID).SendAsync
            ("ReceivePrivateMessage", UserName, Context.ConnectionId, ReceiverID, message);
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
        await Clients.All.SendAsync("ClientDisconnected", Context.ConnectionId);
        await Clients.All.SendAsync("Broadcast", $"{name} đã rời chat!");
        await Task.CompletedTask;
    }

    public int GetConnectedClientsCount()
    {
        return _connections.Count;
    }
}