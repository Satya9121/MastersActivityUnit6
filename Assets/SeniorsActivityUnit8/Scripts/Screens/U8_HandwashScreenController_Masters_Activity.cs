using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public class U8_HandwashScreenController_Masters_Activity : MonoBehaviour
    {
        [Header("UI Prompts & Controls")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Button btnScrubArea;

        [Header("Hands & Avatar Visual References")]
        [SerializeField] private GameObject handsImage;
        [SerializeField] private GameObject anuInsideAvatar;

        [Header("Hands & Video Visuals")]
        [SerializeField] private RawImage handsVideoImage;
        [SerializeField] private U8_VideoPlayerUI_Masters_Activity videoPlayerUI;
        [SerializeField] private Image handsFallbackImage;
        [SerializeField] private Sprite handWashBorderSprite;
        [SerializeField] private Image waterStreamImage;
        [SerializeField] private GameObject bubbleContainer;
        [SerializeField] private List<Image> bubbleSprites = new List<Image>();

        [Header("4 Friendly Germ Blobs")]
        [SerializeField] private List<Image> germBlobs = new List<Image>();

        [Header("Audio Clips (Inspector)")]
        [SerializeField] private AudioClip voNowWashHands;
        [SerializeField] private AudioClip voKeepGoing;
        [SerializeField] private AudioClip voAllCleanOneStar;
        [SerializeField] private AudioClip musHandwashSong;
        [SerializeField] private AudioClip sfxGermOff;
        [SerializeField] private AudioClip sfxScrub;
        [SerializeField] private AudioClip sfxBubble;
        [SerializeField] private AudioClip sfxSparkle;

        [Header("Settings")]
        [SerializeField] private float defaultDurationSeconds = 10.0f;

        private bool hasStartedWashing = false;
        private bool isHandwashingActive = false;
        private bool isHolding = false;
        private bool hasStartedAudio = false;
        private bool isHandwashingComplete = false;
        private int clearedGermsCount = 0;
        private Coroutine handwashCoroutine;
        private U8_HoldButtonHandler_Masters_Activity holdHandler;

        private void Awake()
        {
            AutoFindUIReferences();
            AutoWireAudioClips();
            SetupHoldInteraction();
        }

        public void SetupHoldInteraction()
        {
            if (btnScrubArea != null)
            {
                btnScrubArea.onClick.RemoveAllListeners();

                holdHandler = btnScrubArea.GetComponent<U8_HoldButtonHandler_Masters_Activity>();
                if (holdHandler == null)
                {
                    holdHandler = btnScrubArea.gameObject.AddComponent<U8_HoldButtonHandler_Masters_Activity>();
                }

                holdHandler.onPointerDown = OnHoldStarted;
                holdHandler.onPointerUp = OnHoldEnded;

                var labelTmp = btnScrubArea.GetComponentInChildren<TextMeshProUGUI>(true);
                if (labelTmp != null)
                {
                    labelTmp.raycastTarget = false;
                }
            }
        }

        public void AutoWireAudioClips()
        {
            if (U8_AudioManager_Masters_Activity.Instance == null) return;
            if (voNowWashHands == null) voNowWashHands = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_06");
            if (voKeepGoing == null) voKeepGoing = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_07");
            if (voAllCleanOneStar == null) voAllCleanOneStar = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_08");
            if (musHandwashSong == null) musHandwashSong = U8_AudioManager_Masters_Activity.Instance.GetClip("MUS_HandwashSong");
            if (sfxGermOff == null) sfxGermOff = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_GermOff");
            if (sfxScrub == null) sfxScrub = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Scrub");
            if (sfxBubble == null) sfxBubble = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Bubble");
            if (sfxSparkle == null) sfxSparkle = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Sparkle");
        }

        private void OnEnable()
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            PrepareReadyState();
        }

        private void OnDisable()
        {
            isHolding = false;
            if (handwashCoroutine != null)
            {
                StopCoroutine(handwashCoroutine);
                handwashCoroutine = null;
            }
            if (videoPlayerUI != null)
            {
                videoPlayerUI.onVideoEnd = null;
                videoPlayerUI.Pause();
            }
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.StopBGM();
                U8_AudioManager_Masters_Activity.Instance.StopVO();
            }
        }

        private void AutoFindUIReferences()
        {
            if (promptText == null)
            {
                Transform t = transform.Find("PromptCard/PromptText") ?? transform.Find("PromptText");
                if (t != null) promptText = t.GetComponent<TextMeshProUGUI>();
                else promptText = GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (timerText == null)
            {
                Transform t = transform.Find("TimerBadge/TimerText") ?? transform.Find("TimerText");
                if (t != null) timerText = t.GetComponent<TextMeshProUGUI>();
            }

            if (btnScrubArea == null)
            {
                Transform t = transform.Find("ScrubAreaButton") ?? transform.Find("HandsImage/HandsVideo") ?? transform.Find("HandsVideo") ?? transform.Find("HandsImage") ?? transform.Find("Btn_Scrub");
                if (t != null) btnScrubArea = t.GetComponent<Button>();
            }

            if (handsImage == null)
            {
                Transform t = transform.Find("HandsImage") ?? transform.Find("HandsVideo");
                if (t != null) handsImage = t.gameObject;
                else if (handsFallbackImage != null) handsImage = handsFallbackImage.gameObject;
                else if (handsVideoImage != null) handsImage = handsVideoImage.gameObject;
            }

            if (anuInsideAvatar == null)
            {
                Transform t = transform.Find("AnuInsideAvatar");
                if (t != null) anuInsideAvatar = t.gameObject;
            }
            if (handsFallbackImage == null)
            {
                Transform t = transform.Find("HandsImage") ?? transform.Find("HandsFallback");
                if (t != null) handsFallbackImage = t.GetComponent<Image>();
                if (handsFallbackImage == null) handsFallbackImage = GetComponentInChildren<Image>(true);
            }
            if (handWashBorderSprite == null)
            {
                #if UNITY_EDITOR
                handWashBorderSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/hand wash border.png");
                #endif
            }
            if (handsVideoImage == null)
            {
                Transform t = transform.Find("HandsImage/HandsVideo") ?? transform.Find("HandsVideo");
                if (t != null) handsVideoImage = t.GetComponent<RawImage>();
                if (handsVideoImage == null) handsVideoImage = GetComponentInChildren<RawImage>(true);
            }
            if (videoPlayerUI == null)
            {
                videoPlayerUI = GetComponentInChildren<U8_VideoPlayerUI_Masters_Activity>(true);
            }
            if (bubbleContainer == null)
            {
                Transform t = transform.Find("Bubbles") ?? transform.Find("BubbleContainer");
                if (t != null) bubbleContainer = t.gameObject;
            }
            if (germBlobs == null || germBlobs.Count == 0)
            {
                germBlobs = new List<Image>();
                Transform germsParent = transform.Find("Germs");
                if (germsParent != null)
                {
                    foreach (Transform child in germsParent)
                    {
                        var g = child.GetComponent<Image>();
                        if (g != null) germBlobs.Add(g);
                    }
                }

                if (germBlobs.Count == 0)
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        Transform t = transform.Find($"Germ_{i}") ?? transform.Find($"Germs/Germ_{i}");
                        if (t != null)
                        {
                            Image g = t.GetComponent<Image>();
                            if (g != null && !germBlobs.Contains(g)) germBlobs.Add(g);
                        }
                    }
                }
            }
        }

        public void PrepareReadyState()
        {
            if (handwashCoroutine != null)
            {
                StopCoroutine(handwashCoroutine);
                handwashCoroutine = null;
            }

            hasStartedWashing = false;
            isHandwashingActive = false;
            isHolding = false;
            hasStartedAudio = false;
            isHandwashingComplete = false;
            clearedGermsCount = 0;

            SetupHoldInteraction();

            // 1. Hide the timer elements
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (timerSlider != null) timerSlider.gameObject.SetActive(false);
            Transform badgeT = transform.Find("TimerBadge");
            if (badgeT != null) badgeT.gameObject.SetActive(false);
            Transform timerTxtT = transform.Find("TimerText");
            if (timerTxtT != null) timerTxtT.gameObject.SetActive(false);

            // 2. Enable and show the WASH HANDS button
            if (btnScrubArea != null)
            {
                btnScrubArea.gameObject.SetActive(true);
                btnScrubArea.interactable = true;
                var labelTmp = btnScrubArea.GetComponentInChildren<TextMeshProUGUI>(true);
                if (labelTmp != null)
                {
                    labelTmp.text = "WASH HANDS";
                    labelTmp.raycastTarget = false;
                }
            }

            // 3. Prepare Hands Visual active, ensure AnuInsideAvatar is inactive during washing
            if (handsImage != null)
            {
                handsImage.SetActive(true);
            }
            if (anuInsideAvatar != null)
            {
                anuInsideAvatar.SetActive(false);
            }

            if (handsFallbackImage != null)
            {
                if (handWashBorderSprite == null)
                {
                    #if UNITY_EDITOR
                    handWashBorderSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/hand wash border.png");
                    #endif
                }

                if (handWashBorderSprite != null)
                {
                    handsFallbackImage.sprite = handWashBorderSprite;
                }
                handsFallbackImage.color = Color.white;
                handsFallbackImage.preserveAspect = true;
                handsFallbackImage.gameObject.SetActive(true);
            }

            if (handsVideoImage != null)
            {
                handsVideoImage.gameObject.SetActive(true);
            }

            // 4. Do NOT autoplay video - show preview and pause until button held
            if (videoPlayerUI != null)
            {
                videoPlayerUI.gameObject.SetActive(true);
                videoPlayerUI.ResetToPreview();
                videoPlayerUI.Pause();
                videoPlayerUI.onVideoEnd = OnVideoCompleted;
            }

            if (bubbleContainer != null)
            {
                bubbleContainer.SetActive(true);
            }

            // 5. Reset all friendly germ blobs
            foreach (var blob in germBlobs)
            {
                if (blob != null)
                {
                    blob.gameObject.SetActive(true);
                    blob.transform.localScale = Vector3.one;
                }
            }

            if (promptText != null) promptText.text = "Press and hold 'WASH HANDS' to wash!";

            // Play hold instruction voice line BEFORE player performs the action
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_06"); // "Press and hold Wash Hands to keep washing your hands."
            }
        }

        public void OnHoldStarted()
        {
            if (isHandwashingComplete) return;

            isHolding = true;

            // 1. Activate visuals
            if (handsFallbackImage != null)
            {
                handsFallbackImage.gameObject.SetActive(true);
            }
            if (handsVideoImage != null)
            {
                handsVideoImage.gameObject.SetActive(true);
            }
            if (bubbleContainer != null)
            {
                bubbleContainer.SetActive(true);
            }

            if (!hasStartedWashing)
            {
                hasStartedWashing = true;
                isHandwashingActive = true;

                // 2. Play video
                if (videoPlayerUI != null)
                {
                    videoPlayerUI.gameObject.SetActive(true);
                    videoPlayerUI.onVideoEnd = OnVideoCompleted;
                    videoPlayerUI.Play();
                }

                // 3. Audio: Handwash music plays while actively scrubbing (stop initial VO if still playing)
                if (!hasStartedAudio)
                {
                    hasStartedAudio = true;
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.StopVO();
                        U8_AudioManager_Masters_Activity.Instance.PlayBGM("MUS_HandwashSong", false);
                    }
                }
                else
                {
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.ResumeBGM();
                    }
                }

                // 4. Start session coroutine
                if (handwashCoroutine != null) StopCoroutine(handwashCoroutine);
                handwashCoroutine = StartCoroutine(HandwashVideoSessionRoutine());
            }
            else
            {
                // Resuming video and audio from paused state
                if (videoPlayerUI != null)
                {
                    videoPlayerUI.Play();
                }

                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.ResumeBGM();
                }
            }

            if (promptText != null) promptText.text = "Washing hands with soap and water...";
        }

        public void OnHoldEnded()
        {
            if (isHandwashingComplete) return;

            isHolding = false;

            // Immediately pause video at current position
            if (videoPlayerUI != null)
            {
                videoPlayerUI.Pause();
            }

            // Immediately pause audio
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PauseBGM();
            }

            if (hasStartedWashing && promptText != null)
            {
                promptText.text = "Press and hold 'WASH HANDS' to continue washing!";
            }
        }

        public void OnHoldExited()
        {
            if (isHolding)
            {
                OnHoldEnded();
            }
        }

        public void OnHoldReEntered()
        {
            if (!isHolding && holdHandler != null && holdHandler.IsPointerDown)
            {
                OnHoldStarted();
            }
        }

        private void OnVideoCompleted()
        {
            if (isHandwashingActive && !isHandwashingComplete)
            {
                FinishHandwashing();
            }
        }

        private IEnumerator HandwashVideoSessionRoutine()
        {
            double duration = defaultDurationSeconds;
            if (videoPlayerUI != null && videoPlayerUI.Duration > 1.0)
            {
                duration = videoPlayerUI.Duration;
            }

            int totalGerms = germBlobs.Count;
            float wiggleTimer = 0f;

            while (isHandwashingActive)
            {
                // Washing progress ONLY advances while the player is actually holding the button and video is playing
                if (isHolding && videoPlayerUI != null && videoPlayerUI.IsPlaying)
                {
                    double currentVideoTime = 0.0;
                    if (videoPlayerUI.Player != null)
                    {
                        currentVideoTime = videoPlayerUI.Player.time;
                    }

                    float normalizedProgress = duration > 0 ? Mathf.Clamp01((float)(currentVideoTime / duration)) : 0f;

                    // Gradually clear germs according to normalized video progress
                    if (totalGerms > 0)
                    {
                        int targetClear = Mathf.Clamp(Mathf.FloorToInt(normalizedProgress * totalGerms), 0, totalGerms);
                        while (clearedGermsCount < targetClear && clearedGermsCount < totalGerms)
                        {
                            if (germBlobs[clearedGermsCount] != null && germBlobs[clearedGermsCount].gameObject.activeSelf)
                            {
                                StartCoroutine(SlideOffGermRoutine(germBlobs[clearedGermsCount]));
                            }
                            clearedGermsCount++;
                        }
                    }

                    // Periodic bubble / germ wiggle effect while actively scrubbing
                    wiggleTimer += Time.deltaTime;
                    if (wiggleTimer >= 2.5f)
                    {
                        wiggleTimer = 0f;
                        WiggleActiveGerms();
                    }

                    if (normalizedProgress >= 0.99f && !isHandwashingComplete)
                    {
                        FinishHandwashing();
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private void WiggleActiveGerms()
        {
            foreach (var g in germBlobs)
            {
                if (g != null && g.gameObject.activeSelf)
                {
                    StartCoroutine(QuickWiggleRoutine(g.transform));
                }
            }
        }

        private IEnumerator QuickWiggleRoutine(Transform t)
        {
            if (t == null) yield break;
            Vector3 origScale = Vector3.one;
            t.localScale = new Vector3(1.12f, 0.88f, 1f);
            yield return new WaitForSeconds(0.06f);
            if (t != null) t.localScale = new Vector3(0.92f, 1.08f, 1f);
            yield return new WaitForSeconds(0.06f);
            if (t != null) t.localScale = origScale;
        }

        private IEnumerator SlideOffGermRoutine(Image germImage)
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_GermOff");
            }

            Vector3 startPos = germImage.transform.localPosition;
            for (float t = 0; t < 1f; t += Time.deltaTime * 3.5f)
            {
                germImage.transform.localPosition = startPos + new Vector3(t * 60f, -t * 80f, 0);
                germImage.transform.localScale = Vector3.one * (1f - t);
                yield return null;
            }
            germImage.gameObject.SetActive(false);
            germImage.transform.localPosition = startPos;
        }

        private void FinishHandwashing()
        {
            if (!isHandwashingActive || isHandwashingComplete) return;
            isHandwashingActive = false;
            isHandwashingComplete = true;
            isHolding = false;

            if (handwashCoroutine != null)
            {
                StopCoroutine(handwashCoroutine);
                handwashCoroutine = null;
            }

            // 1. Stop and disable video
            if (videoPlayerUI != null)
            {
                videoPlayerUI.onVideoEnd = null;
                videoPlayerUI.Stop();
                videoPlayerUI.gameObject.SetActive(false);
            }
            if (handsVideoImage != null)
            {
                handsVideoImage.gameObject.SetActive(false);
            }

            // 2. Hide WASH HANDS button now that handwashing is genuinely complete
            if (btnScrubArea != null)
            {
                btnScrubArea.interactable = false;
                btnScrubArea.gameObject.SetActive(false);
            }

            // 3. Hide all remaining germs and bubble clutter
            foreach (var blob in germBlobs)
            {
                if (blob != null) blob.gameObject.SetActive(false);
            }
            if (bubbleContainer != null)
            {
                bubbleContainer.SetActive(false);
            }

            // 4. IMMEDIATELY deactivate hands visual and activate AnuInsideAvatar
            if (handsImage != null)
            {
                handsImage.SetActive(false);
            }
            if (handsFallbackImage != null)
            {
                handsFallbackImage.gameObject.SetActive(false);
            }
            if (handsVideoImage != null)
            {
                handsVideoImage.gameObject.SetActive(false);
            }

            if (anuInsideAvatar != null)
            {
                anuInsideAvatar.SetActive(true);
            }

            if (promptText != null) promptText.text = "All clean! Star 1 Earned!";

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.StopBGM();
                U8_AudioManager_Masters_Activity.Instance.StopVO();
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Sparkle");
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_08"); // "All clean! One star."
            }

            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.isHandsWashed = true;
                U8_GameManager_Masters_Activity.Instance.AwardStar();
            }

            StartCoroutine(ReturnToWashroomRoutine());
        }

        private IEnumerator ReturnToWashroomRoutine()
        {
            yield return new WaitForSeconds(2.2f);
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part2_Inside);
            }
        }
    }
}
