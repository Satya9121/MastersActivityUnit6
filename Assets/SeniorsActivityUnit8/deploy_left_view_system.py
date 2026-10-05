import os

walking_ctrl_code = """using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Googolplex.Unit8
{
    [RequireComponent(typeof(RectTransform))]
    public class U8_AnuWalkingController_Masters_Activity : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image anuImage;
        [SerializeField] private Animator animator;

        [Header("Movement Trajectory")]
        [Tooltip("Starting position on right side corridor entrance.")]
        [SerializeField] private Vector2 startPosition = new Vector2(620f, -70f);

        [Tooltip("Target position right in front of the door labeled SHUT.")]
        [SerializeField] private Vector2 doorPosition = new Vector2(-424f, -70f);

        [Tooltip("Walking speed in canvas units per second.")]
        [SerializeField] private float walkSpeed = 280f;

        [Tooltip("Duration per walking frame in seconds.")]
        [SerializeField] private float stepFrameDuration = 0.13f;

        [Header("Left-View Walking & Knocking Sprites")]
        [SerializeField] private Sprite[] walkSprites = new Sprite[4];
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] knockSprites = new Sprite[3];
        [SerializeField] private Sprite sheepishSprite;
        [SerializeField] private Sprite normalSprite;

        private Coroutine activeRoutine;
        private bool isWalking = false;

        public bool IsWalking => isWalking;
        public Vector2 TargetDoorPosition => doorPosition;

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (anuImage == null) anuImage = GetComponent<Image>();
            if (animator == null) animator = GetComponent<Animator>();

            if (animator != null)
            {
                animator.enabled = false;
            }

            AutoAssignSpritesIfNeeded();

            if (anuImage != null)
            {
                anuImage.preserveAspect = true;
                if (anuImage.sprite == null)
                {
                    anuImage.sprite = (idleSprite != null) ? idleSprite : normalSprite;
                }
            }
        }

        private void OnValidate()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (anuImage == null) anuImage = GetComponent<Image>();
            AutoAssignSpritesIfNeeded();
        }

        public void AutoAssignSpritesIfNeeded()
        {
#if UNITY_EDITOR
            if (walkSprites == null || walkSprites.Length < 4 || walkSprites[0] == null)
            {
                walkSprites = new Sprite[4];
                walkSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_1.png");
                walkSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_2.png");
                walkSprites[2] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_3.png");
                walkSprites[3] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_4.png");
            }
            if (idleSprite == null)
            {
                idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_Idle.png");
            }
            if (knockSprites == null || knockSprites.Length < 3 || knockSprites[0] == null)
            {
                knockSprites = new Sprite[3];
                knockSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Knock_1.png");
                knockSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Knock_2.png");
                knockSprites[2] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Knock_3.png");
            }

            if (normalSprite == null || sheepishSprite == null)
            {
                string charSheetPath = "Assets/SeniorsActivityUnit8/Art/more sprites/Character — Anu U8 SA.png";
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(charSheetPath);
                if (assets != null)
                {
                    foreach (var a in assets)
                    {
                        if (a is Sprite s)
                        {
                            if (s.name == "SPR_Anu_Normal" && normalSprite == null) normalSprite = s;
                            if (s.name == "SPR_Anu_Sheepish" && sheepishSprite == null) sheepishSprite = s;
                        }
                    }
                }
            }

            if (idleSprite == null) idleSprite = normalSprite;
#endif
        }

        /// <summary>
        /// Walks Anu from right entrance to the closed door, facing left.
        /// Stops at the door and reveals choices without knocking automatically.
        /// </summary>
        public void StartWalkToDoorSequence(Action onArrived = null)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }
            activeRoutine = StartCoroutine(WalkToDoorRoutine(onArrived));
        }

        // Backward compatibility
        public void StartWalkAndKnockSequence(Action onCompleted = null)
        {
            StartWalkToDoorSequence(onCompleted);
        }

        private IEnumerator WalkToDoorRoutine(Action onArrived)
        {
            isWalking = true;

            // 1. Initial Setup: Place at right-side entrance
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition;
                rectTransform.localEulerAngles = Vector3.zero;
                rectTransform.localScale = Vector3.one;
            }

            // Starting sprite facing left
            SetSpriteSafely((walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null) 
                ? walkSprites[0] 
                : (idleSprite ?? normalSprite));

            yield return null;

            // 2. Walk naturally across toward the closed washroom door (left view)
            float startX = startPosition.x;
            float targetX = doorPosition.x;
            float currentX = startX;
            float stepTimer = 0f;
            int stepIndex = 0;

            while (Mathf.Abs(currentX - targetX) > 1.5f)
            {
                float stepDist = walkSpeed * Time.deltaTime;
                currentX = Mathf.MoveTowards(currentX, targetX, stepDist);

                float bobY = Mathf.Sin(stepTimer * Mathf.PI * 4f) * 3f;

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(currentX, doorPosition.y + bobY);
                }

                // Advance alternating footstep frames in left view
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepFrameDuration)
                {
                    stepTimer = 0f;
                    stepIndex = (stepIndex + 1) % 4;

                    if (walkSprites != null && walkSprites.Length >= 4 && walkSprites[stepIndex] != null)
                    {
                        SetSpriteSafely(walkSprites[stepIndex]);
                    }
                }

                yield return null;
            }

            // 3. Stop precisely at destination in front of the door labeled SHUT
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = doorPosition;
            }
            isWalking = false;

            // Settle into standing idle pose facing the door
            SetSpriteSafely(idleSprite ?? normalSprite);
            yield return new WaitForSeconds(0.15f);

            activeRoutine = null;
            onArrived?.Invoke();
        }

        /// <summary>
        /// Knocks directly on the door labeled SHUT when Knock and Wait button is clicked.
        /// </summary>
        public IEnumerator PlayKnockSequenceRoutine()
        {
            // First knock
            // 1. Raise fist toward door
            SetKnockSprite(0);
            yield return new WaitForSeconds(0.15f);

            // 2. Knock impact directly on the door labeled SHUT
            SetKnockSprite(1);
            PlayKnockSFX();
            yield return new WaitForSeconds(0.20f);

            // 3. Recoil
            SetKnockSprite(2);
            yield return new WaitForSeconds(0.14f);

            // Second knock!
            SetKnockSprite(1);
            PlayKnockSFX();
            yield return new WaitForSeconds(0.22f);

            // Recoil
            SetKnockSprite(2);
            yield return new WaitForSeconds(0.18f);

            // Settle to idle facing the door
            SetIdlePose();
        }

        public void PlayKnockAnimation(Action onFinished = null)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(KnockAnimationWrapper(onFinished));
        }

        private IEnumerator KnockAnimationWrapper(Action onFinished)
        {
            yield return StartCoroutine(PlayKnockSequenceRoutine());
            activeRoutine = null;
            onFinished?.Invoke();
        }

        public void SetIdlePose()
        {
            SetSpriteSafely(idleSprite ?? normalSprite);
        }

        public void SetSheepishPose()
        {
            SetSpriteSafely(sheepishSprite ?? normalSprite);
        }

        private void SetKnockSprite(int index)
        {
            if (knockSprites != null && index >= 0 && index < knockSprites.Length && knockSprites[index] != null)
            {
                SetSpriteSafely(knockSprites[index]);
            }
            else
            {
                SetSpriteSafely(idleSprite ?? normalSprite);
            }
        }

        private void SetSpriteSafely(Sprite sprite)
        {
            if (anuImage != null && sprite != null)
            {
                anuImage.sprite = sprite;
                anuImage.preserveAspect = true;
            }
        }

        private void PlayKnockSFX()
        {
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Knock");
            }
        }

        public void StopAllMovement()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }
            isWalking = false;
            if (rectTransform != null) rectTransform.anchoredPosition = doorPosition;
            SetIdlePose();
        }
    }
}
"""

door_ctrl_code = """using System.Collections;
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

        [Header("Controllers")]
        [SerializeField] private U8_AnuWalkingController_Masters_Activity anuWalkingController;

        private bool isProcessingChoice = false;

        private void Awake()
        {
            if (btnKnockAndWait != null) btnKnockAndWait.onClick.AddListener(OnKnockAndWaitClicked);
            if (btnBangLoudly != null) btnBangLoudly.onClick.AddListener(OnBangLoudlyClicked);
            if (btnPushOpen != null) btnPushOpen.onClick.AddListener(OnPushOpenClicked);

            AutoWireComponents();
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
        }

        public void ResetScreen()
        {
            isProcessingChoice = false;
            if (doorImage != null && doorClosedSprite != null) doorImage.sprite = doorClosedSprite;
            if (shutSignObject != null) shutSignObject.SetActive(true);

            if (promptText != null) promptText.text = "Anu needs the washroom.";
            if (feedbackText != null) feedbackText.text = "";

            // Hide/disable choices while Anu is walking to the door
            SetButtonsInteractable(false);
            if (choiceContainer != null) choiceContainer.SetActive(false);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_01"); // "Anu needs the washroom."
            }

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

        private void OnAnuArrivedAtDoor()
        {
            if (promptText != null) promptText.text = "The door is closed. What should Anu do?";
            if (choiceContainer != null) choiceContainer.SetActive(true);
            SetButtonsInteractable(true);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_02"); // "The door is closed. What should Anu do?"
            }
        }

        private void SetButtonsInteractable(bool state)
        {
            if (btnKnockAndWait != null) btnKnockAndWait.interactable = state;
            if (btnBangLoudly != null) btnBangLoudly.interactable = state;
            if (btnPushOpen != null) btnPushOpen.interactable = state;
        }

        private void OnKnockAndWaitClicked()
        {
            if (isProcessingChoice) return;
            StartCoroutine(KnockAndWaitRoutine());
        }

        private IEnumerator KnockAndWaitRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            if (feedbackText != null) feedbackText.text = "Knock, knock! Anu waits patiently...";

            // Knock directly on the door labeled SHUT!
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

            yield return new WaitForSeconds(1.2f);

            // Door opens
            if (anuWalkingController != null)
            {
                anuWalkingController.SetIdlePose();
            }
            else if (anuAvatar != null && anuNormalSprite != null)
            {
                anuAvatar.sprite = anuNormalSprite;
                anuAvatar.preserveAspect = true;
            }

            if (doorImage != null && doorOpenSprite != null) doorImage.sprite = doorOpenSprite;
            if (shutSignObject != null) shutSignObject.SetActive(false);
            if (feedbackText != null) feedbackText.text = "The other student comes out. Anu goes in!";

            yield return new WaitForSeconds(1.5f);

            // Transition to Part 2
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part2_Inside);
            }
        }

        private void OnBangLoudlyClicked()
        {
            if (isProcessingChoice) return;
            StartCoroutine(BangLoudlyRoutine());
        }

        private IEnumerator BangLoudlyRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            if (feedbackText != null) feedbackText.text = "BANG! BANG! BANG!";
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BangDoor");
            }

            yield return new WaitForSeconds(0.6f);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_VOICE_1"); // "Just a minute!" (muffled)
            }

            if (anuWalkingController != null)
            {
                anuWalkingController.SetSheepishPose();
            }
            else if (anuAvatar != null && anuSheepishSprite != null)
            {
                anuAvatar.sprite = anuSheepishSprite;
                anuAvatar.preserveAspect = true;
            }

            if (feedbackText != null) feedbackText.text = "Someone is inside: 'Just a minute!' Anu feels sheepish.";

            yield return new WaitForSeconds(2.0f);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_ANU_1"); // "Sorry!"
            }

            yield return new WaitForSeconds(1.5f);

            // Reset for retry
            if (anuWalkingController != null)
            {
                anuWalkingController.SetIdlePose();
            }
            else if (anuAvatar != null && anuNormalSprite != null)
            {
                anuAvatar.sprite = anuNormalSprite;
                anuAvatar.preserveAspect = true;
            }

            if (feedbackText != null) feedbackText.text = "Let's try again with polite manners!";
            isProcessingChoice = false;
            SetButtonsInteractable(true);
        }

        private void OnPushOpenClicked()
        {
            if (isProcessingChoice) return;
            StartCoroutine(PushOpenRoutine());
        }

        private IEnumerator PushOpenRoutine()
        {
            isProcessingChoice = true;
            SetButtonsInteractable(false);

            if (feedbackText != null) feedbackText.text = "Anu pushes the door...";
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_BoltRattle");
            }

            if (doorImage != null)
            {
                Vector3 originalPos = doorImage.transform.localPosition;
                for (int i = 0; i < 4; i++)
                {
                    doorImage.transform.localPosition = originalPos + new Vector3(Random.Range(-5f, 5f), 0, 0);
                    yield return new WaitForSeconds(0.05f);
                }
                doorImage.transform.localPosition = originalPos;
            }

            if (feedbackText != null) feedbackText.text = "It is bolted! Closed doors require patience.";

            yield return new WaitForSeconds(2.0f);

            isProcessingChoice = false;
            SetButtonsInteractable(true);
        }
    }
}
"""

anu_walk_to_door_code = """using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Googolplex.Unit8;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class U8_AnuWalkToDoor : MonoBehaviour
{
    [Header("ANU")]
    [SerializeField] private RectTransform anu;
    [SerializeField] private Image anuImage;

    [Header("Existing Anu Sprites")]
    [SerializeField] private Sprite anuNormalSprite;
    [SerializeField] private Sprite anuKnockingSprite;

    [Header("2D Walking and Knocking Sprites (Left View)")]
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite[] knockSprites;
    [SerializeField] private Sprite idleFacingDoorSprite;
    [SerializeField] private float stepFrameRate = 0.13f;

    [Header("Walking Settings")]
    [SerializeField] private float walkDuration = 2.5f;
    [SerializeField] private float walkingBobAmount = 3f;
    [SerializeField] private float walkingBobSpeed = 12f;

    [Header("Trajectory")]
    [SerializeField] private Vector2 startPosition = new Vector2(620f, -70f);
    [Tooltip("Position right in front of the door labeled SHUT.")]
    [SerializeField] private Vector2 doorPosition = new Vector2(-424f, -70f);

    [Header("Knocking Settings")]
    [SerializeField] private int numberOfKnocks = 2;
    [SerializeField] private float knockDistance = 8f;
    [SerializeField] private float knockTime = 0.18f;
    [SerializeField] private float knockPause = 0.18f;

    [Header("Existing Choice Buttons")]
    [SerializeField] private GameObject btnKnockAndWait;
    [SerializeField] private GameObject btnBangLoudly;
    [SerializeField] private GameObject btnPushOpen;

    [Header("Optional Parent")]
    [Tooltip("Optional. If assigned, this can be used to enable/disable the whole choice-button area.")]
    [SerializeField] private GameObject choiceButtonsParent;

    [Header("Behaviour")]
    [SerializeField] private bool playAutomatically = true;

    private bool sequenceFinished;
    private Coroutine activeRoutine;

    private void Awake()
    {
        AutoFindReferences();
        HideChoiceButtons();
    }

    private void OnValidate()
    {
        AutoFindReferences();
    }

    private void AutoFindReferences()
    {
        if (anu == null)
        {
            Transform t = transform.Find("AnuAvatar") ?? transform.Find("Anu_Image");
            if (t != null) anu = t.GetComponent<RectTransform>();
        }

        if (anuImage == null && anu != null)
        {
            anuImage = anu.GetComponent<Image>();
        }

        if (choiceButtonsParent == null)
        {
            Transform cc = transform.Find("ChoiceContainer");
            if (cc != null) choiceButtonsParent = cc.gameObject;
        }

        if (choiceButtonsParent != null)
        {
            if (btnKnockAndWait == null)
            {
                Transform b = choiceButtonsParent.transform.Find("Btn_Knock") ?? choiceButtonsParent.transform.Find("KnockAndWait");
                if (b != null) btnKnockAndWait = b.gameObject;
            }
            if (btnBangLoudly == null)
            {
                Transform b = choiceButtonsParent.transform.Find("Btn_Bang") ?? choiceButtonsParent.transform.Find("BangLoudly");
                if (b != null) btnBangLoudly = b.gameObject;
            }
            if (btnPushOpen == null)
            {
                Transform b = choiceButtonsParent.transform.Find("Btn_Push") ?? choiceButtonsParent.transform.Find("PushOpen");
                if (b != null) btnPushOpen = b.gameObject;
            }
        }

#if UNITY_EDITOR
        if (walkSprites == null || walkSprites.Length < 4 || walkSprites[0] == null)
        {
            walkSprites = new Sprite[4];
            walkSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_1.png");
            walkSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_2.png");
            walkSprites[2] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_3.png");
            walkSprites[3] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_4.png");
        }

        if (idleFacingDoorSprite == null)
        {
            idleFacingDoorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_Idle.png");
        }

        if (knockSprites == null || knockSprites.Length < 3 || knockSprites[0] == null)
        {
            knockSprites = new Sprite[3];
            knockSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Knock_1.png");
            knockSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Knock_2.png");
            knockSprites[2] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Knock_3.png");
        }
#endif
    }

    private void Start()
    {
        var doorController = GetComponent<U8_Part1_DoorController_Masters_Activity>();
        if (doorController != null)
        {
            return;
        }

        if (playAutomatically)
        {
            StartWalkSequence();
        }
    }

    public void StartWalkSequence()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(WalkRoutine());
    }

    private IEnumerator WalkRoutine()
    {
        if (anu == null || anuImage == null) yield break;

        sequenceFinished = false;
        HideChoiceButtons();

        var dedicatedWalkCtrl = anu.GetComponent<U8_AnuWalkingController_Masters_Activity>();
        if (dedicatedWalkCtrl != null)
        {
            bool arrived = false;
            dedicatedWalkCtrl.StartWalkToDoorSequence(() => { arrived = true; });
            while (!arrived) yield return null;
            ShowChoiceButtons();
            sequenceFinished = true;
            yield break;
        }

        // Direct fallback
        anu.anchoredPosition = startPosition;
        anu.localEulerAngles = Vector3.zero;
        anu.localScale = Vector3.one;
        anuImage.preserveAspect = true;

        if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
            anuImage.sprite = walkSprites[0];

        float elapsed = 0f;
        float frameTimer = 0f;
        int frameIndex = 0;

        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / walkDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            Vector2 movementPosition = Vector2.Lerp(startPosition, doorPosition, smoothProgress);

            float walkingBob = Mathf.Sin(elapsed * walkingBobSpeed) * walkingBobAmount;
            movementPosition.y += walkingBob;
            anu.anchoredPosition = movementPosition;

            frameTimer += Time.deltaTime;
            if (frameTimer >= stepFrameRate)
            {
                frameTimer = 0f;
                if (walkSprites != null && walkSprites.Length > 0)
                {
                    frameIndex = (frameIndex + 1) % walkSprites.Length;
                    if (walkSprites[frameIndex] != null) anuImage.sprite = walkSprites[frameIndex];
                }
            }

            yield return null;
        }

        anu.anchoredPosition = doorPosition;

        if (idleFacingDoorSprite != null)
            anuImage.sprite = idleFacingDoorSprite;

        yield return new WaitForSeconds(0.15f);

        ShowChoiceButtons();
        sequenceFinished = true;
        activeRoutine = null;
    }

    /// <summary>
    /// Triggered when Knock and Wait button is clicked
    /// </summary>
    public void KnockDoorOnButtonClick()
    {
        var dedicatedWalkCtrl = anu.GetComponent<U8_AnuWalkingController_Masters_Activity>();
        if (dedicatedWalkCtrl != null)
        {
            dedicatedWalkCtrl.PlayKnockAnimation();
        }
    }

    private void HideChoiceButtons()
    {
        if (choiceButtonsParent != null) choiceButtonsParent.SetActive(false);
        if (btnKnockAndWait != null) btnKnockAndWait.SetActive(false);
        if (btnBangLoudly != null) btnBangLoudly.SetActive(false);
        if (btnPushOpen != null) btnPushOpen.SetActive(false);
    }

    private void ShowChoiceButtons()
    {
        if (choiceButtonsParent != null) choiceButtonsParent.SetActive(true);
        if (btnKnockAndWait != null) btnKnockAndWait.SetActive(true);
        if (btnBangLoudly != null) btnBangLoudly.SetActive(true);
        if (btnPushOpen != null) btnPushOpen.SetActive(true);
    }

    public bool IsSequenceFinished() => sequenceFinished;
}
"""

targets = [
    (r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs", walking_ctrl_code),
    (r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs", walking_ctrl_code),
    (r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part1_DoorController_Masters_Activity.cs", door_ctrl_code),
    (r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part1_DoorController_Masters_Activity.cs", door_ctrl_code),
    (r"D:\engsnap\MastersActivityUnit6\Assets\Own_Scripts\U8_AnuWalkToDoor.cs", anu_walk_to_door_code),
    (r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\Own_Scripts\U8_AnuWalkToDoor.cs", anu_walk_to_door_code),
]

for path, code in targets:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(code)
    print("Deployed:", path)
