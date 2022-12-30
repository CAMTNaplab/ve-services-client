namespace VEServicesClient
{
    [System.Serializable]
    public class GroupData
    {
        public string groupId;
        public string title;
        public string iconUrl;

        public GroupData()
        {
            groupId = string.Empty;
            title = string.Empty;
            iconUrl = string.Empty;
        }
    }
}
