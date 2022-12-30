using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class ChatRoom : BaseRoomManager<object>
    {
        public static event System.Action<ChatData> onLocalChat;
        public static event System.Action<ChatData> onGlobalChat;
        public static event System.Action<ChatData> onWhisperChat;
        public static event System.Action<ChatData> onGroupChat;
        public static event System.Action<GroupData> onCreateGroup;
        public static event System.Action<GroupData> onUpdateGroup;
        public static event System.Action<GroupInvitationListResp> onGroupInvitationList;
        public static event System.Action<GroupUserListResp> onGroupUserList;
        public static event System.Action<GroupListResp> onGroupList;
        public static event System.Action<GroupJoinResp> onGroupJoin;
        public static event System.Action<GroupLeaveResp> onGroupLeave;

        public static readonly Dictionary<string, GroupData> Groups = new Dictionary<string, GroupData>();
        public static readonly Dictionary<string, GroupData> GroupInvitations = new Dictionary<string, GroupData>();
        public static readonly Dictionary<string, UserData> GroupUsers = new Dictionary<string, UserData>();
        public static readonly Dictionary<string, List<string>> GroupUserIds = new Dictionary<string, List<string>>();

        public ChatRoom() : base("chatRoom", new Dictionary<string, object>())
        {
        }

        public override async Task<bool> Join()
        {
            Options["token"] = ClientInstance.ChatUserToken;
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            Options["token"] = ClientInstance.ChatUserToken;
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
            if (onGroupLeave != null)
                onGroupLeave.Invoke(data);
        }

        private void OnGroupInvitationList(GroupInvitationListResp data)
        {
            GroupInvitations.Clear();
            foreach (var entry in data.list)
            {
                GroupInvitations.Add(entry.groupId, entry);
            }
            if (onGroupInvitationList != null)
                onGroupInvitationList.Invoke(data);
        }

        private void OnGroupUserList(GroupUserListResp data)
        {
            GroupUsers.Clear();
            GroupUserIds[data.groupId] = new List<string>();
            foreach (var entry in data.list)
            {
                GroupUsers.Add(entry.userId, entry);
                GroupUserIds[data.groupId].Add(entry.userId);
            }
            if (onGroupUserList != null)
                onGroupUserList.Invoke(data);
        }

        private void OnGroupList(GroupListResp data)
        {
            Groups.Clear();
            foreach (var entry in data.list)
            {
                Groups.Add(entry.groupId, entry);
            }
            if (onGroupList != null)
                onGroupList.Invoke(data);
        }

        private void OnGroupJoin(GroupJoinResp data)
        {
            if (onGroupJoin != null)
                onGroupJoin.Invoke(data);
        }

        private void OnLocalChat(ChatData data)
        {
            if (onLocalChat != null)
                onLocalChat.Invoke(data);
        }

        private void OnGlobalChat(ChatData data)
        {
            if (onGlobalChat != null)
                onGlobalChat.Invoke(data);
        }

        private void OnWhisperChat(ChatData data)
        {
            if (onWhisperChat != null)
                onWhisperChat.Invoke(data);
        }

        private void OnGroupChat(ChatData data)
        {
            if (onGroupChat != null)
                onGroupChat.Invoke(data);
        }

        private void OnGroupCreate(GroupData data)
        {
            Groups[data.groupId] = data;
            if (onCreateGroup != null)
                onCreateGroup.Invoke(data);
        }

        private void OnGroupUpdate(GroupData data)
        {
            Groups[data.groupId] = data;
            if (onUpdateGroup != null)
                onUpdateGroup.Invoke(data);
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
