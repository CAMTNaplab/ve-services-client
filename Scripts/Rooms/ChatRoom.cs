using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VEServicesClient
{
    public class ChatRoom : BaseRoomManager<ChatRoomState>
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
            Room.OnMessage<ChatResp>("local", OnLocalChat);
            Room.OnMessage<ChatResp>("global", OnGlobalChat);
            Room.OnMessage<ChatResp>("whisper", OnWhisperChat);
            Room.OnMessage<ChatResp>("group", OnGroupChat);
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

        private void OnLocalChat(ChatResp data)
        {

        }

        private void OnGlobalChat(ChatResp data)
        {

        }

        private void OnWhisperChat(ChatResp data)
        {

        }

        private void OnGroupChat(ChatResp data)
        {

        }

        private void OnGroupCreate(GroupData data)
        {

        }

        private void OnGroupUpdate(GroupData data)
        {

        }
    }
}
