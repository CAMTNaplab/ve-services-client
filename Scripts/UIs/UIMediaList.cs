using UnityEngine;

namespace VEServicesClient
{
    public class UIMediaList : MonoBehaviour
    {
        public UIMediaListEntry entryPrefab;
        public Transform entryContainer;

        public async void Load(string playListId)
        {
            var listResult = await MediaService.GetVideos(playListId);
            if (listResult.IsHttpError || listResult.IsNetworkError)
                return;
            var list = listResult.Content;
            for (int i = entryContainer.childCount - 1; i >= 0; --i)
            {
                Destroy(entryContainer.GetChild(i).gameObject);
            }
            foreach (var entry in list)
            {
                var newEntry = Instantiate(entryPrefab, entryContainer);
                newEntry.transform.position = Vector3.zero;
                newEntry.transform.rotation = Quaternion.identity;
                newEntry.transform.localScale = Vector3.one;
                newEntry.Data = entry;
            }
        }
    }
}
