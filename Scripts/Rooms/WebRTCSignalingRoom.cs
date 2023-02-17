using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

namespace VEServicesClient
{
    public class WebRTCSignalingRoom : BaseRoomManager<object>
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

        public bool IsSpeaking { get; private set; }

        private static Dictionary<string, RTCPeerConnection> rtcPeers = new Dictionary<string, RTCPeerConnection>();
        private static Dictionary<string, MediaStream> rtcPeerReceiveStreams = new Dictionary<string, MediaStream>();
        private static Dictionary<string, Dictionary<string, AudioSource>> rtcPeerAudioOutputSources = new Dictionary<string, Dictionary<string, AudioSource>>();
        private static Dictionary<string, List<RTCIceCandidate>> rtcPeerIceCandidates = new Dictionary<string, List<RTCIceCandidate>>();
        private static Dictionary<string, List<RTCSessionDescription>> rtcPeerDescs = new Dictionary<string, List<RTCSessionDescription>>();
        private static HashSet<string> offeredRtcPeers = new HashSet<string>();

        private AudioClip audioInputClip;
        private static AudioStreamTrack audioInputTrack;
        private static MediaStream sendStream;
        private string micDeviceName;
        private int samplingFrequency = 48000;
        private int lengthSeconds = 1;

        public WebRTCSignalingRoom() : base("webrtcSignalingRoom", new Dictionary<string, object>())
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
            Room.OnMessage<OnCandidateMsg>("candidate", OnRtcCandidate);
            Room.OnMessage<OnDescMsg>("desc", OnRtcDesc);

            if (sendStream != null)
                sendStream.Dispose();
            sendStream = new MediaStream();
            if (audioInputTrack != null)
                audioInputTrack.Dispose();
            audioInputTrack = new AudioStreamTrack(ClientInstance.Instance.inputAudioSource);
            audioInputTrack.Loopback = false;
            var sessionIds = new List<string>(rtcPeers.Keys);
            foreach (var sessionId in sessionIds)
            {
                RemoveRtcPeer(sessionId);
            }
        }

        public void StartMicRecord()
        {
            if (IsSpeaking)
                return;

            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("Cannot start mic record, there is no mic devices");
                return;
            }

            micDeviceName = Microphone.devices[0];
            audioInputClip = Microphone.Start(micDeviceName, true, lengthSeconds, samplingFrequency);
            // set the latency to “0” samples before the audio starts to play.
            while (!(Microphone.GetPosition(micDeviceName) > 0)) { }

            ClientInstance.Instance.inputAudioSource.loop = true;
            ClientInstance.Instance.inputAudioSource.clip = audioInputClip;
            ClientInstance.Instance.inputAudioSource.volume = 1f;
            ClientInstance.Instance.inputAudioSource.Play();

            IsSpeaking = true;
        }

        public void StopMicRecord()
        {
            if (!IsSpeaking)
                return;

            ClientInstance.Instance.inputAudioSource.Stop();
            Microphone.End(micDeviceName);

            IsSpeaking = false;
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

        public void CreateRtcPeer(string sessionId)
        {
            if (rtcPeers.ContainsKey(sessionId))
            {
                Debug.LogWarning($"Already added peer for: {sessionId}");
                return;
            }

            // Create new peer connection from this to another
            var config = CreateRTCConfiguration();
            var peer = new RTCPeerConnection(ref config)
            {
                OnIceCandidate = async (RTCIceCandidate candidate) =>
                {
                    var info = new RTCIceCandidateInit();
                    info.candidate = candidate.Candidate;
                    info.sdpMid = candidate.SdpMid;
                    info.sdpMLineIndex = candidate.SdpMLineIndex;
                    await SendCandidate(sessionId, info);
                },
                OnTrack = (RTCTrackEvent trackEvent) =>
                {
                    rtcPeerReceiveStreams[sessionId].AddTrack(trackEvent.Track);
                },
            };
            rtcPeers[sessionId] = peer;

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
                    if (rtcPeerAudioOutputSources.ContainsKey(sessionId) && rtcPeerAudioOutputSources[sessionId].ContainsKey(trackId))
                    {
                        Debug.LogWarning($"Adding audio track for {sessionId}/{trackId} again, it actually should has only one?");
                        Object.Destroy(rtcPeerAudioOutputSources[sessionId][trackId].gameObject);
                        rtcPeerAudioOutputSources[sessionId].Remove(trackId);
                    }
                    if (!rtcPeerAudioOutputSources.ContainsKey(sessionId))
                        rtcPeerAudioOutputSources.Add(sessionId, new Dictionary<string, AudioSource>());
                    var audioOutputSource = new GameObject($"{sessionId}/{trackId}_AudioOutputSource").AddComponent<AudioSource>();
                    audioOutputSource.SetTrack(audioTrack);
                    audioOutputSource.loop = true;
                    audioOutputSource.Play();
                    rtcPeerAudioOutputSources[sessionId][trackId] = audioOutputSource;
                }
            };
            rtcPeerReceiveStreams[sessionId] = peerMediaStream;
        }

        public async void CreateRtcOffer(string sessionId)
        {
            if (offeredRtcPeers.Contains(sessionId))
                return;
            offeredRtcPeers.Add(sessionId);

            if (!rtcPeers.TryGetValue(sessionId, out var peer))
            {
                CreateRtcPeer(sessionId);
                peer = rtcPeers[sessionId];
            }
            peer.AddTrack(audioInputTrack, sendStream);

            // Offer another peer to receive stream from this
            Debug.Log($"Creating RTC offer to {sessionId}");
            var createOfferAsyncOp = peer.CreateOffer();

            while (!createOfferAsyncOp.IsDone)
            {
                await Task.Yield();
            }

            if (createOfferAsyncOp.IsError)
            {
                Debug.LogError($"Error when create offer: {createOfferAsyncOp.Error.errorType} {createOfferAsyncOp.Error.message}");
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
                Debug.LogError($"Error when set local desc, after create offer: {setLocalDescAsyncOp.Error.errorType} {setLocalDescAsyncOp.Error.message}");
                return;
            }

            await SendDesc(sessionId, desc);
        }

        public void RemoveRtcPeer(string sessionId)
        {
            if (rtcPeerReceiveStreams.TryGetValue(sessionId, out var peerReceiveStream))
            {
                peerReceiveStream.Dispose();
                rtcPeerReceiveStreams.Remove(sessionId);
            }

            if (rtcPeerAudioOutputSources.ContainsKey(sessionId))
            {
                var trackIds = new List<string>(rtcPeerAudioOutputSources[sessionId].Keys);
                foreach (var trackId in trackIds)
                {
                    if (rtcPeerAudioOutputSources[sessionId][trackId] != null)
                        Object.Destroy(rtcPeerAudioOutputSources[sessionId][trackId].gameObject);
                    rtcPeerAudioOutputSources[sessionId].Remove(trackId);
                }
                rtcPeerAudioOutputSources.Remove(sessionId);
            }

            if (rtcPeers.TryGetValue(sessionId, out var peer))
            {
                peer.Close();
                peer.Dispose();
                rtcPeers.Remove(sessionId);
            }

            rtcPeerIceCandidates.Remove(sessionId);
            rtcPeerDescs.Remove(sessionId);
            offeredRtcPeers.Remove(sessionId);
        }

        public List<AudioSource> GetAudioSources(string sessionId)
        {
            var result = new List<AudioSource>();
            if (!rtcPeerAudioOutputSources.TryGetValue(sessionId, out var dict))
                return result;
            var trackIds = new List<string>(dict.Keys);
            foreach (var trackId in trackIds)
            {
                if (dict[trackId] != null)
                    result.Add(dict[trackId]);
            }
            return result;
        }

        public void SetAudioSourcesPosition(string sessionId, Vector3 position)
        {
            if (!rtcPeerAudioOutputSources.TryGetValue(sessionId, out var dict))
                return;
            var trackIds = new List<string>(dict.Keys);
            foreach (var trackId in trackIds)
            {
                if (dict[trackId] != null)
                    dict[trackId].transform.position = position;
            }
        }

        private async void ProceedRtcPeerData(string sessionId)
        {
            if (!rtcPeers.TryGetValue(sessionId, out var peer))
            {
                CreateRtcPeer(sessionId);
                peer = rtcPeers[sessionId];
            }

            if (rtcPeerDescs.TryGetValue(sessionId, out var descList))
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

                    if (setRemoteDescAsyncOp.IsError)
                    {
                        Debug.LogError($"Error when set remote desc, before create answer: {setRemoteDescAsyncOp.Error.errorType} {setRemoteDescAsyncOp.Error.message}, {Room.SessionId} {sessionId} {desc.type}");
                        continue;
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
                        Debug.LogError($"Error when create answer: {createAnswerAsyncOp.Error.errorType} {createAnswerAsyncOp.Error.message}");
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
                        Debug.LogError($"Error when set local desc, after create answer: {setLocalDescAsyncOp.Error.errorType} {setLocalDescAsyncOp.Error.message}");
                        continue;
                    }

                    await SendDesc(sessionId, desc);
                }
            }

            if (rtcPeerIceCandidates.TryGetValue(sessionId, out var candidateList))
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
        }

        private void OnRtcCandidate(OnCandidateMsg data)
        {
            var sessionId = data.sessionId;
            var info = new RTCIceCandidateInit();
            info.candidate = data.candidate;
            info.sdpMid = data.sdpMid;
            if (data.sdpMLineIndex.HasValue)
                info.sdpMLineIndex = data.sdpMLineIndex;
            RTCSessionDescription? sessionDesc = null;
            if (rtcPeers.ContainsKey(sessionId))
            {
                try
                {
                    sessionDesc = rtcPeers[sessionId].CurrentRemoteDescription;
                }
                catch
                {
                    sessionDesc = null;
                }
            }
            if (!rtcPeerIceCandidates.ContainsKey(sessionId))
                rtcPeerIceCandidates[sessionId] = new List<RTCIceCandidate>();
            rtcPeerIceCandidates[sessionId].Add(new RTCIceCandidate(info));
            if (sessionDesc.HasValue)
            {
                ProceedRtcPeerData(sessionId);
            }
        }

        private void OnRtcDesc(OnDescMsg data)
        {
            var sessionId = data.sessionId;
            var desc = new RTCSessionDescription();
            desc.type = (RTCSdpType)data.type;
            desc.sdp = data.sdp;
            if (!rtcPeerDescs.ContainsKey(sessionId))
                rtcPeerDescs[sessionId] = new List<RTCSessionDescription>();
            rtcPeerDescs[sessionId].Add(desc);
            ProceedRtcPeerData(sessionId);
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
