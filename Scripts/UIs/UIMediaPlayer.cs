using System.Collections;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VEServicesClient
{
    public class UIMediaPlayer : MonoBehaviour
    {
        public RenderHeads.Media.AVProVideo.MediaPlayer mediaPlayer;
        public Slider seekSlider;
        public Slider volumeSlider;
        public UIMediaList mediaList;
        protected float _lastRespTime;
        protected string _url;

        protected VideoRenderMode defaultSourceRenderMode;
        protected RenderTexture defaultSourceRenderTexture;
        protected VideoAudioOutputMode defaultSourceAudioOutputMode;
        protected MediaPlayer source;
        public MediaPlayer Source
        {
            get { return source; }
            set
            {
                if (source != null)
                {
                    mediaPlayer.Stop();
                    if (source.avProPlayer != null)
                        source.avProPlayer.AudioMuted = false;
                    ClientInstance.Instance.MediaRoom.SendSub(source.playListId);
                }
                source = value;
                if (source != null)
                {
                    _url = string.Empty;
                    if (source.avProPlayer != null)
                        source.avProPlayer.AudioMuted = true;
                    ClientInstance.Instance.MediaRoom.SendSub(source.playListId);
                    if (mediaList)
                        mediaList.Load(source.playListId);
                    Instance_onResp(source.LastResp);
                }
            }
        }

        protected void OnEnable()
        {
            mediaPlayer.Events.AddListener(AVProMediaPlayer_HandleEvent);
            MediaRoom.onResp += Instance_onResp;
            if (seekSlider)
                seekSlider.onValueChanged.AddListener(OnSeekSliderValueChanged);
            if (volumeSlider)
                volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        }

        protected void OnDisable()
        {
            mediaPlayer.Events.RemoveListener(AVProMediaPlayer_HandleEvent);
            MediaRoom.onResp -= Instance_onResp;
            if (seekSlider)
                seekSlider.onValueChanged.RemoveListener(OnSeekSliderValueChanged);
            if (volumeSlider)
                volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderValueChanged);
            Source = null;
        }

        protected virtual void AVProMediaPlayer_HandleEvent(RenderHeads.Media.AVProVideo.MediaPlayer mediaPlayer, RenderHeads.Media.AVProVideo.MediaPlayerEvent.EventType eventType, RenderHeads.Media.AVProVideo.ErrorCode code)
        {
            if (eventType != RenderHeads.Media.AVProVideo.MediaPlayerEvent.EventType.ReadyToPlay)
                return;
            UpdatePlayer();
        }

        protected virtual void Instance_onResp(MediaResp resp)
        {
            if (resp.playListId != source.playListId)
                return;
            if (_url != source.CurrentVideoUrl)
            {
                _url = source.CurrentVideoUrl;
                mediaPlayer.OpenMedia(new RenderHeads.Media.AVProVideo.MediaPath(source.CurrentVideoUrl, RenderHeads.Media.AVProVideo.MediaPathType.AbsolutePathOrURL), false);
            }
            else
            {
                UpdatePlayer();
            }
        }

        protected virtual void UpdatePlayer()
        {
            if (mediaPlayer.Control != null && Mathf.Abs((float)mediaPlayer.Control.GetCurrentTime() - source.LastResp.time) > 1f)
            {
                mediaPlayer.Control.Seek(source.LastResp.time);
            }
            if (source.LastResp.isPlaying)
            {
                mediaPlayer.Play();
            }
            else if (source.LastResp.time <= 0f)
            {
                mediaPlayer.Stop();
            }
            else
            {
                mediaPlayer.Pause();
            }
            mediaPlayer.AudioVolume = source.LastResp.volume;
        }

        protected void Update()
        {
            if (source == null || source.LastResp == null)
                return;

            if (_lastRespTime != source.LastRespTime || source.LastResp.isPlaying)
            {
                _lastRespTime = source.LastRespTime;
                if (seekSlider != null)
                {
                    seekSlider.minValue = 0;
                    seekSlider.maxValue = source.LastResp.duration;
                    seekSlider.SetValueWithoutNotify(source.LastResp.time + Time.unscaledTime - source.LastRespTime);
                }
                if (volumeSlider != null)
                {
                    volumeSlider.minValue = 0;
                    volumeSlider.maxValue = 1;
                    volumeSlider.SetValueWithoutNotify(source.LastResp.volume);
                }
            }
        }

        private async void OnSeekSliderValueChanged(float value)
        {
            if (source == null)
                return;
            await ClientInstance.Instance.MediaRoom.SendSeek(source.playListId, value);
        }

        private async void OnVolumeSliderValueChanged(float value)
        {
            if (source == null)
                return;
            await ClientInstance.Instance.MediaRoom.SendVolume(source.playListId, value);
        }

        public async void OnClickPlay()
        {
            if (source == null)
                return;
            await ClientInstance.Instance.MediaRoom.SendPlay(source.playListId);
        }

        public async void OnClickPause()
        {
            if (source == null)
                return;
            await ClientInstance.Instance.MediaRoom.SendPause(source.playListId);
        }

        public async void OnClickStop()
        {
            if (source == null)
                return;
            await ClientInstance.Instance.MediaRoom.SendStop(source.playListId);
        }

        public async void OnClickDelete()
        {
            if (source == null)
                return;
            await MediaService.DeleteMedia(source.CurrentMediaId);
        }

        public void OnClickUpload()
        {
            if (source == null)
                return;
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Video Files", ".mp4"));
            StartCoroutine(OpenFile());
        }

        IEnumerator OpenFile()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Load Files", "Load");

            if (FileBrowser.Success)
            {
                UploadVideo(FileBrowser.Result[0]);
            }
            else
            {
                Debug.LogError("Wrong select file path");
            }
        }

        public async void UploadVideo(string path)
        {
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(path);
            var splitedPath = FileBrowser.Result[0].Split('.');
            await MediaService.UploadMedia(source.playListId, bytes, splitedPath[splitedPath.Length - 1]);
            // Reload video list
            if (mediaList)
                mediaList.Load(source.playListId);
        }
    }
}
