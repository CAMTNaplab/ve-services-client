using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class MediaRoom : BaseRoomManager<MediaRoomState>
    {
        public MediaRoom() : base("mediaRoom", new Dictionary<string, object>())
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
            Room.OnMessage<MediaResp>("resp", OnRespMsg);
        }

        private void OnRespMsg(MediaResp data)
        {

        }
    }
}
