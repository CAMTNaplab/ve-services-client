using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityRestClient;

namespace VEServicesClient
{
    public class ListingService
    {
        [System.Serializable]
        public struct ListingResult
        {
            public bool success;
            public GameServerData[] gameServers;
        }

        [System.Serializable]
        public struct TotalPlayerResult
        {
            public bool success;
            public int totalPlayer;
        }

        public static async Task<RestClient.Result<ListingResult>> Listing()
        {
            return await RestClient.Get<ListingResult>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/listing"));
        }

        public static async Task<RestClient.Result<TotalPlayerResult>> TotalPlayer()
        {
            return await RestClient.Get<TotalPlayerResult>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/listing/total-player"));
        }
    }
}
