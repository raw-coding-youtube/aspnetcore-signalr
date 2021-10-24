using System.Collections.Generic;
using System.Linq;

namespace ChatApp
{
    public class ChatRegistry
    {
        private readonly Dictionary<string, List<UserMessage>> _roomMessages = new();

        public void CreateRoom(string room)
        {
            _roomMessages[room] = new();
        }

        public void AddMessage(string room, UserMessage message)
        {
            _roomMessages[room].Add(message);
        }

        public List<UserMessage> GetMessages(string room)
        {
            return _roomMessages[room];
        }

        public List<string> GetRooms()
        {
            return _roomMessages.Keys.ToList();
        }
    }
}