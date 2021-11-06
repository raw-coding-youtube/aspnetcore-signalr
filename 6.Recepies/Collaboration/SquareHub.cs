using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Collaboration
{
    [Authorize]
    public class SquareHub : Hub
    {
        private readonly State _state;

        public SquareHub(State state)
        {
            _state = state;
        }

        public IEnumerable<int> GetSquares() => _state.Squares;

        public Task SendDragEvent(DragEvent dEvent)
        {
            var username = Context.User?.Claims.FirstOrDefault(x => x.Type == "userid")?.Value ?? "barry";
            var colour = Context.User?.Claims.FirstOrDefault(x => x.Type == "color")?.Value ?? "#ff0000";
            return Clients.Others.SendAsync("on_drag", new UserDragEvent(dEvent, username, colour));
        }

        public async Task<IEnumerable<int>> EndDrag(DragEvent dEvent)
        {
            var username = Context.User?.Claims.FirstOrDefault(x => x.Type == "userid")?.Value ?? "barry";
            _state.Move(dEvent.OriginalPos, dEvent.CurrentPos);
            await Clients.Others.SendAsync("end_drag", new { _state.Squares, Username = username });
            return _state.Squares;
        }
    }

    public record DragEvent(int CurrentPos, int OriginalPos);

    public record UserDragEvent(DragEvent Drag, string Username, string Colour)
        : DragEvent(Drag.CurrentPos, Drag.OriginalPos);
}