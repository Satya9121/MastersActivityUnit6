using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public class U8_Part4_EmptySoapController_Masters_Activity : MonoBehaviour
    {
        [Header("UI Prompts & Feedback")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Choice Buttons")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private Button btnTellTeacher;
        [SerializeField] private Button btnJustLeave;
        [SerializeField] private Button btnFinishUnit;

        [Header("Visual Elements")]
        [SerializeField] private Image meeraAvatar;
        [SerializeField] private Sprite meeraPumpingSoapSprite;
        [SerializeField] private Sprite meeraShruggingSprite;
        [SerializeField] private Sprite meeraHappySprite;
        [SerializeField] private Image soapDispenserImage;
        [SerializeField] private Sprite soapEmptySprite;
        [SerializeField] private Sprite soapFullRefilledSprite;
        [SerializeField] private GameObject otherChildrenUsingSoapOverlay;

        private bool isProcessingChoice = false;

        [SerializeField] private Sprite[] meeraWalkSprites = new Sprite[4];

        public void EnsureMeeraSpritesLoaded()
        {
#if UNITY_EDITOR
            if (meeraWalkSprites == null || meeraWalkSprites.Length < 4 || meeraWalkSprites[0] == null)
            {
                meeraWalkSprites = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    meeraWalkSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_{i + 1}.png");
                }
            }
#endif
        }

        private void Awake()
        {
            AutoFindUIReferences();
            EnsureMeeraSpritesLoaded();

            if (btnTellTeacher != null) btnTellTeacher.onClick.AddListener(OnTellTeacherClicked);
            if (btnJustLeave != null) btnJustLeave.onClick.AddListener(OnJustLeaveClicked);
            if (btnFinishUnit != null) btnFinishUnit.onClick.AddListener(OnFinishUnitClicked);
        }

        private void OnEnable()
        {
            AutoFindUIReferences();
            EnsureMeeraSpritesLoaded();
            StartCoroutine(InitSoapEmptyRoutine());
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

            if (choiceContainer == null)
            {
                Transform t = transform.Find("ChoiceContainer") ?? transform.Find("Choices") ?? transform.Find("ButtonsContainer");
                if (t != null) choiceContainer = t.gameObject;
            }
            if (btnTellTeacher == null)
            {
                Transform t = transform.Find("ChoiceContainer/Btn_TellTeacher") ?? transform.Find("Btn_TellTeacher");
                if (t != null) btnTellTeacher = t.GetComponent<Button>();
                if (choiceContainer == null && btnTellTeacher != null && btnTellTeacher.transform.parent != transform)
                {
                    choiceContainer = btnTellTeacher.transform.parent.gameObject;
                }
            }
            if (btnJustLeave == null)
            {
                Transform t = transform.Find("ChoiceContainer/Btn_JustLeave") ?? transform.Find("Btn_JustLeave");
                if (t != null) btnJustLeave = t.GetComponent<Button>();
            }
            if (btnFinishUnit == null)
            {
                var allButtons = GetComponentsInChildren<Button>(true);
                foreach (var b in allButtons)
                {
                    if (b != null && (b.name.IndexOf("Finish", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      b.name.IndexOf("Proceed", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      b.name.IndexOf("Next", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      b.name.IndexOf("Done", System.StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        btnFinishUnit = b;
                        break;
                    }
                }
            }
            if (meeraAvatar == null)
            {
                Transform t = transform.Find("MeeraAvatar") ?? transform.Find("Meera_Image");
                if (t != null) meeraAvatar = t.GetComponent<Image>();
            }
            if (meeraAvatar != null)
            {
                meeraAvatar.preserveAspect = true;
                meeraAvatar.rectTransform.localScale = Vector3.one;
                meeraAvatar.rectTransform.localRotation = Quaternion.identity;
                meeraAvatar.rectTransform.sizeDelta = new Vector2(580f, 754f);
                if (meeraPumpingSoapSprite != null && meeraAvatar.sprite == null) meeraAvatar.sprite = meeraPumpingSoapSprite;
            }
            if (soapDispenserImage == null)
            {
                Transform t = transform.Find("SoapDispenserImage") ?? transform.Find("SoapDispenser") ?? transform.Find("Soap");
                if (t != null) soapDispenserImage = t.GetComponent<Image>();
            }
        }

        private IEnumerator InitSoapEmptyRoutine()
        {
            isProcessingChoice = false;
            if (choiceContainer != null) choiceContainer.SetActive(true);
            if (btnTellTeacher != null) btnTellTeacher.gameObject.SetActive(true);
            if (btnJustLeave != null) btnJustLeave.gameObject.SetActive(true);
            if (btnFinishUnit != null) btnFinishUnit.gameObject.SetActive(false);
            if (otherChildrenUsingSoapOverlay != null) otherChildrenUsingSoapOverlay.SetActive(false);
            if (soapDispenserImage != null && soapEmptySprite != null)
            {
                soapDispenserImage.sprite = soapEmptySprite;
                soapDispenserImage.gameObject.SetActive(true);
            }

            if (meeraAvatar != null)
            {
                meeraAvatar.preserveAspect = true;
                meeraAvatar.rectTransform.localScale = Vector3.one;
                meeraAvatar.rectTransform.localRotation = Quaternion.identity;
                meeraAvatar.rectTransform.sizeDelta = new Vector2(580f, 754f);
                meeraAvatar.rectTransform.anchoredPosition = new Vector2(160f, -50f);
                if (meeraPumpingSoapSprite != null) meeraAvatar.sprite = meeraPumpingSoapSprite;
            }
            if (promptText != null) promptText.text = "Meera presses the soap dispenser...";

            yield return new WaitForSeconds(0.6f);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_SoapPump");
                yield return new WaitForSeconds(0.3f);
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_SoapEmpty");
            }

            if (feedbackText != null) feedbackText.text = "Nothing comes out! The soap is finished.";

            yield return new WaitForSeconds(1.2f);

            if (promptText != null) promptText.text = "The soap is finished. What should Meera do?";
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_14"); // "The soap is finished. What should Meera do?"
            }

            SetButtonsInteractable(true);
        }

        private void SetButtonsInteractable(bool state)
        {
            if (btnTellTeacher != null) btnTellTeacher.interactable = state;
            if (btnJustLeave != null) btnJustLeave.interactable = state;
        }

        private void OnTellTeacherClicked()
        {
            if (isProcessingChoice) return;
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            // Immediately hide choice buttons so they never remain visible under the transition
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (btnTellTeacher != null) btnTellTeacher.gameObject.SetActive(false);
            if (btnJustLeave != null) btnJustLeave.gameObject.SetActive(false);

            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.BlinkTransition(
                    onPeakDarkness: () =>
                    {
                        ApplyCompletedResultState();
                    },
                    customFadeDur: 0.7f,
                    customHoldDur: 0.35f,
                    onComplete: () =>
                    {
                        OnTransitionFinished();
                    }
                );
            }
            else
            {
                ApplyCompletedResultState();
                OnTransitionFinished();
            }
        }

        private void ApplyCompletedResultState()
        {
            // Ensure choices and overlays are hidden
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (btnTellTeacher != null) btnTellTeacher.gameObject.SetActive(false);
            if (btnJustLeave != null) btnJustLeave.gameObject.SetActive(false);
            if (otherChildrenUsingSoapOverlay != null) otherChildrenUsingSoapOverlay.SetActive(false);

            // 1. Refill soap dispenser with green soap
            if (soapDispenserImage != null)
            {
                if (soapFullRefilledSprite != null) soapDispenserImage.sprite = soapFullRefilledSprite;
                soapDispenserImage.gameObject.SetActive(true);
            }

            // 2. Meera in happy pose in correct position & standard size
            if (meeraAvatar != null)
            {
                if (meeraHappySprite != null) meeraAvatar.sprite = meeraHappySprite;
                meeraAvatar.preserveAspect = true;
                meeraAvatar.rectTransform.localScale = Vector3.one;
                meeraAvatar.rectTransform.localRotation = Quaternion.identity;
                meeraAvatar.rectTransform.sizeDelta = new Vector2(580f, 754f);
                meeraAvatar.rectTransform.anchoredPosition = new Vector2(160f, -50f);
                meeraAvatar.gameObject.SetActive(true);
            }

            // 3. Feedback displays completed message
            if (feedbackText != null)
            {
                feedbackText.text = "The soap is refilled full! Other children can wash their hands cleanly. Star 3 Earned!";
            }
        }

        private void OnTransitionFinished()
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Sparkle");
            }

            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.AwardStar();
            }

            if (btnFinishUnit != null)
            {
                btnFinishUnit.gameObject.SetActive(true);
                btnFinishUnit.transform.SetAsLastSibling();
                btnFinishUnit.interactable = true;
                btnFinishUnit.onClick.RemoveListener(OnFinishUnitClicked);
                btnFinishUnit.onClick.AddListener(OnFinishUnitClicked);
            }
        }

        private void OnJustLeaveClicked()
        {
            if (isProcessingChoice) return;
            StartCoroutine(JustLeaveRoutine());
        }

        private IEnumerator JustLeaveRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            if (meeraAvatar != null && meeraShruggingSprite != null)
            {
                meeraAvatar.sprite = meeraShruggingSprite;
                meeraAvatar.preserveAspect = true;
            }
            if (feedbackText != null) feedbackText.text = "Meera shrugs and walks away...";

            yield return new WaitForSeconds(1.2f);

            // Meera turns and walks away
            Vector2 origPos = meeraAvatar != null ? meeraAvatar.rectTransform.anchoredPosition : Vector2.zero;
            if (meeraAvatar != null)
            {
                Vector2 walkTarget = new Vector2(origPos.x - 220f, origPos.y);
                float curX = origPos.x;
                float walkTimer = 0f;
                float stepTimer = 0f;
                int stepIdx = 0;
                while (curX > walkTarget.x)
                {
                    walkTimer += Time.deltaTime;
                    stepTimer += Time.deltaTime;
                    curX = Mathf.MoveTowards(curX, walkTarget.x, 200f * Time.deltaTime);
                    float bobY = Mathf.Sin(walkTimer * 18f) * 1.5f;
                    meeraAvatar.rectTransform.anchoredPosition = new Vector2(curX, origPos.y + bobY);

                    if (stepTimer >= 0.12f)
                    {
                        stepTimer = 0f;
                        stepIdx = (stepIdx + 1) % 4;
                        if (meeraWalkSprites != null && stepIdx < meeraWalkSprites.Length && meeraWalkSprites[stepIdx] != null)
                        {
                            meeraAvatar.sprite = meeraWalkSprites[stepIdx];
                        }
                    }
                    yield return null;
                }
            }

            if (feedbackText != null) feedbackText.text = "Three more children try the empty soap and leave in frustration.";

            yield return new WaitForSeconds(2.5f);

            if (feedbackText != null) feedbackText.text = "If we tell a teacher, it fixes the problem for everybody!";
            yield return new WaitForSeconds(1.5f);

            // Reset back
            if (meeraAvatar != null)
            {
                meeraAvatar.rectTransform.anchoredPosition = origPos;
                if (meeraPumpingSoapSprite != null) meeraAvatar.sprite = meeraPumpingSoapSprite;
            }

            isProcessingChoice = false;
            SetButtonsInteractable(true);
        }

        private void OnFinishUnitClicked()
        {
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Ending);
            }
        }
    }
}
