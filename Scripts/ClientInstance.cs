using Colyseus;
using System.Threading.Tasks;
using UnityEngine;

namespace VEServicesClient
{
    public class ClientInstance : MonoBehaviour
    {
        public static ClientInstance Instance { get; private set; }

        public string address = "localhost:2567";
        public bool secured = false;
        public string secret = "secret";
        public AudioSource inputAudioSource;
        public string[] iceServerUrls = new[] { "stun:stun.l.google.com:19302" };
        public float voipSpeakDistance = 10f;

        public BroadcastRoom BroadcastRoom { get; private set; } = null;
        public ChatRoom ChatRoom { get; private set; } = null;
        public ListingRoom ListingRoom { get; private set; } = null;
        public MediaRoom MediaRoom { get; private set; } = null;
        public WebRTCSignalingRoom SignalingRoom { get; private set; } = null;
        public static string ChatUserToken { get; set; }
        public static string MediaUserToken { get; set; }


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public string GetWsAddress()
        {
            return (secured ? "wss://" : "ws://") + address;
        }

        public string GetApiAddress()
        {
            return (secured ? "https://" : "http://") + address;
        }

        public string GetMediaContentAddress()
        {
            return (secured ? "https://" : "http://") + address + "/media/uploads/";
        }

        public async Task<BroadcastRoom> JoinBroadcastRoom()
        {
            if (BroadcastRoom != null)
                await BroadcastRoom.Leave();
            BroadcastRoom = new BroadcastRoom();
            if (await BroadcastRoom.Join())
            {
                return BroadcastRoom;
            }
            return null;
        }

        public async Task LeaveBroadcastRoom()
        {
            if (BroadcastRoom != null)
            {
                await BroadcastRoom.Leave();
                BroadcastRoom = null;
            }
        }

        public async Task<ChatRoom> JoinChatRoom()
        {
            if (ChatRoom != null)
                await ChatRoom.Leave();
            ChatRoom = new ChatRoom();
            if (await ChatRoom.Join())
            {
                return ChatRoom;
            }
            return null;
        }

        public async Task LeaveChatRoom()
        {
            if (ChatRoom != null)
            {
                await ChatRoom.Leave();
                ChatRoom = null;
            }
        }

        public async Task<ListingRoom> JoinListingRoom(GameServerData data)
        {
            if (ListingRoom != null)
                await ListingRoom.Leave();
            ListingRoom = new ListingRoom(data);
            if (await ListingRoom.Join())
            {
                return ListingRoom;
            }
            return null;
        }

        public async Task LeaveListingRoom()
        {
            if (ListingRoom != null)
            {
                await ListingRoom.Leave();
                ListingRoom = null;
            }
        }

        public async Task<MediaRoom> JoinMediaRoom()
        {
            if (MediaRoom != null)
                await MediaRoom.Leave();
            MediaRoom = new MediaRoom();
            if (await MediaRoom.Join())
            {
                return MediaRoom;
            }
            return null;
        }

        public async Task LeaveMediaRoom()
        {
            if (MediaRoom != null)
            {
                await MediaRoom.Leave();
                MediaRoom = null;
            }
        }

        public async Task<WebRTCSignalingRoom> JoinSignalingRoom()
        {
            if (SignalingRoom != null)
                await SignalingRoom.Leave();
            SignalingRoom = new WebRTCSignalingRoom();
            if (await SignalingRoom.Join())
            {
                return SignalingRoom;
            }
            return null;
        }

        public async Task LeaveSignalingRoom()
        {
            if (SignalingRoom != null)
            {
                await SignalingRoom.Leave();
                SignalingRoom = null;
            }
        }
    }
}
