using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityRestClient;

namespace VEServicesClient
{
    public class MediaService
    {
        public static async Task<RestClient.Result<ClientData>> AddUser(string userId)
        {
            Dictionary<string, object> formData = new Dictionary<string, object>();
            formData["userId"] = userId;
            return await RestClient.Post<Dictionary<string, object>, ClientData>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/add-user"), formData);
        }

        public static async Task<RestClient.Result> RemoveUser(string userId)
        {
            Dictionary<string, object> formData = new Dictionary<string, object>();
            formData["userId"] = userId;
            return await RestClient.Post(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/remove-user"), formData);
        }

        public static async Task<RestClient.Result> DeleteMedia(string id)
        {
            return await RestClient.Delete(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/" + id), string.Empty);
        }

        public static async Task<RestClient.Result<List<VideoData>>> GetVideos(string playListId)
        {
            return await RestClient.Get<List<VideoData>>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/" + playListId));
        }
    }
}
