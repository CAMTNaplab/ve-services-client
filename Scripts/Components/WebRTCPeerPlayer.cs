using LiteNetLibManager;
using UnityEngine;
using VECY;

namespace VEServicesClient
{
    public class WebRTCPeerPlayer : LiteNetLibBehaviour
    {
        public Transform audioSourceTransform;
        [SyncField]
        public string sessionId;

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
                if (!string.IsNullOrWhiteSpace(sessionId))
                    ClientInstance.Instance.SignalingRoom.SetAudioSourcesPosition(sessionId, audioSourceTransform.position);
            }
            else
            {
                sessionId = string.Empty;
            }
        }
    }
}
