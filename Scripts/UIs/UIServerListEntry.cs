using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace VEServicesClient
{
    public class UIServerListEntry : MonoBehaviour
    {
        public UIServerList list;
        public GameServerData serverData;
        public Text textTitle;
        public Text textDescription;
        public Text textMap;
        [FormerlySerializedAs("formatPlayer")]
        public string formatPlayersCount = "{0}/{1}";
        [FormerlySerializedAs("textPlayer")]
        public Text textPlayersCount;
        public Text textPlayersCountFromAllServers;

        private void Update()
        {
            if (textTitle != null)
                textTitle.text = serverData.title;
            if (textDescription != null)
                textDescription.text = serverData.description;
            if (textMap != null)
                textMap.text = serverData.map;
            if (textPlayersCount != null)
                textPlayersCount.text = string.Format(formatPlayersCount, serverData.currentPlayer, serverData.maxPlayer);
            if (textPlayersCountFromAllServers != null)
                textPlayersCountFromAllServers.text = list.PlayersCountFromAllServers.ToString("N0");
        }
    }
}
