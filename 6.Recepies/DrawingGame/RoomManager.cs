using System.Collections.Generic;
using System.Linq;

namespace DrawingGame
{
    public class RoomManager
    {
        public List<Room> Rooms { get; } = new();
        public Room GetRoomByUserId(string userId) => Rooms.FirstOrDefault(x => x.Users.Contains(userId));
    }

    public class Room
    {
        public string Id { get; set; }
        public List<string> Users { get; } = new();
        public List<DrawEvent> DrawEvents { get; } = new();
        public string Admin => Users.FirstOrDefault();
        public string DrawingUser { get; set; }
    }

    public record DrawEvent(int FromY, int FromX, int ToX, int ToY, string Color, int Size);
}