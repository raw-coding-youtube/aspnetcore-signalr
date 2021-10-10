using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FeatureOverview
{
    public class GroupsHub : Hub
    {
        public Task Join() => Groups.AddToGroupAsync(Context.ConnectionId, "group_name");

        public Task Leave() => Groups.RemoveFromGroupAsync(Context.ConnectionId, "group_name");

        public Task Message() => Clients
            .Groups("group_name")
            .SendAsync("group_message", new Data(69, "secret group message"));

    }
}