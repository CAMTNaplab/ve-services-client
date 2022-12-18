using Colyseus;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class BaseRoomManager<T>
    {
        public string SessionId { get; private set; }
        public Dictionary<string, object> Options { get; private set; }
        public string RoomName { get; private set; }
        public ColyseusRoom<T> Room { get; private set; }
        public bool IsConnected
        {
            get { return Room != null && Room.colyseusConnection != null && Room.colyseusConnection.IsOpen; }
        }

        public BaseRoomManager(string roomName, Dictionary<string, object> options)
        {
            if (options == null)
                options = new Dictionary<string, object>();
            RoomName = roomName;
            Options = options;
        }

        public virtual async Task<bool> Join()
        {
            try
            {
                Room = await ClientInstance.Instance.Client.JoinOrCreate<T>(RoomName, Options);
                SessionId = Room.SessionId;
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Room = null;
                return false;
            }
        }

        public virtual async Task<bool> JoinById(string id)
        {
            try
            {
                Room = await ClientInstance.Instance.Client.JoinById<T>(id, Options);
                SessionId = Room.SessionId;
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Room = null;
                return false;
            }
        }

        public virtual async Task Leave(bool consented = true)
        {
            try
            {
                if (Room != null)
                    await Room.Leave(consented);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Room = null;
            }
        }

        protected virtual async void OnApplicationQuit()
        {
            await Leave();
        }
    }
}
