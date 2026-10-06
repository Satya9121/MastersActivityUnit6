using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Googolplex.Unit8
{
    /// <summary>
    /// Lightweight UI Video Player for playing MP4 videos on a Unity UI RawImage.
    /// Handles VideoPlayer setup, RenderTexture creation, and playback controls automatically.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class U8_VideoPlayerUI_Masters_Activity : MonoBehaviour
    {
        [Header("Video Settings")]
        [SerializeField] private VideoClip videoClip;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnAwake = true;

        private VideoPlayer videoPlayer;
        private RawImage rawImage;
        private RenderTexture renderTexture;

        public VideoPlayer Player => videoPlayer;
        public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null) videoPlayer = gameObject.AddComponent<VideoPlayer>();

            SetupPlayer();
        }

        private void OnEnable()
        {
            if (playOnAwake && videoPlayer != null)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Pause();
        }

        public void SetupPlayer()
        {
            if (videoClip == null)
            {
                #if UNITY_EDITOR
                videoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit8/Art/more sprites/Child_washing_hands_in_sink_20261001163100.mp4");
                #endif
            }

            if (renderTexture == null)
            {
                int width = videoClip != null ? (int)videoClip.width : 1920;
                int height = videoClip != null ? (int)videoClip.height : 1080;
                if (width <= 0) width = 1920;
                if (height <= 0) height = 1080;

                renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                renderTexture.Create();
            }

            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = loop;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = renderTexture;
                if (videoClip != null)
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = videoClip;
                }
                else
                {
                    string fullPath = System.IO.Path.Combine(Application.dataPath, "SeniorsActivityUnit8/Art/more sprites/Child_washing_hands_in_sink_20261001163100.mp4");
                    if (System.IO.File.Exists(fullPath))
                    {
                        videoPlayer.source = VideoSource.Url;
                        videoPlayer.url = fullPath;
                    }
                }
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Game audio handled by U8_AudioManager
            }

            if (rawImage != null)
            {
                rawImage.texture = renderTexture;
                rawImage.color = Color.white;
            }
        }

        public void Play()
        {
            if (videoPlayer != null)
            {
                if (videoPlayer.targetTexture == null && renderTexture != null)
                {
                    videoPlayer.targetTexture = renderTexture;
                }
                videoPlayer.Play();
            }
        }

        public void Pause()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
        }

        public void Stop()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }

        public void SetClip(VideoClip clip)
        {
            videoClip = clip;
            SetupPlayer();
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
