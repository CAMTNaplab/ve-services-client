using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace VEServicesClient
{
    public class UIServerList : MonoBehaviour
    {
        [FormerlySerializedAs("uiPrefab")]
        public UIServerListEntry entryPrefab;
        [FormerlySerializedAs("container")]
        public Transform entryContainer;
        public GameObject noServerState;
        public float updateInterval = 1f;
        public bool sortByTitle = true;
        public bool sortDescendy;
        public List<string> filterTitles = new List<string>();
        public List<string> filterMaps = new List<string>();
        private float intervalCountDown;
        public int PlayersCountFromAllServers { get; private set; }

        private void Update()
        {
            if (intervalCountDown > 0)
                intervalCountDown -= Time.deltaTime;
            if (intervalCountDown <= 0)
            {
                intervalCountDown = updateInterval;
                GetList();
            }
        }

        public async void GetList()
        {
            var listResult = await ListingService.Listing();
            if (listResult.IsHttpError || listResult.IsNetworkError)
                return;
            var list = new List<GameServerData>(listResult.Content.gameServers);
            if (entryContainer != null)
            {
                for (var i = entryContainer.childCount - 1; i >= 0; --i)
                {
                    if (entryContainer.GetChild(i) != null && entryContainer.GetChild(i).gameObject != null)
                    {
                        Destroy(entryContainer.GetChild(i).gameObject);
                    }
                }
            }
            if (sortByTitle)
            {
                if (sortDescendy)
                    list = list.OrderByDescending(o => o.title).ToList();
                else
                    list = list.OrderBy(o => o.title).ToList();
            }
            PlayersCountFromAllServers = 0;
            foreach (var data in list)
            {
                if (entryPrefab != null && entryContainer != null &&
                    (filterTitles.Count == 0 || filterTitles.Where(o => o.ToLower().Trim().Contains(data.title.ToLower().Trim())).Count() > 0) &&
                    (filterMaps.Count == 0 || filterMaps.Where(o => o.ToLower().Trim().Contains(data.map.ToLower().Trim())).Count() > 0))
                {
                    var newUI = Instantiate(entryPrefab, entryContainer);
                    newUI.list = this;
                    newUI.serverData = data;
                }
                PlayersCountFromAllServers += data.currentPlayer;
            }
            if (noServerState != null)
                noServerState.SetActive(entryContainer.childCount == 0);
        }
    }
}
