using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public class U8_EndingScreen_Masters_Activity : MonoBehaviour
    {
        [Header("Celebration UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI discussionQuestionText;

        [Header("Star Icons")]
        [SerializeField] private List<Image> starImages = new List<Image>();
        [SerializeField] private Sprite starEarnedSprite;
        [SerializeField] private Sprite starEmptySprite;

        [Header("Visual Elements")]
        [SerializeField] private Image meeraWavingAvatar;
        [SerializeField] private GameObject confettiEffectObject;
        [SerializeField] private Button btnRestartActivity;

        private void Awake()
        {
            AutoFindUIReferences();
            if (btnRestartActivity != null)
            {
                btnRestartActivity.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(PlayEndingSequence());
        }

        private void AutoFindUIReferences()
        {
            if (titleText == null)
            {
                Transform t = transform.Find("TitleCard/TitleText") ?? transform.Find("TitleText");
                if (t != null) titleText = t.GetComponent<TextMeshProUGUI>();
            }
            if (discussionQuestionText == null)
            {
                Transform t = transform.Find("QuestionCard/DiscussionQuestionText") ?? transform.Find("DiscussionQuestionText");
                if (t != null) discussionQuestionText = t.GetComponent<TextMeshProUGUI>();
            }
            if (btnRestartActivity == null)
            {
                Transform t = transform.Find("Btn_Restart") ?? transform.Find("BottomBar/Btn_Restart");
                if (t != null) btnRestartActivity = t.GetComponent<Button>();
            }
            if (starImages == null || starImages.Count == 0)
            {
                starImages = new List<Image>();
                for (int i = 1; i <= 3; i++)
                {
                    Transform t = transform.Find($"Star_{i}") ?? transform.Find($"StarsContainer/Star_{i}");
                    if (t != null)
                    {
                        Image img = t.GetComponent<Image>();
                        if (img != null) starImages.Add(img);
                    }
                }
            }

            if (meeraWavingAvatar == null)
            {
                Transform t = transform.Find("MeeraWavingAvatar") ?? transform.Find("MeeraAvatar") ?? transform.Find("Meera_Image");
                if (t != null) meeraWavingAvatar = t.GetComponent<Image>();
            }
            if (meeraWavingAvatar != null)
            {
                meeraWavingAvatar.preserveAspect = true;
            }

            if (starEarnedSprite == null || starEmptySprite == null)
            {
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var s in allSprites)
                {
                    if (starEarnedSprite == null && s != null && s.name == "SPR_Icon_GoldStar")
                        starEarnedSprite = s;
                    if (starEmptySprite == null && s != null && s.name == "SPR_Icon_StarOutline")
                        starEmptySprite = s;
                }
            }
        }

        private IEnumerator PlayEndingSequence()
        {
            if (meeraWavingAvatar != null)
            {
                meeraWavingAvatar.preserveAspect = true;
            }

            if (titleText != null) titleText.text = "READY FOR THE NEXT PERSON!";
            if (subtitleText != null) subtitleText.text = "Washroom Etiquette Complete!";
            if (discussionQuestionText != null) discussionQuestionText.text = "";

            // Reset stars to empty outlines initially
            foreach (var star in starImages)
            {
                if (star != null)
                {
                    star.preserveAspect = true;
                    if (starEmptySprite != null) star.sprite = starEmptySprite;
                    star.transform.localScale = Vector3.one;
                    star.color = Color.white;
                }
            }

            if (confettiEffectObject != null) confettiEffectObject.SetActive(true);

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Confetti");
                U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Clap");
                U8_AudioManager_Masters_Activity.Instance.PlayBGM("MUS_Win", false);
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_15"); // "Three stars! Ready for the next person!"
            }

            yield return new WaitForSeconds(1.0f);

            int totalEarned = (U8_GameManager_Masters_Activity.Instance != null) 
                ? U8_GameManager_Masters_Activity.Instance.StarCount 
                : 3;

            // Reveal stars one by one with pop animation into gold stars
            for (int i = 0; i < totalEarned && i < starImages.Count; i++)
            {
                if (starImages[i] != null)
                {
                    if (starEarnedSprite != null) starImages[i].sprite = starEarnedSprite;
                    if (U8_AudioManager_Masters_Activity.Instance != null)
                    {
                        U8_AudioManager_Masters_Activity.Instance.PlaySFX("SFX_Star");
                    }

                    // Punch / Pop animation from 1.0 -> 1.35 -> 1.0
                    for (float t = 0; t < 1f; t += Time.deltaTime * 6f)
                    {
                        float s = Mathf.Sin(t * Mathf.PI) * 0.35f + 1f;
                        starImages[i].transform.localScale = Vector3.one * s;
                        yield return null;
                    }
                    starImages[i].transform.localScale = Vector3.one;
                }
                yield return new WaitForSeconds(0.4f);
            }

            yield return new WaitForSeconds(1.5f);

            // Hold discussion question on screen for teacher closure
            if (discussionQuestionText != null)
            {
                discussionQuestionText.text = "Would the next person be happy?";
            }

            if (U8_AudioManager_Masters_Activity.Instance != null)
            {
                U8_AudioManager_Masters_Activity.Instance.PlayVO("VO_U8_16"); // "Would the next person be happy?"
            }
        }

        private void OnRestartClicked()
        {
            if (U8_GameManager_Masters_Activity.Instance != null)
            {
                U8_GameManager_Masters_Activity.Instance.ResetUnitState();
                U8_GameManager_Masters_Activity.Instance.SetStarCount(0);
                U8_GameManager_Masters_Activity.Instance.ShowPart(U8_GamePart.Part1_Door);
            }
        }
    }
}
