using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Googolplex.Unit8
{
    public class U8_Part1_DoorController_Masters_Activity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image doorImage;
        [SerializeField] private Image anuAvatar;
        [SerializeField] private GameObject shutSignObject;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Choice Buttons")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private Button btnKnockAndWait;
        [SerializeField] private Button btnBangLoudly;
        [SerializeField] private Button btnPushOpen;

        [Header("Sprites")]
        [SerializeField] private Sprite doorClosedSprite;
        [SerializeField] private Sprite doorOpenSprite;
        [SerializeField] private Sprite anuNormalSprite;
        [SerializeField] private Sprite anuKnockingSprite;
        [SerializeField] private Sprite anuSheepishSprite;

        [Header("Second Girl (Meera)")]
        [SerializeField] private Image meeraAvatar;
        [SerializeField] private Sprite[] meeraWalkSprites = new Sprite[8];
        [SerializeField] private Sprite meeraEnteringSprite;

        [Header("Audio Clips (Inspector)")]
        [SerializeField] private AudioClip voAnuNeedsWashroom;
        [SerializeField] private AudioClip voDoorClosed;
        [SerializeField] private AudioClip voKnockAndWait;
        [SerializeField] private AudioClip voJustAMinute;
        [SerializeField] private AudioClip voSorry;
        [SerializeField] private AudioClip sfxKnock;
        [SerializeField] private AudioClip sfxBangDoor;
        [SerializeField] private AudioClip sfxBoltRattle;
        [SerializeField] private AudioClip sfxBoltClick;
        [Header("Controllers")]
        [SerializeField] private U8_AnuWalkingController_Masters_Activity anuWalkingController;

        private bool isProcessingChoice = false;

        private void Awake()
        {
            AutoWireComponents();
            AutoWireAudioClips();
            EnsureMeeraSpritesLoaded();

            if (btnKnockAndWait != null) btnKnockAndWait.onClick.AddListener(OnKnockAndWaitClicked);
            if (btnBangLoudly != null) btnBangLoudly.onClick.AddListener(OnBangLoudlyClicked);
            if (btnPushOpen != null) btnPushOpen.onClick.AddListener(OnPushOpenClicked);
        }

        private void OnDestroy()
        {
            if (btnKnockAndWait != null) btnKnockAndWait.onClick.RemoveListener(OnKnockAndWaitClicked);
            if (btnBangLoudly != null) btnBangLoudly.onClick.RemoveListener(OnBangLoudlyClicked);
            if (btnPushOpen != null) btnPushOpen.onClick.RemoveListener(OnPushOpenClicked);
        }

        private void Start()
        {
            ResetScreen();
        }

        private void AutoWireComponents()
        {
            if (choiceContainer == null)
            {
                Transform cc = transform.Find("ChoiceContainer");
                if (cc != null) choiceContainer = cc.gameObject;
            }

            if (choiceContainer != null)
            {
                if (btnKnockAndWait == null)
                {
                    Transform b = choiceContainer.transform.Find("Btn_Knock") ?? choiceContainer.transform.Find("KnockAndWait");
                    if (b != null) btnKnockAndWait = b.GetComponent<Button>();
                }
                if (btnBangLoudly == null)
                {
                    Transform b = choiceContainer.transform.Find("Btn_Bang") ?? choiceContainer.transform.Find("BangLoudly");
                    if (b != null) btnBangLoudly = b.GetComponent<Button>();
                }
                if (btnPushOpen == null)
                {
                    Transform b = choiceContainer.transform.Find("Btn_Push") ?? choiceContainer.transform.Find("PushOpen");
                    if (b != null) btnPushOpen = b.GetComponent<Button>();
                }
            }

            if (doorImage == null)
            {
                Transform d = transform.Find("WashroomDoor") ?? transform.Find("DoorImage");
                if (d != null) doorImage = d.GetComponent<Image>();
            }

            if (anuAvatar == null)
            {
                Transform a = transform.Find("AnuAvatar") ?? transform.Find("Anu_Image");
                if (a != null) anuAvatar = a.GetComponent<Image>();
            }

            if (anuAvatar != null)
            {
                if (anuWalkingController == null)
                {
                    anuWalkingController = anuAvatar.GetComponent<U8_AnuWalkingController_Masters_Activity>();
                    if (anuWalkingController == null)
                    {
                        anuWalkingController = anuAvatar.gameObject.AddComponent<U8_AnuWalkingController_Masters_Activity>();
                    }
                }
            }

            if (promptText == null)
            {
                Transform pt = transform.Find("PromptCard/PromptText") ?? transform.Find("PromptText");
                if (pt != null) promptText = pt.GetComponent<TextMeshProUGUI>();
            }

            if (feedbackText == null)
            {
                Transform ft = transform.Find("FeedbackCard/FeedbackText") ?? transform.Find("FeedbackText");
                if (ft != null) feedbackText = ft.GetComponent<TextMeshProUGUI>();
            }

            Transform m = transform.Find("MeeraAvatar") ?? transform.Find("SecondGirlAvatar");
            if (m != null)
            {
                m.gameObject.SetActive(false);
            }

#if UNITY_EDITOR
            if (anuSheepishSprite == null || anuSheepishSprite.name.Contains("Idle"))
            {
                anuSheepishSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Sheepish_Side.png");
            }
#endif
        }

        public void AutoWireAudioClips()
        {
            if (U8_AudioManager_Masters_Activity.Instance == null) return;
            if (voAnuNeedsWashroom == null) voAnuNeedsWashroom = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_01");
            if (voDoorClosed == null) voDoorClosed = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_02");
            if (voKnockAndWait == null) voKnockAndWait = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_03");
            if (voJustAMinute == null) voJustAMinute = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_VOICE_1");
            if (voSorry == null) voSorry = U8_AudioManager_Masters_Activity.Instance.GetClip("VO_U8_ANU_1");
            if (sfxKnock == null) sfxKnock = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_Knock");
            if (sfxBangDoor == null) sfxBangDoor = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_BangDoor");
            if (sfxBoltRattle == null) sfxBoltRattle = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_BoltRattle");
            if (sfxBoltClick == null) sfxBoltClick = U8_AudioManager_Masters_Activity.Instance.GetClip("SFX_BoltClick");
        }

        public void EnsureMeeraSpritesLoaded()
        {
#if UNITY_EDITOR
            meeraWalkSprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                meeraWalkSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_{i + 1}.png");
            }
            if (meeraWalkSprites[0] != null) meeraEnteringSprite = meeraWalkSprites[0];
#endif
        }

        private Coroutine safetyTimeoutRoutine;

        public void ResetScreen()
        {
            isProcessingChoice = false;
            if (doorImage != null && doorClosedSprite != null) doorImage.sprite = doorClosedSprite;
            if (shutSignObject != null) shutSignObject.SetActive(true);

            if (anuAvatar != null)
            {
                anuAvatar.gameObject.SetActive(true);
                anuAvatar.transform.localScale = Vector3.one;
                if (doorImage != null)
                {
                    anuAvatar.transform.SetSiblingIndex(doorImage.transform.GetSiblingIndex() + 1);
                }
            }
            if (meeraAvatar != null)
            {
                meeraAvatar.gameObject.SetActive(false);
            }
            Transform m = transform.Find("MeeraAvatar") ?? transform.Find("SecondGirlAvatar");
            if (m != null)
            {
                m.gameObject.SetActive(false);
            }

            if (promptText != null) promptText.text = "Anu needs the washroom.";
            SetFeedback("");

            // Hide choice buttons while Anu is walking to the door
            SetChoiceButtonsActive(false);
            SetButtonsInteractable(false);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_01"); // "Anu needs the washroom."
            }

            // Failsafe safety timer: guarantees choices appear within 3.8 seconds
            if (safetyTimeoutRoutine != null) StopCoroutine(safetyTimeoutRoutine);
            safetyTimeoutRoutine = StartCoroutine(SafetyTimeoutRoutine(8.5f));

            if (anuWalkingController != null)
            {
                // Anu walks facing left to the door and stops in front of it (no auto-knock)
                anuWalkingController.StartWalkToDoorSequence(OnAnuArrivedAtDoor);
            }
            else
            {
                OnAnuArrivedAtDoor();
            }
        }

        private IEnumerator SafetyTimeoutRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (choiceContainer != null && (!choiceContainer.activeSelf || (btnKnockAndWait != null && !btnKnockAndWait.gameObject.activeSelf)))
            {
                Debug.Log("[U8_Part1_DoorController] Safety timer triggered: Revealing choices.");
                OnAnuArrivedAtDoor();
            }
        }

        private void OnAnuArrivedAtDoor()
        {
            if (safetyTimeoutRoutine != null)
            {
                StopCoroutine(safetyTimeoutRoutine);
                safetyTimeoutRoutine = null;
            }

            if (promptText != null) promptText.text = "The door is closed. What should Anu do?";
            
            // Explicitly show ChoiceContainer and all three buttons!
            SetChoiceButtonsActive(true);
            SetButtonsInteractable(true);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_02"); // "The door is closed. What should Anu do?"
            }
        }

        public void SetChoiceButtonsActive(bool active)
        {
            if (choiceContainer == null)
            {
                Transform cc = transform.Find("ChoiceContainer");
                if (cc != null) choiceContainer = cc.gameObject;
            }

            if (choiceContainer != null)
            {
                choiceContainer.SetActive(active);
                if (active)
                {
                    choiceContainer.transform.SetAsLastSibling();
                    RectTransform rt = choiceContainer.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = new Vector2(0f, -385f);
                        rt.sizeDelta = new Vector2(1400f, 110f);
                        rt.localScale = Vector3.one;
                    }
                }

                foreach (Transform child in choiceContainer.transform)
                {
                    child.gameObject.SetActive(active);
                    if (active)
                    {
                        var cr = child.GetComponent<CanvasRenderer>();
                        if (cr != null) cr.cull = false;
                        var img = child.GetComponent<Image>();
                        if (img != null) img.enabled = true;
                    }
                }
            }

            if (btnKnockAndWait != null) btnKnockAndWait.gameObject.SetActive(active);
            if (btnBangLoudly != null) btnBangLoudly.gameObject.SetActive(active);
            if (btnPushOpen != null) btnPushOpen.gameObject.SetActive(active);
        }

        private void SetButtonsInteractable(bool state)
        {
            if (btnKnockAndWait != null) btnKnockAndWait.interactable = state;
            if (btnBangLoudly != null) btnBangLoudly.interactable = state;
            if (btnPushOpen != null) btnPushOpen.interactable = state;
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                Transform parent = feedbackText.transform.parent;
                if (parent != null && parent.name == "FeedbackCard")
                {
                    parent.gameObject.SetActive(!string.IsNullOrEmpty(message));
                }
            }
        }

        private void OnKnockAndWaitClicked()
        {
            if (isProcessingChoice) return;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayButtonClick();
            }
            StartCoroutine(KnockAndWaitRoutine());
        }

        private IEnumerator KnockAndWaitRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            SetFeedback("Knock, knock! Anu waits patiently...");

            // STEP 3: Anu knocks on the door
            if (anuWalkingController != null)
            {
                yield return StartCoroutine(anuWalkingController.PlayKnockSequenceRoutine());
            }
            else if (anuAvatar != null && anuKnockingSprite != null)
            {
                anuAvatar.sprite = anuKnockingSprite;
                anuAvatar.preserveAspect = true;
                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Knock");
                }
                yield return new WaitForSeconds(0.35f);
                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Knock");
                }
                yield return new WaitForSeconds(0.45f);
            }

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_03"); // "Knock, and wait."
            }

            yield return new WaitForSeconds(0.6f);

            // STEP 4: Washroom door opens
            if (anuWalkingController != null)
            {
                anuWalkingController.SetIdlePose();
            }
            else if (anuAvatar != null && anuNormalSprite != null)
            {
                anuAvatar.sprite = anuNormalSprite;
                anuAvatar.preserveAspect = true;
            }

            if (doorImage != null && doorOpenSprite != null)
            {
                doorImage.sprite = doorOpenSprite;
            }
            if (shutSignObject != null)
            {
                shutSignObject.SetActive(false);
            }
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BoltClick");
            }

            SetFeedback("The door opens. Anu goes in!");

            yield return new WaitForSeconds(0.4f);

            // STEP 5: ANU ENTERS THE WASHROOM
            if (anuWalkingController != null)
            {
                yield return StartCoroutine(anuWalkingController.WalkIntoWashroomRoutine(new Vector2(-790f, -70f)));
            }
            else if (anuAvatar != null)
            {
                RectTransform art = anuAvatar.rectTransform;
                Vector2 startP = art.anchoredPosition;
                Vector2 endP = new Vector2(-790f, -70f);
                float t = 0f;
                while (t < 1.4f)
                {
                    t += Time.deltaTime;
                    art.anchoredPosition = Vector2.Lerp(startP, endP, t / 1.4f);
                    yield return null;
                }
                art.anchoredPosition = endP;
            }

            // STEP 6: ANU IS IN THE WASHROOM (Door remains OPEN for Part 2)
            SetFeedback("Anu is in the washroom!");
            yield return new WaitForSeconds(0.8f);

            // Transition to Part 2 (Inside the washroom, door open, ready for Step 1: Close the cubicle door first)
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part2_Inside);
            }
        }

        private IEnumerator PlayMeeraExitRoutine()
        {
            EnsureMeeraSpritesLoaded();

            if (meeraAvatar == null)
            {
                Transform m = transform.Find("MeeraAvatar") ?? transform.Find("SecondGirlAvatar");
                if (m != null) meeraAvatar = m.GetComponent<Image>();
            }

            if (meeraAvatar == null) yield break;

            // Ensure Meera renders on top of the door frame
            meeraAvatar.transform.SetAsLastSibling();

            RectTransform rt = meeraAvatar.rectTransform;
            // Slightly bigger character (~18% larger: 540x790, matching Anu's visual scale)
            rt.sizeDelta = new Vector2(540f, 790f);
            // Face LEFT: the sprites are natively drawn facing LEFT (90-degree side profile)
            rt.localScale = Vector3.one;

            // Starting position: clearly inside the cubicle stall doorway opening, feet grounded on floor baseline
            Vector2 startPos = new Vector2(-750f, -60f);
            rt.anchoredPosition = startPos;
            meeraAvatar.gameObject.SetActive(true);

            if (meeraWalkSprites != null && meeraWalkSprites.Length > 0 && meeraWalkSprites[0] != null)
            {
                meeraAvatar.sprite = meeraWalkSprites[0];
            }
            else if (meeraEnteringSprite != null)
            {
                meeraAvatar.sprite = meeraEnteringSprite;
            }

            // Small hesitation as cubicle door opens and she emerges
            yield return new WaitForSeconds(0.3f);

            // Natural, smooth 8-frame walk out through the doorway towards the LEFT exit
            float currentX = startPos.x;
            float targetX = -1350f; // completely off the visible screen to the left
            float speed = 208f;     // synchronized with 8-frame stride to eliminate sliding
            float timer = 0f;
            float stepTimer = 0f;
            int stepIdx = 0;

            while (currentX > targetX)
            {
                timer += Time.deltaTime;
                stepTimer += Time.deltaTime;

                currentX -= speed * Time.deltaTime;
                // Subtle natural vertical body bounce (down/up with each footstep)
                float bobY = Mathf.Sin(timer * 17.5f) * 1.8f;
                rt.anchoredPosition = new Vector2(currentX, -60f + bobY);

                if (stepTimer >= 0.12f)
                {
                    stepTimer = 0f;
                    int frameCount = (meeraWalkSprites != null && meeraWalkSprites.Length > 0) ? meeraWalkSprites.Length : 4;
                    stepIdx = (stepIdx + 1) % frameCount;
                    if (meeraWalkSprites != null && stepIdx < meeraWalkSprites.Length && meeraWalkSprites[stepIdx] != null)
                    {
                        meeraAvatar.sprite = meeraWalkSprites[stepIdx];
                    }
                }

                yield return null;
            }

            // Finished walking out completely
            meeraAvatar.gameObject.SetActive(false);
        }

        private void OnBangLoudlyClicked()
        {
            if (isProcessingChoice) return;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayButtonClick();
            }
            StartCoroutine(BangLoudlyRoutine());
        }

        private IEnumerator BangLoudlyRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            SetFeedback("BANG! BANG! BANG!");

            // Vigorously bang on the door!
            if (anuWalkingController != null)
            {
                yield return StartCoroutine(anuWalkingController.PlayBangSequenceRoutine(() =>
                {
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BangDoor");
                    }
                    if (doorImage != null)
                    {
                        StartCoroutine(ShakeDoorRoutine(0.12f, 7f));
                    }
                }));
            }
            else
            {
                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BangDoor");
                }
                if (doorImage != null)
                {
                    StartCoroutine(ShakeDoorRoutine(0.2f, 7f));
                }
                yield return new WaitForSeconds(0.4f);
            }

            yield return new WaitForSeconds(0.3f);

            // Muffled voice from inside
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_VOICE_1"); // "Just a minute!" (muffled)
            }

            // Anu turns sheepish / embarrassed
            if (anuWalkingController != null)
            {
                anuWalkingController.SetSheepishPose();
            }
            else if (anuAvatar != null && anuSheepishSprite != null)
            {
                anuAvatar.sprite = anuSheepishSprite;
                anuAvatar.preserveAspect = true;
            }

            SetFeedback("Someone is inside: 'Just a minute!' Anu feels sheepish.");

            yield return new WaitForSeconds(2.0f);

            // Anu says Sorry!
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_ANU_1"); // "Sorry!"
            }

            yield return new WaitForSeconds(1.5f);

            // Reset back to idle facing the door
            if (anuWalkingController != null)
            {
                anuWalkingController.SetIdlePose();
            }
            else if (anuAvatar != null && anuNormalSprite != null)
            {
                anuAvatar.sprite = anuNormalSprite;
                anuAvatar.preserveAspect = true;
            }

            SetFeedback("Let's try again with polite manners!");
            isProcessingChoice = false;
            SetButtonsInteractable(true);
        }

        private void OnPushOpenClicked()
        {
            if (isProcessingChoice) return;
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayButtonClick();
            }
            StartCoroutine(PushOpenRoutine());
        }

        private IEnumerator PushOpenRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            SetFeedback("Anu pushes against the door...");

            // Anu leans forward and pushes against the door
            if (anuWalkingController != null)
            {
                yield return StartCoroutine(anuWalkingController.PlayPushSequenceRoutine(() =>
                {
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BoltRattle");
                    }
                    if (doorImage != null)
                    {
                        StartCoroutine(ShakeDoorRoutine(0.45f, 5f));
                    }
                }));
            }
            else
            {
                if (U8_AudioManager_Masters_Activity.Instance != null)
                {
                    U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BoltRattle");
                }
                if (doorImage != null)
                {
                    StartCoroutine(ShakeDoorRoutine(0.45f, 5f));
                }
                yield return new WaitForSeconds(0.5f);
            }

            SetFeedback("It is bolted! Closed doors require patience.");

            yield return new WaitForSeconds(2.0f);

            if (anuWalkingController != null)
            {
                anuWalkingController.SetIdlePose();
            }

            isProcessingChoice = false;
            SetButtonsInteractable(true);
        }

        private IEnumerator ShakeDoorRoutine(float duration, float magnitude)
        {
            if (doorImage == null) yield break;
            Vector3 originalPos = doorImage.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = originalPos.x + Random.Range(-magnitude, magnitude);
                doorImage.transform.localPosition = new Vector3(x, originalPos.y, originalPos.z);
                yield return null;
            }
            doorImage.transform.localPosition = originalPos;
        }
    }
}
