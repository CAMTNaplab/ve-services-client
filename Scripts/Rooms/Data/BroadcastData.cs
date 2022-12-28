namespace VEServicesClient
{
    [System.Serializable]
    public class BroadcastData
    {
        public int type;
        public string data;

        public BroadcastData()
        {
            type = 0;
            data = string.Empty;
        }
    }
}
