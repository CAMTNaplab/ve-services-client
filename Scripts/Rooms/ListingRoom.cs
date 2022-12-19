using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class ListingRoom : BaseRoomManager<ListingRoomState>
    {
        public ListingRoom(GameServerData data) : base("listingRoom", new Dictionary<string, object>())
        {
            Options["id"] = data.id;
            Options["address"] = data.address;
            Options["port"] = data.port;
            Options["title"] = data.title;
            Options["description"] = data.description;
            Options["map"] = data.map;
            Options["currentPlayer"] = data.currentPlayer;
            Options["maxPlayer"] = data.maxPlayer;
        }

        public override async Task<bool> Join()
        {
            Options["secret"] = ClientInstance.Instance.secret;
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            Options["secret"] = ClientInstance.Instance.secret;
            if (await base.JoinById(id))
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        private void SetupRoom()
        {
        }
    }
}
