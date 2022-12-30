namespace VEServicesClient
{
    [System.Serializable]
    public class VideoData
    {
        public string id;
        public string playListId;
        public string filePath;
        public float duration;
        public int sortOrder;

        public VideoData()
        {
            id = string.Empty;
            playListId = string.Empty;
            filePath = string.Empty;
            duration = 0f;
            sortOrder = 0;
        }
    }
}
