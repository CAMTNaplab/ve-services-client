using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VEServicesClient
{
    public class ChatRoom : BaseRoomManager<ChatRoomState>
    {
        public ChatRoom() : base("chatRoom", new Dictionary<string, object>())
        {
        }
    }
}
