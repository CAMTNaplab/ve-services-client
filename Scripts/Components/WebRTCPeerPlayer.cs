using LiteNetLibManager;
using UnityEngine;

namespace VEServicesClient
{
    public class WebRTCPeerPlayer : LiteNetLibBehaviour
    {
        public static WebRTCPeerPlayer Mine { get; private set; }
        public Transform audioSourceTransform;
        [SyncField(
            alwaysSync = true,
            clientDeliveryMethod = LiteNetLib.DeliveryMethod.ReliableOrdered,
            deliveryMethod = LiteNetLib.DeliveryMethod.ReliableOrdered,
            syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast)]
        public string sessionId;

        [SyncField(
            alwaysSync = true,
            clientDeliveryMethod = LiteNetLib.DeliveryMethod.ReliableOrdered,
            deliveryMethod = LiteNetLib.DeliveryMethod.ReliableOrdered,
            syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast)]
        public bool isSpeaking;

        private void Start()
        {
            if (audioSourceTransform == null)
                audioSourceTransform = transform;
        }

        private void Update()
        {
            if (ClientInstance.Instance.SignalingRoom != null && ClientInstance.Instance.SignalingRoom.IsConnected)
            {
                if (IsOwnerClient)
                {
                    sessionId = ClientInstance.Instance.SignalingRoom.SessionId;
                    isSpeaking = ClientInstance.Instance.SignalingRoom.IsSpeaking;
                    Mine = this;
                }
                else if (!string.IsNullOrWhiteSpace(sessionId) && Mine != null)
                {
                    if (Vector3.Distance(transform.position, Mine.transform.position) <= ClientInstance.Instance.voipSpeakDistance)
                    {
                        ClientInstance.Instance.SignalingRoom.CreateOffer(sessionId);
                    }
                    else
                    {
                        ClientInstance.Instance.SignalingRoom.RemovePeer(sessionId);
                    }
                }
                if (!string.IsNullOrWhiteSpace(sessionId))
                    ClientInstance.Instance.SignalingRoom.SetAudioSourcesPosition(sessionId, audioSourceTransform.position);
            }
            else
            {
                if (IsOwnerClient)
                {
                    sessionId = string.Empty;
                    isSpeaking = false;
                    Mine = null;
                }
            }
        }
    }
}
