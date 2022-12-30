namespace VEServicesClient
{
    [System.Serializable]
    public class GroupInvitationListResp
    {
        public GroupData[] list;

        public GroupInvitationListResp()
        {
            list = new GroupData[0];
        }
    }
}
