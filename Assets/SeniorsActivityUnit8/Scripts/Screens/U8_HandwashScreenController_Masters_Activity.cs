using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public class U8_HandwashScreenController_Masters_Activity : MonoBehaviour
    {
        [Header("UI Prompts & Feedback")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Button btnScrubArea;

        [Header("Hands & Soap Visuals")]
        [SerializeField] private RawImage handsVideoImage;
        [SerializeField] private U8_VideoPlayerUI_Masters_Activity videoPlayerUI;
        [SerializeField] private Image handsFallbackImage;
        [SerializeField] private Sprite handsNormalSprite;
        [SerializeField] private Sprite handsSoapySprite;
        [SerializeField] private Sprite handsSparklingSprite;
        [SerializeField] private Image waterStreamImage;
        [SerializeField] private GameObject bubbleContainer;
        [SerializeField] private List<Image> bubbleSprites = new List<Image>();

        [Header("4 Friendly Germ Blobs")]
        [SerializeField] private List<Image> germBlobs = new List<Image>();

        [Header("Settings & Timer")]
        [SerializeField] private float songDurationSeconds = 20.0f;
        [SerializeField] private int scrubsPerGerm = 4;

        private float currentTimer = 0f;
        private int scrubCount = 0;
        private int germsCleared = 0;
        private bool isHandwashingActive = false;
        private float timeSinceLastScrub = 0f;
        private Coroutine handwashCoroutine;

        private void Awake()
        {
            AutoFindUIReferences();
            if (btnScrubArea != null)
            {
                btnScrubArea.onClick.AddListener(OnScrubClicked);
            }
        }

        private void OnEnable()
        {
            StartHandwashingSession();
        }

        private void OnDisable()
        {
            if (handwashCoroutine != null) StopCoroutine(handwashCoroutine);
            if (videoPlayerUI != null) videoPlayerUI.Pause();
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
            }
            if (handsVideoImage == null)
            {
                Transform t = transform.Find("HandsVideo") ?? transform.Find("HandsImage/HandsVideo");
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

        public void StartHandwashingSession()
        {
            isHandwashingActive = true;
            currentTimer = songDurationSeconds;
            scrubCount = 0;
            germsCleared = 0;
            timeSinceLastScrub = 0f;

            // Reset Hands Image & enable video child
            if (handsFallbackImage != null && handsSoapySprite != null)
            {
                handsFallbackImage.sprite = handsSoapySprite;
                handsFallbackImage.gameObject.SetActive(true);
            }

            if (handsVideoImage != null)
            {
                handsVideoImage.gameObject.SetActive(true);
            }

            if (videoPlayerUI != null)
            {
                videoPlayerUI.gameObject.SetActive(true);
                videoPlayerUI.Play();
            }

            if (bubbleContainer != null)
            {
                bubbleContainer.SetActive(true);
            }

            // Reset all friendly germ blobs
            foreach (var blob in germBlobs)
            {
                if (blob != null)
                {
                    blob.gameObject.SetActive(true);
                    blob.transform.localScale = Vector3.one;
                }
            }

            if (promptText != null) promptText.text = $"Tap or rub to scrub away all {germBlobs.Count} germs!";
            if (timerSlider != null)
            {
                timerSlider.maxValue = songDurationSeconds;
                timerSlider.value = songDurationSeconds;
            }

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_06"); // "Now wash your hands. Keep tapping!"
                U8_AudioManager_Masters_Activity.Instance.PlayBGM("MUS_HandwashSong", false);
            }

            if (handwashCoroutine != null) StopCoroutine(handwashCoroutine);
            handwashCoroutine = StartCoroutine(HandwashTimerRoutine());
        }

        public void OnScrubClicked()
        {
            if (!isHandwashingActive) return;

            scrubCount++;
            timeSinceLastScrub = 0f;

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Scrub");
                if (scrubCount % 2 == 0) U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Bubble");
            }

            // Animate germ wiggle on tap
            WiggleActiveGerms();

            // Animate bubble growth
            if (bubbleSprites != null)
            {
                foreach (var bubble in bubbleSprites)
                {
                    if (bubble != null)
                    {
                        bubble.transform.localScale = Vector3.one * Mathf.Clamp(0.5f + (scrubCount * 0.04f), 0.5f, 1.4f);
                    }
                }
            }

            // Paced Germ Clearance across the 20-second duration:
            // Ensures germs clear gradually as student taps, lasting through the full song
            float timeElapsed = songDurationSeconds - currentTimer;
            float secondsPerGerm = (germBlobs.Count > 0) ? (songDurationSeconds / germBlobs.Count) : 2.5f;
            int maxPacedGerms = Mathf.Clamp(Mathf.FloorToInt(timeElapsed / secondsPerGerm) + 1, 1, germBlobs.Count);
            int tapPacedGerms = Mathf.Clamp(scrubCount / Mathf.Max(1, scrubsPerGerm), 0, germBlobs.Count);

            int targetGermsToClear = Mathf.Min(maxPacedGerms, tapPacedGerms);
            if (targetGermsToClear > germsCleared)
            {
                int indexToClear = germsCleared;
                germsCleared = targetGermsToClear;
                if (indexToClear < germBlobs.Count && germBlobs[indexToClear] != null)
                {
                    StartCoroutine(SlideOffGermRoutine(germBlobs[indexToClear]));
                }

                if (promptText != null)
                {
                    promptText.text = $"Great scrubbing! {germsCleared}/{germBlobs.Count} germs washed away!";
                }
            }
        }

        private void WiggleActiveGerms()
        {
            for (int i = germsCleared; i < germBlobs.Count; i++)
            {
                if (germBlobs[i] != null && germBlobs[i].gameObject.activeSelf)
                {
                    StartCoroutine(QuickWiggleRoutine(germBlobs[i].transform));
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

        private IEnumerator HandwashTimerRoutine()
        {
            while (currentTimer > 0f)
            {
                currentTimer -= Time.deltaTime;
                timeSinceLastScrub += Time.deltaTime;

                if (timerSlider != null) timerSlider.value = currentTimer;
                if (timerText != null) timerText.text = $"{Mathf.CeilToInt(currentTimer)}s";

                // Encouragement if student pauses tapping
                if (timeSinceLastScrub > 3.0f && isHandwashingActive)
                {
                    if (promptText != null) promptText.text = "Keep going! Scrub the germs away!";
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_07"); // "Keep going!"
                    }
                    timeSinceLastScrub = 0f; // Reset warning cooldown
                }

                yield return null;
            }

            FinishHandwashing();
        }

        private void FinishHandwashing()
        {
            isHandwashingActive = false;

            // 1. Immediately disable child video GameObject
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

            // 3. Reveal clean sparkling hands on parent HandsImage
            if (handsFallbackImage != null)
            {
                if (handsSparklingSprite != null)
                {
                    handsFallbackImage.sprite = handsSparklingSprite;
                }
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
