using System.Collections;
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
        var doorController = GetComponent<U8_Part1_DoorController_Masters_Activity>();
        if (doorController != null)
        {
            enabled = false;
            return;
        }
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
        if (choiceButtonsParent != null)
        {
            choiceButtonsParent.SetActive(true);
            foreach (Transform child in choiceButtonsParent.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        if (btnKnockAndWait != null) btnKnockAndWait.SetActive(true);
        if (btnBangLoudly != null) btnBangLoudly.SetActive(true);
        if (btnPushOpen != null) btnPushOpen.SetActive(true);
    }

    public bool IsSequenceFinished() => sequenceFinished;
}
