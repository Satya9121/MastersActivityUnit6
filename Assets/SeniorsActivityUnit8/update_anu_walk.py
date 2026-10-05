import os

code = """using System.Collections;
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
    [SerializeField] private float stepFrameRate = 0.12f;

    [Header("Walking Settings")]
    [SerializeField] private float walkDuration = 2.0f;
    [SerializeField] private float walkingBobAmount = 4f;
    [SerializeField] private float walkingBobSpeed = 10f;

    [Header("Door Destination")]
    [Tooltip("Position where Anu should stop before the door.")]
    [SerializeField] private Vector2 doorPosition = new Vector2(-382f, -70f);

    [Header("Knocking Settings")]
    [SerializeField] private int numberOfKnocks = 2;
    [SerializeField] private float knockDistance = 8f;
    [SerializeField] private float knockTime = 0.15f;
    [SerializeField] private float knockPause = 0.2f;

    [Header("Existing Choice Buttons")]
    [SerializeField] private GameObject btnKnockAndWait;
    [SerializeField] private GameObject btnBangLoudly;
    [SerializeField] private GameObject btnPushOpen;

    [Header("Optional Parent")]
    [Tooltip("Optional. If assigned, this can be used to enable/disable the whole choice-button area.")]
    [SerializeField] private GameObject choiceButtonsParent;

    [Header("Behaviour")]
    [SerializeField] private bool playAutomatically = true;

    private Vector2 startPosition;
    private bool sequenceFinished;
    private Coroutine activeRoutine;

    private void Awake()
    {
        AutoFindReferences();

        if (anu != null)
        {
            startPosition = anu.anchoredPosition;
        }

        // Hide choices before Anu reaches the door
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

        // Otherwise execute custom alternating footstep sequence
        startPosition = anu.anchoredPosition;

        // Ensure Anu faces the washroom door (facing left)
        anu.localEulerAngles = Vector3.zero;
        anu.localScale = Vector3.one;
        anuImage.preserveAspect = true;

        if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
        {
            anuImage.sprite = walkSprites[0];
        }
        else if (anuNormalSprite != null)
        {
            anuImage.sprite = anuNormalSprite;
        }

        // ---------------------------------------------------------
        // STEP 1: WALK TO THE DOOR (Visibly alternating steps)
        // ---------------------------------------------------------
        float elapsed = 0f;
        float frameTimer = 0f;
        int frameIndex = 0;

        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / walkDuration);

            // Natural smooth ease in-out
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            Vector2 movementPosition = Vector2.Lerp(startPosition, doorPosition, smoothProgress);

            // Subtle vertical ground bounce
            float walkingBob = Mathf.Sin(elapsed * walkingBobSpeed) * walkingBobAmount;
            movementPosition.y += walkingBob;
            anu.anchoredPosition = movementPosition;

            // Advance walking frames (alternating feet)
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

        // Put Anu precisely at the door destination
        anu.anchoredPosition = doorPosition;

        // Settle into idle facing door
        if (idleFacingDoorSprite != null)
        {
            anuImage.sprite = idleFacingDoorSprite;
        }
        else if (anuNormalSprite != null)
        {
            anuImage.sprite = anuNormalSprite;
        }
        yield return new WaitForSeconds(0.25f);

        // ---------------------------------------------------------
        // STEP 2 & 3: KNOCK ON THE DOOR
        // ---------------------------------------------------------
        for (int i = 0; i < numberOfKnocks; i++)
        {
            // Fist raise pose
            if (knockSprites != null && knockSprites.Length > 0 && knockSprites[0] != null)
            {
                anuImage.sprite = knockSprites[0];
            }
            yield return new WaitForSeconds(knockTime * 0.7f);

            // Knock impact pose
            if (knockSprites != null && knockSprites.Length > 1 && knockSprites[1] != null)
            {
                anuImage.sprite = knockSprites[1];
            }
            else if (anuKnockingSprite != null)
            {
                anuImage.sprite = anuKnockingSprite;
            }

            // Knock sound
            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Knock");
            }

            yield return new WaitForSeconds(knockTime);

            // Recoil pose
            if (knockSprites != null && knockSprites.Length > 2 && knockSprites[2] != null)
            {
                anuImage.sprite = knockSprites[2];
            }
            yield return new WaitForSeconds(knockPause);
        }

        // Standing idle pose facing the door
        if (idleFacingDoorSprite != null)
        {
            anuImage.sprite = idleFacingDoorSprite;
        }
        else if (anuNormalSprite != null)
        {
            anuImage.sprite = anuNormalSprite;
        }
        yield return new WaitForSeconds(0.15f);

        // ---------------------------------------------------------
        // STEP 4: SHOW THE THREE ANSWER BUTTONS
        // ---------------------------------------------------------
        ShowChoiceButtons();
        sequenceFinished = true;
        activeRoutine = null;

        Debug.Log("U8 Part 1: Anu reached the door, knocked, and choices are now available.");
    }

    private void HideChoiceButtons()
    {
        if (choiceButtonsParent != null)
        {
            choiceButtonsParent.SetActive(false);
        }
        if (btnKnockAndWait != null) btnKnockAndWait.SetActive(false);
        if (btnBangLoudly != null) btnBangLoudly.SetActive(false);
        if (btnPushOpen != null) btnPushOpen.SetActive(false);
    }

    private void ShowChoiceButtons()
    {
        if (choiceButtonsParent != null)
        {
            choiceButtonsParent.SetActive(true);
        }
        if (btnKnockAndWait != null) btnKnockAndWait.SetActive(true);
        if (btnBangLoudly != null) btnBangLoudly.SetActive(true);
        if (btnPushOpen != null) btnPushOpen.SetActive(true);
    }

    public void ResetSequence()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        sequenceFinished = false;

        if (anu != null)
        {
            anu.anchoredPosition = startPosition;
        }

        if (anuImage != null)
        {
            if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
                anuImage.sprite = walkSprites[0];
            else if (anuNormalSprite != null)
                anuImage.sprite = anuNormalSprite;
        }

        HideChoiceButtons();

        if (playAutomatically)
        {
            StartWalkAndKnock();
        }
    }

    public bool IsSequenceFinished()
    {
        return sequenceFinished;
    }
}
"""

targets = [
    r'D:\engsnap\MastersActivityUnit6\Assets\Own_Scripts\U8_AnuWalkToDoor.cs',
    r'c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\Own_Scripts\U8_AnuWalkToDoor.cs'
]

for t in targets:
    os.makedirs(os.path.dirname(t), exist_ok=True)
    with open(t, 'w', encoding='utf-8') as f:
        f.write(code)
    print('Successfully updated:', t)
