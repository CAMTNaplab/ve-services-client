namespace VEServicesClient
{
    [System.Serializable]
    public class UserData
    {
        public string userId;
        public string name;
        public string iconUrl;

        public UserData()
        {
            userId = string.Empty;
            name = string.Empty;
            iconUrl = string.Empty;
        }
    }
}
