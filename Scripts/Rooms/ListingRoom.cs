using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class ListingRoom : BaseRoomManager<ListingRoomState>
    {
        private GameServerData data;

        public ListingRoom(GameServerData data) : base("listingRoom", new Dictionary<string, object>())
        {
            this.data = data;
        }

        public override async Task<bool> Join()
        {
            Options["secret"] = ClientInstance.Instance.secret;
            Options["data"] = data;
            if (await base.Join())
            {
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            Options["secret"] = ClientInstance.Instance.secret;
            Options["data"] = data;
            if (await base.JoinById(id))
            {
                return true;
            }
            return false;
        }

        public async Task SendUpdate(GameServerData data)
        {
            this.data = data;
            await Room.Send("update", data);
        }
    }
}
