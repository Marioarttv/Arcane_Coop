using Microsoft.AspNetCore.SignalR;

namespace Arcane_Coop.Hubs;

public class GameHub : Hub
{
    public async Task JoinRoom(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("PlayerJoined", playerName, Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomId, string playerName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("PlayerLeft", playerName, Context.ConnectionId);
    }

    public async Task SendMessageToRoom(string roomId, string playerName, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", playerName, message);
    }

    public async Task SendPlayerAction(string roomId, string action, object data)
    {
        await Clients.OthersInGroup(roomId).SendAsync("PlayerActionReceived", Context.ConnectionId, action, data);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}