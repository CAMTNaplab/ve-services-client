using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VEServicesClient
{
    public class BroadcastRoom : BaseRoomManager<BroadcastRoomState>
    {
        public BroadcastRoom() : base("broadcastRoom", new Dictionary<string, object>())
        {
        }

        public override async Task<bool> Join()
        {
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            if (await base.JoinById(id))
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        private void SetupRoom()
        {
            Room.OnMessage<object>("all", OnAllMsg);
            Room.OnMessage<object>("other", OnOtherMsg);
        }

        private void OnAllMsg(object data)
        {

        }

        private void OnOtherMsg(object data)
        {

        }
    }
}
