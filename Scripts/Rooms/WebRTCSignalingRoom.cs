using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

namespace VEServicesClient
{
    public class WebRTCSignalingRoom : BaseRoomManager<WebRTCSignalingRoomState>
    {
        [System.Serializable]
        public class OnCandidateMsg
        {
            public string sessionId;
            public string candidate;
            public string sdpMid;
            public int? sdpMLineIndex;

            public OnCandidateMsg()
            {
                sessionId = string.Empty;
                candidate = string.Empty;
                sdpMid = string.Empty;
                sdpMLineIndex = null;
            }
        }

        [System.Serializable]
        public class OnDescMsg
        {
            public string sessionId;
            public int type;
            public string sdp;

            public OnDescMsg()
            {
                sessionId = string.Empty;
                type = 0;
                sdp = string.Empty;
            }
        }

        private Dictionary<string, RTCPeerConnection> peers = new Dictionary<string, RTCPeerConnection>();
        private Dictionary<string, MediaStream> peerReceiveStreams = new Dictionary<string, MediaStream>();
        private Dictionary<string, Dictionary<string, AudioSource>> peerAudioOutputSources = new Dictionary<string, Dictionary<string, AudioSource>>();
        private Dictionary<string, List<RTCIceCandidate>> peerIceCandidates = new Dictionary<string, List<RTCIceCandidate>>();
        private Dictionary<string, List<RTCSessionDescription>> peerDescs = new Dictionary<string, List<RTCSessionDescription>>();
        private HashSet<string> offeredPeers = new HashSet<string>();

        private AudioClip audioInputClip;
        private AudioStreamTrack audioInputTrack;
        private MediaStream sendStream;
        private string micDeviceName;
        private int samplingFrequency = 48000;
        private int lengthSeconds = 1;

        public WebRTCSignalingRoom() : base("webrtcSignalingRoom", new Dictionary<string, object>())
        {

        }

        public override async Task<bool> Join()
        {
            peers.Clear();
            if (await base.Join())
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        public override async Task<bool> JoinById(string id)
        {
            peers.Clear();
            if (await base.JoinById(id))
            {
                SetupRoom();
                return true;
            }
            return false;
        }

        private void SetupRoom()
        {
            Room.OnMessage<OnCandidateMsg>("candidate", OnCandidate);
            Room.OnMessage<OnDescMsg>("desc", OnDesc);

            micDeviceName = Microphone.devices[0];
            audioInputClip = Microphone.Start(micDeviceName, true, lengthSeconds, samplingFrequency);
            // set the latency to “0” samples before the audio starts to play.
            while (!(Microphone.GetPosition(micDeviceName) > 0)) { }

            ClientInstance.Instance.inputAudioSource.loop = true;
            ClientInstance.Instance.inputAudioSource.clip = audioInputClip;
            ClientInstance.Instance.inputAudioSource.Play();

            audioInputTrack = new AudioStreamTrack(ClientInstance.Instance.inputAudioSource);
            sendStream = new MediaStream();
        }

        private static RTCConfiguration CreateRTCConfiguration()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {
                new RTCIceServer {
                    urls = ClientInstance.Instance.iceServerUrls
                }
            };
            return config;
        }

        public void CreatePeer(string sessionId)
        {
            if (peers.ContainsKey(sessionId))
            {
                Debug.LogWarning($"Already added peer for: {sessionId}");
                return;
            }

            // Create new peer connection from this to another
            var config = CreateRTCConfiguration();
            var peerConnection = new RTCPeerConnection(ref config);
            peerConnection.OnIceCandidate = async (RTCIceCandidate candidate) =>
            {
                var info = new RTCIceCandidateInit();
                info.candidate = candidate.Candidate;
                info.sdpMid = candidate.SdpMid;
                info.sdpMLineIndex = candidate.SdpMLineIndex;
                await SendCandidate(sessionId, info);
            };
            peerConnection.OnTrack = (RTCTrackEvent trackEvent) =>
            {
                peerReceiveStreams[sessionId].AddTrack(trackEvent.Track);
            };
            peerConnection.AddTrack(audioInputTrack, sendStream);
            peers[sessionId] = peerConnection;

            // Create media receive stream from another to this
            var peerMediaStream = new MediaStream();
            peerMediaStream.OnAddTrack = (trackEvent) =>
            {
                var trackId = trackEvent.Track.Id;
                if (trackEvent.Track is VideoStreamTrack videoTrack)
                {
                    videoTrack.OnVideoReceived += (Texture texture) =>
                    {
                        // Render the video
                    };
                }
                if (trackEvent.Track is AudioStreamTrack audioTrack)
                {
                    // Play audio
                    if (peerAudioOutputSources.ContainsKey(sessionId) && peerAudioOutputSources[sessionId].ContainsKey(trackId))
                    {
                        Debug.LogWarning($"Adding audio track for {sessionId}/{trackId} again, it actually should has only one?");
                        Object.Destroy(peerAudioOutputSources[sessionId][trackId].gameObject);
                        peerAudioOutputSources[sessionId].Remove(trackId);
                    }
                    if (!peerAudioOutputSources.ContainsKey(sessionId))
                        peerAudioOutputSources.Add(sessionId, new Dictionary<string, AudioSource>());
                    var audioOutputSource = new GameObject($"{sessionId}/{trackId}_AudioOutputSource").AddComponent<AudioSource>();
                    audioOutputSource.SetTrack(audioTrack);
                    audioOutputSource.loop = true;
                    audioOutputSource.Play();
                    peerAudioOutputSources[sessionId][trackId] = audioOutputSource;
                }
            };
            peerReceiveStreams[sessionId] = peerMediaStream;
        }

        public async void CreateOffer(string sessionId)
        {
            if (offeredPeers.Contains(sessionId))
                return;

            if (!peers.TryGetValue(sessionId, out var peer))
            {
                CreatePeer(sessionId);
                peer = peers[sessionId];
            }

            // Offer another peer to receive stream from this
            Debug.Log($"Creating RTC offer to {sessionId}");
            var createOfferAsyncOp = peer.CreateOffer();

            while (!createOfferAsyncOp.IsDone)
            {
                await Task.Yield();
            }

            if (createOfferAsyncOp.IsError)
            {
                Debug.LogError($"Error when create offer: {createOfferAsyncOp.Error}");
                return;
            }

            var desc = createOfferAsyncOp.Desc;
            var setLocalDescAsyncOp = peer.SetLocalDescription(ref desc);

            while (!setLocalDescAsyncOp.IsDone)
            {
                await Task.Yield();
            }

            if (setLocalDescAsyncOp.IsError)
            {
                Debug.LogError($"Error when set local desc, after create offer: {setLocalDescAsyncOp.Error}");
                return;
            }

            await SendDesc(sessionId, desc);
        }

        public void RemovePeer(string sessionId)
        {
            if (peerReceiveStreams.TryGetValue(sessionId, out var peerReceiveStream))
            {
                peerReceiveStream.Dispose();
                peerReceiveStreams.Remove(sessionId);
            }

            if (peerAudioOutputSources.ContainsKey(sessionId))
            {
                var trackIds = new List<string>(peerAudioOutputSources[sessionId].Keys);
                foreach (var trackId in trackIds)
                {
                    if (peerAudioOutputSources[sessionId][trackId] != null)
                        Object.Destroy(peerAudioOutputSources[sessionId][trackId].gameObject);
                    peerAudioOutputSources[sessionId].Remove(trackId);
                }
                peerAudioOutputSources.Remove(sessionId);
            }

            if (peers.TryGetValue(sessionId, out var peer))
            {
                peer.Close();
                peer.Dispose();
                peers.Remove(sessionId);
            }

            peerIceCandidates.Remove(sessionId);
            peerDescs.Remove(sessionId);
            offeredPeers.Remove(sessionId);
        }

        private async void ProceedPeerData(string sessionId)
        {
            if (!peers.TryGetValue(sessionId, out var peer))
            {
                CreatePeer(sessionId);
                peer = peers[sessionId];
            }

            if (peerIceCandidates.TryGetValue(sessionId, out var candidateList))
            {
                while (candidateList.Count > 0)
                {
                    RTCIceCandidate candidate = null;
                    lock (candidateList)
                    {
                        candidate = candidateList[0];
                        candidateList.RemoveAt(0);
                    }
                    if (candidate == null)
                        continue;
                    peer.AddIceCandidate(candidate);
                }
            }

            if (peerDescs.TryGetValue(sessionId, out var descList))
            {
                while (descList.Count > 0)
                {
                    RTCSessionDescription? entry;
                    lock (descList)
                    {
                        entry = descList[0];
                        descList.RemoveAt(0);
                    }
                    if (!entry.HasValue)
                        continue;

                    var desc = entry.Value;
                    var setRemoteDescAsyncOp = peer.SetRemoteDescription(ref desc);
                    while (!setRemoteDescAsyncOp.IsDone)
                    {
                        await Task.Yield();
                    }

                    if (desc.type != RTCSdpType.Offer)
                        continue;

                    Debug.Log($"Creating RTC answer to {sessionId}");
                    var createAnswerAsyncOp = peer.CreateAnswer();

                    while (!createAnswerAsyncOp.IsDone)
                    {
                        await Task.Yield();
                    }

                    if (createAnswerAsyncOp.IsError)
                    {
                        Debug.LogError($"Error when create answer: {createAnswerAsyncOp.Error}");
                        continue;
                    }

                    desc = createAnswerAsyncOp.Desc;
                    var setLocalDescAsyncOp = peer.SetLocalDescription(ref desc);

                    while (!setLocalDescAsyncOp.IsDone)
                    {
                        await Task.Yield();
                    }

                    if (setLocalDescAsyncOp.IsError)
                    {
                        Debug.LogError($"Error when set local desc, after create answer: {setLocalDescAsyncOp.Error}");
                        continue;
                    }

                    await SendDesc(sessionId, desc);
                }
            }
        }

        private void OnCandidate(OnCandidateMsg data)
        {
            var sessionId = data.sessionId;
            var info = new RTCIceCandidateInit();
            info.candidate = data.candidate;
            info.sdpMid = data.sdpMid;
            if (data.sdpMLineIndex.HasValue)
                info.sdpMLineIndex = data.sdpMLineIndex;
            if (!peerIceCandidates.TryGetValue(sessionId, out var list))
            {
                list = new List<RTCIceCandidate>();
                peerIceCandidates[sessionId] = list;
            }
            lock (list)
                list.Add(new RTCIceCandidate(info));
            ProceedPeerData(sessionId);
        }

        private void OnDesc(OnDescMsg data)
        {
            var sessionId = data.sessionId;
            var desc = new RTCSessionDescription();
            desc.type = (RTCSdpType)data.type;
            desc.sdp = data.sdp;
            if (!peerDescs.TryGetValue(sessionId, out var list))
            {
                list = new List<RTCSessionDescription>();
                peerDescs[sessionId] = list;
            }
            lock (list)
                list.Add(desc);
            ProceedPeerData(sessionId);
        }

        public async Task SendCandidate(string sessionId, RTCIceCandidateInit candidateInit)
        {
            var data = new OnCandidateMsg();
            data.sessionId = sessionId;
            data.candidate = candidateInit.candidate;
            data.sdpMid = candidateInit.sdpMid;
            if (candidateInit.sdpMLineIndex.HasValue)
                data.sdpMLineIndex = candidateInit.sdpMLineIndex.Value;
            await Room.Send("candidate", data);
        }

        public async Task SendDesc(string sessionId, RTCSessionDescription desc)
        {
            var data = new OnDescMsg();
            data.sessionId = sessionId;
            data.type = (int)desc.type;
            data.sdp = desc.sdp;
            await Room.Send("desc", data);
        }
    }
}
