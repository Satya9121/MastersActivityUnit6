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
        [SerializeField] private Vector2 startPosition = new Vector2(620f, -70f);
        [SerializeField] private Vector2 doorPosition = new Vector2(-440f, -70f);
        [SerializeField] private float walkSpeed = 280f;
        [SerializeField] private float stepFrameDuration = 0.13f;

        [Header("Walking Sprite Frames (Alternating Footsteps)")]
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

            // Disable animator so it never overrides UI Image sprite with null
            if (animator != null)
            {
                animator.enabled = false;
            }

            AutoAssignSpritesIfNeeded();

            // Set initial appearance safely
            if (anuImage != null)
            {
                anuImage.preserveAspect = true;
                if (anuImage.sprite == null)
                {
                    anuImage.sprite = (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null) 
                        ? walkSprites[0] 
                        : (normalSprite ?? idleSprite);
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

            // Fallback sprites from Character sheet
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
                            if (s.name == "SPR_Anu_Knocking" && (knockSprites == null || knockSprites[1] == null))
                            {
                                if (knockSprites == null) knockSprites = new Sprite[3];
                                if (knockSprites[1] == null) knockSprites[1] = s;
                            }
                        }
                    }
                }
            }

            if (idleSprite == null) idleSprite = normalSprite;
            if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] == null && normalSprite != null)
            {
                for (int i = 0; i < walkSprites.Length; i++) walkSprites[i] = normalSprite;
            }
#endif
        }

        public void StartWalkAndKnockSequence(Action onCompleted = null)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }
            activeRoutine = StartCoroutine(WalkAndKnockRoutine(onCompleted));
        }

        private IEnumerator WalkAndKnockRoutine(Action onCompleted)
        {
            isWalking = true;

            // 1. Initial Setup: Place at entrance on the right
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition;
                rectTransform.localEulerAngles = Vector3.zero;
                rectTransform.localScale = Vector3.one;
            }

            // Ensure starting sprite is visible
            SetSpriteSafely((walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null) 
                ? walkSprites[0] 
                : (idleSprite ?? normalSprite));

            yield return null;

            // 2. Walk naturally across toward washroom door
            float startX = startPosition.x;
            float targetX = doorPosition.x;
            float currentX = startX;
            float stepTimer = 0f;
            int stepIndex = 0;

            while (Mathf.Abs(currentX - targetX) > 1.5f)
            {
                float stepDist = walkSpeed * Time.deltaTime;
                currentX = Mathf.MoveTowards(currentX, targetX, stepDist);

                // Subtle footfall bobbing
                float bobY = Mathf.Sin(stepTimer * Mathf.PI * 4f) * 3f;

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(currentX, doorPosition.y + bobY);
                }

                // Advance walking frames (alternating footsteps)
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

            // 3. Precisely lock at destination in front of the door
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = doorPosition;
            }
            isWalking = false;

            // Settle into idle pose facing the door
            SetSpriteSafely(idleSprite ?? normalSprite);
            yield return new WaitForSeconds(0.22f);

            // 4. Knocking sequence directly on the door
            yield return StartCoroutine(PlayKnockSequenceRoutine());

            // 5. Standing idle pose near door
            SetIdlePose();

            activeRoutine = null;
            onCompleted?.Invoke();
        }

        public IEnumerator PlayKnockSequenceRoutine()
        {
            // First knock
            // Pose: Raising fist
            SetKnockSprite(0);
            yield return new WaitForSeconds(0.15f);

            // Pose: Impact knock on door!
            SetKnockSprite(1);
            PlayKnockSFX();
            yield return new WaitForSeconds(0.20f);

            // Pose: Recoil between knocks
            SetKnockSprite(2);
            yield return new WaitForSeconds(0.14f);

            // Second knock!
            SetKnockSprite(1);
            PlayKnockSFX();
            yield return new WaitForSeconds(0.22f);

            // Recoil
            SetKnockSprite(2);
            yield return new WaitForSeconds(0.18f);

            // Settle to idle facing door
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

anu_walk_code = """using System.Collections;
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

    [Header("2D Walking and Knocking Sprites (Alternating Steps)")]
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
    [Tooltip("Position where Anu stops before the door.")]
    [SerializeField] private Vector2 doorPosition = new Vector2(-440f, -70f);

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
        // If U8_Part1_DoorController_Masters_Activity is present, it will drive the sequence
        var doorController = GetComponent<U8_Part1_DoorController_Masters_Activity>();
        if (doorController != null)
        {
            return;
        }

        if (playAutomatically)
        {
            StartWalkAndKnock();
        }
    }

    public void StartWalkAndKnock()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(WalkAndKnockSequence());
    }

    private IEnumerator WalkAndKnockSequence()
    {
        if (anu == null)
        {
            Debug.LogError("U8_AnuWalkToDoor: Anu RectTransform is not assigned.");
            yield break;
        }

        if (anuImage == null)
        {
            Debug.LogError("U8_AnuWalkToDoor: Anu Image is not assigned.");
            yield break;
        }

        sequenceFinished = false;
        HideChoiceButtons();

        // Check if Anu has U8_AnuWalkingController_Masters_Activity
        var dedicatedWalkCtrl = anu.GetComponent<U8_AnuWalkingController_Masters_Activity>();
        if (dedicatedWalkCtrl != null)
        {
            bool arrived = false;
            dedicatedWalkCtrl.StartWalkAndKnockSequence(() => {
                arrived = true;
            });
            while (!arrived)
            {
                yield return null;
            }
            ShowChoiceButtons();
            sequenceFinished = true;
            Debug.Log("U8 Part 1: Anu reached the door, knocked, and choices are now available.");
            yield break;
        }

        // Direct fallback walking loop
        anu.anchoredPosition = startPosition;
        anu.localEulerAngles = Vector3.zero;
        anu.localScale = Vector3.one;
        anuImage.preserveAspect = true;

        if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
            anuImage.sprite = walkSprites[0];
        else if (anuNormalSprite != null)
            anuImage.sprite = anuNormalSprite;

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
                    if (walkSprites[frameIndex] != null)
                    {
                        anuImage.sprite = walkSprites[frameIndex];
                    }
                }
            }

            yield return null;
        }

        anu.anchoredPosition = doorPosition;

        if (idleFacingDoorSprite != null)
            anuImage.sprite = idleFacingDoorSprite;
        else if (anuNormalSprite != null)
            anuImage.sprite = anuNormalSprite;

        yield return new WaitForSeconds(0.22f);

        // Knocking sequence
        for (int i = 0; i < numberOfKnocks; i++)
        {
            if (knockSprites != null && knockSprites.Length > 0 && knockSprites[0] != null)
                anuImage.sprite = knockSprites[0];
            yield return new WaitForSeconds(knockTime * 0.6f);

            if (knockSprites != null && knockSprites.Length > 1 && knockSprites[1] != null)
                anuImage.sprite = knockSprites[1];
            else if (anuKnockingSprite != null)
                anuImage.sprite = anuKnockingSprite;

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Knock");
            }
            yield return new WaitForSeconds(knockTime);

            if (knockSprites != null && knockSprites.Length > 2 && knockSprites[2] != null)
                anuImage.sprite = knockSprites[2];
            yield return new WaitForSeconds(knockPause);
        }

        if (idleFacingDoorSprite != null)
            anuImage.sprite = idleFacingDoorSprite;
        else if (anuNormalSprite != null)
            anuImage.sprite = anuNormalSprite;

        yield return new WaitForSeconds(0.15f);

        ShowChoiceButtons();
        sequenceFinished = true;
        activeRoutine = null;

        Debug.Log("U8 Part 1: Anu reached the door, knocked, and choices are now available.");
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

    public void ResetSequence()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        sequenceFinished = false;
        if (anu != null) anu.anchoredPosition = startPosition;
        HideChoiceButtons();
        StartWalkAndKnock();
    }

    public bool IsSequenceFinished() => sequenceFinished;
}
"""

targets = [
    (r'D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs', walking_ctrl_code),
    (r'c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs', walking_ctrl_code),
    (r'D:\engsnap\MastersActivityUnit6\Assets\Own_Scripts\U8_AnuWalkToDoor.cs', anu_walk_code),
    (r'c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\Own_Scripts\U8_AnuWalkToDoor.cs', anu_walk_code)
]

for path, code in targets:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(code)
    print('Deployed to:', path)
