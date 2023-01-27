using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class MediaRoom : BaseRoomManager<object>
    {
        public static event System.Action<MediaResp> onResp;
        public static readonly Dictionary<string, MediaResp> Resps = new Dictionary<string, MediaResp>();

        public MediaRoom() : base("mediaRoom", new Dictionary<string, object>())
        {
        }

        public override async Task<bool> Join()
        {
            Options["token"] = ClientInstance.MediaUserToken;
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            Options["token"] = ClientInstance.MediaUserToken;
            if (await base.JoinById(id))
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        private void SetupRoom()
        {
            Room.OnMessage<MediaResp>("resp", OnRespMsg);
        }

        private void OnRespMsg(MediaResp data)
        {
            if (onResp != null)
                onResp.Invoke(data);
            if (!string.IsNullOrEmpty(data.playListId))
                Resps[data.playListId] = data;
        }

        public async Task SendSub(string playListId)
        {
            await Room.Send("sub", playListId);
        }

        public async Task SendPlay(string playListId)
        {
            await Room.Send("play", playListId);
        }

        public async Task SendPause(string playListId)
        {
            await Room.Send("pause", playListId);
        }

        public async Task SendStop(string playListId)
        {
            await Room.Send("stop", playListId);
        }

        public async Task SendSeek(string playListId, float time)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["playListId"] = playListId;
            data["time"] = time;
            await Room.Send("seek", data);
        }

        public async Task SendVolume(string playListId, float volume)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["playListId"] = playListId;
            data["volume"] = volume;
            await Room.Send("volume", data);
        }

        public async Task SendSwitch(string playListId, string mediaId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["playListId"] = playListId;
            data["mediaId"] = mediaId;
            await Room.Send("switch", data);
        }
    }
}
