namespace VEServicesClient
{
    [System.Serializable]
    public class GroupUserListResp
    {
        public string groupId;
        public UserData[] list;

        public GroupUserListResp()
        {
            groupId = string.Empty;
            list = new UserData[0];
        }
    }
}
