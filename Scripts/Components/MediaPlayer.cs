using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace VEServicesClient
{
    public class MediaPlayer : MonoBehaviour
    {
        public string playListId;
        public VideoPlayer videoPlayer;
        public RenderHeads.Media.AVProVideo.MediaPlayer avProPlayer;
        public bool convertToAvPro = true;
        public string CurrentMediaId { get; protected set; }
        public MediaResp LastResp { get; protected set; }
        public float LastRespTime { get; protected set; }
        private string _url = string.Empty;
        public string CurrentVideoUrl { get { return _url; } }
        private bool _prepared = false;
        private bool _avProCreated = false;

        public void SetAudioToUnity()
        {
            if (avProPlayer == null)
                return;
            avProPlayer.PlatformOptionsWindows.audioOutput = RenderHeads.Media.AVProVideo.Windows.AudioOutput.Unity;
            avProPlayer.PlatformOptionsMacOSX.audioMode = RenderHeads.Media.AVProVideo.MediaPlayer.OptionsApple.AudioMode.Unity;
            avProPlayer.PlatformOptionsAndroid.audioOutput = RenderHeads.Media.AVProVideo.Android.AudioOutput.Unity;
            avProPlayer.PlatformOptionsIOS.audioMode = RenderHeads.Media.AVProVideo.MediaPlayer.OptionsApple.AudioMode.Unity;
        }

        public void SetAudioToDirect()
        {
            if (avProPlayer == null)
                return;
            avProPlayer.PlatformOptionsWindows.audioOutput = RenderHeads.Media.AVProVideo.Windows.AudioOutput.System;
            avProPlayer.PlatformOptionsMacOSX.audioMode = RenderHeads.Media.AVProVideo.MediaPlayer.OptionsApple.AudioMode.SystemDirect;
            avProPlayer.PlatformOptionsAndroid.audioOutput = RenderHeads.Media.AVProVideo.Android.AudioOutput.System;
            avProPlayer.PlatformOptionsIOS.audioMode = RenderHeads.Media.AVProVideo.MediaPlayer.OptionsApple.AudioMode.SystemDirect;
        }

        protected virtual async void OnEnable()
        {
            MediaRoom.onResp += Instance_onResp;
            if (convertToAvPro && !_avProCreated && videoPlayer != null && avProPlayer == null)
            {
                // Convert player
                Renderer renderer = videoPlayer.targetMaterialRenderer;
                GameObject rendererGameObject = renderer == null ? null : renderer.gameObject;
                if (rendererGameObject != null)
                {
                    avProPlayer = videoPlayer.gameObject.AddComponent<RenderHeads.Media.AVProVideo.MediaPlayer>();
                    SetAudioToUnity();
                    RenderHeads.Media.AVProVideo.ApplyToMesh applyToMaterial = rendererGameObject.AddComponent<RenderHeads.Media.AVProVideo.ApplyToMesh>();
                    applyToMaterial.Player = avProPlayer;
                    applyToMaterial.MeshRenderer = renderer;
                    applyToMaterial.TexturePropertyName = videoPlayer.targetMaterialProperty;
                }

                // Convert audio source
                RenderHeads.Media.AVProVideo.AudioOutput audioOutput = null;
                if (videoPlayer.audioTrackCount > 0)
                {
                    AudioSource audioSource = videoPlayer.GetTargetAudioSource(0);
                    if (audioSource != null)
                    {
                        audioSource.gameObject.SetActive(true);
                        audioOutput = audioSource.gameObject.AddComponent<RenderHeads.Media.AVProVideo.AudioOutput>();
                    }
                }
                else
                {
                    AudioSource audioSource = GetComponentInChildren<AudioSource>(true);
                    if (audioSource != null)
                    {
                        audioSource.gameObject.SetActive(true);
                        audioOutput = audioSource.gameObject.AddComponent<RenderHeads.Media.AVProVideo.AudioOutput>();
                    }
                }
                if (audioOutput != null)
                {
                    System.Type type = typeof(RenderHeads.Media.AVProVideo.AudioOutput);
                    FieldInfo fieldInfo = type.GetField("_supportPositionalAudio", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldInfo.SetValue(audioOutput, true);
                    audioOutput.Player = avProPlayer;
                }

                _avProCreated = true;
            }
            // Setup events
            if (videoPlayer != null)
                videoPlayer.prepareCompleted += VideoPlayer_PrepareCompleted;
            if (avProPlayer != null)
                avProPlayer.Events.AddListener(AVProMediaPlayer_HandleEvent);
            // Wait until it is connected to media room
            while (ClientInstance.Instance.MediaRoom == null || !ClientInstance.Instance.MediaRoom.IsConnected)
            {
                await Task.Yield();
            }
            await ClientInstance.Instance.MediaRoom.SendSub(playListId);
        }

        protected virtual void OnDisable()
        {
            MediaRoom.onResp -= Instance_onResp;
            if (videoPlayer != null)
                videoPlayer.prepareCompleted -= VideoPlayer_PrepareCompleted;
            if (avProPlayer != null)
                avProPlayer.Events.RemoveListener(AVProMediaPlayer_HandleEvent);
        }

        protected virtual void Instance_onResp(MediaResp resp)
        {
            if (!resp.playListId.Equals(playListId))
                return;

            CurrentMediaId = resp.mediaId;
            SetVolume(resp.volume);
            LastResp = resp;
            LastRespTime = Time.unscaledTime;
            if (string.IsNullOrEmpty(resp.filePath))
            {
                // AVPro
                if (avProPlayer != null)
                    avProPlayer.Stop();
                // Unity's video player
                else if (videoPlayer != null)
                    videoPlayer.Stop();
            }
            else
            {
                // Prepare data to play video
                var url = ClientInstance.Instance.GetMediaContentAddress() + resp.filePath;
                if (!url.Equals(_url))
                {
                    PreparePlayer(url);
                }
                else if (_prepared)
                {
                    UpdatePlayer();
                }
            }
        }

        protected virtual void VideoPlayer_PrepareCompleted(VideoPlayer source)
        {
            SetVolume(LastResp.volume);
            UpdatePlayer();
            _prepared = true;
        }

        protected virtual void AVProMediaPlayer_HandleEvent(RenderHeads.Media.AVProVideo.MediaPlayer source, RenderHeads.Media.AVProVideo.MediaPlayerEvent.EventType eventType, RenderHeads.Media.AVProVideo.ErrorCode code)
        {
            if (eventType != RenderHeads.Media.AVProVideo.MediaPlayerEvent.EventType.ReadyToPlay)
                return;
            SetVolume(LastResp.volume);
            UpdatePlayer();
            _prepared = true;
        }

        protected virtual void PreparePlayer(string url)
        {
            _url = url;
            _prepared = false;
            // AVPro
            if (avProPlayer != null)
            {
                avProPlayer.OpenMedia(new RenderHeads.Media.AVProVideo.MediaPath(_url, RenderHeads.Media.AVProVideo.MediaPathType.AbsolutePathOrURL), false);
            }
            // Unity's video player
            else if (videoPlayer != null)
            {
                videoPlayer.url = _url;
                videoPlayer.Prepare();
            }
        }

        protected virtual void UpdatePlayer()
        {
            // AVPro
            if (avProPlayer != null)
            {
                if (Mathf.Abs((float)avProPlayer.Control.GetCurrentTime() - LastResp.time) > 1f)
                {
                    avProPlayer.Control.Seek(LastResp.time);
                }
                if (LastResp.isPlaying)
                {
                    avProPlayer.Play();
                }
                else if (LastResp.time <= 0f)
                {
                    avProPlayer.Stop();
                }
                else
                {
                    avProPlayer.Pause();
                }
            }
            // Unity's video player
            else if (videoPlayer != null)
            {
                if (Mathf.Abs((float)videoPlayer.time - LastResp.time) > 1f)
                {
                    videoPlayer.time = LastResp.time;
                }
                if (LastResp.isPlaying)
                {
                    videoPlayer.Play();
                }
                else if (LastResp.time <= 0f)
                {
                    videoPlayer.Stop();
                }
                else
                {
                    videoPlayer.Pause();
                }
            }
        }

        protected virtual void SetVolume(float volume)
        {
            // AVPro
            if (avProPlayer != null)
            {
                avProPlayer.AudioVolume = volume;
            }
            // Unity's video player
            else if (videoPlayer != null)
            {
                for (ushort i = 0; i < videoPlayer.audioTrackCount; ++i)
                {
                    videoPlayer.SetDirectAudioVolume(i, volume);
                    var audio = videoPlayer.GetTargetAudioSource(i);
                    if (audio)
                        audio.volume = volume;
                }
            }
        }
    }
}
