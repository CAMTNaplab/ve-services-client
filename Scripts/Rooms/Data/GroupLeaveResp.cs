namespace VEServicesClient
{
    [System.Serializable]
    public class GroupLeaveResp
    {
        public string groupId;
        public string userId;

        public GroupLeaveResp()
        {
            groupId = string.Empty;
            userId = string.Empty;
        }
    }
}
