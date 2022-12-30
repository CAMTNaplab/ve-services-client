namespace VEServicesClient
{
    [System.Serializable]
    public class GameServerData
    {
        public string id;
        public string address;
        public int port;
        public string title;
        public string description;
        public string map;
        public int currentPlayer;
        public int maxPlayer;

        public GameServerData()
        {
            id = string.Empty;
            address = string.Empty;
            port = 0;
            title = string.Empty;
            description = string.Empty;
            map = string.Empty;
            currentPlayer = 0;
            maxPlayer = 0;
        }
    }
}
