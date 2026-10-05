import os

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
            AutoWireComponents();

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
        }

        public void ResetScreen()
        {
            isProcessingChoice = false;
            if (doorImage != null && doorClosedSprite != null) doorImage.sprite = doorClosedSprite;
            if (shutSignObject != null) shutSignObject.SetActive(true);

            if (promptText != null) promptText.text = "Anu needs the washroom.";
            if (feedbackText != null) feedbackText.text = "";

            // Hide choice buttons while Anu is walking to the door
            SetChoiceButtonsActive(false);
            SetButtonsInteractable(false);

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
            if (choiceContainer != null)
            {
                choiceContainer.SetActive(active);
                foreach (Transform child in choiceContainer.transform)
                {
                    child.gameObject.SetActive(active);
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

targets = [
    r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part1_DoorController_Masters_Activity.cs",
    r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part1_DoorController_Masters_Activity.cs",
]

for p in targets:
    with open(p, "w", encoding="utf-8") as fp:
        fp.write(door_ctrl_code)
    print("Updated DoorController:", p)
