import os

ctrl_code = """using System;
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

        [Header("Left-View Walking and Knocking Sprites")]
        [SerializeField] private Sprite[] walkSprites = new Sprite[4];
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] knockSprites = new Sprite[3];
        [SerializeField] private Sprite sheepishSprite;
        [SerializeField] private Sprite sheepishSideSprite;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pushSprite;
        [SerializeField] private Sprite[] bangSprites = new Sprite[2];

        private Coroutine activeRoutine;
        private bool isWalking = false;

        public bool IsWalking => isWalking;
        public Vector2 TargetDoorPosition => doorPosition;

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (anuImage == null) anuImage = GetComponent<Image>();
            if (animator == null) animator = GetComponent<Animator>();

            EnsureActionSpritesLoaded();
            AutoAssignSpritesIfNeeded();

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(580f, 754f);
            }

            if (anuImage != null)
            {
                anuImage.preserveAspect = true;
            }

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.enabled = true;
            }
        }

        private void Start()
        {
            EnsureActionSpritesLoaded();
        }

        private void OnValidate()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (anuImage == null) anuImage = GetComponent<Image>();
            if (animator == null) animator = GetComponent<Animator>();
            AutoAssignSpritesIfNeeded();
            EnsureActionSpritesLoaded();
        }

        public void EnsureActionSpritesLoaded()
        {
#if UNITY_EDITOR
            if (pushSprite == null)
            {
                pushSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Push.png");
            }
            if (bangSprites == null || bangSprites.Length < 2 || bangSprites[0] == null || bangSprites[1] == null)
            {
                bangSprites = new Sprite[2];
                bangSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Bang_1.png");
                bangSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Bang_2.png");
            }
            if (sheepishSideSprite == null)
            {
                sheepishSideSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Sheepish_Side.png");
            }
            if (idleSprite == null)
            {
                idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_Idle.png");
            }
#endif
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

            if (pushSprite == null)
            {
                pushSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Push.png");
            }
            if (bangSprites == null || bangSprites.Length < 2 || bangSprites[0] == null)
            {
                bangSprites = new Sprite[2];
                bangSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Bang_1.png");
                bangSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Bang_2.png");
            }
            if (sheepishSideSprite == null)
            {
                sheepishSideSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Sheepish_Side.png");
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
                rectTransform.sizeDelta = new Vector2(580f, 754f);
            }

            bool hasAnimator = (animator != null && animator.runtimeAnimatorController != null);
            if (hasAnimator)
            {
                animator.enabled = true;
                animator.SetBool("IsWalking", true);
                animator.Play("Walk", 0, 0f);
            }
            else
            {
                SetSpriteSafely((walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
                    ? walkSprites[0]
                    : (idleSprite ?? normalSprite));
            }

            yield return null;

            // 2. Walk naturally across toward the closed washroom door (left view)
            float startX = startPosition.x;
            float targetX = doorPosition.x;
            float currentX = startX;
            float stepSpeed = walkSpeed > 0 ? walkSpeed : 280f;
            float walkTimer = 0f;
            float stepTimer = 0f;
            int stepIndex = 0;

            while (Mathf.Abs(currentX - targetX) > 1.5f)
            {
                walkTimer += Time.deltaTime;
                stepTimer += Time.deltaTime;

                currentX = Mathf.MoveTowards(currentX, targetX, stepSpeed * Time.deltaTime);

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(currentX, doorPosition.y);
                }

                // Fallback frame advancement if animator is absent
                if (!hasAnimator)
                {
                    if (stepTimer >= stepFrameDuration)
                    {
                        stepTimer = 0f;
                        stepIndex = (stepIndex + 1) % 4;

                        if (walkSprites != null && walkSprites.Length >= 4 && walkSprites[stepIndex] != null)
                        {
                            SetSpriteSafely(walkSprites[stepIndex]);
                        }
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

            if (hasAnimator)
            {
                animator.SetBool("IsWalking", false);
                animator.Play("Idle", 0, 0f);
            }
            else
            {
                SetIdlePose();
            }

            yield return new WaitForSeconds(0.15f);

            activeRoutine = null;
            onArrived?.Invoke();
        }

        /// <summary>
        /// Knocks directly on the door labeled SHUT when Knock and Wait button is clicked.
        /// </summary>
        public IEnumerator PlayKnockSequenceRoutine()
        {
            bool hasAnimator = (animator != null && animator.runtimeAnimatorController != null);
            if (hasAnimator)
            {
                animator.enabled = true;
                animator.SetTrigger("Knock");
                animator.Play("Knock", 0, 0f);

                PlayKnockSFX();
                yield return new WaitForSeconds(0.35f);
                PlayKnockSFX();
                yield return new WaitForSeconds(0.80f);

                animator.SetBool("IsWalking", false);
                animator.Play("Idle", 0, 0f);
            }
            else
            {
                // Fallback manual knocking sequence
                SetKnockSprite(0);
                yield return new WaitForSeconds(0.15f);

                SetKnockSprite(1);
                PlayKnockSFX();
                yield return new WaitForSeconds(0.20f);

                SetKnockSprite(2);
                yield return new WaitForSeconds(0.14f);

                // Second knock
                SetKnockSprite(1);
                PlayKnockSFX();
                yield return new WaitForSeconds(0.22f);

                SetKnockSprite(2);
                yield return new WaitForSeconds(0.18f);

                SetIdlePose();
            }
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
            EnsureActionSpritesLoaded();
            if (animator != null && animator.runtimeAnimatorController != null && animator.enabled)
            {
                animator.SetBool("IsWalking", false);
                animator.Play("Idle", 0, 0f);
            }
            else
            {
                SetSpriteSafely(idleSprite ?? normalSprite);
            }
        }

        public void SetSheepishPose()
        {
            EnsureActionSpritesLoaded();
            if (animator != null && animator.enabled)
            {
                animator.enabled = false;
            }
            SetSpriteSafely(sheepishSideSprite ?? sheepishSprite ?? normalSprite);
        }

        /// <summary>
        /// Plays rapid banging on the washroom door with fists.
        /// </summary>
        public IEnumerator PlayBangSequenceRoutine(Action onHit = null)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            EnsureActionSpritesLoaded();

            if (animator != null && animator.enabled)
            {
                animator.enabled = false;
            }

            Vector2 basePos = rectTransform != null ? rectTransform.anchoredPosition : doorPosition;

            for (int i = 0; i < 3; i++)
            {
                if (rectTransform != null) rectTransform.anchoredPosition = new Vector2(basePos.x - 7f, basePos.y);
                SetSpriteSafely((bangSprites != null && bangSprites.Length > 0 && bangSprites[0] != null) 
                    ? bangSprites[0] : (knockSprites != null && knockSprites.Length > 0 ? knockSprites[0] : idleSprite));
                onHit?.Invoke();
                yield return new WaitForSeconds(0.16f);

                if (rectTransform != null) rectTransform.anchoredPosition = basePos;
                SetSpriteSafely((bangSprites != null && bangSprites.Length > 1 && bangSprites[1] != null) 
                    ? bangSprites[1] : (idleSprite ?? normalSprite));
                yield return new WaitForSeconds(0.14f);
            }

            if (rectTransform != null) rectTransform.anchoredPosition = basePos;
            yield return new WaitForSeconds(0.15f);
        }

        /// <summary>
        /// Plays pushing against the door with palms flat, leaning into it.
        /// </summary>
        public IEnumerator PlayPushSequenceRoutine(Action onPush = null)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            EnsureActionSpritesLoaded();

            if (animator != null && animator.enabled)
            {
                animator.enabled = false;
            }

            SetSpriteSafely(pushSprite != null ? pushSprite : (idleSprite ?? normalSprite));
            onPush?.Invoke();

            Vector2 basePos = rectTransform != null ? rectTransform.anchoredPosition : doorPosition;
            if (rectTransform != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    float jitter = Mathf.Sin(i * 2.2f) * 2.5f;
                    rectTransform.anchoredPosition = new Vector2(basePos.x - 8f + jitter, basePos.y);
                    yield return new WaitForSeconds(0.10f);
                }
                rectTransform.anchoredPosition = doorPosition;
            }
            else
            {
                yield return new WaitForSeconds(1.2f);
            }

            SetIdlePose();
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

        /// <summary>
        /// Walks Anu from her standing position in front of the door into the washroom doorway.
        /// </summary>
        public IEnumerator WalkIntoWashroomRoutine(Vector2 targetPos, Action onEntered = null)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            isWalking = true;

            bool hasAnimator = (animator != null && animator.runtimeAnimatorController != null);
            if (hasAnimator)
            {
                animator.enabled = true;
                animator.SetBool("IsWalking", true);
                animator.Play("Walk", 0, 0f);
            }

            float startX = rectTransform != null ? rectTransform.anchoredPosition.x : doorPosition.x;
            float targetX = targetPos.x;
            float currentX = startX;
            float stepSpeed = walkSpeed > 0 ? walkSpeed : 260f;
            float walkTimer = 0f;
            float stepTimer = 0f;
            int stepIndex = 0;

            while (currentX > targetX)
            {
                walkTimer += Time.deltaTime;
                stepTimer += Time.deltaTime;

                currentX = Mathf.MoveTowards(currentX, targetX, stepSpeed * Time.deltaTime);

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(currentX, doorPosition.y);
                }

                if (!hasAnimator)
                {
                    if (stepTimer >= stepFrameDuration)
                    {
                        stepTimer = 0f;
                        stepIndex = (stepIndex + 1) % 4;
                        if (walkSprites != null && walkSprites.Length >= 4 && walkSprites[stepIndex] != null)
                        {
                            SetSpriteSafely(walkSprites[stepIndex]);
                        }
                    }
                }

                yield return null;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = targetPos;
            }
            isWalking = false;

            if (hasAnimator)
            {
                animator.SetBool("IsWalking", false);
                animator.Play("Idle", 0, 0f);
            }
            else
            {
                SetIdlePose();
            }

            yield return new WaitForSeconds(0.12f);
            activeRoutine = null;
            onEntered?.Invoke();
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

targets = [
    r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs",
    r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs"
]

for t in targets:
    with open(t, "w", encoding="utf-8") as f:
        f.write(ctrl_code)
    print("Written:", t)
