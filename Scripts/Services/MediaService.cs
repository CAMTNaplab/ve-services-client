using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityRestClient;

namespace VEServicesClient
{
    public class MediaService
    {
        public static async Task<RestClient.Result<ClientData>> AddUser(string userId)
        {
            Dictionary<string, object> formData = new Dictionary<string, object>();
            formData["userId"] = userId;
            return await RestClient.Post<Dictionary<string, object>, ClientData>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/add-user"), formData, ClientInstance.Instance.secret);
        }

        public static async Task<RestClient.Result> RemoveUser(string userId)
        {
            Dictionary<string, object> formData = new Dictionary<string, object>();
            formData["userId"] = userId;
            return await RestClient.Post(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/remove-user"), formData, ClientInstance.Instance.secret);
        }

        public static async Task<RestClient.Result> DeleteMedia(string id)
        {
            return await RestClient.Delete(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/" + id), ClientInstance.MediaUserToken);
        }

        public static async Task<RestClient.Result<List<VideoData>>> GetVideos(string playListId)
        {
            return await RestClient.Get<List<VideoData>>(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/" + playListId));
        }

        public static async Task<RestClient.Result> UploadMedia(string playListId, byte[] file, string fileExt, System.Action<float> onProgress = null, System.Action onDone = null)
        {
            WWWForm form = new WWWForm();
            if (fileExt.Equals("mp4"))
                form.AddBinaryData("file", file, "file.mp4", "video/mp4");
            else if (fileExt.Equals("wav"))
                form.AddBinaryData("file", file, "file.wav", "audio/x-wav");
            form.AddField(nameof(playListId), playListId);

            UnityWebRequest webRequest = UnityWebRequest.Post(RestClient.GetUrl(ClientInstance.Instance.GetApiAddress(), "/media/upload"), form);
            webRequest.certificateHandler = new SimpleWebRequestCert();
            webRequest.SetRequestHeader("Authorization", "Bearer " + ClientInstance.MediaUserToken);

            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
                if (onProgress != null)
                    onProgress.Invoke(asyncOp.progress);
            }
            if (onDone != null)
                onDone.Invoke();

            long responseCode = -1;
            bool isHttpError = true;
            bool isNetworkError = true;
            string stringContent = string.Empty;
            string error = string.Empty;

            responseCode = webRequest.responseCode;
#if UNITY_2020_2_OR_NEWER
            isHttpError = (webRequest.result == UnityWebRequest.Result.ProtocolError);
            isNetworkError = (webRequest.result == UnityWebRequest.Result.ConnectionError);
#else
            isHttpError = webRequest.isHttpError;
            isNetworkError = webRequest.isNetworkError;
#endif
            if (!isNetworkError)
                stringContent = webRequest.downloadHandler.text;
            else
                error = webRequest.error;

            return new RestClient.Result(responseCode, isHttpError, isNetworkError, stringContent, error);
        }
    }
}
