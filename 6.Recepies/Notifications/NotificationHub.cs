using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Notifications
{
    [Authorize]
    public class NotificationHub : Hub
    {

    }
}