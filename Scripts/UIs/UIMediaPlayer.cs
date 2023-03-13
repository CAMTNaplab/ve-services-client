using System.Collections;
using RenderHeads.Media.AVProVideo;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VEServicesClient
{
    public class UIMediaPlayer : MonoBehaviour
    {
        public DisplayUGUI display;
        public AudioOutput audioOutput;
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
                    var audio = source.GetComponentInChildren<AudioOutput>();
                    if (audio)
                        audio.ForceMute = false;
                }
                source = value;
                if (source != null)
                {
                    _url = string.Empty;
                    var audio = source.GetComponentInChildren<AudioOutput>();
                    if (audio)
                        audio.ForceMute = true;
                    display.CurrentMediaPlayer = source.avProPlayer;
                    audioOutput.ChangeMediaPlayer(source.avProPlayer);
                    if (mediaList)
                        mediaList.Load(source.playListId);
                }
            }
        }

        protected void OnEnable()
        {
            if (seekSlider)
                seekSlider.onValueChanged.AddListener(OnSeekSliderValueChanged);
            if (volumeSlider)
                volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        }

        protected void OnDisable()
        {
            if (seekSlider)
                seekSlider.onValueChanged.RemoveListener(OnSeekSliderValueChanged);
            if (volumeSlider)
                volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderValueChanged);
            Source = null;
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
