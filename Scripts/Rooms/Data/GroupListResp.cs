namespace VEServicesClient
{
    [System.Serializable]
    public class GroupListResp
    {
        public GroupData[] list;

        public GroupListResp()
        {
            list = new GroupData[0];
        }
    }
}
