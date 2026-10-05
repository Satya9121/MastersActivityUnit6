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
        private Coroutine handwashCoroutine;

        private void Awake()
        {
            AutoFindUIReferences();
            AutoWireAudioClips();
            if (btnScrubArea != null)
            {
                btnScrubArea.onClick.RemoveAllListeners();
                btnScrubArea.onClick.AddListener(OnWashHandsButtonClicked);
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
            if (handwashCoroutine != null) StopCoroutine(handwashCoroutine);
            if (videoPlayerUI != null)
            {
                videoPlayerUI.onVideoEnd = null;
                videoPlayerUI.Pause();
            }
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.StopBGM();
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
            if (handwashCoroutine != null) StopCoroutine(handwashCoroutine);

            hasStartedWashing = false;
            isHandwashingActive = false;

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
                }
            }

            // 3. Prepare Hands Image strictly with hand wash border.png (active before clicking)
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

            // 4. Do NOT autoplay video - show preview and pause until button clicked
            if (videoPlayerUI != null)
            {
                videoPlayerUI.gameObject.SetActive(true);
                videoPlayerUI.ResetToPreview();
                videoPlayerUI.Pause();
                videoPlayerUI.onVideoEnd = null;
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

            if (promptText != null) promptText.text = "Click 'WASH HANDS' to start washing!";
        }

        public void OnWashHandsButtonClicked()
        {
            if (hasStartedWashing) return;

            hasStartedWashing = true;
            isHandwashingActive = true;

            // 1. Disable and hide the WASH HANDS button immediately
            if (btnScrubArea != null)
            {
                btnScrubArea.interactable = false;
                btnScrubArea.gameObject.SetActive(false);
            }

            // 2. Activate HandsImage border and video when button is clicked
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

            // 3. Play the video (non-looping)
            if (videoPlayerUI != null)
            {
                videoPlayerUI.gameObject.SetActive(true);
                videoPlayerUI.onVideoEnd = () =>
                {
                    if (isHandwashingActive)
                    {
                        FinishHandwashing();
                    }
                };
                videoPlayerUI.Play();
            }

            // 4. Play audio (song + VO)
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_06"); // "Now wash your hands!"
                U8_AudioManager_Masters_Activity.Instance.PlayBGM("MUS_HandwashSong", false);
            }

            if (promptText != null) promptText.text = "Washing hands with soap and water...";

            // 5. Start session coroutine
            if (handwashCoroutine != null) StopCoroutine(handwashCoroutine);
            handwashCoroutine = StartCoroutine(HandwashVideoSessionRoutine());
        }

        private IEnumerator HandwashVideoSessionRoutine()
        {
            double duration = defaultDurationSeconds;
            if (videoPlayerUI != null && videoPlayerUI.Duration > 1.0)
            {
                duration = videoPlayerUI.Duration;
            }

            float elapsed = 0f;
            int totalGerms = germBlobs.Count;
            int clearedSoFar = 0;

            while (elapsed < (float)duration)
            {
                elapsed += Time.deltaTime;

                // Gradually clear germs as video plays
                if (totalGerms > 0)
                {
                    int targetClear = Mathf.Clamp(Mathf.FloorToInt((elapsed / (float)duration) * totalGerms), 0, totalGerms);
                    while (clearedSoFar < targetClear && clearedSoFar < totalGerms)
                    {
                        if (germBlobs[clearedSoFar] != null && germBlobs[clearedSoFar].gameObject.activeSelf)
                        {
                            StartCoroutine(SlideOffGermRoutine(germBlobs[clearedSoFar]));
                        }
                        clearedSoFar++;
                    }
                }

                // Periodic bubble effect
                if (Mathf.FloorToInt(elapsed) % 3 == 0)
                {
                    WiggleActiveGerms();
                }

                yield return null;
            }

            // Once video ends, finish activity and transition to next step in gameplay flow
            FinishHandwashing();
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
            if (!isHandwashingActive) return;
            isHandwashingActive = false;

            // 1. Stop and disable video
            if (videoPlayerUI != null)
            {
                videoPlayerUI.Stop();
                videoPlayerUI.gameObject.SetActive(false);
            }
            if (handsVideoImage != null)
            {
                handsVideoImage.gameObject.SetActive(false);
            }

            // 2. Hide all remaining germs and bubble clutter
            foreach (var blob in germBlobs)
            {
                if (blob != null) blob.gameObject.SetActive(false);
            }
            if (bubbleContainer != null)
            {
                bubbleContainer.SetActive(false);
            }

            // 3. Hands Image stays with handWashBorderSprite
            if (handsFallbackImage != null)
            {
                if (handWashBorderSprite != null)
                {
                    handsFallbackImage.sprite = handWashBorderSprite;
                }
                handsFallbackImage.color = Color.white;
                handsFallbackImage.preserveAspect = true;
                handsFallbackImage.gameObject.SetActive(true);
            }

            if (promptText != null) promptText.text = "All clean! Star 1 Earned!";

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
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
