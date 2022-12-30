using UnityEngine;
using UnityEngine.UI;

namespace VEServicesClient
{
    public class UIMediaListEntry : MonoBehaviour
    {
        public Text textId;
        public Text textPlayListId;
        public Text textTitle;
        public Text textDuration;
        public Text textSortOrder;
        public VideoData Data { get; set; }

        void Update()
        {
            if (textId)
                textId.text = Data.id;

            if (textPlayListId)
                textPlayListId.text = Data.playListId;

            if (textTitle)
                textTitle.text = "";

            if (textDuration)
                textDuration.text = Data.duration.ToString("N2");

            if (textSortOrder)
                textSortOrder.text = Data.sortOrder.ToString("N2");
        }

        public async void OnClickSwitch()
        {
            await ClientInstance.Instance.MediaRoom.SendSwitch(Data.playListId, Data.id);
        }

        public async void OnClickDelete()
        {
            await MediaService.DeleteMedia(Data.id);
        }
    }
}
