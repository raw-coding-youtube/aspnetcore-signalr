using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrawingGame
{
    [ApiController]
    [Authorize]
    [Route("/api/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly RoomManager _manager;

        public RoomController(RoomManager manager) => _manager = manager;

        [HttpGet("my")]
        public IActionResult MyRoom()
        {
            var userId = HttpContext.UserId();

            var myRoom = _manager.GetRoomByUserId(userId);

            if (myRoom == null)
                return NoContent();

            return Ok(new RoomView(myRoom, userId));
        }

        public IActionResult List()
        {
            return Ok(_manager.Rooms.Select(x => new RoomView(x, HttpContext.UserId())));
        }
    }
}