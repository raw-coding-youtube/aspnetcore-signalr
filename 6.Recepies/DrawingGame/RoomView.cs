using System.Collections.Generic;

namespace DrawingGame
{
    public class RoomView
    {
        public RoomView(Room room, string userId)
        {
            Id = room.Id;
            Users = room.Users;
            IsAdmin = userId == room.Admin;
            DrawingUser = room.DrawingUser;
            MyUserId = userId;
        }

        public string Id { get; set; }
        public List<string> Users { get; set; }
        public bool IsAdmin { get; set; }
        public string DrawingUser { get; set; }
        public string MyUserId { get; set; }
    }
}