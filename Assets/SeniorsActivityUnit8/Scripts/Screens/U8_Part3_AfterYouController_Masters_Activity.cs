using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public class U8_Part3_AfterYouController_Masters_Activity : MonoBehaviour
    {
        [Header("UI Prompts & Action Buttons")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button btnTryAgain;
        [SerializeField] private Button btnProceedToPart4;

        [Header("Meera Character Avatars & Poses")]
        [SerializeField] private Image meeraAvatar;
        [SerializeField] private Sprite meeraEnteringSprite;
        [SerializeField] private Sprite meeraIdleSprite;
        [SerializeField] private Sprite meeraSlippingSprite;
        [SerializeField] private Sprite meeraUnimpressedSprite;
        [SerializeField] private Sprite meeraPumpingSoapSprite;
        [SerializeField] private Sprite meeraHappySmileSprite;
        [SerializeField] private Sprite[] meeraWalkSprites = new Sprite[8];

        [Header("Walking Settings")]
        [SerializeField] private float walkSpeed = 240f;
        [SerializeField] private float stepFrameDuration = 0.12f;

        [Header("Washroom State Overlay Visuals")]
        [SerializeField] private Image sinkImage;
        [SerializeField] private Sprite sinkCleanSprite;
        [SerializeField] private Sprite sinkSplashedSprite;
        [SerializeField] private GameObject wetFloorPuddleObject;
        [SerializeField] private GameObject soggyTowelFloorObject;
        [SerializeField] private GameObject runningWaterStreamObject;
        [SerializeField] private GameObject splashedSinkOverlay;
        [SerializeField] private GameObject cleanSinkSparkleOverlay;
        [Header("Audio Clips (Inspector)")]
        [SerializeField] private AudioClip voHereComesMeera;
        [SerializeField] private AudioClip voOhDearShallWeTryAgain;
        [SerializeField] private AudioClip voMeeraGasp;
        [SerializeField] private AudioClip sfxSlip;
        [SerializeField] private AudioClip sfxTapOff;
        [SerializeField] private AudioClip sfxSparkle;

        private bool isEvaluating = false;

        private void Awake()
        {
            AutoFindUIReferences();
            AutoWireAudioClips();
            EnsureMeeraSpritesLoaded();

            if (btnTryAgain != null) btnTryAgain.onClick.AddListener(OnTryAgainClicked);
            if (btnProceedToPart4 != null) btnProceedToPart4.onClick.AddListener(OnProceedToPart4Clicked);
        }

        public void AutoWireAudioClips()
        {
            if (U8_AudioManager_Masters_Activity.Instance == null) return;
            if (voHereComesMeera == null) voHereComesMeera = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_12");
            if (voOhDearShallWeTryAgain == null) voOhDearShallWeTryAgain = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_13");
            if (voMeeraGasp == null) voMeeraGasp = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_MEE_2");
            if (sfxSlip == null) sfxSlip = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Slip");
            if (sfxTapOff == null) sfxTapOff = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_TapOff");
            if (sfxSparkle == null) sfxSparkle = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Sparkle");
        }

        private void OnEnable()
        {
            AutoFindUIReferences();
            EnsureMeeraSpritesLoaded();
            StartCoroutine(EvaluateMeeraEntrySequence());
        }

        private void AutoFindUIReferences()
        {
            if (promptText == null)
            {
                Transform t = transform.Find("PromptCard/PromptText") ?? transform.Find("PromptText");
                if (t != null) promptText = t.GetComponent<TextMeshProUGUI>();
                else promptText = GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (feedbackText == null)
            {
                Transform t = transform.Find("FeedbackCard/FeedbackText") ?? transform.Find("FeedbackText");
                if (t != null) feedbackText = t.GetComponent<TextMeshProUGUI>();
            }

            if (btnTryAgain == null)
            {
                Transform t = transform.Find("ChoiceContainer/Btn_TryAgain") ?? transform.Find("Btn_TryAgain") ?? transform.Find("BottomBar/Btn_TryAgain");
                if (t != null) btnTryAgain = t.GetComponent<Button>();
            }
            if (btnProceedToPart4 == null)
            {
                Transform t = transform.Find("ChoiceContainer/Btn_ProceedToPart4") ?? transform.Find("Btn_ProceedToPart4") ?? transform.Find("Btn_Next") ?? transform.Find("BottomBar/Btn_Proceed");
                if (t != null) btnProceedToPart4 = t.GetComponent<Button>();
            }
            if (meeraAvatar == null)
            {
                Transform t = transform.Find("MeeraAvatar") ?? transform.Find("Meera_Image");
                if (t != null) meeraAvatar = t.GetComponent<Image>();
            }
            if (meeraAvatar != null)
            {
                meeraAvatar.gameObject.SetActive(true);
                meeraAvatar.preserveAspect = true;
                meeraAvatar.rectTransform.localScale = Vector3.one;
                meeraAvatar.rectTransform.localRotation = Quaternion.identity;
                meeraAvatar.rectTransform.sizeDelta = new Vector2(580f, 754f);
                if (meeraAvatar.sprite == null || meeraAvatar.sprite.name.Contains("Meera U8 SA _0"))
                {
                    meeraAvatar.sprite = (meeraIdleSprite != null) ? meeraIdleSprite : meeraEnteringSprite;
                }
            }

            // Auto-find all overlay GameObjects
            if (sinkImage == null)
            {
                Transform t = transform.Find("SinkImage") ?? transform.Find("Sink/SinkImage") ?? transform.Find("Sink");
                if (t != null) sinkImage = t.GetComponent<Image>();
            }
            if (wetFloorPuddleObject == null)
            {
                Transform t = transform.Find("WetFloorPuddleObject") ?? transform.Find("WetFloorPuddle") ?? transform.Find("Puddle");
                if (t != null) wetFloorPuddleObject = t.gameObject;
            }
            if (soggyTowelFloorObject == null)
            {
                Transform t = transform.Find("SoggyTowelFloorObject") ?? transform.Find("SoggyTowel") ?? transform.Find("TowelOnFloor");
                if (t != null) soggyTowelFloorObject = t.gameObject;
            }
            if (splashedSinkOverlay == null)
            {
                Transform t = transform.Find("SplashedSinkOverlay") ?? transform.Find("SinkSplashed") ?? transform.Find("Sink_Splashed");
                if (t != null) splashedSinkOverlay = t.gameObject;
            }
            if (cleanSinkSparkleOverlay == null)
            {
                Transform t = transform.Find("CleanSinkSparkleOverlay") ?? transform.Find("SinkClean") ?? transform.Find("Sink_Clean");
                if (t != null) cleanSinkSparkleOverlay = t.gameObject;
            }
            if (runningWaterStreamObject == null)
            {
                Transform t = transform.Find("RunningWaterStreamObject") ?? transform.Find("TapWaterStream") ?? transform.Find("WaterStream");
                if (t != null) runningWaterStreamObject = t.gameObject;
            }
        }

        public void EnsureMeeraSpritesLoaded()
        {
#if UNITY_EDITOR
            if (meeraWalkSprites == null || meeraWalkSprites.Length < 8 || meeraWalkSprites[0] == null)
            {
                meeraWalkSprites = new Sprite[8];
                for (int i = 0; i < 8; i++)
                {
                    meeraWalkSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_Right_{i + 1}.png");
                }
            }
            if (meeraIdleSprite == null)
            {
                meeraIdleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_Right_Idle.png");
            }
            if (meeraEnteringSprite == null || meeraEnteringSprite.name.Contains("Meera U8 SA _0"))
            {
                meeraEnteringSprite = (meeraIdleSprite != null) ? meeraIdleSprite : meeraWalkSprites[0];
            }
            if (meeraAvatar != null && (meeraAvatar.sprite == null || meeraAvatar.sprite.name.Contains("Meera U8 SA _0")))
            {
                meeraAvatar.sprite = (meeraIdleSprite != null) ? meeraIdleSprite : meeraWalkSprites[0];
            }
#endif
        }

        private IEnumerator EvaluateMeeraEntrySequence()
        {
            isEvaluating = true;
            EnsureMeeraSpritesLoaded();

            if (btnTryAgain != null) btnTryAgain.gameObject.SetActive(false);
            if (btnProceedToPart4 != null) btnProceedToPart4.gameObject.SetActive(false);

            var gm = U8_GameManager_Masters_Activity.Instance;
            bool isAllClean = (gm != null && gm.AreAllStepsCompleted());

            // 1. Determine washroom states based on student's actions in Part 2
            bool isFloorWet = !isAllClean && (!gm.isSinkWiped || !gm.isTapTurnedOff);
            bool isTowelOnFloor = !isAllClean && !gm.isTowelInBin;
            bool isTapRunning = !isAllClean && !gm.isTapTurnedOff;
            bool isSinkSplashed = !isAllClean && !gm.isSinkWiped;

            if (sinkImage != null)
            {
                sinkImage.gameObject.SetActive(true);
                if (isSinkSplashed && sinkSplashedSprite != null) sinkImage.sprite = sinkSplashedSprite;
                else if (sinkCleanSprite != null) sinkImage.sprite = sinkCleanSprite;
            }

            if (wetFloorPuddleObject != null) wetFloorPuddleObject.SetActive(isFloorWet);
            if (soggyTowelFloorObject != null) soggyTowelFloorObject.SetActive(isTowelOnFloor);
            if (runningWaterStreamObject != null) runningWaterStreamObject.SetActive(isTapRunning);
            if (splashedSinkOverlay != null) splashedSinkOverlay.SetActive(isSinkSplashed);
            if (cleanSinkSparkleOverlay != null) cleanSinkSparkleOverlay.SetActive(isAllClean);

            if (isTapRunning && U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayAmbience("SFX_TapOn", true);
            }

            // Phase 1: Meera is already standing on the ground at her correct position on frame 1
            Vector2 standPos = isFloorWet ? new Vector2(-220f, -50f) : new Vector2(-95f, -50f);

            if (meeraAvatar != null)
            {
                meeraAvatar.gameObject.SetActive(true);
                meeraAvatar.preserveAspect = true;
                meeraAvatar.transform.localRotation = Quaternion.identity;
                meeraAvatar.rectTransform.localScale = Vector3.one;
                meeraAvatar.rectTransform.sizeDelta = new Vector2(580f, 754f);
                meeraAvatar.rectTransform.anchoredPosition = standPos;
                meeraAvatar.sprite = (meeraIdleSprite != null) ? meeraIdleSprite : meeraAvatar.sprite;
            }

            yield return new WaitForSeconds(0.2f);

            // Phase 2: Evaluate Consequence immediately
            if (isAllClean)
            {
                // ====================================================
                // === CLEAN OUTCOME: Meera is happy with clean room ===
                // ====================================================
                if (meeraAvatar != null)
                {
                    // Meera smiles happily with open arms
                    if (meeraHappySmileSprite != null)
                    {
                        meeraAvatar.sprite = meeraHappySmileSprite;
                        meeraAvatar.preserveAspect = true;
                    }
                }

                if (promptText != null) promptText.text = "The washroom is clean, dry, and welcoming!";

                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Sparkle");
                }

                yield return new WaitForSeconds(1.2f);

                if (feedbackText != null) feedbackText.text = "Meera is happy! Star 2 Earned!";

                if (gm != null) gm.AwardStar();

                yield return new WaitForSeconds(1.0f);
                ShowChoiceButtons();
            }
            else
            {
                // ====================================================
                // === DIRTY / MESSY OUTCOME: The room is against Meera ===
                // ====================================================

                // 1. WET FLOOR: Meera slips, slides across the puddle and catches herself on the sink!
                if (isFloorWet && meeraAvatar != null)
                {
                    if (meeraSlippingSprite != null)
                    {
                        meeraAvatar.sprite = meeraSlippingSprite;
                        meeraAvatar.preserveAspect = true;
                    }

                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Slip");
                        U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_MEE_2"); // Startled gasp
                    }

                    // Fast comic slide & tilt from wet puddle into the sink
                    Vector2 slipStart = meeraAvatar.rectTransform.anchoredPosition;
                    Vector2 slipEnd = new Vector2(-75f, -50f);

                    for (float t = 0; t < 1f; t += Time.deltaTime * 3.8f)
                    {
                        float curve = Mathf.Sin(t * Mathf.PI);
                        meeraAvatar.rectTransform.anchoredPosition = Vector2.Lerp(slipStart, slipEnd, t);
                        meeraAvatar.transform.localRotation = Quaternion.Euler(0, 0, -22f * curve);
                        yield return null;
                    }
                    meeraAvatar.rectTransform.anchoredPosition = slipEnd;
                    meeraAvatar.transform.localRotation = Quaternion.identity;

                    if (promptText != null) promptText.text = "Oops! Meera slipped on the wet floor!";
                    yield return new WaitForSeconds(1.6f);
                }

                // 2. TAP LEFT RUNNING: Tap squeaks shut and water stops
                if (isTapRunning)
                {
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.StopAmbience();
                        U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_TapOff");
                    }
                    if (runningWaterStreamObject != null) runningWaterStreamObject.SetActive(false);
                    yield return new WaitForSeconds(0.6f);
                }

                // 3. SINK SPLASHED: Meera looks angry/unimpressed at the splashed sink left behind
                if (sinkImage != null && sinkSplashedSprite != null)
                {
                    sinkImage.sprite = sinkSplashedSprite;
                    sinkImage.gameObject.SetActive(true);
                }
                if (splashedSinkOverlay != null)
                {
                    splashedSinkOverlay.SetActive(true);
                }

                if (meeraAvatar != null && meeraUnimpressedSprite != null)
                {
                    meeraAvatar.sprite = meeraUnimpressedSprite;
                    meeraAvatar.preserveAspect = true;
                }

                if (promptText != null) promptText.text = "The sink was splashed and messy!";
                if (feedbackText != null) feedbackText.text = "Meera finds a wet, splashed washroom! Anu forgot to wipe it.";

                yield return new WaitForSeconds(1.8f);

                if (promptText != null) promptText.text = "The washroom was left untidy for the next person.";
                if (feedbackText != null) feedbackText.text = "Let's try again so Meera finds a clean washroom!";

                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_13"); // "Oh dear! Shall we try again?"
                }

                yield return new WaitForSeconds(1.2f);
                ShowChoiceButtons();
            }

            isEvaluating = false;
        }

        private void ShowChoiceButtons()
        {
            Transform cc = transform.Find("ChoiceContainer");
            if (cc != null) cc.gameObject.SetActive(true);

            if (btnTryAgain != null)
            {
                btnTryAgain.gameObject.SetActive(true);
                btnTryAgain.transform.localScale = Vector3.one;
                var rt = btnTryAgain.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(-240f, 0f);
                btnTryAgain.interactable = true;
            }

            if (btnProceedToPart4 != null)
            {
                btnProceedToPart4.gameObject.SetActive(true);
                btnProceedToPart4.transform.localScale = Vector3.one;
                var rt = btnProceedToPart4.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(240f, 0f);
                btnProceedToPart4.interactable = true;
            }
        }

        private void OnTryAgainClicked()
        {
            if (isEvaluating) return;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayButtonClick();
            }
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ResetUnitState();
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part2_Inside);
            }
        }

        private void OnProceedToPart4Clicked()
        {
            if (isEvaluating) return;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayButtonClick();
            }
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part4_EmptySoap);
            }
        }
    }
}
