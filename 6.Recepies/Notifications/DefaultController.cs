using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notifications
{
    public class DefaultController : ControllerBase
    {
        private readonly INotificationSink _notificationSink;

        public DefaultController(INotificationSink notificationSink) => _notificationSink = notificationSink;

        [Authorize]
        [HttpGet("/notify")]
        public async Task<IActionResult> Notify(string user, string message)
        {
            await _notificationSink.PushAsync(new(user, message));
            return Ok();
        }
    }
}