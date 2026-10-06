using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Googolplex.Unit6
{
    [System.Serializable]
    public class U6_WaiterMomentData_Masters_Activity
    {
        public string situationTitle;
        public string situationPrompt;
        public Sprite fullBodyPoseSprite;
        public Sprite propSprite;
        public Sprite anuReactionSprite;

        public string optionA_Badge; // E.g. "[POLITE: PLEASE]"
        [TextArea] public string optionA_Text;
        [TextArea] public string optionA_Outcome;
        public string optionA_VO;
        public AudioClip optionA_Clip;
        public U6_WaiterFaceState optionA_Face;
        public Sprite optionA_Avatar;
        public Sprite optionA_RaviPose;

        public string optionB_Badge; // E.g. "[BLUNT: SHOUT]"
        [TextArea] public string optionB_Text;
        [TextArea] public string optionB_Outcome;
        public string optionB_VO;
        public AudioClip optionB_Clip;
        public U6_WaiterFaceState optionB_Face;
        public Sprite optionB_Avatar;
        public Sprite optionB_RaviPose;
    }

    public class U6_WaiterInteractionScreen_Masters_Activity : MonoBehaviour
    {
        [Header("Title Banner (Shown Only At Beginning)")]
        [SerializeField] private GameObject titleBanner;
        [SerializeField] private float titleBannerDuration = 3.5f;
        private Coroutine hideTitleBannerCoroutine;

        [Header("Situation Prompt")]
        [SerializeField] private TextMeshProUGUI promptText;

        [Header("Full Body Characters & Props Stage")]
        [SerializeField] private Image waiterFullBodyAvatar;
        [SerializeField] private Image anuCharacterAvatar;
        [SerializeField] private Image propItemImage;

        [Header("Table Water Glasses (3 People on Table)")]
        [SerializeField] private Image[] tableWaterGlasses;
        [SerializeField] private Sprite filledWaterGlassSprite; // "Sprite water glasses_2"
        private Sprite[] cachedDefaultGlassSprites;
        private bool waterHasBeenServed = false;

        [Header("Table Ordered Dish (User's Dish GameObject)")]
        [SerializeField] private GameObject tableDishObject;
        [SerializeField] private Image tableDishImage;
        private bool foodHasBeenDelivered = false;

        [Header("Dish Sprites Cache (U6 MA Picture Menu Dishes)")]
        [SerializeField] private Sprite spriteDishDosa;
        [SerializeField] private Sprite spriteDishIdli;
        [SerializeField] private Sprite spriteDishNoodles;
        [SerializeField] private Sprite spriteDishRice;
        [SerializeField] private Sprite spriteDishRoti;
        [SerializeField] private Sprite spriteDishIceCream;

        [Header("Pre-Reader Emotion Avatars & Preview")]
        [SerializeField] private Sprite politeAvatarSprite;
        [SerializeField] private Sprite impoliteAvatarSprite;

        [Header("Family Dining Table Sprites (Parents + Anu)")]
        [SerializeField] private Sprite familyTableCalmSprite;
        [SerializeField] private Sprite familyTableHandRaiseSprite;
        [SerializeField] private Sprite familyTableReactSprite;

        [Header("Full Body Waiter Poses (u6 MA more waiter ravi sprites)")]
        [SerializeField] private Sprite raviGreeting;
        [SerializeField] private Sprite raviTakingOrder;
        [SerializeField] private Sprite raviPouring;
        [SerializeField] private Sprite raviServing;
        [SerializeField] private Sprite raviCutlery;
        [SerializeField] private Sprite raviApology;
        [SerializeField] private Sprite raviSurprised;
        [SerializeField] private Sprite raviClearing;
        [SerializeField] private Sprite raviBill;
        [SerializeField] private Sprite raviWave;

        [Header("Waiter Reaction Badge & State")]
        [SerializeField] private Image waiterExpressionBadge;
        [SerializeField] private Sprite waiterSmileSprite;
        [SerializeField] private Sprite waiterNeutralSprite;
        [SerializeField] private Sprite waiterStiffSprite;
        [SerializeField] private TextMeshProUGUI waiterNameBadge;

        [Header("Speech Options (Styled to match MenuScreen OrderChoicePanel)")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private Button optionA_Button;
        [SerializeField] private TextMeshProUGUI optionA_Label;
        [SerializeField] private Button optionB_Button;
        [SerializeField] private TextMeshProUGUI optionB_Label;
        [SerializeField] private Sprite button9SliceSprite;

        [Header("Choice Container & Card Sizing (Big & Clear for Kids)")]
        [SerializeField] private Vector2 choiceContainerSize = new Vector2(1750f, 210f);
        [SerializeField] private Vector2 choiceContainerPos = new Vector2(0f, 35f);
        [SerializeField] private Vector2 avatarIconSize = new Vector2(140f, 140f);
        [SerializeField] private Vector2 listenButtonSize = new Vector2(140f, 75f);
        [SerializeField] private float labelFontSizeMin = 26f;
        [SerializeField] private float labelFontSizeMax = 44f;

        [Header("Outcome Feedback")]
        [SerializeField] private GameObject outcomePanel;
        [SerializeField] private TextMeshProUGUI outcomeText;

#pragma warning disable 0414
        [Header("Audios & SFX Used On This Screen (Inspector Priority)")]
        [SerializeField] private string voRaviIntro = "VO_U6_08 (This is Ravi...)";
        [SerializeField] private AudioClip voRaviIntroClip;
        [SerializeField] private string voWrongDish = "VO_U6_09 (Oh! That is the wrong dish!)";
        [SerializeField] private AudioClip voWrongDishClip;
        [SerializeField] private string voAnuThankYou = "VO_U6_ANU_4 (Thank you!)";
        [SerializeField] private AudioClip voAnuThankYouClip;
        [SerializeField] private AudioClip voAnuWrongDishPoliteClip;

        [Header("Dynamic Wrong Dish Audio Clips (All 6 Dishes for Moment 2)")]
        [SerializeField] private AudioClip voWrongDishDosaClip;
        [SerializeField] private AudioClip voWrongDishIdliClip;
        [SerializeField] private AudioClip voWrongDishNoodlesClip;
        [SerializeField] private AudioClip voWrongDishRiceClip;
        [SerializeField] private AudioClip voWrongDishRotiClip;
        [SerializeField] private AudioClip voWrongDishIceCreamClip;
        [SerializeField] private string voAnuWrongDishLoud = "VO_U6_ANU_6 (This is WRONG!)";
        [SerializeField] private AudioClip voAnuWrongDishLoudClip;
        [SerializeField] private string voAnuDroppedFork = "VO_U6_ANU_8 (Excuse me, could I have another fork?)";
        [SerializeField] private AudioClip voAnuDroppedForkClip;
        [SerializeField] private string voAnuDroppedForkShout = "VO_U6_ANU_9 (I dropped my fork!)";
        [SerializeField] private AudioClip voAnuDroppedForkShoutClip;
        [SerializeField] private string voWaiterApologize = "VO_U6_WAIT_3 (I am so sorry, I will fix that!)";
        [SerializeField] private AudioClip voWaiterApologizeClip;
        [SerializeField] private string sfxPlateDown = "SFX_PlateDown (Plate set down)";
        [SerializeField] private AudioClip sfxPlateDownClip;
        [SerializeField] private string sfxForkDrop = "SFX_ForkDrop (Fork dropped on floor)";
        [SerializeField] private AudioClip sfxForkDropClip;
        [SerializeField] private string sfxSparkle = "SFX_Sparkle (Polite outcome)";
        [SerializeField] private AudioClip sfxSparkleClip;
#pragma warning restore 0414

        [Header("Moments List (Inspector Priority)")]
        [SerializeField] private List<U6_WaiterMomentData_Masters_Activity> moments = new List<U6_WaiterMomentData_Masters_Activity>();

        private int currentMomentIndex = 0;
        private int politeAnswersCount = 0;
        private bool isResolving = false;

        public void PlayScreenAudio(AudioClip clip, string soundId)
        {
            if (clip != null && U6_AudioManager_Masters_Activity.Instance != null)
            {
                U6_AudioManager_Masters_Activity.Instance.PlayVO(clip);
                return;
            }

            if (!string.IsNullOrEmpty(soundId) && U6_AudioManager_Masters_Activity.Instance != null)
            {
                U6_AudioManager_Masters_Activity.Instance.PlayVO(soundId);
            }
        }

        public void PlayScreenSFX(AudioClip clip, string soundId)
        {
            if (clip != null && U6_AudioManager_Masters_Activity.Instance != null)
            {
                U6_AudioManager_Masters_Activity.Instance.PlaySFX(clip);
                return;
            }

            if (!string.IsNullOrEmpty(soundId) && U6_AudioManager_Masters_Activity.Instance != null)
            {
                U6_AudioManager_Masters_Activity.Instance.PlaySFX(soundId);
            }
        }

        private void Awake()
        {
            if (optionA_Button != null)
                optionA_Button.onClick.AddListener(() => OnOptionSelected(true));

            if (optionB_Button != null)
                optionB_Button.onClick.AddListener(() => OnOptionSelected(false));

            AutoFindUIReferences();
            CacheDefaultWaterGlasses();
            EnsureFilledWaterGlassSprite();
            EnsurePreReaderAssets();
            EnsureDishSprites();
#if UNITY_EDITOR
            EnsureWrongDishClips();
#endif
        }

        private void OnEnable()
        {
            AutoFindUIReferences();
            CacheDefaultWaterGlasses();
            EnsureFilledWaterGlassSprite();
            EnsurePreReaderAssets();
            EnsureDishSprites();
#if UNITY_EDITOR
            EnsureWrongDishClips();
            AutoLoadSpritesAndMoments();
#else
            InitializeDefaultMoments();
#endif
            currentMomentIndex = 0;
            politeAnswersCount = 0;
            waterHasBeenServed = false;
            foodHasBeenDelivered = false;
            SetWaterGlassesFilled(false, animate: false);
            UpdateTableDishVisibility(false, animate: false);
            if (waiterNameBadge) waiterNameBadge.text = "Ravi";

            PlayScreenAudio(voRaviIntroClip, voRaviIntro);

            if (titleBanner != null)
            {
                titleBanner.SetActive(true);
                CanvasGroup cg = titleBanner.GetComponent<CanvasGroup>();
                if (cg == null) cg = titleBanner.AddComponent<CanvasGroup>();
                cg.alpha = 1f;

                if (hideTitleBannerCoroutine != null) StopCoroutine(hideTitleBannerCoroutine);
                hideTitleBannerCoroutine = StartCoroutine(HideTitleBannerRoutine());
            }

            DisplayCurrentMoment();
        }

        private void AutoFindUIReferences()
        {
            if (titleBanner == null)
            {
                Transform tb = transform.Find("TitleBanner") 
                            ?? transform.Find("SafeArea/TitleBanner");
                if (tb == null)
                {
                    foreach (var t in GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.Equals("TitleBanner", System.StringComparison.OrdinalIgnoreCase))
                        {
                            tb = t;
                            break;
                        }
                    }
                }
                if (tb != null) titleBanner = tb.gameObject;
            }
            if (promptText == null) promptText = GetComponentInChildren<TextMeshProUGUI>(true);
            if (choiceContainer == null)
            {
                Transform ct = transform.Find("ChoiceContainer") ?? transform.Find("SafeArea/ChoiceContainer");
                if (ct != null) choiceContainer = ct.gameObject;
            }
            if (choiceContainer != null)
            {
                RectTransform ccRT = choiceContainer.GetComponent<RectTransform>();
                if (ccRT != null)
                {
                    ccRT.anchorMin = new Vector2(0.5f, 0f);
                    ccRT.anchorMax = new Vector2(0.5f, 0f);
                    ccRT.pivot = new Vector2(0.5f, 0f);
                    ccRT.sizeDelta = choiceContainerSize;
                    ccRT.anchoredPosition = choiceContainerPos;
                }
            }
            if (optionA_Button == null && choiceContainer != null)
            {
                Button[] btns = choiceContainer.GetComponentsInChildren<Button>(true);
                if (btns.Length > 0) optionA_Button = btns[0];
                if (btns.Length > 1) optionB_Button = btns[1];
            }
            if (optionA_Label == null && optionA_Button != null) optionA_Label = optionA_Button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (optionB_Label == null && optionB_Button != null) optionB_Label = optionB_Button.GetComponentInChildren<TextMeshProUGUI>(true);

            if (button9SliceSprite == null && optionA_Button != null)
            {
                var img = optionA_Button.GetComponent<Image>();
                if (img != null && img.sprite != null) button9SliceSprite = img.sprite;
            }

            if (waiterFullBodyAvatar == null)
            {
                Transform t = transform.Find("WaiterFullBody") 
                           ?? transform.Find("SafeArea/WaiterFullBody")
                           ?? transform.Find("WaiterAvatarBox") 
                           ?? transform.Find("Waiter_Image") 
                           ?? transform.Find("RaviAvatar");
                if (t != null) waiterFullBodyAvatar = t.GetComponentInChildren<Image>(true);
            }
            if (anuCharacterAvatar == null)
            {
                Transform t = transform.Find("AnuAvatar") 
                           ?? transform.Find("SafeArea/AnuAvatar")
                           ?? transform.Find("Anu_Avatar") 
                           ?? transform.Find("Anu") 
                           ?? transform.Find("AnuCharacter");
                if (t != null) anuCharacterAvatar = t.GetComponentInChildren<Image>(true);
            }
            if (propItemImage == null)
            {
                Transform t = transform.Find("PropItem") 
                           ?? transform.Find("SafeArea/PropItem")
                           ?? transform.Find("Prop_Item") 
                           ?? transform.Find("Prop") 
                           ?? transform.Find("PropImage") 
                           ?? transform.Find("DishImage");
                if (t != null) propItemImage = t.GetComponentInChildren<Image>(true);
            }
            if (tableWaterGlasses == null || tableWaterGlasses.Length == 0)
            {
                List<Image> glasses = new List<Image>();
                string[] glassNames = { "PropItem", "PropItem (1)", "PropItem (2)" };
                foreach (string gName in glassNames)
                {
                    Transform t = transform.Find(gName) ?? transform.Find($"SafeArea/{gName}");
                    if (t != null)
                    {
                        Image img = t.GetComponent<Image>();
                        if (img != null) glasses.Add(img);
                    }
                }
                if (glasses.Count > 0)
                {
                    tableWaterGlasses = glasses.ToArray();
                }
            }

            if (waiterExpressionBadge == null)
            {
                Transform t = transform.Find("ExpressionBadge") 
                           ?? transform.Find("SafeArea/ExpressionBadge")
                           ?? transform.Find("WaiterBadge")
                           ?? transform.Find("Badge");
                if (t != null) waiterExpressionBadge = t.GetComponentInChildren<Image>(true);
            }
            if (outcomePanel == null)
            {
                Transform t = transform.Find("OutcomePanel") ?? transform.Find("SafeArea/OutcomePanel");
                if (t != null) outcomePanel = t.gameObject;
            }
            if (outcomeText == null && outcomePanel != null)
            {
                outcomeText = outcomePanel.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (tableDishObject == null)
            {
                Transform dt = transform.Find("Dish") 
                            ?? transform.Find("SafeArea/Dish");
                if (dt == null)
                {
                    foreach (var t in GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.Equals("Dish", System.StringComparison.OrdinalIgnoreCase))
                        {
                            dt = t;
                            break;
                        }
                    }
                }
                if (dt != null)
                {
                    tableDishObject = dt.gameObject;
                    if (tableDishImage == null) tableDishImage = dt.GetComponent<Image>();
                }
            }
            if (tableDishImage == null && tableDishObject != null)
            {
                tableDishImage = tableDishObject.GetComponent<Image>();
            }
        }

        private void CacheDefaultWaterGlasses()
        {
            if (tableWaterGlasses != null && tableWaterGlasses.Length > 0)
            {
                if (cachedDefaultGlassSprites == null || cachedDefaultGlassSprites.Length != tableWaterGlasses.Length)
                {
                    cachedDefaultGlassSprites = new Sprite[tableWaterGlasses.Length];
                }
                for (int i = 0; i < tableWaterGlasses.Length; i++)
                {
                    if (tableWaterGlasses[i] != null)
                    {
                        if (cachedDefaultGlassSprites[i] == null)
                        {
                            cachedDefaultGlassSprites[i] = tableWaterGlasses[i].sprite;
                        }
                        tableWaterGlasses[i].preserveAspect = true;
                    }
                }
            }
        }

        private void EnsureFilledWaterGlassSprite()
        {
            if (filledWaterGlassSprite == null)
            {
                var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var sp in allSprites)
                {
                    if (sp != null && sp.name == "Sprite water glasses_2")
                    {
                        filledWaterGlassSprite = sp;
                        break;
                    }
                }
            }
        }

        public void SetWaterGlassesFilled(bool filled, bool animate = true)
        {
            waterHasBeenServed = filled;
            CacheDefaultWaterGlasses();
            EnsureFilledWaterGlassSprite();

            if (tableWaterGlasses == null) return;

            for (int i = 0; i < tableWaterGlasses.Length; i++)
            {
                var glass = tableWaterGlasses[i];
                if (glass == null) continue;

                glass.gameObject.SetActive(true);
                glass.preserveAspect = true;

                if (filled)
                {
                    if (filledWaterGlassSprite != null)
                    {
                        glass.sprite = filledWaterGlassSprite;
                    }
                }
                else
                {
                    if (cachedDefaultGlassSprites != null && i < cachedDefaultGlassSprites.Length && cachedDefaultGlassSprites[i] != null)
                    {
                        glass.sprite = cachedDefaultGlassSprites[i];
                    }
                }

                if (animate && gameObject.activeInHierarchy)
                {
                    StartCoroutine(PopAvatarAnimation(glass.transform));
                }
            }
        }

        private bool IsWaterGlassImage(Image img)
        {
            if (img == null) return false;
            if (tableWaterGlasses != null)
            {
                for (int i = 0; i < tableWaterGlasses.Length; i++)
                {
                    if (tableWaterGlasses[i] == img) return true;
                }
            }
            string n = img.gameObject.name;
            return n == "PropItem" || n == "PropItem (1)" || n == "PropItem (2)";
        }

        private void EnsurePreReaderAssets()
        {
            if (politeAvatarSprite == null || impoliteAvatarSprite == null || button9SliceSprite == null || familyTableCalmSprite == null || familyTableReactSprite == null)
            {
                var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var sp in allSprites)
                {
                    if (sp == null) continue;
                    if (politeAvatarSprite == null && (sp.name == "SPR_Anu_HandRaise" || sp.name == "SPR_Anu_SittingStraight"))
                    {
                        politeAvatarSprite = sp;
                    }
                    if (impoliteAvatarSprite == null && (sp.name == "SPR_Anu_ShoutingHungry" || sp.name == "SPR_Anu_FranticWave"))
                    {
                        impoliteAvatarSprite = sp;
                    }
                    if (button9SliceSprite == null && sp.name == "UI_RoundedBox_9Slice")
                    {
                        button9SliceSprite = sp;
                    }
                    if (familyTableCalmSprite == null && (sp.name == "U6_MAct_Family_SittingStraight" || sp.name == "SPR_FamilyTable_SittingStraight"))
                    {
                        familyTableCalmSprite = sp;
                    }
                    if (familyTableHandRaiseSprite == null && (sp.name == "U6_MAct_Family_HandRaise" || sp.name == "SPR_FamilyTable_HandRaise"))
                    {
                        familyTableHandRaiseSprite = sp;
                    }
                    if (familyTableReactSprite == null && (sp.name == "U6_MAct_Family_ShoutingHungry" || sp.name == "SPR_FamilyTable_ShoutingHungry"))
                    {
                        familyTableReactSprite = sp;
                    }
                }
            }
        }

        private void EnsureDishSprites()
        {
            if (spriteDishDosa == null || spriteDishIdli == null || spriteDishNoodles == null || 
                spriteDishRice == null || spriteDishRoti == null || spriteDishIceCream == null)
            {
#if UNITY_EDITOR
                string assetPath = "Assets/SeniorsActivityUnit6/Art/U6_MastersActivitySprites/U6 MA Picture Menu Dishes.png";
                var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var obj in assets)
                {
                    if (obj is Sprite sp)
                    {
                        if (sp.name == "SPR_Dish_Dosa" && spriteDishDosa == null) spriteDishDosa = sp;
                        else if (sp.name == "SPR_Dish_Idli" && spriteDishIdli == null) spriteDishIdli = sp;
                        else if (sp.name == "SPR_Dish_Noodles" && spriteDishNoodles == null) spriteDishNoodles = sp;
                        else if (sp.name == "SPR_Dish_Rice" && spriteDishRice == null) spriteDishRice = sp;
                        else if (sp.name == "SPR_Dish_Roti" && spriteDishRoti == null) spriteDishRoti = sp;
                        else if (sp.name == "SPR_Dish_IceCream" && spriteDishIceCream == null) spriteDishIceCream = sp;
                    }
                }
#endif
                if (spriteDishDosa == null || spriteDishIdli == null || spriteDishNoodles == null || 
                    spriteDishRice == null || spriteDishRoti == null || spriteDishIceCream == null)
                {
                    var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                    foreach (var sp in allSprites)
                    {
                        if (sp == null) continue;
                        if (spriteDishDosa == null && sp.name == "SPR_Dish_Dosa") spriteDishDosa = sp;
                        else if (spriteDishIdli == null && sp.name == "SPR_Dish_Idli") spriteDishIdli = sp;
                        else if (spriteDishNoodles == null && sp.name == "SPR_Dish_Noodles") spriteDishNoodles = sp;
                        else if (spriteDishRice == null && sp.name == "SPR_Dish_Rice") spriteDishRice = sp;
                        else if (spriteDishRoti == null && sp.name == "SPR_Dish_Roti") spriteDishRoti = sp;
                        else if (spriteDishIceCream == null && sp.name == "SPR_Dish_IceCream") spriteDishIceCream = sp;
                    }
                }
            }
        }

        public Sprite GetOrderedDishSprite(string dishName)
        {
            EnsureDishSprites();

            // 1. Direct sprite from GameManager if set
            if (U6_GameManager_Masters_Activity.Instance != null && U6_GameManager_Masters_Activity.Instance.OrderedDishSprite != null)
            {
                return U6_GameManager_Masters_Activity.Instance.OrderedDishSprite;
            }

            // 2. Resolve by name
            string name = !string.IsNullOrEmpty(dishName) ? dishName.ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(name) && U6_GameManager_Masters_Activity.Instance != null)
            {
                name = (U6_GameManager_Masters_Activity.Instance.OrderedDishName ?? "").ToLowerInvariant();
            }

            if (name.Contains("idli")) return spriteDishIdli != null ? spriteDishIdli : spriteDishDosa;
            if (name.Contains("noodle")) return spriteDishNoodles != null ? spriteDishNoodles : spriteDishDosa;
            if (name.Contains("rice")) return spriteDishRice != null ? spriteDishRice : spriteDishDosa;
            if (name.Contains("roti")) return spriteDishRoti != null ? spriteDishRoti : spriteDishDosa;
            if (name.Contains("ice") || name.Contains("cream")) return spriteDishIceCream != null ? spriteDishIceCream : spriteDishDosa;

            return spriteDishDosa;
        }

        public string GetOrderedDishWrongAudioKey(string dishName)
        {
            string name = !string.IsNullOrEmpty(dishName) ? dishName.ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(name) && U6_GameManager_Masters_Activity.Instance != null)
            {
                name = (U6_GameManager_Masters_Activity.Instance.OrderedDishName ?? "").ToLowerInvariant();
            }

            if (name.Contains("idli")) return "VO_U6_ANU_WRONG_IDLI";
            if (name.Contains("noodle")) return "VO_U6_ANU_WRONG_NOODLES";
            if (name.Contains("rice")) return "VO_U6_ANU_WRONG_RICE";
            if (name.Contains("roti")) return "VO_U6_ANU_WRONG_ROTI";
            if (name.Contains("ice") || name.Contains("cream")) return "VO_U6_ANU_WRONG_ICECREAM";

            return "VO_U6_ANU_WRONG_DOSA";
        }

        public AudioClip GetOrderedDishWrongClip(string dishName)
        {
            string name = !string.IsNullOrEmpty(dishName) ? dishName.ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(name) && U6_GameManager_Masters_Activity.Instance != null)
            {
                name = (U6_GameManager_Masters_Activity.Instance.OrderedDishName ?? "").ToLowerInvariant();
            }

            AudioClip clip = null;
            if (name.Contains("idli")) clip = voWrongDishIdliClip;
            else if (name.Contains("noodle")) clip = voWrongDishNoodlesClip;
            else if (name.Contains("rice")) clip = voWrongDishRiceClip;
            else if (name.Contains("roti")) clip = voWrongDishRotiClip;
            else if (name.Contains("ice") || name.Contains("cream")) clip = voWrongDishIceCreamClip;
            else clip = voWrongDishDosaClip;

            if (clip != null) return clip;

#if UNITY_EDITOR
            string key = GetOrderedDishWrongAudioKey(name);
            string path = $"Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/{key}.mp3";
            clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null) return clip;
#endif

            return voAnuWrongDishPoliteClip;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-Assign Wrong Dish Audio Clips")]
        public void EnsureWrongDishClips()
        {
            if (voWrongDishDosaClip == null) voWrongDishDosaClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/VO_U6_ANU_WRONG_DOSA.mp3") ?? voAnuWrongDishPoliteClip;
            if (voWrongDishIdliClip == null) voWrongDishIdliClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/VO_U6_ANU_WRONG_IDLI.mp3");
            if (voWrongDishNoodlesClip == null) voWrongDishNoodlesClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/VO_U6_ANU_WRONG_NOODLES.mp3");
            if (voWrongDishRiceClip == null) voWrongDishRiceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/VO_U6_ANU_WRONG_RICE.mp3");
            if (voWrongDishRotiClip == null) voWrongDishRotiClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/VO_U6_ANU_WRONG_ROTI.mp3");
            if (voWrongDishIceCreamClip == null) voWrongDishIceCreamClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SeniorsActivityUnit6/Audio/U6_MastersActivity_audios/VO_U6_ANU_WRONG_ICECREAM.mp3");

            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                if (gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }
        }
#endif

        public void UpdateTableDishVisibility(bool isDelivered, bool animate = false)
        {
            if (tableDishObject == null)
            {
                AutoFindUIReferences();
            }

            if (tableDishObject == null) return;

            if (!isDelivered)
            {
                tableDishObject.SetActive(false);
                return;
            }

            // Food is delivered to the table!
            string orderedName = (U6_GameManager_Masters_Activity.Instance != null) ? U6_GameManager_Masters_Activity.Instance.OrderedDishName : "Dosa";
            Sprite dishSprite = GetOrderedDishSprite(orderedName);

            if (tableDishImage == null)
            {
                tableDishImage = tableDishObject.GetComponent<Image>();
            }

            if (tableDishImage != null && dishSprite != null)
            {
                tableDishImage.sprite = dishSprite;
                tableDishImage.preserveAspect = true;
            }

            bool wasInactive = !tableDishObject.activeSelf;
            tableDishObject.SetActive(true);

            if (wasInactive && animate && gameObject.activeInHierarchy)
            {
                StartCoroutine(PopAvatarAnimation(tableDishObject.transform));
            }
        }

        private void SetupChoiceCard(Button btn, bool isPolite, string currentVO, AudioClip currentClip, Sprite specificAvatar)
        {
            if (btn == null) return;

            // 0. Visual styling
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                if (button9SliceSprite != null) btnImg.sprite = button9SliceSprite;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = isPolite ? new Color(0.18f, 0.62f, 0.35f, 0.98f) : new Color(0.82f, 0.42f, 0.20f, 0.98f);
            }

            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = isPolite ? new Color(0.207f, 0.713f, 0.4025f, 1f) : new Color(0.943f, 0.483f, 0.23f, 1f);
            cb.pressedColor = isPolite ? new Color(0.153f, 0.527f, 0.2975f, 1f) : new Color(0.697f, 0.357f, 0.17f, 1f);
            cb.selectedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            cb.disabledColor = new Color(0.784f, 0.784f, 0.784f, 0.502f);
            btn.colors = cb;

            // 1. Label — center text with proper margins for avatar on left and listen button on right
            Transform lblT = btn.transform.Find("Label");
            if (lblT != null)
            {
                var tmp = lblT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = labelFontSizeMin;
                    tmp.fontSizeMax = labelFontSizeMax;
                    tmp.color = Color.white;
                }

                RectTransform lRT = lblT.GetComponent<RectTransform>();
                if (lRT != null)
                {
                    lRT.anchorMin = Vector2.zero;
                    lRT.anchorMax = Vector2.one;
                    lRT.pivot = new Vector2(0.5f, 0.5f);
                    float leftMargin = avatarIconSize.x + 36f;
                    float rightMargin = listenButtonSize.x + 36f;
                    lRT.offsetMin = new Vector2(leftMargin, 8f);
                    lRT.offsetMax = new Vector2(-rightMargin, -8f);
                }
            }

            // 2. Avatar Portrait — sized to avatarIconSize (big and clear for kids)
            Transform avT = btn.transform.Find("AvatarPortrait");
            Sprite targetSprite = specificAvatar != null ? specificAvatar : (isPolite ? politeAvatarSprite : impoliteAvatarSprite);
            if (avT == null)
            {
                GameObject avObj = new GameObject("AvatarPortrait", typeof(RectTransform), typeof(Image));
                avObj.transform.SetParent(btn.transform, false);
                avT = avObj.transform;
            }

            var avRT = avT.GetComponent<RectTransform>();
            if (avRT != null)
            {
                avRT.anchorMin = new Vector2(0f, 0.5f);
                avRT.anchorMax = new Vector2(0f, 0.5f);
                avRT.pivot = new Vector2(0f, 0.5f);
                avRT.anchoredPosition = new Vector2(24f, 0f);
                avRT.sizeDelta = avatarIconSize;
            }

            var avImg = avT.GetComponent<Image>();
            if (avImg != null)
            {
                if (targetSprite != null) avImg.sprite = targetSprite;
                avImg.preserveAspect = true;
                avImg.raycastTarget = false;
            }

            // 3. Listen button — sized to listenButtonSize
            string listenName = isPolite ? "HearPoliteButton" : "HearImpoliteButton";
            Transform hT = btn.transform.Find(listenName)
                        ?? btn.transform.Find(isPolite ? "HearOptionA_Button" : "HearOptionB_Button");
            Button hBtn = null;
            if (hT == null)
            {
                GameObject hObj = new GameObject(listenName, typeof(RectTransform), typeof(Image), typeof(Button));
                hObj.transform.SetParent(btn.transform, false);
                hT = hObj.transform;
            }

            var hRT = hT.GetComponent<RectTransform>();
            if (hRT != null)
            {
                hRT.anchorMin = new Vector2(1f, 0.5f);
                hRT.anchorMax = new Vector2(1f, 0.5f);
                hRT.pivot = new Vector2(1f, 0.5f);
                hRT.anchoredPosition = new Vector2(-24f, 0f);
                hRT.sizeDelta = listenButtonSize;
            }

            var hImg = hT.GetComponent<Image>();
            if (hImg != null)
            {
                if (button9SliceSprite != null)
                {
                    hImg.sprite = button9SliceSprite;
                    hImg.type = Image.Type.Sliced;
                }
                else if (btnImg != null && btnImg.sprite != null)
                {
                    hImg.sprite = btnImg.sprite;
                    hImg.type = Image.Type.Sliced;
                }
                hImg.color = isPolite ? new Color(0.12f, 0.42f, 0.20f) : new Color(0.65f, 0.25f, 0.10f);
            }

            Transform hLblT = hT.Find("Text");
            TextMeshProUGUI tmpListen = null;
            if (hLblT == null)
            {
                GameObject hLbl = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                hLbl.transform.SetParent(hT, false);
                var lRT = hLbl.GetComponent<RectTransform>();
                lRT.anchorMin = Vector2.zero;
                lRT.anchorMax = Vector2.one;
                lRT.sizeDelta = Vector2.zero;
                tmpListen = hLbl.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                tmpListen = hLblT.GetComponent<TextMeshProUGUI>();
            }

            if (tmpListen != null)
            {
                tmpListen.text = "LISTEN";
                tmpListen.fontSize = 24;
                tmpListen.fontStyle = FontStyles.Bold;
                tmpListen.alignment = TextAlignmentOptions.Center;
                tmpListen.color = Color.white;
            }

            hBtn = hT.GetComponent<Button>();

            bool hasVoiceAudio = (currentClip != null) || (!string.IsNullOrEmpty(currentVO) && !currentVO.Equals("VO_U6_ANU_3", System.StringComparison.OrdinalIgnoreCase) && !currentVO.ToLower().Contains("nothing"));
            if (hT != null)
            {
                hT.gameObject.SetActive(hasVoiceAudio);
            }

            if (hBtn != null && hasVoiceAudio)
            {
                hBtn.onClick.RemoveAllListeners();
                string voToPlay = currentVO;
                AudioClip clipToPlay = currentClip;
                hBtn.onClick.AddListener(() =>
                {
                    PlayScreenAudio(clipToPlay, voToPlay);
                    StartCoroutine(PopAvatarAnimation(hBtn.transform));
                });
            }
        }


#if UNITY_EDITOR
        [ContextMenu("Apply Big Choice Card Sizing")]
        public void ApplyBigChoiceCardSizing()
        {
            AutoFindUIReferences();
            if (optionA_Button != null) SetupChoiceCard(optionA_Button, true, "", null, null);
            if (optionB_Button != null) SetupChoiceCard(optionB_Button, false, "", null, null);

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
                if (choiceContainer != null) EditorUtility.SetDirty(choiceContainer);
                if (gameObject.scene.IsValid())
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        [ContextMenu("Auto-Assign Everything (Fix for APK)")]
        public void AutoLoadSpritesAndMoments()
        {
            ApplyBigChoiceCardSizing();
            EnsureWrongDishClips();

            Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/SeniorsActivityUnit6/Art" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in allObjects)
                {
                    if (obj is Sprite sp && !spriteDict.ContainsKey(sp.name))
                    {
                        spriteDict[sp.name] = sp;
                    }
                }
            }

            Sprite GetSprite(string name)
            {
                if (spriteDict.TryGetValue(name, out Sprite s)) return s;
                foreach (var kvp in spriteDict)
                {
                    if (kvp.Key.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return kvp.Value;
                }
                return null;
            }

            // CRITICAL: Inspector Priority!
            EnsureWrongDishClips();
            // Only assign component-level sprite fields if they are currently null!
            if (filledWaterGlassSprite == null) filledWaterGlassSprite = GetSprite("Sprite water glasses_2") ?? GetSprite("water glasses_2");
            if (waiterSmileSprite == null) waiterSmileSprite = GetSprite("SPR_Ravi_WarmSmile");
            if (waiterNeutralSprite == null) waiterNeutralSprite = GetSprite("SPR_Ravi_NeutralBlank");
            if (waiterStiffSprite == null) waiterStiffSprite = GetSprite("SPR_Ravi_StiffPolite");
            if (button9SliceSprite == null) button9SliceSprite = GetSprite("UI_RoundedBox_9Slice");

            Sprite raviWater = GetSprite("SPR_WaiterWater") ?? GetSprite("raving walking right to left") ?? GetSprite("SPR_Ravi_WaterJug");
            Sprite raviPad = GetSprite("SPR_Ravi_StandingPad");
            Sprite raviPlate = GetSprite("SPR_Ravi_ServingPlate");

            // Main stage characters: ALWAYS use the family table sprites (Mother, Father, Anu sitting at dining table)
            // Sourced directly from Assets/SeniorsActivityUnit6/Art/U6_MastersActivitySprites/U6_seniorsActivityBook.png
            Sprite familyStraight = GetSprite("U6_MAct_Family_SittingStraight") ?? GetSprite("SPR_FamilyTable_SittingStraight");
            Sprite familyHandRaise = GetSprite("U6_MAct_Family_HandRaise") ?? GetSprite("SPR_FamilyTable_HandRaise");
            Sprite familyShouting = GetSprite("U6_MAct_Family_ShoutingHungry") ?? GetSprite("SPR_FamilyTable_ShoutingHungry");

            Sprite tableWaiting = familyStraight;
            Sprite tableCalling = familyHandRaise != null ? familyHandRaise : familyStraight;

            // Single Anu portraits for small avatar button icons
            Sprite anuStraight = GetSprite("SPR_Anu_SittingStraight") ?? familyStraight;
            Sprite anuHandRaise = GetSprite("SPR_Anu_HandRaise") ?? familyHandRaise;
            Sprite anuFranticWave = GetSprite("SPR_Anu_FranticWave");
            Sprite anuShouting = GetSprite("SPR_Anu_ShoutingHungry");

            if (politeAvatarSprite == null) politeAvatarSprite = anuHandRaise ?? anuStraight;
            if (impoliteAvatarSprite == null) impoliteAvatarSprite = anuShouting ?? anuFranticWave;

            Sprite emptyGlass = GetSprite("SPR_EmptyGlass");
            Sprite spoonFork = GetSprite("SPR_Spoon_and_Fork");
            Sprite dishDosa = GetSprite("SPR_Dish_Dosa");
            Sprite dishNoodles = GetSprite("SPR_Dish_Noodles");

            if (spriteDishDosa == null) spriteDishDosa = dishDosa;
            if (spriteDishIdli == null) spriteDishIdli = GetSprite("SPR_Dish_Idli");
            if (spriteDishNoodles == null) spriteDishNoodles = dishNoodles;
            if (spriteDishRice == null) spriteDishRice = GetSprite("SPR_Dish_Rice");
            if (spriteDishRoti == null) spriteDishRoti = GetSprite("SPR_Dish_Roti");
            if (spriteDishIceCream == null) spriteDishIceCream = GetSprite("SPR_Dish_IceCream");

            // Full-Body Waiter Ravi Sprites from 'u6 MA more waiter ravi sprites.png':
            if (raviGreeting == null) raviGreeting = GetSprite("Greeting & Welcome") ?? GetSprite("Greeting");
            if (raviTakingOrder == null) raviTakingOrder = GetSprite("Taking Order");
            if (raviPouring == null) raviPouring = GetSprite("SPR_WaiterWater") ?? GetSprite("raving walking right to left") ?? GetSprite("Pouring Water") ?? GetSprite("Pouring");
            if (raviServing == null) raviServing = GetSprite("Serving Hot Dish") ?? GetSprite("Serving");
            if (raviCutlery == null) raviCutlery = GetSprite("Replacing Cutlery") ?? GetSprite("Cutlery");
            if (raviApology == null) raviApology = GetSprite("Polite Apology") ?? GetSprite("Apology");
            if (raviSurprised == null) raviSurprised = GetSprite("Surprised  Alert") ?? GetSprite("Surprised") ?? GetSprite("Alert");
            if (raviClearing == null) raviClearing = GetSprite("Clearing Table") ?? GetSprite("Clearing");
            if (raviBill == null) raviBill = GetSprite("Presenting Bill") ?? GetSprite("Bill");
            if (raviWave == null) raviWave = GetSprite("Farewell & Wave") ?? GetSprite("Farewell");

            Sprite poseWater = raviPouring ?? raviWater;
            Sprite poseOrder = raviTakingOrder ?? raviPad;
            Sprite poseServing = raviServing ?? raviPlate;
            Sprite poseGreeting = raviGreeting ?? raviPad;
            Sprite poseCutlery = raviCutlery ?? raviPad;
            Sprite poseApology = raviApology ?? poseGreeting;
            Sprite poseSurprised = raviSurprised ?? poseOrder;
            Sprite poseClearing = raviClearing ?? poseOrder;

            if (familyTableCalmSprite == null) familyTableCalmSprite = familyStraight;
            if (familyTableHandRaiseSprite == null) familyTableHandRaiseSprite = familyHandRaise;
            if (familyTableReactSprite == null) familyTableReactSprite = familyShouting;

            // CRITICAL: Inspector Priority!
            // If moments were already configured or serialized in the Inspector, NEVER CLEAR THEM!
            if (moments != null && moments.Count > 0)
            {
                for (int i = 0; i < moments.Count; i++)
                {
                    var m = moments[i];
                    if (m == null) continue;

                    // Preserve inspector audio clips
                    if (m.optionA_Clip == null && !string.IsNullOrEmpty(m.optionA_VO))
                    {
                        // optional fallback
                    }

                    // Only provide fallback if inspector field is empty
                    if (m.fullBodyPoseSprite == null)
                    {
                        if (i == 0) m.fullBodyPoseSprite = poseWater;
                        else if (i == 1) m.fullBodyPoseSprite = poseOrder;
                        else if (i == 2 || i == 3) m.fullBodyPoseSprite = poseServing;
                        else if (i == 4) m.fullBodyPoseSprite = poseGreeting;
                    }

                    if (m.anuReactionSprite == null)
                    {
                        m.anuReactionSprite = (i == 1) ? tableCalling : tableWaiting;
                    }

                    if (m.optionA_Avatar == null)
                    {
                        m.optionA_Avatar = (i == 1 || i == 2 || i == 4) ? anuHandRaise : anuStraight;
                    }

                    if (m.optionB_Avatar == null)
                    {
                        m.optionB_Avatar = (i == 1) ? (anuFranticWave ?? anuShouting) : anuShouting;
                    }

                    if (m.optionA_RaviPose == null)
                    {
                        if (i == 2) m.optionA_RaviPose = poseApology;
                        else if (i == 4) m.optionA_RaviPose = poseCutlery;
                        else m.optionA_RaviPose = poseGreeting;
                    }

                    if (m.optionB_RaviPose == null)
                    {
                        if (i == 0) m.optionB_RaviPose = poseOrder;
                        else if (i == 2) m.optionB_RaviPose = poseClearing;
                        else m.optionB_RaviPose = poseSurprised;
                    }

                    if (m.propSprite == null)
                    {
                        if (i == 0) m.propSprite = emptyGlass;
                        else if (i == 3) m.propSprite = dishDosa;
                        else if (i == 4) m.propSprite = spoonFork;
                    }

                    if (string.IsNullOrEmpty(m.optionA_VO))
                    {
                        if (i == 0) m.optionA_VO = "VO_U6_ANU_4";
                        else if (i == 1) m.optionA_VO = "VO_U6_ANU_CALL_POLITE";
                        else if (i == 2) m.optionA_VO = "VO_U6_ANU_5";
                        else if (i == 3) m.optionA_VO = "VO_U6_ANU_4";
                        else if (i == 4) m.optionA_VO = "VO_U6_ANU_8";
                    }
                    
                    if (string.IsNullOrEmpty(m.optionB_VO))
                    {
                        if (i == 0) m.optionB_VO = "";
                        else if (i == 1) m.optionB_VO = "VO_U6_ANU_CALL_SHOUT";
                        else if (i == 2) m.optionB_VO = "VO_U6_ANU_6";
                        else if (i == 3) m.optionB_VO = "VO_U6_ANU_EAT_FAST";
                        else if (i == 4) m.optionB_VO = "VO_U6_ANU_9";
                    }
                    if (i == 0)
                    {
                        m.optionB_VO = "";
                        m.optionB_Clip = null;
                    }
                }
                Debug.Log($"[U6_WaiterInteractionScreen] Preserved all {moments.Count} Inspector-configured moments with priority!");
                return;
            }

            // Only if moments is empty, initialize defaults:
            moments = new List<U6_WaiterMomentData_Masters_Activity>();

            // 1. Water Served
            moments.Add(new U6_WaiterMomentData_Masters_Activity
            {
                situationTitle = "Water Served",
                situationPrompt = "Ravi brings a fresh jug of water to the table.",
                fullBodyPoseSprite = poseWater,
                propSprite = emptyGlass,
                anuReactionSprite = tableWaiting,
                optionA_Badge = "[POLITE: PLEASE]",
                optionA_Text = "\"Thank you!\"",
                optionA_Outcome = "Ravi smiles warmly and pours the water.",
                optionA_VO = "VO_U6_ANU_4",
                optionA_Face = U6_WaiterFaceState.WarmSmile,
                optionA_Avatar = anuStraight,
                optionA_RaviPose = poseGreeting,
                optionB_Badge = "[SAY NOTHING]",
                optionB_Text = "\"...\" (Say nothing)",
                optionB_Outcome = "Ravi pours the water quietly.",
                optionB_VO = "",
                optionB_Face = U6_WaiterFaceState.BlankNeutral,
                optionB_Avatar = anuStraight,
                optionB_RaviPose = poseOrder
            });

            // 2. Calling Ravi
            moments.Add(new U6_WaiterMomentData_Masters_Activity
            {
                situationTitle = "Calling Ravi",
                situationPrompt = "Anu needs something and Ravi is across the room.",
                fullBodyPoseSprite = poseOrder,
                propSprite = null,
                anuReactionSprite = tableCalling,
                optionA_Badge = "[POLITE: RAISE HAND]",
                optionA_Text = "\"Excuse me, Ravi!\"",
                optionA_Outcome = "Ravi notices and comes over calmly: \"Of course, one moment!\"",
                optionA_VO = "VO_U6_ANU_CALL_POLITE",
                optionA_Face = U6_WaiterFaceState.WarmSmile,
                optionA_Avatar = anuHandRaise,
                optionA_RaviPose = poseGreeting,
                optionB_Badge = "[BLUNT: SHOUT]",
                optionB_Text = "\"Hey! Over here!\"",
                optionB_Outcome = "Ravi hurries over while other tables look over.",
                optionB_VO = "VO_U6_ANU_CALL_SHOUT",
                optionB_Face = U6_WaiterFaceState.BlankNeutral,
                optionB_Avatar = anuFranticWave != null ? anuFranticWave : anuShouting,
                optionB_RaviPose = poseSurprised
            });

            // 3. Wrong Dish (No food prop displayed on table to avoid contradiction)
            moments.Add(new U6_WaiterMomentData_Masters_Activity
            {
                situationTitle = "Wrong Dish",
                situationPrompt = "Ravi brings the wrong dish by mistake!",
                fullBodyPoseSprite = poseServing,
                propSprite = null,
                anuReactionSprite = tableWaiting,
                optionA_Badge = "[POLITE: SPEAK GENTLY]",
                optionA_Text = "\"Sorry, I think this is the wrong dish.\"",
                optionA_Outcome = "Ravi apologises warmly: \"I will fix that right away!\"",
                optionA_VO = "VO_U6_ANU_5",
                optionA_Face = U6_WaiterFaceState.WarmSmile,
                optionA_Avatar = anuStraight,
                optionA_RaviPose = poseApology,
                optionB_Badge = "[BLUNT: SHOUT ANGRILY]",
                optionB_Text = "\"This is WRONG!\"",
                optionB_Outcome = "Ravi apologises stiffly and fixes it quietly.",
                optionB_VO = "VO_U6_ANU_6",
                optionB_Face = U6_WaiterFaceState.StiffPolite,
                optionB_Avatar = anuShouting,
                optionB_RaviPose = poseClearing
            });

            // 4. Food Served
            moments.Add(new U6_WaiterMomentData_Masters_Activity
            {
                situationTitle = "Food Served",
                situationPrompt = "Ravi sets the hot dish down in front of Anu.",
                fullBodyPoseSprite = poseServing,
                propSprite = dishDosa,
                anuReactionSprite = tableWaiting,
                optionA_Badge = "[POLITE: THANK YOU]",
                optionA_Text = "\"Thank you, Ravi!\"",
                optionA_Outcome = "Ravi nods happily: \"Enjoy your meal!\"",
                optionA_VO = "VO_U6_ANU_4",
                optionA_Face = U6_WaiterFaceState.WarmSmile,
                optionA_Avatar = anuStraight,
                optionA_RaviPose = poseGreeting,
                optionB_Badge = "[BLUNT: GRAB FOOD]",
                optionB_Text = "\"Yum! Give me that!\"",
                optionB_Outcome = "Ravi steps away quietly as Anu grabs the food.",
                optionB_VO = "VO_U6_ANU_EAT_FAST",
                optionB_Face = U6_WaiterFaceState.BlankNeutral,
                optionB_Avatar = anuShouting,
                optionB_RaviPose = poseSurprised
            });

            // 5. Dropped Fork
            moments.Add(new U6_WaiterMomentData_Masters_Activity
            {
                situationTitle = "Dropped Fork",
                situationPrompt = "Anu accidentally drops her fork on the floor.",
                fullBodyPoseSprite = poseGreeting,
                propSprite = spoonFork,
                anuReactionSprite = tableWaiting,
                optionA_Badge = "[POLITE: ASK NICELY]",
                optionA_Text = "\"Excuse me, could I have another fork please?\"",
                optionA_Outcome = "Ravi brings a clean fork right away with a smile.",
                optionA_VO = "VO_U6_ANU_8",
                optionA_Face = U6_WaiterFaceState.WarmSmile,
                optionA_Avatar = anuHandRaise,
                optionA_RaviPose = poseCutlery,
                optionB_Badge = "[BLUNT: DEMAND]",
                optionB_Text = "\"I dropped my fork!\"",
                optionB_Outcome = "Ravi brings one while the nearby diners look up.",
                optionB_VO = "VO_U6_ANU_9",
                optionB_Face = U6_WaiterFaceState.StiffPolite,
                optionB_Avatar = anuShouting,
                optionB_RaviPose = poseSurprised
            });

            Debug.Log($"[U6_WaiterInteractionScreen] Auto-Loaded default moments with full body poses and buttons.");
        }
#endif

        private void InitializeDefaultMoments()
        {
            if (moments == null || moments.Count == 0)
            {
                moments = new List<U6_WaiterMomentData_Masters_Activity>();

                // 1. Water Served
                moments.Add(new U6_WaiterMomentData_Masters_Activity
                {
                    situationTitle = "Water Served",
                    situationPrompt = "Ravi brings a fresh jug of water to the table.",
                    optionA_Badge = "[POLITE: PLEASE]",
                    optionA_Text = "\"Thank you!\"",
                    optionA_Outcome = "Ravi smiles warmly and pours the water.",
                    optionA_VO = "VO_U6_ANU_4",
                    optionA_Face = U6_WaiterFaceState.WarmSmile,
                    optionB_Badge = "[SAY NOTHING]",
                    optionB_Text = "\"...\" (Say nothing)",
                    optionB_Outcome = "Ravi pours the water quietly.",
                    optionB_VO = "",
                    optionB_Face = U6_WaiterFaceState.BlankNeutral
                });

                // 2. Calling Ravi
                moments.Add(new U6_WaiterMomentData_Masters_Activity
                {
                    situationTitle = "Calling Ravi",
                    situationPrompt = "Anu needs something and Ravi is across the room.",
                    optionA_Badge = "[POLITE: RAISE HAND]",
                    optionA_Text = "\"Excuse me, Ravi!\"",
                    optionA_Outcome = "Ravi notices and comes over calmly: \"Of course, one moment!\"",
                    optionA_VO = "VO_U6_ANU_CALL_POLITE",
                    optionA_Face = U6_WaiterFaceState.WarmSmile,
                    optionB_Badge = "[BLUNT: SHOUT]",
                    optionB_Text = "\"Hey! Over here!\"",
                    optionB_Outcome = "Ravi hurries over while other tables look over.",
                    optionB_VO = "VO_U6_ANU_CALL_SHOUT",
                    optionB_Face = U6_WaiterFaceState.BlankNeutral
                });

                // 3. Wrong Dish
                moments.Add(new U6_WaiterMomentData_Masters_Activity
                {
                    situationTitle = "Wrong Dish",
                    situationPrompt = "Ravi brings the wrong dish by mistake!",
                    optionA_Badge = "[POLITE: SPEAK GENTLY]",
                    optionA_Text = "\"Sorry, I think this is the wrong dish.\"",
                    optionA_Outcome = "Ravi apologises warmly: \"I will fix that right away!\"",
                    optionA_VO = "VO_U6_ANU_5",
                    optionA_Face = U6_WaiterFaceState.WarmSmile,
                    optionB_Badge = "[BLUNT: SHOUT ANGRILY]",
                    optionB_Text = "\"This is WRONG!\"",
                    optionB_Outcome = "Ravi apologises stiffly and fixes it quietly.",
                    optionB_VO = "VO_U6_ANU_6",
                    optionB_Face = U6_WaiterFaceState.StiffPolite
                });

                // 4. Food Served
                moments.Add(new U6_WaiterMomentData_Masters_Activity
                {
                    situationTitle = "Food Served",
                    situationPrompt = "Ravi sets the hot dish down in front of Anu.",
                    optionA_Badge = "[POLITE: THANK YOU]",
                    optionA_Text = "\"Thank you, Ravi!\"",
                    optionA_Outcome = "Ravi nods happily: \"Enjoy your meal!\"",
                    optionA_VO = "VO_U6_ANU_4",
                    optionA_Face = U6_WaiterFaceState.WarmSmile,
                    optionB_Badge = "[BLUNT: GRAB FOOD]",
                    optionB_Text = "\"Yum! Give me that!\"",
                    optionB_Outcome = "Ravi steps away quietly as Anu grabs the food.",
                    optionB_VO = "VO_U6_ANU_EAT_FAST",
                    optionB_Face = U6_WaiterFaceState.BlankNeutral
                });

                // 5. Dropped Fork
                moments.Add(new U6_WaiterMomentData_Masters_Activity
                {
                    situationTitle = "Dropped Fork",
                    situationPrompt = "Anu accidentally drops her fork on the floor.",
                    optionA_Badge = "[POLITE: ASK NICELY]",
                    optionA_Text = "\"Excuse me, could I have another fork please?\"",
                    optionA_Outcome = "Ravi brings a clean fork right away with a smile.",
                    optionA_VO = "VO_U6_ANU_8",
                    optionA_Face = U6_WaiterFaceState.WarmSmile,
                    optionB_Badge = "[BLUNT: DEMAND]",
                    optionB_Text = "\"I dropped my fork!\"",
                    optionB_Outcome = "Ravi brings one while the nearby diners look up.",
                    optionB_VO = "VO_U6_ANU_9",
                    optionB_Face = U6_WaiterFaceState.StiffPolite
                });
            }
        }

        private void DisplayCurrentMoment()
        {
            if (currentMomentIndex >= moments.Count)
            {
                StartCoroutine(CompletePart3Sequence());
                return;
            }

            U6_WaiterMomentData_Masters_Activity m = moments[currentMomentIndex];

            if (titleBanner != null)
            {
                titleBanner.SetActive(true);
                CanvasGroup cg = titleBanner.GetComponent<CanvasGroup>();
                if (cg == null) cg = titleBanner.AddComponent<CanvasGroup>();
                cg.alpha = 1f;

                TextMeshProUGUI titleText = titleBanner.GetComponentInChildren<TextMeshProUGUI>(true);
                if (titleText != null && !string.IsNullOrEmpty(m.situationTitle))
                {
                    titleText.text = m.situationTitle;
                }

                if (hideTitleBannerCoroutine != null) StopCoroutine(hideTitleBannerCoroutine);
                hideTitleBannerCoroutine = StartCoroutine(HideTitleBannerRoutine());
            }
            
            // Dynamic check for ordered dish from Part 2 Menu
            string orderedName = (U6_GameManager_Masters_Activity.Instance != null) ? U6_GameManager_Masters_Activity.Instance.OrderedDishName : "dosa";
            Sprite orderedSprite = GetOrderedDishSprite(orderedName);

            // Food delivery tracking:
            // Before food delivery (Moments 0, 1, 2) -> Dish GameObject is disabled.
            // When delivered (Moment 3 "Food Served", and subsequently Moment 4 "Dropped Fork") -> Dish GameObject is enabled with ordered dish sprite!
            bool isFoodDeliveryMoment = (m.situationTitle != null && m.situationTitle.IndexOf("Food", System.StringComparison.OrdinalIgnoreCase) >= 0)
                                     || currentMomentIndex >= 3;

            if (isFoodDeliveryMoment)
            {
                foodHasBeenDelivered = true;
            }
            else
            {
                foodHasBeenDelivered = false;
            }

            UpdateTableDishVisibility(foodHasBeenDelivered, animate: (currentMomentIndex == 3));

            if (currentMomentIndex == 2) // Wrong Dish
            {
                if (!string.IsNullOrEmpty(orderedName))
                {
                    m.optionA_Text = $"\"Sorry, I think I ordered {orderedName.ToLower()}.\"";
                }
                // Removed PlayScreenAudio(voWrongDishClip, voWrongDish); to prevent audio playing before user makes a choice.
            }
            else if (currentMomentIndex == 3) // Food Served
            {
                if (orderedSprite != null)
                {
                    m.propSprite = orderedSprite; // Fallback only if no prop was assigned in inspector
                }
                if (!string.IsNullOrEmpty(orderedName))
                {
                    m.situationPrompt = $"Ravi sets the hot {orderedName.ToLower()} down in front of Anu.";
                }
                PlayScreenSFX(sfxPlateDownClip, sfxPlateDown);
            }

            if (promptText)
            {
                promptText.text = !string.IsNullOrEmpty(m.situationPrompt) ? m.situationPrompt : "What will Anu say to the waiter?";
            }

            string badgeA = !string.IsNullOrEmpty(m.optionA_Badge) ? m.optionA_Badge : "[POLITE: PLEASE]";
            string badgeB = !string.IsNullOrEmpty(m.optionB_Badge) ? m.optionB_Badge : "[BLUNT / RUDE]";

            if (optionA_Label)
            {
                string textA = !string.IsNullOrEmpty(m.optionA_Text) ? m.optionA_Text.Trim('\"') : "Thank you!";
                optionA_Label.text = $"<color=#D4EFDF><size=82%><b>{badgeA}</b></size></color>\n<b>\"{textA}\"</b>";
                optionA_Label.alignment = TextAlignmentOptions.Center;
                optionA_Label.enableAutoSizing = true;
                optionA_Label.fontSizeMin = labelFontSizeMin;
                optionA_Label.fontSizeMax = labelFontSizeMax;
                optionA_Label.color = Color.white;
            }
            if (optionB_Label)
            {
                string textB = !string.IsNullOrEmpty(m.optionB_Text) ? m.optionB_Text.Trim('\"') : "(Say nothing)";
                optionB_Label.text = $"<color=#FADBD8><size=82%><b>{badgeB}</b></size></color>\n<b>\"{textB}\"</b>";
                optionB_Label.alignment = TextAlignmentOptions.Center;
                optionB_Label.enableAutoSizing = true;
                optionB_Label.fontSizeMin = labelFontSizeMin;
                optionB_Label.fontSizeMax = labelFontSizeMax;
                optionB_Label.color = Color.white;
            }

            string voKeyA = m.optionA_VO;
            AudioClip clipA = m.optionA_Clip;
            if (currentMomentIndex == 2)
            {
                voKeyA = GetOrderedDishWrongAudioKey(orderedName);
                clipA = GetOrderedDishWrongClip(orderedName);
            }

            string voKeyB = m.optionB_VO;
            AudioClip clipB = m.optionB_Clip;
            if (currentMomentIndex == 0 || (!string.IsNullOrEmpty(m.optionB_Text) && m.optionB_Text.ToLower().Contains("nothing")))
            {
                voKeyB = "";
                clipB = null;
            }

            SetupChoiceCard(optionA_Button, isPolite: true, currentVO: voKeyA, currentClip: clipA, specificAvatar: m.optionA_Avatar);
            SetupChoiceCard(optionB_Button, isPolite: false, currentVO: voKeyB, currentClip: clipB, specificAvatar: m.optionB_Avatar);

            // 1. Update Full Body Ravi: Inspector fullBodyPoseSprite has top priority!
            if (waiterFullBodyAvatar != null)
            {
                Sprite raviPose = m.fullBodyPoseSprite != null ? m.fullBodyPoseSprite : (raviTakingOrder ?? raviGreeting);
                if (raviPose != null)
                {
                    waiterFullBodyAvatar.sprite = raviPose;
                    waiterFullBodyAvatar.preserveAspect = true;
                    waiterFullBodyAvatar.gameObject.SetActive(true);
                    StartCoroutine(PopAvatarAnimation(waiterFullBodyAvatar.transform));
                    Debug.Log($"[U6_WaiterInteractionScreen] Moment {currentMomentIndex + 1}: Ravi Pose set to '{raviPose.name}'");
                }
            }

            // 2. Update Anu: Inspector anuReactionSprite has top priority!
            if (anuCharacterAvatar != null)
            {
                Sprite tableSprite = m.anuReactionSprite != null ? m.anuReactionSprite : familyTableCalmSprite;
                if (tableSprite != null)
                {
                    anuCharacterAvatar.sprite = tableSprite;
                    anuCharacterAvatar.preserveAspect = true;
                    anuCharacterAvatar.gameObject.SetActive(true);
                    StartCoroutine(PopAvatarAnimation(anuCharacterAvatar.transform));
                }
            }

            // 3. Keep Table Water Glasses visible and maintain state (empty before Ravi gives water, filled after)
            SetWaterGlassesFilled(waterHasBeenServed, animate: false);

            // 4. Update Table Prop (for Food Dishes / Fork if a separate prop exists)
            if (propItemImage != null && !IsWaterGlassImage(propItemImage))
            {
                if (m.propSprite != null)
                {
                    propItemImage.sprite = m.propSprite;
                    propItemImage.preserveAspect = true;
                    propItemImage.gameObject.SetActive(true);
                    StartCoroutine(PopAvatarAnimation(propItemImage.transform));
                }
                else
                {
                    propItemImage.gameObject.SetActive(false);
                }
            }

            if (waiterExpressionBadge != null)
            {
                waiterExpressionBadge.gameObject.SetActive(false);
            }

            if (choiceContainer) choiceContainer.SetActive(true);
            if (outcomePanel) outcomePanel.SetActive(false);

            isResolving = false;
        }

        private IEnumerator PopAvatarAnimation(Transform target)
        {
            if (target == null) yield break;
            Vector3 originalScale = Vector3.one;
            target.localScale = originalScale * 0.88f;

            float elapsed = 0f;
            float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI * 0.5f);
                target.localScale = Vector3.Lerp(originalScale * 0.88f, originalScale * 1.05f, t);
                yield return null;
            }

            target.localScale = originalScale;
        }

        private void OnOptionSelected(bool choseA)
        {
            if (isResolving) return;
            isResolving = true;

            if (hideTitleBannerCoroutine != null)
            {
                StopCoroutine(hideTitleBannerCoroutine);
                hideTitleBannerCoroutine = null;
            }
            if (titleBanner != null && titleBanner.activeSelf)
            {
                titleBanner.SetActive(false);
            }

            StartCoroutine(ResolveMomentSequence(choseA));
        }

        private IEnumerator HideTitleBannerRoutine()
        {
            if (titleBanner == null) yield break;

            yield return new WaitForSeconds(titleBannerDuration);

            CanvasGroup cg = titleBanner.GetComponent<CanvasGroup>();
            if (cg == null) cg = titleBanner.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            float fadeDuration = 0.45f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (cg != null) cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            if (titleBanner != null)
            {
                titleBanner.SetActive(false);
                if (cg != null) cg.alpha = 1f;
            }
        }

        private IEnumerator ResolveMomentSequence(bool choseA)
        {
            U6_WaiterMomentData_Masters_Activity m = moments[currentMomentIndex];
            if (choiceContainer) choiceContainer.SetActive(false);
            if (outcomePanel) outcomePanel.SetActive(true);

            if (currentMomentIndex == 0)
            {
                // Moment 0 is "Water Served": Waiter Ravi serves the jug of water!
                // Swap all 3 table water glasses to Sprite water glasses_2 with a joyful bounce!
                SetWaterGlassesFilled(true, animate: true);
            }

            if (choseA)
            {
                politeAnswersCount++;
                if (outcomeText) outcomeText.text = !string.IsNullOrEmpty(m.optionA_Outcome) ? m.optionA_Outcome : "Ravi smiles warmly.";
                
                Sprite poseA = (currentMomentIndex == 1 || currentMomentIndex == 2 || currentMomentIndex == 4)
                    ? (m.fullBodyPoseSprite != null ? m.fullBodyPoseSprite : (raviGreeting != null ? raviGreeting : m.fullBodyPoseSprite))
                    : (m.optionA_RaviPose != null ? m.optionA_RaviPose : (raviGreeting != null ? raviGreeting : m.fullBodyPoseSprite));
                SetWaiterPose(poseA, m.optionA_Face);

                bool isAskingMoment = (currentMomentIndex == 1 || currentMomentIndex == 2 || currentMomentIndex == 4);
                Sprite handRaiseTable = familyTableHandRaiseSprite != null ? familyTableHandRaiseSprite : m.anuReactionSprite;
                Sprite calmTable = familyTableCalmSprite != null ? familyTableCalmSprite : m.anuReactionSprite;

                Sprite tableA = isAskingMoment ? (handRaiseTable != null ? handRaiseTable : calmTable) : calmTable;
                if (anuCharacterAvatar != null && tableA != null)
                {
                    anuCharacterAvatar.sprite = tableA;
                    StartCoroutine(PopAvatarAnimation(anuCharacterAvatar.transform));
                }

                PlayScreenSFX(sfxSparkleClip, sfxSparkle);
                if (currentMomentIndex == 2)
                {
                    string orderedDish = (U6_GameManager_Masters_Activity.Instance != null) ? U6_GameManager_Masters_Activity.Instance.OrderedDishName : "dosa";
                    AudioClip wrongClip = GetOrderedDishWrongClip(orderedDish);
                    string wrongKey = GetOrderedDishWrongAudioKey(orderedDish);
                    PlayScreenAudio(wrongClip, wrongKey);
                }
                else
                {
                    PlayScreenAudio(m.optionA_Clip, m.optionA_VO);
                }

                if (currentMomentIndex == 1) 
                {
                    float waitDuration = (m.optionA_Clip != null && m.optionA_Clip.length > 0) ? (m.optionA_Clip.length + 0.35f) : 2.2f;
                    yield return new WaitForSeconds(waitDuration);
                    if (anuCharacterAvatar != null && calmTable != null)
                    {
                        anuCharacterAvatar.sprite = calmTable;
                    }
                    Sprite comingPose = (raviTakingOrder != null) ? raviTakingOrder : (m.optionA_RaviPose ?? raviGreeting);
                    if (comingPose != null)
                    {
                        SetWaiterPose(comingPose, U6_WaiterFaceState.WarmSmile);
                    }
                    PlayScreenAudio(null, "VO_U6_WAIT_2");
                }
                else if (currentMomentIndex == 2) 
                {
                    string orderedDish = (U6_GameManager_Masters_Activity.Instance != null) ? U6_GameManager_Masters_Activity.Instance.OrderedDishName : "dosa";
                    AudioClip wrongClip = GetOrderedDishWrongClip(orderedDish);
                    float waitDuration = (wrongClip != null && wrongClip.length > 0) ? (wrongClip.length + 0.45f) : 3.0f;
                    yield return new WaitForSeconds(waitDuration);
                    if (anuCharacterAvatar != null && calmTable != null)
                    {
                        anuCharacterAvatar.sprite = calmTable;
                    }
                    Sprite apologyPose = (raviApology != null) ? raviApology : m.optionA_RaviPose;
                    if (apologyPose != null)
                    {
                        SetWaiterPose(apologyPose, U6_WaiterFaceState.WarmSmile);
                    }
                    PlayScreenAudio(voWaiterApologizeClip, voWaiterApologize);
                }
                else if (currentMomentIndex == 4)
                {
                    float waitDuration = (m.optionA_Clip != null && m.optionA_Clip.length > 0) ? (m.optionA_Clip.length + 0.35f) : 2.5f;
                    yield return new WaitForSeconds(waitDuration);
                    if (anuCharacterAvatar != null && calmTable != null)
                    {
                        anuCharacterAvatar.sprite = calmTable;
                    }
                    Sprite cutleryPose = (raviCutlery != null) ? raviCutlery : m.optionA_RaviPose;
                    if (cutleryPose != null)
                    {
                        SetWaiterPose(cutleryPose, U6_WaiterFaceState.WarmSmile);
                    }
                }
            }
            else
            {
                if (outcomeText) outcomeText.text = !string.IsNullOrEmpty(m.optionB_Outcome) ? m.optionB_Outcome : "Ravi steps away quietly.";
                
                Sprite poseB = m.optionB_RaviPose != null ? m.optionB_RaviPose : (raviSurprised != null ? raviSurprised : (raviTakingOrder != null ? raviTakingOrder : m.fullBodyPoseSprite));
                SetWaiterPose(poseB, m.optionB_Face);

                Sprite tableB = familyTableReactSprite != null ? familyTableReactSprite : (impoliteAvatarSprite != null ? impoliteAvatarSprite : m.anuReactionSprite);
                if (anuCharacterAvatar != null && tableB != null)
                {
                    anuCharacterAvatar.sprite = tableB;
                    StartCoroutine(PopAvatarAnimation(anuCharacterAvatar.transform));
                }

                bool isSayNothing = (currentMomentIndex == 0)
                                 || (!string.IsNullOrEmpty(m.optionB_VO) && m.optionB_VO.Equals("VO_U6_ANU_3", System.StringComparison.OrdinalIgnoreCase))
                                 || (!string.IsNullOrEmpty(m.optionB_Text) && m.optionB_Text.ToLower().Contains("nothing"));

                if (!isSayNothing)
                {
                    PlayScreenAudio(m.optionB_Clip, m.optionB_VO);
                }
            }

            yield return new WaitForSeconds(4.0f);

            currentMomentIndex++;
            DisplayCurrentMoment();
        }

        private void SetWaiterPose(Sprite targetPose, U6_WaiterFaceState faceFallback)
        {
            Sprite poseToUse = targetPose;
            if (poseToUse == null)
            {
                if (faceFallback == U6_WaiterFaceState.WarmSmile) poseToUse = raviGreeting;
                else if (faceFallback == U6_WaiterFaceState.StiffPolite) poseToUse = raviSurprised ?? raviTakingOrder;
                else poseToUse = raviTakingOrder;
            }

            if (waiterFullBodyAvatar != null && poseToUse != null)
            {
                waiterFullBodyAvatar.sprite = poseToUse;
                waiterFullBodyAvatar.color = Color.white;
                waiterFullBodyAvatar.preserveAspect = true;
                StartCoroutine(PopAvatarAnimation(waiterFullBodyAvatar.transform));
                return;
            }
            SetWaiterFace(faceFallback);
        }

        private void SetWaiterFace(U6_WaiterFaceState state)
        {
            Sprite targetFace = null;
            Color tintColor = Color.white;

            switch (state)
            {
                case U6_WaiterFaceState.WarmSmile:
                    targetFace = waiterSmileSprite;
                    tintColor = new Color(0.95f, 1f, 0.95f, 1f);
                    break;
                case U6_WaiterFaceState.BlankNeutral:
                    targetFace = waiterNeutralSprite;
                    tintColor = Color.white;
                    break;
                case U6_WaiterFaceState.StiffPolite:
                    targetFace = waiterStiffSprite;
                    tintColor = new Color(1f, 0.92f, 0.92f, 1f);
                    break;
            }

            // Show expression reaction directly in place of the main waiter avatar
            if (waiterFullBodyAvatar != null && targetFace != null)
            {
                waiterFullBodyAvatar.sprite = targetFace;
                waiterFullBodyAvatar.color = tintColor;
                StartCoroutine(PopAvatarAnimation(waiterFullBodyAvatar.transform));
            }

            if (waiterExpressionBadge != null)
            {
                waiterExpressionBadge.gameObject.SetActive(false);
            }
        }

        private IEnumerator CompletePart3Sequence()
        {
            if (outcomePanel) outcomePanel.SetActive(true);

            if (politeAnswersCount >= 3)
            {
                if (promptText) promptText.text = "Great job showing kindness to Ravi!";
                if (outcomeText) outcomeText.text = $"Star 2 Earned! ({politeAnswersCount}/5 Polite Choices)";

                if (waiterFullBodyAvatar != null && (raviWave != null || raviGreeting != null))
                {
                    waiterFullBodyAvatar.sprite = raviWave != null ? raviWave : raviGreeting;
                    waiterFullBodyAvatar.color = Color.white;
                    StartCoroutine(PopAvatarAnimation(waiterFullBodyAvatar.transform));
                }

                if (U6_GameManager_Masters_Activity.Instance != null)
                {
                    U6_GameManager_Masters_Activity.Instance.AwardStar();
                }
            }
            else
            {
                if (promptText) promptText.text = "Remember to be polite to the waiter next time!";
                if (outcomeText) outcomeText.text = $"Be gentler next time to earn Star 2 ({politeAnswersCount}/5 Polite Choices)";
            }

            yield return new WaitForSeconds(3.2f);

            if (U6_GameManager_Masters_Activity.Instance != null)
            {
                U6_GameManager_Masters_Activity.Instance.StartPart4();
            }
        }
    }
}
