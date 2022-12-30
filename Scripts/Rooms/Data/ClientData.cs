namespace VEServicesClient
{
    [System.Serializable]
    public class ClientData
    {
        public string userId;
        public string name;
        public string connectionKey;
        public string token;

        public ClientData()
        {
            userId = string.Empty;
            name = string.Empty;
            connectionKey = string.Empty;
            token = string.Empty;
        }
    }
}
