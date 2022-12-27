using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class ChatRoom : BaseRoomManager<object>
    {
        public ChatRoom() : base("chatRoom", new Dictionary<string, object>())
        {
        }

        public override async Task<bool> Join()
        {
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            if (await base.JoinById(id))
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        private void SetupRoom()
        {
            Room.OnMessage<GroupLeaveResp>("group-leave", OnGroupLeave);
            Room.OnMessage<GroupInvitationListResp>("group-invitation-list", OnGroupInvitationList);
            Room.OnMessage<GroupUserListResp>("group-user-list", OnGroupUserList);
            Room.OnMessage<GroupListResp>("group-list", OnGroupList);
            Room.OnMessage<GroupJoinResp>("group-join", OnGroupJoin);
            Room.OnMessage<ChatData>("local", OnLocalChat);
            Room.OnMessage<ChatData>("global", OnGlobalChat);
            Room.OnMessage<ChatData>("whisper", OnWhisperChat);
            Room.OnMessage<ChatData>("group", OnGroupChat);
            Room.OnMessage<GroupData>("create-group", OnGroupCreate);
            Room.OnMessage<GroupData>("update-group", OnGroupUpdate);
        }

        private void OnGroupLeave(GroupLeaveResp data)
        {

        }

        private void OnGroupInvitationList(GroupInvitationListResp data)
        {

        }

        private void OnGroupUserList(GroupUserListResp data)
        {

        }

        private void OnGroupList(GroupListResp data)
        {

        }

        private void OnGroupJoin(GroupJoinResp data)
        {

        }

        private void OnLocalChat(ChatData data)
        {

        }

        private void OnGlobalChat(ChatData data)
        {

        }

        private void OnWhisperChat(ChatData data)
        {

        }

        private void OnGroupChat(ChatData data)
        {

        }

        private void OnGroupCreate(GroupData data)
        {

        }

        private void OnGroupUpdate(GroupData data)
        {

        }

        public async Task SendValidateUser(ClientData data)
        {
            await Room.Send("validate-user", data);
        }

        public async Task SendLocalChat(ChatData data)
        {
            await Room.Send("local", data);
        }

        public async Task SendGlobalChat(ChatData data)
        {
            await Room.Send("global", data);
        }

        public async Task SendWhisperChat(ChatData data)
        {
            await Room.Send("whisper", data);
        }

        public async Task SendWhisperByIdChat(ChatData data)
        {
            await Room.Send("whisper-by-id", data);
        }

        public async Task SendGroupChat(ChatData data)
        {
            await Room.Send("group", data);
        }

        public async Task SendCreateGroupChat(GroupData data)
        {
            await Room.Send("create-group", data);
        }

        public async Task SendUpdateGroupChat(GroupData data)
        {
            await Room.Send("update-group", data);
        }

        public async Task SendGroupInvitationList()
        {
            await Room.Send("group-invitation-list");
        }

        public async Task SendGroupUserList(string groupId)
        {
            var data = new Dictionary<string, object>();
            data["groupId"] = groupId;
            await Room.Send("group-user-list", data);
        }

        public async Task SendGroupList()
        {
            await Room.Send("group-list");
        }

        public async Task SendGroupInvite(string groupId, string userId)
        {
            var data = new Dictionary<string, object>();
            data["groupId"] = groupId;
            data["userId"] = userId;
            await Room.Send("group-invite", data);
        }

        public async Task SendGroupInviteAccept(string groupId)
        {
            var data = new Dictionary<string, object>();
            data["groupId"] = groupId;
            await Room.Send("group-invite", data);
        }

        public async Task SendGroupInviteDecline(string groupId)
        {
            var data = new Dictionary<string, object>();
            data["groupId"] = groupId;
            await Room.Send("group-invite", data);
        }

        public async Task SendLeaveGroup(string groupId)
        {
            var data = new Dictionary<string, object>();
            data["groupId"] = groupId;
            await Room.Send("group-invite", data);
        }

        public async Task SendKickUser(string groupId, string userId)
        {
            var data = new Dictionary<string, object>();
            data["groupId"] = groupId;
            data["userId"] = userId;
            await Room.Send("group-invite", data);
        }
    }
}
