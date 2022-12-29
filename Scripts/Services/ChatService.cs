using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityRestClient;

namespace VEServicesClient
{
    public class ChatService
    {
        public static async Task<RestClient.Result<ClientData>> AddUser(string userId, string name, string iconUrl)
        {
            Dictionary<string, object> formData = new Dictionary<string, object>();
            formData["userId"] = userId;
            formData["name"] = name;
            formData["iconUrl"] = iconUrl;
            return await RestClient.Post<Dictionary<string, object>, ClientData>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/chat/add-user"), formData, ClientInstance.Instance.secret);
        }

        public static async Task<RestClient.Result> RemoveUser(string userId)
        {
            Dictionary<string, object> formData = new Dictionary<string, object>();
            formData["userId"] = userId;
            return await RestClient.Post(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/chat/remove-user"), formData, ClientInstance.Instance.secret);
        }
    }
}
