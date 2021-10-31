using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatRegistry _registry;

        public ChatHub(ChatRegistry registry)
        {
            _registry = registry;
        }

        public async Task<List<OutputMessage>> JoinRoom(RoomRequest request)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, request.Room);

            return _registry.GetMessages(request.Room)
                .Select(m => m.Output)
                .ToList();
        }

        public Task LeaveRoom(RoomRequest request)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, request.Room);
        }

        public Task SendMessage(InputMessage message)
        {
            var username = Context.User.Claims.FirstOrDefault(x => x.Type == "username").Value;

            var userMessage = new UserMessage(
                new(Context.UserIdentifier, username),
                message.Message,
                message.Room,
                DateTimeOffset.Now
            );

            _registry.AddMessage(message.Room, userMessage);
            return Clients.GroupExcept(message.Room, new[] { Context.ConnectionId })
                .SendAsync("send_message", userMessage.Output);
        }
    }
}