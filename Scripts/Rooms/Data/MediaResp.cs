namespace VEServicesClient
{
    [System.Serializable]
    public struct MediaResp
    {
        public string playListId;
        public string mediaId;
        public float duration;
        public string filePath;
        public bool isPlaying;
        public float time;
        public float volume;
    }
}
