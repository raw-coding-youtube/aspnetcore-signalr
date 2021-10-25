using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace DrawingGame
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection?.User?.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        }
    }
}