using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class BroadcastRoom : BaseRoomManager<object>
    {
        public delegate void BroadcastCallback(int type, string data);
        private static readonly Dictionary<int, BroadcastCallback> _broadcastCallbacks = new Dictionary<int, BroadcastCallback>();

        public BroadcastRoom() : base("broadcastRoom", new Dictionary<string, object>())
        {
        }

        public override async Task<bool> Join()
        {
            Options["secret"] = ClientInstance.Instance.secret;
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            Options["secret"] = ClientInstance.Instance.secret;
            if (await base.JoinById(id))
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        private void SetupRoom()
        {
            Room.OnMessage<BroadcastData>("all", OnMsg);
            Room.OnMessage<BroadcastData>("other", OnMsg);
        }

        private void OnMsg(BroadcastData data)
        {
            if (_broadcastCallbacks.ContainsKey(data.type))
            {
                _broadcastCallbacks[data.type].Invoke(data.type, data.data);
            }
        }

        public async Task SendAll(BroadcastData msg)
        {
            await Room.Send("all", msg);
        }

        public async Task SendOther(BroadcastData msg)
        {
            await Room.Send("other", msg);
        }

        public static void RegisterBroadcast(int type, BroadcastCallback callback)
        {
            if (callback == null)
            {
                Debug.LogWarning($"[BroadcastRoom] Registering broadcast's callback for type: {type} is empty, cannot register the broadcast.");
                return;
            }
            if (_broadcastCallbacks.ContainsKey(type))
            {
                Debug.LogWarning($"[BroadcastRoom] Registering broadcast type: {type} was registered, it will replacing old callback.");
            }
            _broadcastCallbacks[type] = callback;
        }

        public static void UnregisterBroadcast(int type)
        {
            if (!_broadcastCallbacks.Remove(type))
            {
                Debug.LogWarning($"[BroadcastRoom] Cannot unregistering broadcast, the type: {type} was not registered.");
            }
        }
    }
}
