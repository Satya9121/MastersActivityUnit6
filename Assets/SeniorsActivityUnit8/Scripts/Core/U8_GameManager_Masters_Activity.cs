using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public enum U8_GamePart
    {
        Part1_Door = 0,
        Part2_Inside = 1,
        HandwashScreen = 2,
        Part3_AfterYou = 3,
        Part4_EmptySoap = 4,
        Ending = 5
    }

    public class U8_GameManager_Masters_Activity : MonoBehaviour
    {
        public static U8_GameManager_Masters_Activity Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private U8_GamePart currentPart = U8_GamePart.Part1_Door;
        [SerializeField] private int starCount = 0;

        [Header("Part 2 Washroom Step Flags")]
        public bool isCubicleClosedFirst = false;
        public bool isFlushed = false;
        public bool isHandsWashed = false;
        public bool isTapTurnedOff = false;
        public bool isTowelInBin = false;
        public bool isSinkWiped = false;

        [Header("Screen References")]
        [SerializeField] private GameObject part1DoorScreen;
        [SerializeField] private GameObject part2InsideScreen;
        [SerializeField] private GameObject handwashScreen;
        [SerializeField] private GameObject part3AfterYouScreen;
        [SerializeField] private GameObject part4EmptySoapScreen;
        [SerializeField] private GameObject endingScreen;

        [Header("Blink Transition Overlay")]
        [SerializeField] private GameObject transitionOverlayObject;
        [SerializeField] private Image transitionOverlayImage;
        [SerializeField] private CanvasGroup transitionOverlayCanvasGroup;
        [SerializeField] private float fadeDuration = 0.18f;
        [SerializeField] private float darkHoldDuration = 0.04f;
        [Header("Audio Clips (Inspector)")]
        [SerializeField] private AudioClip ambWashroom;
        [SerializeField] private AudioClip sfxStar;

        private bool isTransitioning = false;
        private Coroutine activeTransitionCoroutine;

        // Events
        public event Action<U8_GamePart> OnPartChanged;
        public event Action<int> OnStarEarned;

        public U8_GamePart CurrentPart => currentPart;
        public int StarCount => starCount;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoFindScreens();
            AutoWireAudioClips();
            EnsureTransitionOverlay();
        }

        public void AutoWireAudioClips()
        {
            if (U8_AudioManager_Masters_Activity.Instance == null) return;
            if (ambWashroom == null) ambWashroom = U8_AudioManager_Masters_Activity.Instance.GetClip("AMB_Washroom");
            if (sfxStar == null) sfxStar = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Star");
        }

        private void Start()
        {
            ResetUnitState();
            // Start game directly on Part 1 without initial blink
            ShowPartDirect(U8_GamePart.Part1_Door);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayAmbience("AMB_Washroom");
            }
        }

        private void AutoFindScreens()
        {
            if (part1DoorScreen == null)
            {
                var s = FindFirstObjectByType<U8_Part1_DoorController_Masters_Activity>(FindObjectsInactive.Include);
                if (s != null) part1DoorScreen = s.gameObject;
            }
            if (part2InsideScreen == null)
            {
                var s = FindFirstObjectByType<U8_Part2_WashroomInsideController_Masters_Activity>(FindObjectsInactive.Include);
                if (s != null) part2InsideScreen = s.gameObject;
            }
            if (handwashScreen == null)
            {
                var s = FindFirstObjectByType<U8_HandwashScreenController_Masters_Activity>(FindObjectsInactive.Include);
                if (s != null) handwashScreen = s.gameObject;
            }
            if (part3AfterYouScreen == null)
            {
                var s = FindFirstObjectByType<U8_Part3_AfterYouController_Masters_Activity>(FindObjectsInactive.Include);
                if (s != null) part3AfterYouScreen = s.gameObject;
            }
            if (part4EmptySoapScreen == null)
            {
                var s = FindFirstObjectByType<U8_Part4_EmptySoapController_Masters_Activity>(FindObjectsInactive.Include);
                if (s != null) part4EmptySoapScreen = s.gameObject;
            }
            if (endingScreen == null)
            {
                var s = FindFirstObjectByType<U8_EndingScreen_Masters_Activity>(FindObjectsInactive.Include);
                if (s != null) endingScreen = s.gameObject;
            }
        }

        public void EnsureTransitionOverlay()
        {
            if (transitionOverlayObject != null)
            {
                if (transitionOverlayCanvasGroup == null) transitionOverlayCanvasGroup = transitionOverlayObject.GetComponent<CanvasGroup>();
                if (transitionOverlayImage == null) transitionOverlayImage = transitionOverlayObject.GetComponent<Image>();
                return;
            }

            // Find Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("TransitionOverlay");
            if (existing != null)
            {
                transitionOverlayObject = existing.gameObject;
                transitionOverlayCanvasGroup = existing.GetComponent<CanvasGroup>();
                transitionOverlayImage = existing.GetComponent<Image>();
            }
            else
            {
                GameObject go = new GameObject("TransitionOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                go.transform.SetParent(canvas.transform, false);
                go.transform.SetAsLastSibling();

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;

                Image img = go.GetComponent<Image>();
                img.color = Color.black;
                img.raycastTarget = false;

                CanvasGroup cg = go.GetComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                cg.interactable = false;

                transitionOverlayObject = go;
                transitionOverlayImage = img;
                transitionOverlayCanvasGroup = cg;
            }

            if (transitionOverlayObject != null)
            {
                transitionOverlayObject.transform.SetAsLastSibling();
                transitionOverlayObject.SetActive(false);
            }
        }

        /// <summary>
        /// Transitions smoothly to the specified screen using an eye-blink transition.
        /// </summary>
        public void ShowPart(U8_GamePart part)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[U8_GameManager] Ignoring transition to {part} because a transition is currently in progress.");
                return;
            }

            if (activeTransitionCoroutine != null)
            {
                StopCoroutine(activeTransitionCoroutine);
            }

            activeTransitionCoroutine = StartCoroutine(BlinkTransitionRoutine(part));
        }

        /// <summary>
        /// Performs a smooth screen blink/fade transition without changing the active screen,
        /// executing the onPeakDarkness callback while the screen is fully dark.
        /// </summary>
        public void BlinkTransition(Action onPeakDarkness, float customFadeDur = 0.7f, float customHoldDur = 0.35f, Action onComplete = null)
        {
            if (activeTransitionCoroutine != null)
            {
                StopCoroutine(activeTransitionCoroutine);
            }

            activeTransitionCoroutine = StartCoroutine(CustomBlinkTransitionRoutine(onPeakDarkness, customFadeDur, customHoldDur, onComplete));
        }

        private IEnumerator CustomBlinkTransitionRoutine(Action onPeakDarkness, float customFadeDur, float customHoldDur, Action onComplete)
        {
            isTransitioning = true;
            EnsureTransitionOverlay();

            if (transitionOverlayObject != null)
            {
                transitionOverlayObject.transform.SetAsLastSibling();
                transitionOverlayObject.SetActive(true);
            }

            if (transitionOverlayCanvasGroup != null)
            {
                transitionOverlayCanvasGroup.blocksRaycasts = true;
            }
            if (transitionOverlayImage != null)
            {
                transitionOverlayImage.raycastTarget = true;
            }

            // 1. FADE IN TO BLACK
            float t = 0f;
            float dur = Mathf.Max(0.08f, customFadeDur);
            while (t < dur)
            {
                t += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0f, 1f, t / dur);
                SetOverlayAlpha(alpha);
                yield return null;
            }
            SetOverlayAlpha(1f);

            // 2. HOLD FULL DARK MOMENTARILY & EXECUTE CALLBACK
            if (customHoldDur > 0f)
            {
                yield return new WaitForSeconds(customHoldDur);
            }

            onPeakDarkness?.Invoke();

            // Give 1 frame to settle UI
            yield return null;

            // 3. FADE OUT FROM BLACK
            t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float alpha = Mathf.SmoothStep(1f, 0f, t / dur);
                SetOverlayAlpha(alpha);
                yield return null;
            }
            SetOverlayAlpha(0f);

            // 4. CLEANUP
            if (transitionOverlayCanvasGroup != null)
            {
                transitionOverlayCanvasGroup.blocksRaycasts = false;
            }
            if (transitionOverlayImage != null)
            {
                transitionOverlayImage.raycastTarget = false;
            }
            if (transitionOverlayObject != null)
            {
                transitionOverlayObject.SetActive(false);
            }

            isTransitioning = false;
            activeTransitionCoroutine = null;

            onComplete?.Invoke();
        }

        /// <summary>
        /// Directly switches active screen GameObject without transition overlay (used at startup / tests).
        /// </summary>
        public void ShowPartDirect(U8_GamePart part)
        {
            currentPart = part;
            AutoFindScreens();

            if (part1DoorScreen != null) part1DoorScreen.SetActive(part == U8_GamePart.Part1_Door);
            if (part2InsideScreen != null) part2InsideScreen.SetActive(part == U8_GamePart.Part2_Inside);
            if (handwashScreen != null) handwashScreen.SetActive(part == U8_GamePart.HandwashScreen);
            if (part3AfterYouScreen != null) part3AfterYouScreen.SetActive(part == U8_GamePart.Part3_AfterYou);
            if (part4EmptySoapScreen != null) part4EmptySoapScreen.SetActive(part == U8_GamePart.Part4_EmptySoap);
            if (endingScreen != null) endingScreen.SetActive(part == U8_GamePart.Ending);

            OnPartChanged?.Invoke(part);
            Debug.Log($"[U8_GameManager] Switched active screen to: {part}");
        }

        private IEnumerator BlinkTransitionRoutine(U8_GamePart targetPart)
        {
            isTransitioning = true;
            EnsureTransitionOverlay();

            if (transitionOverlayObject != null)
            {
                transitionOverlayObject.transform.SetAsLastSibling();
                transitionOverlayObject.SetActive(true);
            }

            if (transitionOverlayCanvasGroup != null)
            {
                transitionOverlayCanvasGroup.blocksRaycasts = true;
            }
            if (transitionOverlayImage != null)
            {
                transitionOverlayImage.raycastTarget = true;
            }

            // 1. FADE IN TO BLACK (0.15 - 0.20s)
            float t = 0f;
            float dur = Mathf.Max(0.08f, fadeDuration);
            while (t < dur)
            {
                t += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0f, 1f, t / dur);
                SetOverlayAlpha(alpha);
                yield return null;
            }
            SetOverlayAlpha(1f);

            // 2. HOLD FULL DARK MOMENTARILY & SWITCH SCREEN
            if (darkHoldDuration > 0f)
            {
                yield return new WaitForSeconds(darkHoldDuration);
            }

            // Strictly switch screen while completely covered by black overlay
            ShowPartDirect(targetPart);

            // Give new screen 1 frame to initialize Awake / OnEnable while hidden
            yield return null;

            // 3. FADE OUT FROM BLACK (0.15 - 0.20s)
            t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float alpha = Mathf.SmoothStep(1f, 0f, t / dur);
                SetOverlayAlpha(alpha);
                yield return null;
            }
            SetOverlayAlpha(0f);

            // 4. CLEANUP
            if (transitionOverlayCanvasGroup != null)
            {
                transitionOverlayCanvasGroup.blocksRaycasts = false;
            }
            if (transitionOverlayImage != null)
            {
                transitionOverlayImage.raycastTarget = false;
            }
            if (transitionOverlayObject != null)
            {
                transitionOverlayObject.SetActive(false);
            }

            isTransitioning = false;
            activeTransitionCoroutine = null;
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (transitionOverlayCanvasGroup != null)
            {
                transitionOverlayCanvasGroup.alpha = alpha;
            }
            else if (transitionOverlayImage != null)
            {
                transitionOverlayImage.color = new Color(0f, 0f, 0f, alpha);
            }
        }

        public void AwardStar()
        {
            starCount = Mathf.Clamp(starCount + 1, 0, 3);
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Star");
            }
            OnStarEarned?.Invoke(starCount);
            Debug.Log($"[U8_GameManager] ⭐ Star Earned! Total stars: {starCount}/3");
        }

        public void SetStarCount(int count)
        {
            starCount = Mathf.Clamp(count, 0, 3);
            OnStarEarned?.Invoke(starCount);
        }

        /// <summary>
        /// Resets Part 2 washroom state flags for the "TRY AGAIN" classroom retry flow.
        /// </summary>
        public void ResetUnitState()
        {
            isCubicleClosedFirst = false;
            isFlushed = false;
            isHandsWashed = false;
            isTapTurnedOff = false;
            isTowelInBin = false;
            isSinkWiped = false;
            Debug.Log("[U8_GameManager] Washroom state flags reset. Replaying Part 2...");
        }

        public bool AreAllStepsCompleted()
        {
            return isCubicleClosedFirst && isFlushed && isHandsWashed && isTapTurnedOff && isTowelInBin && isSinkWiped;
        }
    }
}
