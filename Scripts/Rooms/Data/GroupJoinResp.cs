namespace VEServicesClient
{
    [System.Serializable]
    public class GroupJoinResp
    {
        public string groupId;
        public string userId;

        public GroupJoinResp()
        {
            groupId = string.Empty;
            userId = string.Empty;
        }
    }
}
