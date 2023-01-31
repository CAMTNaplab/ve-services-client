using LiteNetLibManager;
using UnityEngine;
using VECY;

namespace VEServicesClient
{
    public class WebRTCPeerPlayer : LiteNetLibBehaviour
    {
        [SyncField]
        public string sessionId;

        private void Update()
        {
            if (IsOwnerClient)
            {
                if (ClientInstance.Instance.SignalingRoom != null && ClientInstance.Instance.SignalingRoom.IsConnected)
                    sessionId = ClientInstance.Instance.SignalingRoom.SessionId;
                else
                    sessionId = string.Empty;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                    return;

                if (Vector3.Distance(transform.position, VECharacter.Mine.transform.position) <= ClientInstance.Instance.voipSpeakDistance)
                    ClientInstance.Instance.SignalingRoom.CreateOffer(sessionId);
                else
                    ClientInstance.Instance.SignalingRoom.RemovePeer(sessionId);
            }
        }
    }
}
