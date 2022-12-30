namespace VEServicesClient
{
    [System.Serializable]
    public class MediaResp
    {
        public string playListId;
        public string mediaId;
        public float duration;
        public string filePath;
        public bool isPlaying;
        public float time;
        public float volume;

        public MediaResp()
        {
            playListId = string.Empty;
            mediaId = string.Empty;
            duration = 0f;
            filePath = string.Empty;
            isPlaying = false;
            time = 0f;
            volume = 0f;
        }
    }
}
