using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public class U8_Part2_WashroomInsideController_Masters_Activity : MonoBehaviour
    {
        [Header("UI & Step Tracking")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button btnNextStep;
        [SerializeField] private Button btnProceedToPart3;

        [Header("1. Cubicle (Left)")]
        [SerializeField] private Image cubicleDoorImage;
        [SerializeField] private Sprite cubicleOpenSprite;
        [SerializeField] private Sprite cubicleClosedSprite;
        [SerializeField] private Button btnCloseCubicle;
        [SerializeField] private Button btnFlush;
        [SerializeField] private Button btnAnuExitCubicle;

        [Header("2. Sink, Tap & Soap (Center)")]
        [SerializeField] private Image sinkImage;
        [SerializeField] private Sprite sinkCleanSprite;
        [SerializeField] private Sprite sinkSplashedSprite;
        [SerializeField] private Image tapWaterStream;
        [SerializeField] private Button btnWashHands;
        [SerializeField] private Button btnTurnTapOff;
        [SerializeField] private Button btnWipeSink;

        [Header("3. Paper Towels & Bin (Right)")]
        [SerializeField] private Button btnPaperTowel;
        [SerializeField] private Image towelInAirImage;
        [SerializeField] private Image soggyTowelOnFloorImage;
        [SerializeField] private Image binImage;

        [Header("Anu Character Avatar")]
        [SerializeField] private Image anuInsideAvatar;
        [SerializeField] private Sprite anuNormalSprite;
        [SerializeField] private Sprite anuWashingSprite;
        [SerializeField] private Sprite anuWipingSprite;
        [SerializeField] private Sprite[] anuWalkSprites = new Sprite[8];

        private int currentStepIndex = 1;
        private Vector2 anuEditorPosition = new Vector2(-240f, -80f);
        private Vector2 anuCubiclePosition = new Vector2(-710f, -80f);
        private Vector2 towelInAirEditorPos;
        private bool positionsCached = false;

        private void Awake()
        {
            EnsureAnuSpritesLoaded();
            AutoFindUIReferences();
            CacheEditorPositions();
            RegisterListeners();
        }

        public void EnsureAnuSpritesLoaded()
        {
#if UNITY_EDITOR
            if (anuNormalSprite == null)
            {
                anuNormalSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Normal.png");
            }
            anuWipingSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/crop_wipe.png");
            if (cubicleOpenSprite == null)
            {
                cubicleOpenSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Cubicle_Open.png");
            }
            if (cubicleClosedSprite == null)
            {
                cubicleClosedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Cubicle_Closed.png");
            }
            if (sinkCleanSprite == null)
            {
                sinkCleanSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Sink_Clean.png");
            }
            if (sinkSplashedSprite == null)
            {
                sinkSplashedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Sink_Splashed.png");
            }
            EnsureAnuWalkSpritesLoaded();
#endif
        }

        public void EnsureAnuWalkSpritesLoaded()
        {
#if UNITY_EDITOR
            if (anuWalkSprites == null || anuWalkSprites.Length < 8 || anuWalkSprites[0] == null)
            {
                anuWalkSprites = new Sprite[8];
                for (int i = 0; i < 8; i++)
                {
                    anuWalkSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_Right_{i + 1}.png");
                }
            }
#endif
        }

        private void CacheEditorPositions()
        {
            if (positionsCached) return;

            if (anuInsideAvatar != null)
            {
                anuEditorPosition = anuInsideAvatar.rectTransform.anchoredPosition;
                if (anuEditorPosition.x < -400f)
                {
                    anuEditorPosition = new Vector2(-240f, -80f);
                }
                
                // Position Anu inside the cubicle stall (centered near x = -710f, in front of the toilet)
                anuCubiclePosition = new Vector2(-710f, anuEditorPosition.y);
            }

            if (towelInAirImage != null)
            {
                towelInAirEditorPos = towelInAirImage.rectTransform.anchoredPosition;
            }

            positionsCached = true;
        }

        private void OnEnable()
        {
            EnsureAnuSpritesLoaded();
            AutoFindUIReferences();
            RegisterListeners();
            CacheEditorPositions();

            // If returning from Handwash Screen, continue from Step 5
            if (U8_GameManager_Masters_Activity.Instance != null && U8_GameManager_Masters_Activity.Instance.isHandsWashed)
            {
                currentStepIndex = 5;
                if (anuInsideAvatar != null)
                {
                    anuInsideAvatar.gameObject.SetActive(true);
                    anuInsideAvatar.rectTransform.anchoredPosition = anuEditorPosition;
                    anuInsideAvatar.rectTransform.localScale = Vector3.one;
                    anuInsideAvatar.rectTransform.sizeDelta = new Vector2(250f, 650f);
                    if (anuNormalSprite != null) anuInsideAvatar.sprite = anuNormalSprite;
                }
                if (tapWaterStream != null) tapWaterStream.gameObject.SetActive(true);
                if (sinkImage != null && sinkSplashedSprite != null) sinkImage.sprite = sinkSplashedSprite;

                UpdateStepVisuals();
                if (feedbackText != null) feedbackText.text = "Hands clean! Now let's turn off the tap.";
                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlayAmbience("SFX_TapOn", true);
                    U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_09"); // "Turn the tap off."
                }
            }
            else
            {
                ResetWashroomState();
                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_04"); // "Now, what next?"
                }
            }
        }

        private Coroutine activeBtnPulseCoroutine;
        private readonly System.Collections.Generic.Dictionary<Button, string> originalButtonLabels = new System.Collections.Generic.Dictionary<Button, string>();

        private void OnDisable()
        {
            if (activeBtnPulseCoroutine != null)
            {
                StopCoroutine(activeBtnPulseCoroutine);
                activeBtnPulseCoroutine = null;
            }
            ResetAllButtonScales();
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.StopAmbience();
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
            if (feedbackText == null)
            {
                Transform t = transform.Find("FeedbackCard/FeedbackText") ?? transform.Find("FeedbackText");
                if (t != null) feedbackText = t.GetComponent<TextMeshProUGUI>();
            }

            btnProceedToPart3 = FindButtonByKeywords("Proceed", "Next", "Leave", "Meera");

            // Strictly find and assign buttons 1 through 7 dynamically
            btnCloseCubicle = FindStepButton(1, "CloseDoor", "CloseCubicle", "Close", "Door");
            btnFlush = FindStepButton(2, "Flush", "Toilet");
            btnAnuExitCubicle = FindStepButton(3, "StepOut", "ComeOut", "Exit");
            btnWashHands = FindStepButton(4, "WashHands", "Wash", "Soap");
            btnTurnTapOff = FindStepButton(5, "TurnTapOff", "TapOff", "TurnOff", "Tap");
            btnPaperTowel = FindStepButton(6, "PaperTowel", "Towel", "Bin");
            btnWipeSink = FindStepButton(7, "WipeSink", "Wipe", "Dry");

            Debug.Log($"[U8_Part2] Bound Steps: 1={btnCloseCubicle?.name}, 2={btnFlush?.name}, 3={btnAnuExitCubicle?.name}, 4={btnWashHands?.name}, 5={btnTurnTapOff?.name}, 6={btnPaperTowel?.name}, 7={btnWipeSink?.name}");

            if (cubicleDoorImage == null) cubicleDoorImage = FindImageByKeywords("CubicleDoor", "DoorImage", "Cubicle");
            if (sinkImage == null) sinkImage = FindImageByKeywords("SinkImage", "Sink");
            if (tapWaterStream == null) tapWaterStream = FindImageByKeywords("TapWaterStream", "WaterStream", "Tap");
            if (towelInAirImage == null) towelInAirImage = FindImageByKeywords("TowelInAir");
            if (binImage == null) binImage = FindImageByKeywords("TrashBin", "BinImage", "Bin");
            if (soggyTowelOnFloorImage == null) soggyTowelOnFloorImage = FindImageByKeywords("SoggyTowel", "TowelOnFloor");
            if (anuInsideAvatar == null) anuInsideAvatar = FindImageByKeywords("AnuInsideAvatar", "AnuAvatar", "Anu");
            if (anuInsideAvatar != null)
            {
                anuInsideAvatar.preserveAspect = true;
                if (anuNormalSprite != null && anuInsideAvatar.sprite == null) anuInsideAvatar.sprite = anuNormalSprite;
            }
        }

        private Button FindStepButton(int stepNumber, params string[] keywords)
        {
            var buttons = GetComponentsInChildren<Button>(true);

            // 1. First priority: Check child TMP text content starting with step number (e.g. "1. Close Door", "1. ", "1.")
            foreach (var b in buttons)
            {
                if (b == null || b == btnProceedToPart3) continue;
                if (b.name.IndexOf("Proceed", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    b.name.IndexOf("Next", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;

                var tmp = b.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    string txt = tmp.text.Trim();
                    if (txt.StartsWith($"{stepNumber}.") || txt.StartsWith($"{stepNumber} ") || 
                        txt.StartsWith($"Step {stepNumber}") || txt.StartsWith($"Step{stepNumber}"))
                    {
                        return b;
                    }
                }
            }

            // 2. Second priority: Check GameObject name starting with step number (e.g. "1. Close Door", "Btn_1", "Btn1", "Step_1")
            foreach (var b in buttons)
            {
                if (b == null || b == btnProceedToPart3) continue;
                if (b.name.IndexOf("Proceed", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    b.name.IndexOf("Next", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;

                string n = b.name.Trim();
                if (n.StartsWith($"{stepNumber}.") || n.StartsWith($"{stepNumber} ") || 
                    n.StartsWith($"Btn_{stepNumber}") || n.StartsWith($"Btn{stepNumber}") || 
                    n.StartsWith($"Step_{stepNumber}") || n.StartsWith($"Step{stepNumber}"))
                {
                    return b;
                }
            }

            // 3. Third priority: Specific keyword matching
            foreach (var kw in keywords)
            {
                foreach (var b in buttons)
                {
                    if (b == null || b == btnProceedToPart3) continue;
                    if (b.name.IndexOf("Proceed", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        b.name.IndexOf("Next", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;

                    if (b.name.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return b;

                    var tmp = b.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null && tmp.text.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return b;
                }
            }

            return null;
        }

        private Button FindButtonByKeywords(params string[] keywords)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var kw in keywords)
            {
                foreach (var b in buttons)
                {
                    if (b != null && b.name.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return b;
                }
            }
            return null;
        }

        private Image FindImageByKeywords(params string[] keywords)
        {
            var images = GetComponentsInChildren<Image>(true);
            foreach (var kw in keywords)
            {
                foreach (var img in images)
                {
                    if (img != null && img.name.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return img;
                }
            }
            return null;
        }

        private void RegisterListeners()
        {
            if (btnCloseCubicle != null)
            {
                btnCloseCubicle.onClick.RemoveListener(OnCloseCubicleClicked);
                btnCloseCubicle.onClick.AddListener(OnCloseCubicleClicked);
            }
            if (btnFlush != null)
            {
                btnFlush.onClick.RemoveListener(OnFlushClicked);
                btnFlush.onClick.AddListener(OnFlushClicked);
            }
            if (btnAnuExitCubicle != null)
            {
                btnAnuExitCubicle.onClick.RemoveListener(OnAnuExitCubicleClicked);
                btnAnuExitCubicle.onClick.AddListener(OnAnuExitCubicleClicked);
            }
            if (btnWashHands != null)
            {
                btnWashHands.onClick.RemoveListener(OnWashHandsClicked);
                btnWashHands.onClick.AddListener(OnWashHandsClicked);
            }
            if (btnTurnTapOff != null)
            {
                btnTurnTapOff.onClick.RemoveListener(OnTurnTapOffClicked);
                btnTurnTapOff.onClick.AddListener(OnTurnTapOffClicked);
            }
            if (btnPaperTowel != null)
            {
                btnPaperTowel.onClick.RemoveListener(OnPaperTowelClicked);
                btnPaperTowel.onClick.AddListener(OnPaperTowelClicked);
            }
            if (btnWipeSink != null)
            {
                btnWipeSink.onClick.RemoveListener(OnWipeSinkClicked);
                btnWipeSink.onClick.AddListener(OnWipeSinkClicked);
            }
            if (btnProceedToPart3 != null)
            {
                btnProceedToPart3.onClick.RemoveListener(OnProceedToPart3Clicked);
                btnProceedToPart3.onClick.AddListener(OnProceedToPart3Clicked);
            }
        }

        public void ResetWashroomState()
        {
            CacheEditorPositions();
            currentStepIndex = 1;

            if (cubicleDoorImage != null && cubicleOpenSprite != null) cubicleDoorImage.sprite = cubicleOpenSprite;
            
            // Tap is OFF initially: only turns on when "4. Wash Hands" is clicked
            if (tapWaterStream != null) tapWaterStream.gameObject.SetActive(false);
            if (sinkImage != null && sinkCleanSprite != null) sinkImage.sprite = sinkCleanSprite;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.StopAmbience();
            }

            if (soggyTowelOnFloorImage != null) soggyTowelOnFloorImage.gameObject.SetActive(false);
            if (towelInAirImage != null)
            {
                towelInAirImage.gameObject.SetActive(false);
                towelInAirImage.rectTransform.anchoredPosition = towelInAirEditorPos;
            }
            if (btnProceedToPart3 != null) btnProceedToPart3.gameObject.SetActive(false);
            
            // Anu is visible inside the washroom stall when Part 2 starts!
            if (anuInsideAvatar != null)
            {
                anuInsideAvatar.gameObject.SetActive(true);
                anuInsideAvatar.rectTransform.anchoredPosition = anuCubiclePosition;
                anuInsideAvatar.rectTransform.localScale = Vector3.one;
                anuInsideAvatar.rectTransform.sizeDelta = new Vector2(520f, 650f);
                if (anuNormalSprite != null) anuInsideAvatar.sprite = anuNormalSprite;
                anuInsideAvatar.preserveAspect = true;

                // Place Anu in front of the cubicle background so she is fully visible standing inside the cubicle stall!
                Transform cubicleT = cubicleDoorImage != null ? cubicleDoorImage.transform.parent : transform.Find("Cubicle");
                if (cubicleT != null)
                {
                    anuInsideAvatar.transform.SetSiblingIndex(cubicleT.GetSiblingIndex() + 1);
                }
                else
                {
                    Transform promptCard = transform.Find("PromptCard") ?? transform.Find("Prompt");
                    if (promptCard != null)
                    {
                        anuInsideAvatar.transform.SetSiblingIndex(promptCard.GetSiblingIndex());
                    }
                    else
                    {
                        anuInsideAvatar.transform.SetAsLastSibling();
                    }
                }
            }

            UpdateStepVisuals();
        }

        private void UpdateStepVisuals()
        {
            if (activeBtnPulseCoroutine != null)
            {
                StopCoroutine(activeBtnPulseCoroutine);
                activeBtnPulseCoroutine = null;
            }
            ResetAllButtonScales();

            ApplyStepButtonState(btnCloseCubicle, 1, "Close Door");
            ApplyStepButtonState(btnFlush, 2, "Flush");
            ApplyStepButtonState(btnAnuExitCubicle, 3, "Step Out");
            ApplyStepButtonState(btnWashHands, 4, "Wash Hands");
            ApplyStepButtonState(btnTurnTapOff, 5, "Turn Off Tap");
            ApplyStepButtonState(btnPaperTowel, 6, "Bin Paper Towel");
            ApplyStepButtonState(btnWipeSink, 7, "Wipe Sink");

            Button currentActiveBtn = GetButtonForStep(currentStepIndex);
            if (currentActiveBtn != null && gameObject.activeInHierarchy)
            {
                activeBtnPulseCoroutine = StartCoroutine(PulseActiveButton(currentActiveBtn.transform));
            }

            if (btnProceedToPart3 != null)
            {
                btnProceedToPart3.gameObject.SetActive(currentStepIndex >= 4);
                var btnText = btnProceedToPart3.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = (currentStepIndex >= 8) ? "See What Meera Finds" : "Leave Washroom";
                }
            }

            switch (currentStepIndex)
            {
                case 1:
                    if (promptText != null) promptText.text = "Step 1: Close the cubicle door first!";
                    break;
                case 2:
                    if (promptText != null) promptText.text = "Step 2: Flush the toilet.";
                    break;
                case 3:
                    if (promptText != null) promptText.text = "Step 3: Step out to the wash basin.";
                    break;
                case 4:
                    if (promptText != null) promptText.text = "Step 4: Wash hands with soap and water!";
                    break;
                case 5:
                    if (promptText != null) promptText.text = "Step 5: Turn the tap off properly.";
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlayAmbience("SFX_TapOn", true);
                    }
                    break;
                case 6:
                    if (promptText != null) promptText.text = "Step 6: Put the used paper towel in the bin.";
                    break;
                case 7:
                    if (promptText != null) promptText.text = "Step 7: Wipe off the sink for the next person.";
                    break;
                case 8:
                    if (promptText != null) promptText.text = "All set! Let's see how the washroom is left for Meera.";
                    break;
            }
        }

        private Button GetButtonForStep(int step)
        {
            switch (step)
            {
                case 1: return btnCloseCubicle;
                case 2: return btnFlush;
                case 3: return btnAnuExitCubicle;
                case 4: return btnWashHands;
                case 5: return btnTurnTapOff;
                case 6: return btnPaperTowel;
                case 7: return btnWipeSink;
                default: return null;
            }
        }

        private string GetOriginalLabel(Button btn, string fallback)
        {
            if (btn == null) return fallback;
            if (originalButtonLabels.TryGetValue(btn, out string cached)) return cached;

            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            string raw = (text != null) ? text.text : fallback;
            raw = raw.Replace("✓", "").Replace("👉", "").Replace("□", "").Trim();
            if (string.IsNullOrEmpty(raw)) raw = fallback;

            originalButtonLabels[btn] = raw;
            return raw;
        }

        private void ApplyStepButtonState(Button btn, int stepNumber, string defaultLabel)
        {
            if (btn == null) return;

            var cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();

            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            string label = GetOriginalLabel(btn, defaultLabel);
            if (text != null) text.text = label;

            // Keep all buttons 100% fully visible without dimming / greying out
            cg.alpha = 1.0f;

            bool isActiveStep = (currentStepIndex == stepNumber);

            // Toggle interactability and raycasts so only the current step can be clicked
            btn.interactable = isActiveStep;
            cg.blocksRaycasts = isActiveStep;

            // Ensure Unity's disabled button tint doesn't darken the graphic
            var colors = btn.colors;
            if (colors.disabledColor != colors.normalColor)
            {
                colors.disabledColor = colors.normalColor;
                btn.colors = colors;
            }

            if (!isActiveStep)
            {
                btn.transform.localScale = Vector3.one;
            }
        }

        private void ResetAllButtonScales()
        {
            Button[] all = { btnCloseCubicle, btnFlush, btnAnuExitCubicle, btnWashHands, btnTurnTapOff, btnPaperTowel, btnWipeSink };
            foreach (var b in all)
            {
                if (b != null) b.transform.localScale = Vector3.one;
            }
        }

        private IEnumerator PulseActiveButton(Transform btnTransform)
        {
            if (btnTransform == null) yield break;

            // Punchy Pop Entry
            float t = 0f;
            while (t < 0.16f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.0f, 1.14f, t / 0.16f);
                if (btnTransform != null) btnTransform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            t = 0f;
            while (t < 0.12f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.14f, 1.04f, t / 0.12f);
                if (btnTransform != null) btnTransform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            // Gentle rhythmic breathing
            while (true)
            {
                float scale = 1.04f + Mathf.Sin(Time.time * 4.5f) * 0.035f;
                if (btnTransform != null)
                {
                    btnTransform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }
        }

        public void OnCloseCubicleClicked()
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BoltClick");
            }
            if (cubicleDoorImage != null && cubicleClosedSprite != null) cubicleDoorImage.sprite = cubicleClosedSprite;
            if (U8_GameManager_Masters_Activity.Instance != null) U8_GameManager_Masters_Activity.Instance.isCubicleClosedFirst = true;

            // Anu is inside the closed cubicle stall, covered by the closed door
            if (anuInsideAvatar != null)
            {
                anuInsideAvatar.gameObject.SetActive(false);
            }

            currentStepIndex = 2;
            UpdateStepVisuals();
            if (feedbackText != null) feedbackText.text = "Door bolted safely! Remember to flush.";
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_05"); // "Do not forget to flush."
            }
        }

        public void OnFlushClicked()
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Flush");
            }
            if (U8_GameManager_Masters_Activity.Instance != null) U8_GameManager_Masters_Activity.Instance.isFlushed = true;

            currentStepIndex = 3;
            UpdateStepVisuals();
            if (feedbackText != null) feedbackText.text = "Flushed cleanly! Now step out to wash hands.";
        }

        public void OnAnuExitCubicleClicked()
        {
            Debug.Log("[U8_Part2] Step 3 Clicked: Anu stepping out of cubicle to sink.");
            StartCoroutine(AnuExitCubicleRoutine());
        }

        private IEnumerator AnuExitCubicleRoutine()
        {
            if (cubicleDoorImage != null && cubicleOpenSprite != null)
            {
                cubicleDoorImage.sprite = cubicleOpenSprite;
            }
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BoltClick");
            }

            if (anuInsideAvatar != null)
            {
                EnsureAnuWalkSpritesLoaded();
                anuInsideAvatar.gameObject.SetActive(true);
                anuInsideAvatar.preserveAspect = true;

                // Bring Anu forward so she walks in front into the room
                Transform promptCard = transform.Find("PromptCard") ?? transform.Find("Prompt");
                if (promptCard != null)
                {
                    anuInsideAvatar.transform.SetSiblingIndex(promptCard.GetSiblingIndex());
                }
                else
                {
                    anuInsideAvatar.transform.SetAsLastSibling();
                }

                // Native right-facing walk frames
                anuInsideAvatar.rectTransform.localScale = Vector3.one;
                anuInsideAvatar.rectTransform.sizeDelta = new Vector2(520f, 650f);

                Vector2 startPos = anuCubiclePosition;
                Vector2 sinkPos = anuEditorPosition;
                anuInsideAvatar.rectTransform.anchoredPosition = startPos;

                yield return new WaitForSeconds(0.25f);

                // Smooth walking movement from cubicle to sink
                float currentX = startPos.x;
                float targetX = sinkPos.x;
                float speed = 210f;
                float timer = 0f;
                float stepTimer = 0f;
                int stepIdx = 0;

                while (currentX < targetX)
                {
                    timer += Time.deltaTime;
                    stepTimer += Time.deltaTime;

                    currentX += speed * Time.deltaTime;
                    if (currentX > targetX) currentX = targetX;

                    float bobY = Mathf.Sin(timer * 16f) * 2f;
                    anuInsideAvatar.rectTransform.anchoredPosition = new Vector2(currentX, sinkPos.y + bobY);

                    if (stepTimer >= 0.14f)
                    {
                        stepTimer = 0f;
                        stepIdx = (stepIdx + 1) % 4;
                        if (anuWalkSprites != null && anuWalkSprites.Length >= 4 && anuWalkSprites[stepIdx] != null)
                        {
                            anuInsideAvatar.sprite = anuWalkSprites[stepIdx];
                        }
                    }

                    yield return null;
                }

                // Arrived at the sink: face front/normal, polite standing posture (NO washing hands on Screen 2!)
                anuInsideAvatar.rectTransform.anchoredPosition = sinkPos;
                anuInsideAvatar.rectTransform.localScale = Vector3.one;
                anuInsideAvatar.rectTransform.sizeDelta = new Vector2(520f, 650f);
                if (anuNormalSprite != null)
                {
                    anuInsideAvatar.sprite = anuNormalSprite;
                }
            }

            currentStepIndex = 4;
            UpdateStepVisuals();
            if (feedbackText != null) feedbackText.text = "At the sink! Click '4. Wash Hands' to wash with soap and water.";

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_06"); // "Now wash your hands. Keep tapping!"
            }
        }

        public void OnWashHandsClicked()
        {
            if (currentStepIndex != 4) return;
            Debug.Log("[U8_Part2] Step 4 Clicked: Turning tap on and opening Handwash Screen smoothly.");
            StartCoroutine(WashHandsTransitionRoutine());
        }

        private IEnumerator WashHandsTransitionRoutine()
        {
            // 1. Turn the tap ON: water stream starts and sound plays
            if (tapWaterStream != null) tapWaterStream.gameObject.SetActive(true);
            if (sinkImage != null && sinkSplashedSprite != null) sinkImage.sprite = sinkSplashedSprite;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_TapOn");
                U8_AudioManager_Masters_Activity.Instance.PlayAmbience("SFX_TapOn", true);
            }

            if (feedbackText != null) feedbackText.text = "Washing hands with soap and water!";

            // Quick, smooth, natural pause
            yield return new WaitForSeconds(0.4f);

            // Transition smoothly to Handwash Screen
            var gm = U8_GameManager_Masters_Activity.Instance ?? FindFirstObjectByType<U8_GameManager_Masters_Activity>(FindObjectsInactive.Include);
            if (gm != null)
            {
                gm.ShowPart(U8_GamePart.HandwashScreen);
            }
        }

        public void OnTurnTapOffClicked()
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.StopAmbience();
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_TapOff");
            }
            if (tapWaterStream != null) tapWaterStream.gameObject.SetActive(false);
            if (sinkImage != null && sinkCleanSprite != null) sinkImage.sprite = sinkCleanSprite;
            if (U8_GameManager_Masters_Activity.Instance != null) U8_GameManager_Masters_Activity.Instance.isTapTurnedOff = true;

            currentStepIndex = 6;
            UpdateStepVisuals();
            if (feedbackText != null) feedbackText.text = "Water saved! Now dry hands and throw the towel in the bin.";
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_10"); // "Towel in the bin."
            }
        }

        public void OnPaperTowelClicked()
        {
            StartCoroutine(PaperTowelRoutine());
        }

        private IEnumerator PaperTowelRoutine()
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_TowelPull");
            }

            if (towelInAirImage != null)
            {
                // 1. Bring to top layer so it renders above all panels & backgrounds
                towelInAirImage.transform.SetAsLastSibling();

                // 2. Ensure full opacity & proper scale
                towelInAirImage.color = Color.white;
                towelInAirImage.transform.localScale = Vector3.one;
                towelInAirImage.gameObject.SetActive(true);

                // 3. Determine start position: throw directly from Anu's hands
                Vector2 startPos = towelInAirEditorPos;
                if (anuInsideAvatar != null && towelInAirImage.transform.parent != null)
                {
                    Vector3 worldAnuPos = anuInsideAvatar.transform.position;
                    Vector3 localAnuPos = towelInAirImage.transform.parent.InverseTransformPoint(worldAnuPos);
                    // Start from Anu's right hand / chest level
                    startPos = new Vector2(localAnuPos.x + 45f, localAnuPos.y + 15f);
                }
                else
                {
                    Transform towelHolderT = transform.Find("TowelHolder/TowelHolderImage") ?? transform.Find("TowelHolder");
                    if (towelHolderT != null && towelInAirImage.transform.parent != null)
                    {
                        Vector3 worldHolderPos = towelHolderT.position;
                        Vector3 localHolderPos = towelInAirImage.transform.parent.InverseTransformPoint(worldHolderPos);
                        startPos = new Vector2(localHolderPos.x - 20f, localHolderPos.y - 30f);
                    }
                }

                // 4. Target position: top opening of the trash bin
                Vector2 targetBinPos;
                if (binImage != null && towelInAirImage.transform.parent != null)
                {
                    Vector3 worldBinPos = binImage.transform.position;
                    Vector3 localBinPos = towelInAirImage.transform.parent.InverseTransformPoint(worldBinPos);
                    targetBinPos = new Vector2(localBinPos.x, localBinPos.y + 50f);
                }
                else
                {
                    Transform binT = transform.Find("TrashBinImage") ?? transform.Find("TrashBin") ?? transform.Find("Bin");
                    if (binT != null && towelInAirImage.transform.parent != null)
                    {
                        Vector3 worldBinPos = binT.position;
                        Vector3 localBinPos = towelInAirImage.transform.parent.InverseTransformPoint(worldBinPos);
                        targetBinPos = new Vector2(localBinPos.x, localBinPos.y + 50f);
                    }
                    else
                    {
                        targetBinPos = new Vector2(startPos.x + 260f, startPos.y - 120f);
                    }
                }

                towelInAirImage.rectTransform.anchoredPosition = startPos;

                // 5. Smooth parabolic toss animation from Anu's hands into the bin (~0.75s)
                for (float t = 0; t < 1f; t += Time.deltaTime * 1.35f)
                {
                    Vector2 currentPos = Vector2.Lerp(startPos, targetBinPos, t);
                    currentPos.y += Mathf.Sin(t * Mathf.PI) * 90f;
                    towelInAirImage.rectTransform.anchoredPosition = currentPos;
                    towelInAirImage.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, -45f, t));
                    yield return null;
                }

                towelInAirImage.gameObject.SetActive(false);
                towelInAirImage.transform.localRotation = Quaternion.identity;
                towelInAirImage.rectTransform.anchoredPosition = towelInAirEditorPos;
            }

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BinDrop");
            }

            if (U8_GameManager_Masters_Activity.Instance != null) U8_GameManager_Masters_Activity.Instance.isTowelInBin = true;

            currentStepIndex = 7;
            UpdateStepVisuals();
            if (feedbackText != null) feedbackText.text = "Towel in the bin! Final step: wipe off the sink.";
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_11"); // "Wipe the sink."
            }
        }

        public void OnWipeSinkClicked()
        {
            StartCoroutine(WipeSinkRoutine());
        }

        private IEnumerator WipeSinkRoutine()
        {
            if (anuInsideAvatar == null) yield break;

            EnsureAnuSpritesLoaded();

            Vector2 originalPos = anuInsideAvatar.rectTransform.anchoredPosition;
            // Step forward towards the real washroom sink (SinkImage) so Anu's hands with blue cloth wipe the real sink rim & bowl
            Vector2 wipePos = new Vector2(-155f, originalPos.y);

            // Smoothly step forward towards the real sink
            float walkTime = 0.35f;
            float elapsed = 0f;
            while (elapsed < walkTime)
            {
                elapsed += Time.deltaTime;
                anuInsideAvatar.rectTransform.anchoredPosition = Vector2.Lerp(originalPos, wipePos, elapsed / walkTime);
                yield return null;
            }
            anuInsideAvatar.rectTransform.anchoredPosition = wipePos;

            // Switch to clean wiping sprite (Anu holding blue wiping cloth directly on the real washroom sink)
            if (anuWipingSprite != null)
            {
                anuInsideAvatar.sprite = anuWipingSprite;
                anuInsideAvatar.preserveAspect = true;
                anuInsideAvatar.rectTransform.sizeDelta = new Vector2(300f, 650f);
            }

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Wipe");
            }

            // Scrubbing animation back and forth on the real sink rim
            float scrubDuration = 1.6f;
            float scrubTimer = 0f;
            bool secondSfxPlayed = false;

            while (scrubTimer < scrubDuration)
            {
                scrubTimer += Time.deltaTime;
                float offset = Mathf.Sin(scrubTimer * 14f) * 12f;
                anuInsideAvatar.rectTransform.anchoredPosition = new Vector2(wipePos.x + offset, wipePos.y);

                if (!secondSfxPlayed && scrubTimer >= 0.8f)
                {
                    secondSfxPlayed = true;
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Wipe");
                    }
                }

                yield return null;
            }

            anuInsideAvatar.rectTransform.anchoredPosition = wipePos;

            // Real sink sparkles and is clean!
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Sparkle");
            }

            if (sinkImage != null && sinkCleanSprite != null)
            {
                sinkImage.sprite = sinkCleanSprite;
            }
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.isSinkWiped = true;
            }

            yield return new WaitForSeconds(0.4f);

            // Switch back to normal idle pose and step back to original position
            if (anuNormalSprite != null)
            {
                anuInsideAvatar.sprite = anuNormalSprite;
                anuInsideAvatar.preserveAspect = true;
                anuInsideAvatar.rectTransform.sizeDelta = new Vector2(250f, 650f);
            }

            elapsed = 0f;
            while (elapsed < walkTime)
            {
                elapsed += Time.deltaTime;
                anuInsideAvatar.rectTransform.anchoredPosition = Vector2.Lerp(wipePos, originalPos, elapsed / walkTime);
                yield return null;
            }
            anuInsideAvatar.rectTransform.anchoredPosition = originalPos;

            currentStepIndex = 8;
            UpdateStepVisuals();
            if (feedbackText != null) feedbackText.text = "Sink wiped clean and sparkling! Ready for the next person.";

            yield return new WaitForSeconds(1.4f);

            // When all 7 options are completed, blink and open Part 3 (U8_Screen_04_Part3AfterYou)
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part3_AfterYou);
            }
        }

        public void OnProceedToPart3Clicked()
        {
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part3_AfterYou);
            }
        }
    }
}
