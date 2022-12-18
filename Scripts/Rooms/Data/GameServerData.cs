namespace VEServicesClient
{
    [System.Serializable]
    public struct GameServerData
    {
        public string id;
        public string address;
        public string port;
        public string title;
        public string description;
        public string map;
        public int currentPlayer;
        public int maxPlayer;
    }
}
