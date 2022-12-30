namespace VEServicesClient
{
    [System.Serializable]
    public class GroupJoinResp
    {
        public string groupId;
        public string userId;
        public string name;

        public GroupJoinResp()
        {
            groupId = string.Empty;
            userId = string.Empty;
            name = string.Empty;
        }
    }
}
