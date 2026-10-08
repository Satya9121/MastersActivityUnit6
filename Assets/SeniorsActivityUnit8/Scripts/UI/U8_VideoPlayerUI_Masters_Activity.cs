using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Googolplex.Unit8
{
    /// <summary>
    /// Lightweight UI Video Player for playing MP4 videos on a Unity UI RawImage.
    /// Handles VideoPlayer setup, RenderTexture creation, and playback controls automatically.
    /// Supports robust hold-to-play synchronization preventing async playback on tap.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class U8_VideoPlayerUI_Masters_Activity : MonoBehaviour
    {
        [Header("Video Settings")]
        [SerializeField] private VideoClip videoClip;
        [SerializeField] private Texture previewTexture;
        [SerializeField] private bool loop = false;
        [SerializeField] private bool playOnAwake = false;

        private VideoPlayer videoPlayer;
        private RawImage rawImage;
        private RenderTexture renderTexture;
        private bool isPlayRequested = false;

        public System.Action onVideoEnd;

        public VideoPlayer Player => videoPlayer;
        public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;
        public double Duration => videoPlayer != null && videoPlayer.clip != null ? videoPlayer.clip.length : (videoClip != null ? videoClip.length : 10.0);

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
            else
            {
                isPlayRequested = false;
            }
        }

        private void OnDisable()
        {
            Pause();
        }

        private void Update()
        {
            // Defensive guard: if play is NOT requested but VideoPlayer is playing
            // (e.g. async prepare completed after a fast tap release), enforce pause immediately.
            if (!isPlayRequested && videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
        }

        public void SetupPlayer()
        {
            if (videoClip == null)
            {
                #if UNITY_EDITOR
                videoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit8/Art/hand wash video.mp4");
                if (videoClip == null)
                {
                    videoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit8/Art/Hands_washing_with_soap_bubbles_20260921100650.mp4");
                }
                #endif
            }

            if (previewTexture == null)
            {
                #if UNITY_EDITOR
                previewTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>("Assets/SeniorsActivityUnit8/Art/hand wash image.jpeg");
                #endif
            }

            if (renderTexture == null)
            {
                int width = videoClip != null ? (int)videoClip.width : 580;
                int height = videoClip != null ? (int)videoClip.height : 480;
                if (width <= 0) width = 580;
                if (height <= 0) height = 480;

                renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                renderTexture.Create();
            }

            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = loop;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = renderTexture;
                videoPlayer.clip = videoClip;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Game audio handled by U8_AudioManager

                videoPlayer.loopPointReached -= OnLoopPointReached;
                videoPlayer.loopPointReached += OnLoopPointReached;

                videoPlayer.prepareCompleted -= OnPrepareCompleted;
                videoPlayer.prepareCompleted += OnPrepareCompleted;
            }

            ResetToPreview();
        }

        public void ResetToPreview()
        {
            isPlayRequested = false;
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }

            if (rawImage != null)
            {
                if (previewTexture != null)
                {
                    rawImage.texture = previewTexture;
                }
                else if (renderTexture != null)
                {
                    rawImage.texture = renderTexture;
                }
                rawImage.color = Color.white;
            }
        }

        private void OnLoopPointReached(VideoPlayer vp)
        {
            onVideoEnd?.Invoke();
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            if (!isPlayRequested && source != null)
            {
                source.Pause();
            }
        }

        public void Play()
        {
            isPlayRequested = true;

            if (rawImage != null && renderTexture != null)
            {
                rawImage.texture = renderTexture;
                rawImage.color = Color.white;
            }

            if (videoPlayer != null)
            {
                if (videoPlayer.targetTexture == null && renderTexture != null)
                {
                    videoPlayer.targetTexture = renderTexture;
                }
                videoPlayer.isLooping = loop;
                videoPlayer.Play();
            }
        }

        public void Pause()
        {
            isPlayRequested = false;

            if (videoPlayer != null)
            {
                videoPlayer.Pause();
            }
        }

        public void Stop()
        {
            isPlayRequested = false;

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
