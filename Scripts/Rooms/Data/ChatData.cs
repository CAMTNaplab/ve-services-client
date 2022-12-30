namespace VEServicesClient
{
    [System.Serializable]
    public class ChatData
    {
        public string groupId;
        public string userId;
        public string userId2;
        public string name;
        public string name2;
        public string msg;
        public string map;
        public float x;
        public float y;
        public float z;

        public ChatData()
        {
            groupId = string.Empty;
            userId = string.Empty;
            userId2 = string.Empty;
            name = string.Empty;
            name2 = string.Empty;
            msg = string.Empty;
            map = string.Empty;
            x = 0f;
            y = 0f;
            z = 0f;
        }
    }
}
