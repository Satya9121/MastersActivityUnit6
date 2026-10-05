#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Googolplex.Unit8
{
    public static class U8_SceneSetupTool_Masters_Activity
    {
        public const string ScenePath = "Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity";
        private const string ArtPath = "Assets/SeniorsActivityUnit8/Art";
        private const string MoreSpritesPath = "Assets/SeniorsActivityUnit8/Art/more sprites";

        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);

        [MenuItem("Googolplex/Unit 8/Setup Complete Unit 8 Scene", false, 10)]
        public static void SetupCompleteUnit8Scene()
        {
            BuildSpriteCache();

            string sceneDir = "Assets/SeniorsActivityUnit8/Scenes";
            if (!Directory.Exists(sceneDir))
            {
                Directory.CreateDirectory(sceneDir);
                AssetDatabase.Refresh();
            }

            EnsureEventSystem();
            EnsureCamera();

            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            // Clean any old loose background on Canvas root
            Transform looseBg = canvasRect.Find("BG_Image");
            if (looseBg != null)
            {
                Object.DestroyImmediate(looseBg.gameObject);
            }

            // Setup Managers
            GameObject managersGo = GameObject.Find("--- MANAGERS ---");
            if (managersGo == null) managersGo = new GameObject("--- MANAGERS ---");

            var gm = managersGo.GetComponent<U8_GameManager_Masters_Activity>();
            if (gm == null) gm = managersGo.AddComponent<U8_GameManager_Masters_Activity>();

            var am = managersGo.GetComponent<U8_AudioManager_Masters_Activity>();
            if (am == null) am = managersGo.AddComponent<U8_AudioManager_Masters_Activity>();
            am.EnsureAudioSourcesOnMainCamera();
            am.AutoPopulateClipsInEditor();

            // Setup Screens
            GameObject p1 = CreateOrGetScreen(canvasRect, "U8_Screen_01_Part1Door", typeof(U8_Part1_DoorController_Masters_Activity));
            GameObject p2 = CreateOrGetScreen(canvasRect, "U8_Screen_02_Part2Inside", typeof(U8_Part2_WashroomInsideController_Masters_Activity));
            GameObject hw = CreateOrGetScreen(canvasRect, "U8_Screen_03_Handwash", typeof(U8_HandwashScreenController_Masters_Activity));
            GameObject p3 = CreateOrGetScreen(canvasRect, "U8_Screen_04_Part3AfterYou", typeof(U8_Part3_AfterYouController_Masters_Activity));
            GameObject p4 = CreateOrGetScreen(canvasRect, "U8_Screen_05_Part4EmptySoap", typeof(U8_Part4_EmptySoapController_Masters_Activity));
            GameObject end = CreateOrGetScreen(canvasRect, "U8_Screen_06_Ending", typeof(U8_EndingScreen_Masters_Activity));

            SetupPart1UI(p1);
            SetupPart2UI(p2);
            SetupHandwashUI(hw);
            SetupPart3UI(p3);
            SetupPart4UI(p4);
            SetupEndingUI(end);

            WireControllersSerializedSprites(p1, p2, hw, p3, p4, end);

            p1.SetActive(true);
            p2.SetActive(false);
            hw.SetActive(false);
            p3.SetActive(false);
            p4.SetActive(false);
            end.SetActive(false);

            // Mark active scene dirty & save
            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);
            if (string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.SaveScene(currentScene, ScenePath);
            }

            EditorUtility.DisplayDialog("Unit 8 Scene Setup", 
                $"Unit 8: Washroom Etiquette scene hierarchy created!\nBackground stretched edge-to-edge, header text aligned, and typography scaled for mobile.\n\nTarget scene location:\n{ScenePath}", "OK");
            Debug.Log($"[U8_SceneSetupTool] Completed full Unit 8 scene generation. Target: {ScenePath}");
        }

        [MenuItem("Googolplex/Unit 8/Setup Screen 1 Only (Part 1 Door & Anu Walking)", false, 10)]
        public static void SetupScreen1Only()
        {
            BuildSpriteCache();
            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject p1 = CreateOrGetScreen(canvasRect, "U8_Screen_01_Part1Door", typeof(U8_Part1_DoorController_Masters_Activity));
            SetupPart1UI(p1);
            WirePart1Controller(p1);

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Screen 1 Updated", "Screen 1 (Part 1: The Door) has been updated with Anu Walking & Knocking animation, Animator, and Controller!", "OK");
            Debug.Log("[U8_SceneSetupTool] Setup Screen 1 only completed.");
        }

        [MenuItem("Googolplex/Unit 8/Setup Screen 2 Only (Part 2 Inside)", false, 11)]
        public static void SetupScreen2Only()
        {
            BuildSpriteCache();
            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject p2 = CreateOrGetScreen(canvasRect, "U8_Screen_02_Part2Inside", typeof(U8_Part2_WashroomInsideController_Masters_Activity));
            SetupPart2UI(p2);

            var c2 = p2.GetComponent<U8_Part2_WashroomInsideController_Masters_Activity>();
            if (c2 != null)
            {
                var so = new SerializedObject(c2);
                SetSerializedSprite(so, "cubicleOpenSprite", GetSprite("SPR_Cubicle_Open"));
                SetSerializedSprite(so, "cubicleClosedSprite", GetSprite("SPR_Cubicle_Closed"));
                SetSerializedSprite(so, "sinkCleanSprite", GetSprite("SPR_Sink_Clean"));
                SetSerializedSprite(so, "sinkSplashedSprite", GetSprite("SPR_Sink_Splashed"));
                SetSerializedSprite(so, "anuNormalSprite", GetSprite("SPR_Anu_Normal"));
                SetSerializedSprite(so, "anuWashingSprite", GetSprite("SPR_Anu_Washing"));
                SetSerializedSprite(so, "anuWipingSprite", GetSprite("SPR_Anu_Wiping"));
                so.ApplyModifiedProperties();

                Transform anuT = p2.transform.Find("AnuInsideAvatar") ?? p2.transform.Find("AnuAvatar");
                if (anuT != null)
                {
                    var img = anuT.GetComponent<Image>();
                    if (img != null) { img.preserveAspect = true; }
                }
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Screen 2 Updated", "Screen 2 (Part 2: Inside Washroom) has been updated! Screen 1 and other screens were not modified.", "OK");
            Debug.Log("[U8_SceneSetupTool] Setup Screen 2 only completed.");
        }

        [MenuItem("Googolplex/Unit 8/Setup Screen 3 Only (Handwash Video & Song)", false, 12)]
        public static void SetupScreen3Only()
        {
            BuildSpriteCache();
            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject hw = CreateOrGetScreen(canvasRect, "U8_Screen_03_Handwash", typeof(U8_HandwashScreenController_Masters_Activity));
            SetupHandwashUI(hw);

            var chw = hw.GetComponent<U8_HandwashScreenController_Masters_Activity>();
            if (chw != null)
            {
                var so = new SerializedObject(chw);
                SetSerializedSprite(so, "handsSoapySprite", GetSprite("SPR_Hands_Soapy"));
                SetSerializedSprite(so, "handsSparklingSprite", GetSprite("SPR_Hands_Sparkling"));
                so.ApplyModifiedProperties();
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Screen 3 Updated", "Screen 3 (Handwashing Video & 20s Song) has been updated and Video Player is wired!", "OK");
            Debug.Log("[U8_SceneSetupTool] Setup Screen 3 only completed.");
        }

        [MenuItem("Googolplex/Unit 8/Setup Screen 4 Only (Part 3 After You)", false, 13)]
        public static void SetupScreen4Only()
        {
            BuildSpriteCache();
            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject p3 = CreateOrGetScreen(canvasRect, "U8_Screen_04_Part3AfterYou", typeof(U8_Part3_AfterYouController_Masters_Activity));
            SetupPart3UI(p3);

            var c3 = p3.GetComponent<U8_Part3_AfterYouController_Masters_Activity>();
            if (c3 != null)
            {
                var so = new SerializedObject(c3);
                SetSerializedSprite(so, "meeraEnteringSprite", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_Right_1.png"));
                SetSerializedSprite(so, "meeraIdleSprite", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_Right_Idle.png"));
                SetSerializedSprite(so, "meeraSlippingSprite", GetSprite("SPR_Meera_Slip"));
                SetSerializedSprite(so, "meeraUnimpressedSprite", GetSprite("SPR_Meera_Unimpressed"));
                SetSerializedSprite(so, "meeraHappySmileSprite", GetSprite("SPR_Meera_Happy"));
                SetSerializedSprite(so, "sinkCleanSprite", GetSprite("SPR_Sink_Clean"));
                SetSerializedSprite(so, "sinkSplashedSprite", GetSprite("SPR_Sink_Splashed"));

                var walkArrayProp = so.FindProperty("meeraWalkSprites");
                if (walkArrayProp != null)
                {
                    walkArrayProp.arraySize = 8;
                    for (int i = 0; i < 8; i++)
                    {
                        var elem = walkArrayProp.GetArrayElementAtIndex(i);
                        elem.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_Right_{i + 1}.png");
                    }
                }

                SetSerializedGameObject(so, "wetFloorPuddleObject", p3.transform.Find("WetFloorPuddleObject")?.gameObject);
                SetSerializedGameObject(so, "soggyTowelFloorObject", p3.transform.Find("SoggyTowelFloorObject")?.gameObject);
                SetSerializedGameObject(so, "runningWaterStreamObject", p3.transform.Find("RunningWaterStreamObject")?.gameObject);
                SetSerializedGameObject(so, "splashedSinkOverlay", p3.transform.Find("SplashedSinkOverlay")?.gameObject ?? p3.transform.Find("SinkSplashed")?.gameObject);

                Transform sinkT = p3.transform.Find("SinkImage") ?? p3.transform.Find("Sink/SinkImage") ?? p3.transform.Find("Sink");
                if (sinkT != null)
                {
                    var sinkProp = so.FindProperty("sinkImage");
                    if (sinkProp != null) sinkProp.objectReferenceValue = sinkT.GetComponent<Image>();
                }

                Transform avatarT = p3.transform.Find("MeeraAvatar") ?? p3.transform.Find("Meera_Image");
                if (avatarT != null)
                {
                    var avatarProp = so.FindProperty("meeraAvatar");
                    var img = avatarT.GetComponent<Image>();
                    if (avatarProp != null) avatarProp.objectReferenceValue = img;
                    if (img != null)
                    {
                        img.preserveAspect = true;
                        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_Right_Idle.png");
                    }
                }

                so.ApplyModifiedProperties();
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Screen 4 Updated", "Screen 4 (Part 3: AFTER YOU) has been updated with full puddle, sink, tap, and Meera character staging!", "OK");
            Debug.Log("[U8_SceneSetupTool] Setup Screen 4 only completed.");
        }

        [MenuItem("Googolplex/Unit 8/Setup Screen 5 Only (Part 4 Empty Soap)", false, 14)]
        public static void SetupScreen5Only()
        {
            BuildSpriteCache();
            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject p4 = CreateOrGetScreen(canvasRect, "U8_Screen_05_Part4EmptySoap", typeof(U8_Part4_EmptySoapController_Masters_Activity));
            SetupPart4UI(p4);

            var c4 = p4.GetComponent<U8_Part4_EmptySoapController_Masters_Activity>();
            if (c4 != null)
            {
                var so = new SerializedObject(c4);
                SetSerializedSprite(so, "meeraPumpingSoapSprite", GetSprite("SPR_Meera_PumpSoap"));
                SetSerializedSprite(so, "meeraShruggingSprite", GetSprite("SPR_Meera_Unimpressed"));
                SetSerializedSprite(so, "meeraHappySprite", GetSprite("SPR_Meera_Happy"));
                SetSerializedSprite(so, "soapEmptySprite", GetSprite("SPR_Soap_Empty"));
                SetSerializedSprite(so, "soapFullRefilledSprite", GetSprite("SPR_Soap_Full"));

                Transform soapT = p4.transform.Find("SoapDispenserImage") ?? p4.transform.Find("SoapDispenser");
                if (soapT != null)
                {
                    var soapProp = so.FindProperty("soapDispenserImage");
                    if (soapProp != null) soapProp.objectReferenceValue = soapT.GetComponent<Image>();
                }

                Transform avatarT = p4.transform.Find("MeeraAvatar") ?? p4.transform.Find("Meera_Image");
                if (avatarT != null)
                {
                    var avatarProp = so.FindProperty("meeraAvatar");
                    var img = avatarT.GetComponent<Image>();
                    if (avatarProp != null) avatarProp.objectReferenceValue = img;
                    if (img != null) { img.preserveAspect = true; }
                }

                so.ApplyModifiedProperties();
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Screen 5 Updated", "Screen 5 (Part 4: EMPTY SOAP) has been updated with Sink, Soap Dispenser, and Meera!", "OK");
            Debug.Log("[U8_SceneSetupTool] Setup Screen 5 only completed.");
        }

        [MenuItem("Googolplex/Unit 8/Setup Screen 6 Only (Ending & Stars)", false, 15)]
        public static void SetupScreen6Only()
        {
            BuildSpriteCache();
            GameObject canvasGo = EnsureCanvas();
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            GameObject end = CreateOrGetScreen(canvasRect, "U8_Screen_06_Ending", typeof(U8_EndingScreen_Masters_Activity));
            SetupEndingUI(end);

            var cend = end.GetComponent<U8_EndingScreen_Masters_Activity>();
            if (cend != null)
            {
                var so = new SerializedObject(cend);
                SetSerializedSprite(so, "starEarnedSprite", GetOrCreateStarSprite(true));
                SetSerializedSprite(so, "starEmptySprite", GetOrCreateStarSprite(false));

                Transform starsCont = end.transform.Find("StarsContainer") ?? end.transform;
                SerializedProperty starImagesProp = so.FindProperty("starImages");
                if (starImagesProp != null)
                {
                    starImagesProp.ClearArray();
                    for (int i = 1; i <= 3; i++)
                    {
                        Transform starT = starsCont.Find($"Star_{i}") ?? end.transform.Find($"Star_{i}");
                        if (starT != null)
                        {
                            Image img = starT.GetComponent<Image>();
                            if (img != null)
                            {
                                starImagesProp.InsertArrayElementAtIndex(starImagesProp.arraySize);
                                starImagesProp.GetArrayElementAtIndex(starImagesProp.arraySize - 1).objectReferenceValue = img;
                                img.sprite = GetOrCreateStarSprite(false);
                                img.color = Color.white;
                                img.preserveAspect = true;
                            }
                        }
                    }
                }

                Transform avatarT = end.transform.Find("MeeraWavingAvatar") ?? end.transform.Find("MeeraAvatar");
                if (avatarT != null)
                {
                    var avatarProp = so.FindProperty("meeraWavingAvatar");
                    var img = avatarT.GetComponent<Image>();
                    if (avatarProp != null) avatarProp.objectReferenceValue = img;
                    if (img != null) { img.preserveAspect = true; }
                }

                Transform btnRestartT = end.transform.Find("Btn_Restart") ?? end.transform.Find("BottomBar/Btn_Restart");
                if (btnRestartT != null)
                {
                    var btnProp = so.FindProperty("btnRestartActivity");
                    if (btnProp != null) btnProp.objectReferenceValue = btnRestartT.GetComponent<Button>();
                }

                so.ApplyModifiedProperties();
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Screen 6 Updated", "Screen 6 (Ending & Stars) has been updated with SPR_Icon_GoldStar and SPR_Icon_StarOutline from U6 MA Props Sprite Sheet!", "OK");
            Debug.Log("[U8_SceneSetupTool] Setup Screen 6 only completed.");
        }

        [MenuItem("Googolplex/Unit 8/Clean Duplicate UI Buttons", false, 15)]
        public static void CleanDuplicateUIButtons()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            int cleanedCount = 0;
            string[] screens = { "U8_Screen_01_Part1Door", "U8_Screen_02_Part2Inside", "U8_Screen_03_Handwash", "U8_Screen_04_Part3AfterYou", "U8_Screen_05_Part4EmptySoap", "U8_Screen_06_Ending" };
            string[] looseBtnNames = { "Btn_TryAgain", "Btn_ProceedToPart4", "Btn_Finish", "Btn_Knock", "Btn_Bang", "Btn_Push", "Btn_TellTeacher", "Btn_JustLeave" };

            foreach (string screenName in screens)
            {
                Transform screenT = canvas.transform.Find(screenName);
                if (screenT == null) continue;

                Transform choiceContainer = screenT.Find("ChoiceContainer");
                if (choiceContainer != null)
                {
                    foreach (string btnName in looseBtnNames)
                    {
                        Transform looseBtn = screenT.Find(btnName);
                        if (looseBtn != null)
                        {
                            Object.DestroyImmediate(looseBtn.gameObject);
                            cleanedCount++;
                        }
                    }
                }
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Cleaned Duplicate Buttons", $"Removed {cleanedCount} duplicate loose buttons from scene hierarchy!", "OK");
            Debug.Log($"[U8_SceneSetupTool] Removed {cleanedCount} duplicate loose buttons.");
        }

        [MenuItem("Googolplex/Unit 8/Wire Sprites & Audio Only (Non-Destructive)", false, 15)]
        public static void WireSpritesAndAudioNonDestructive()
        {
            BuildSpriteCache();

            // Setup Managers & Audio without touching hierarchy
            GameObject managersGo = GameObject.Find("--- MANAGERS ---");
            if (managersGo != null)
            {
                var am = managersGo.GetComponent<U8_AudioManager_Masters_Activity>();
                if (am != null)
                {
                    am.EnsureAudioSourcesOnMainCamera();
                    am.AutoPopulateClipsInEditor();
                }
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform p1 = canvas.transform.Find("U8_Screen_01_Part1Door");
                Transform p2 = canvas.transform.Find("U8_Screen_02_Part2Inside");
                Transform hw = canvas.transform.Find("U8_Screen_03_Handwash");
                Transform p3 = canvas.transform.Find("U8_Screen_04_Part3AfterYou");
                Transform p4 = canvas.transform.Find("U8_Screen_05_Part4EmptySoap");
                Transform end = canvas.transform.Find("U8_Screen_06_Ending");

                WireControllersSerializedSprites(
                    p1 != null ? p1.gameObject : null,
                    p2 != null ? p2.gameObject : null,
                    hw != null ? hw.gameObject : null,
                    p3 != null ? p3.gameObject : null,
                    p4 != null ? p4.gameObject : null,
                    end != null ? end.gameObject : null
                );

                SanitizeAllSceneTexts(canvas.gameObject);
            }

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);

            EditorUtility.DisplayDialog("Non-Destructive Wire Complete", "All sprites, audio references, and button text glyphs have been sanitized and re-wired! Your GameObjects and positions were kept 100% intact.", "OK");
            Debug.Log("[U8_SceneSetupTool] Non-destructive sprite and audio wiring completed.");
        }

        [MenuItem("Googolplex/Unit 8/Sanitize All UI Text Glyphs", false, 13)]
        public static void SanitizeAllUITextGlyphsMenu()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                int count = SanitizeAllSceneTexts(canvas.gameObject);
                Scene currentScene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(currentScene);
                EditorUtility.DisplayDialog("Text Sanitization Complete", $"Cleaned {count} TextMeshPro labels in the scene. All unsupported arrow and bullet characters were replaced with standard symbols.", "OK");
            }
        }

        private static int SanitizeAllSceneTexts(GameObject root)
        {
            if (root == null) return 0;
            var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            int modifiedCount = 0;
            foreach (var t in texts)
            {
                if (t == null || string.IsNullOrEmpty(t.text)) continue;
                string original = t.text;
                string clean = original
                    .Replace("\u2794", "")
                    .Replace("➔", "")
                    .Replace(">>", "")
                    .Replace("->", "")
                    .Replace("\u21BA", "")
                    .Replace("↺", "")
                    .Replace("\u2022", "-")
                    .Replace("•", "-")
                    .Trim();

                if (clean != original)
                {
                    Undo.RecordObject(t, "Sanitize TMP Unicode Glyphs");
                    t.text = clean;
                    EditorUtility.SetDirty(t);
                    modifiedCount++;
                }
            }
            return modifiedCount;
        }

        public static Sprite GetOrCreateRoundedBoxSprite()
        {
            string dir = ArtPath;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = dir + "/UI_RoundedBox_9Slice.png";
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            int size = 128;
            int radius = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Max(0, Mathf.Max(radius - x, x - (size - 1 - radius)));
                    int dy = Mathf.Max(0, Mathf.Max(radius - y, y - (size - 1 - radius)));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(radius, radius, radius, radius);
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static Sprite GetOrCreateStarSprite(bool earned)
        {
            if (earned)
            {
                Sprite s = GetSprite("SPR_Icon_GoldStar");
                if (s != null) return s;
            }
            else
            {
                Sprite s = GetSprite("SPR_Icon_StarOutline");
                if (s != null) return s;
            }

            string dir = ArtPath;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string filename = earned ? "SPR_Star_Gold.png" : "SPR_Star_Empty.png";
            string path = Path.Combine(dir, filename).Replace("\\", "/");
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerR = size * 0.44f;
            float innerR = outerR * 0.42f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x + 0.5f, y + 0.5f) - center;
                    float angle = Mathf.Atan2(pt.y, pt.x);
                    if (angle < 0) angle += Mathf.PI * 2f;
                    angle = (angle + Mathf.PI / 2f) % (Mathf.PI * 2f);

                    float segment = (Mathf.PI * 2f) / 10f;
                    int segIndex = Mathf.FloorToInt(angle / segment);
                    float localAngle = (angle % segment) / segment;

                    float r1 = (segIndex % 2 == 0) ? outerR : innerR;
                    float r2 = (segIndex % 2 == 0) ? innerR : outerR;
                    float expectedR = Mathf.Lerp(r1, r2, localAngle);

                    float dist = pt.magnitude;
                    float delta = expectedR - dist;
                    float alpha = Mathf.Clamp01(delta + 0.75f);

                    if (earned)
                    {
                        Color gold = new Color(1f, 0.85f, 0.15f, 1f);
                        colors[y * size + x] = new Color(gold.r, gold.g, gold.b, alpha);
                    }
                    else
                    {
                        Color empty = new Color(0.4f, 0.45f, 0.55f, 0.6f);
                        colors[y * size + x] = new Color(empty.r, empty.g, empty.b, alpha * empty.a);
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void BuildSpriteCache()
        {
            spriteCache.Clear();

            string[] searchDirs = new[] { MoreSpritesPath, ArtPath };
            foreach (var dir in searchDirs)
            {
                if (!Directory.Exists(dir)) continue;

                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { dir });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var asset in allAssets)
                    {
                        if (asset is Sprite s)
                        {
                            if (!spriteCache.ContainsKey(s.name))
                                spriteCache[s.name] = s;
                        }
                    }
                }
            }

            Debug.Log($"[U8_SceneSetupTool] Sliced Sprite Cache initialized with {spriteCache.Count} sprites.");
        }

        private static Sprite GetSprite(string name)
        {
            if (spriteCache.TryGetValue(name, out Sprite s)) return s;

            // Direct file path fallback
            Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtPath}/{name}.png");
            if (direct != null) return direct;

            // Common aliases
            if (name == "SPR_Meera_Entering" && spriteCache.TryGetValue("Character \u2014 Meera U8 SA _0", out Sprite s0)) return s0;
            if (name == "SPR_Washroom_BG" && spriteCache.TryGetValue("SPR_Washroom_BG", out Sprite bg)) return bg;

            return null;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }
        }

        private static void EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = Object.FindFirstObjectByType<Camera>();
                if (cam == null)
                {
                    GameObject camGo = new GameObject("Main Camera");
                    cam = camGo.AddComponent<Camera>();
                    cam.tag = "MainCamera";
                }
            }
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.10f, 0.14f, 0.20f);

            // Ensure AudioListener on Main Camera
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener == null) listener = cam.gameObject.AddComponent<AudioListener>();
            listener.enabled = true;

            // Ensure 4 2D AudioSources on Main Camera (BGM, AMB, SFX, VO)
            AudioSource[] sources = cam.GetComponents<AudioSource>();
            while (sources == null || sources.Length < 4)
            {
                cam.gameObject.AddComponent<AudioSource>();
                sources = cam.GetComponents<AudioSource>();
            }

            sources[0].loop = true;
            sources[0].playOnAwake = false;
            sources[0].volume = 0.45f;
            sources[0].spatialBlend = 0f;

            sources[1].loop = true;
            sources[1].playOnAwake = false;
            sources[1].volume = 0.35f;
            sources[1].spatialBlend = 0f;

            sources[2].loop = false;
            sources[2].playOnAwake = false;
            sources[2].volume = 1.0f;
            sources[2].spatialBlend = 0f;

            sources[3].loop = false;
            sources[3].playOnAwake = false;
            sources[3].volume = 1.0f;
            sources[3].spatialBlend = 0f;
        }

        private static GameObject EnsureCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Unit8_Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();
                return canvasGo;
            }
            return canvas.gameObject;
        }

        private static GameObject CreateOrGetScreen(RectTransform parent, string screenName, System.Type controllerType)
        {
            Transform t = parent.Find(screenName);
            GameObject screenGo;
            if (t == null)
            {
                screenGo = new GameObject(screenName, typeof(RectTransform));
                screenGo.transform.SetParent(parent, false);
                RectTransform rt = screenGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                screenGo = t.gameObject;
            }

            if (controllerType != null && screenGo.GetComponent(controllerType) == null)
            {
                screenGo.AddComponent(controllerType);
            }

            return screenGo;
        }

        private static void CleanObsoleteScreenObjects(GameObject screen)
        {
            string[] obsolete = { "PromptText", "FeedbackText", "DiscussionQuestionText", "TitleText", "Btn_TryAgain", "Btn_ProceedToPart4", "Btn_Finish", "Btn_Knock", "Btn_Bang", "Btn_Push", "Btn_TellTeacher", "Btn_JustLeave" };
            foreach (var name in obsolete)
            {
                Transform t = screen.transform.Find(name);
                if (t != null)
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static void SetupPart1UI(GameObject screen)
        {
            CleanObsoleteScreenObjects(screen);

            // Fullscreen Stretched Background
            CreateImage(screen, "Background", Vector2.zero, Vector2.zero, GetSprite("SPR_Washroom_BG"), true);

            // Pinned Top Header Bar
            CreateHeader(screen, "UNIT 8: WASHROOM ETIQUETTE", "Part 1: The Door");

            // Door Image & Anu Avatar
            CreateImage(screen, "WashroomDoor", new Vector2(-798, 41), new Vector2(442, 774), GetSprite("SPR_Cubicle_Closed"));

            // Anu Avatar: SizeDelta (264, 754), starting position (550, -70), facing left
            var anuImg1 = CreateImage(screen, "AnuAvatar", new Vector2(620, -70), new Vector2(264, 754), GetSprite("SPR_Anu_Walk_1") ?? GetSprite("SPR_Anu_Walk_Idle") ?? GetSprite("SPR_Anu_Normal"));
            if (anuImg1 != null)
            {
                anuImg1.preserveAspect = true;
                anuImg1.rectTransform.localEulerAngles = Vector3.zero;
                anuImg1.rectTransform.localScale = Vector3.one;

                // Add Animator component
                var anim = anuImg1.GetComponent<Animator>();
                if (anim == null) anim = anuImg1.gameObject.AddComponent<Animator>();
                var ctrlAsset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/SeniorsActivityUnit8/Animations/Anu_AnimatorController.controller");
                if (ctrlAsset != null) anim.runtimeAnimatorController = ctrlAsset;

                // Add U8_AnuWalkingController_Masters_Activity
                var walkCtrl = anuImg1.GetComponent<U8_AnuWalkingController_Masters_Activity>();
                if (walkCtrl == null) walkCtrl = anuImg1.gameObject.AddComponent<U8_AnuWalkingController_Masters_Activity>();
                walkCtrl.AutoAssignSpritesIfNeeded();
            }

            // Prompt Card
            CreatePromptCard(screen, "Prompt", "The door is closed. What should Anu do?", new Vector2(0, 380), new Vector2(1300, 90), 38);

            // Feedback Card
            CreateFeedbackCard(screen, "Feedback", "", new Vector2(0, -260), new Vector2(1200, 80), 34, Color.yellow);

            // Choice Buttons (Larger, rounded touch targets with bold text)
            GameObject container = CreateContainer(screen, "ChoiceContainer", new Vector2(0, -385), new Vector2(1400, 110));
            CreateButton(container, "Btn_Knock", "Knock and Wait", new Vector2(-430, 0), new Vector2(390, 88), new Color(0.18f, 0.65f, 0.35f), 30);
            CreateButton(container, "Btn_Bang", "Bang Loudly", new Vector2(0, 0), new Vector2(390, 88), new Color(0.85f, 0.32f, 0.25f), 30);
            CreateButton(container, "Btn_Push", "Push Open", new Vector2(430, 0), new Vector2(390, 88), new Color(0.80f, 0.55f, 0.20f), 30);
        }

        private static void SetupPart2UI(GameObject screen)
        {
            CleanObsoleteScreenObjects(screen);

            // Fullscreen Stretched Background
            CreateImage(screen, "Background", Vector2.zero, Vector2.zero, GetSprite("SPR_Washroom_BG"), true);

            // Pinned Top Header Bar
            CreateHeader(screen, "UNIT 8: WASHROOM ETIQUETTE", "Part 2: Inside the Washroom");

            // 1. Cubicle Section (Left)
            GameObject cubicle = CreateContainer(screen, "Cubicle", new Vector2(-560, -20), new Vector2(380, 580));
            CreateImage(cubicle, "CubicleDoorImage", new Vector2(0, 70), new Vector2(300, 480), GetSprite("SPR_Cubicle_Open"));
            CreateButton(cubicle, "Btn_CloseDoor", "1. Close Door", new Vector2(-85, -200), new Vector2(160, 68), new Color(0.2f, 0.6f, 0.9f), 24);
            CreateButton(cubicle, "Btn_Flush", "2. Flush", new Vector2(85, -200), new Vector2(160, 68), new Color(0.2f, 0.7f, 0.4f), 24);
            CreateButton(cubicle, "Btn_ComeOut", "3. Step Out to Sink", new Vector2(0, -270), new Vector2(330, 64), new Color(0.5f, 0.4f, 0.8f), 24);

            // 2. Sink Section (Center)
            GameObject sink = CreateContainer(screen, "Sink", new Vector2(0, -20), new Vector2(440, 580));
            CreateImage(sink, "SinkImage", new Vector2(0, 70), new Vector2(340, 380), GetSprite("SPR_Sink_Splashed"));
            CreateImage(sink, "TapWaterStream", new Vector2(10, -5), new Vector2(90, 200), GetSprite("SPR_WaterStream"));
            CreateButton(sink, "Btn_WashHands", "4. Wash Hands (20s)", new Vector2(0, -150), new Vector2(320, 64), new Color(0.18f, 0.68f, 0.82f), 25);
            CreateButton(sink, "Btn_TurnTapOff", "5. Turn Tap Off", new Vector2(0, -215), new Vector2(320, 64), new Color(0.85f, 0.52f, 0.20f), 25);
            CreateButton(sink, "Btn_WipeSink", "7. Wipe Sink Dry", new Vector2(0, -280), new Vector2(320, 64), new Color(0.20f, 0.78f, 0.42f), 25);

            // 3. Towel & Bin (Right)
            GameObject towelHolder = CreateContainer(screen, "TowelHolder", new Vector2(560, 90), new Vector2(240, 280));
            CreateImage(towelHolder, "TowelHolderImage", new Vector2(0, 20), new Vector2(200, 220), GetSprite("SPR_Towel_Holder"));
            CreateButton(towelHolder, "Btn_Towel", "6. Paper Towel", new Vector2(0, -105), new Vector2(230, 64), new Color(0.92f, 0.65f, 0.20f), 24);

            CreateImage(screen, "TrashBinImage", new Vector2(560, -210), new Vector2(180, 250), GetSprite("SPR_TrashBin"));
            CreateImage(screen, "TowelInAir", new Vector2(420, -50), new Vector2(90, 70), null);
            CreateImage(screen, "SoggyTowelOnFloor", new Vector2(440, -280), new Vector2(160, 100), GetSprite("SPR_Towel_SoggyFloor"));

            // Anu Avatar
            var anuInsideImg = CreateImage(screen, "AnuInsideAvatar", new Vector2(-240, -80), new Vector2(250, 650), GetSprite("SPR_Anu_Normal"));
            if (anuInsideImg != null) { anuInsideImg.preserveAspect = true; }

            // Prompts & Feedback
            CreatePromptCard(screen, "Prompt", "Step 1: Close the cubicle door first!", new Vector2(0, 380), new Vector2(1350, 90), 36);
            CreateFeedbackCard(screen, "Feedback", "", new Vector2(0, -355), new Vector2(1200, 75), 32, Color.yellow);

            CreateButton(screen, "Btn_ProceedToPart3", "See What Meera Finds", new Vector2(0, -435), new Vector2(500, 80), new Color(0.18f, 0.65f, 0.35f), 30);
        }

        private static void SetupHandwashUI(GameObject screen)
        {
            CleanObsoleteScreenObjects(screen);

            // Fullscreen Stretched Background
            CreateImage(screen, "Background", Vector2.zero, Vector2.zero, GetSprite("SPR_Washroom_BG"), true);

            // Pinned Top Header Bar
            CreateHeader(screen, "UNIT 8: HANDWASHING", "20-Second Song & Scrub");

            // Hands Video with U8_VideoPlayerUI
            RawImage handsVideo = CreateRawImage(screen, "HandsVideo", new Vector2(0, 0), new Vector2(580, 480));
            var videoPlayer = handsVideo.GetComponent<U8_VideoPlayerUI_Masters_Activity>();
            if (videoPlayer == null) videoPlayer = handsVideo.gameObject.AddComponent<U8_VideoPlayerUI_Masters_Activity>();
            videoPlayer.SetupPlayer();

            // Bubbles Container
            GameObject bubbles = CreateContainer(screen, "Bubbles", Vector2.zero, new Vector2(500, 400));
            CreateImage(bubbles, "BubbleCluster", new Vector2(200, -50), new Vector2(300, 280), GetSprite("SPR_Bubbles"));

            // 8 Friendly Germ Blobs across handwashing zones
            GameObject germs = CreateContainer(screen, "Germs", Vector2.zero, new Vector2(600, 480));
            CreateImage(germs, "Germ_1", new Vector2(-170, 130), new Vector2(120, 120), GetSprite("SPR_Germ_1"));  // Left Fingers
            CreateImage(germs, "Germ_2", new Vector2(170, 130), new Vector2(120, 120), GetSprite("SPR_Germ_2"));   // Right Fingers
            CreateImage(germs, "Germ_3", new Vector2(-130, 20), new Vector2(125, 125), GetSprite("SPR_Germ_3"));   // Left Palm
            CreateImage(germs, "Germ_4", new Vector2(130, 20), new Vector2(125, 125), GetSprite("SPR_Germ_4"));    // Right Palm
            CreateImage(germs, "Germ_5", new Vector2(-230, -50), new Vector2(110, 110), GetSprite("SPR_Germ_4"));  // Left Thumb
            CreateImage(germs, "Germ_6", new Vector2(230, -50), new Vector2(110, 110), GetSprite("SPR_Germ_1"));   // Right Thumb
            CreateImage(germs, "Germ_7", new Vector2(0, 100), new Vector2(115, 115), GetSprite("SPR_Germ_2"));     // Between Fingers
            CreateImage(germs, "Germ_8", new Vector2(0, -135), new Vector2(120, 120), GetSprite("SPR_Germ_3"));    // Wrist Center

            // Large Touch Scrub Target
            CreateButton(screen, "ScrubAreaButton", "TAP / RUB TO SCRUB!", new Vector2(0, -340), new Vector2(500, 95), new Color(0.15f, 0.65f, 0.85f), 34);

            // Timer Badge Card & Prompt
            CreatePromptCard(screen, "Prompt", "Keep tapping or rubbing to scrub hands with soap!", new Vector2(0, 380), new Vector2(1300, 90), 36);

            GameObject timerCard = CreateCard(screen, "TimerBadge", new Vector2(0, 275), new Vector2(240, 85), new Color(0.08f, 0.22f, 0.38f, 0.95f));
            CreateText(timerCard, "TimerText", "20s", Vector2.zero, new Vector2(220, 75), 50, Color.cyan);
        }

        private static void SetupPart3UI(GameObject screen)
        {
            CleanObsoleteScreenObjects(screen);

            // Fullscreen Stretched Background
            CreateImage(screen, "Background", Vector2.zero, Vector2.zero, GetSprite("SPR_Washroom_BG"), true);

            // Pinned Top Header Bar
            CreateHeader(screen, "UNIT 8: AFTER YOU", "Part 3: What Meera Finds");

            CreateImage(screen, "SinkImage", new Vector2(0, -20), new Vector2(340, 380), GetSprite("SPR_Sink_Splashed"));
            CreateImage(screen, "WetFloorPuddleObject", new Vector2(-220, -250), new Vector2(460, 200), GetSprite("SPR_WetFloorPuddle"));
            CreateImage(screen, "SoggyTowelFloorObject", new Vector2(500, -250), new Vector2(170, 110), GetSprite("SPR_Towel_SoggyFloor"));
            CreateImage(screen, "RunningWaterStreamObject", new Vector2(10, -5), new Vector2(90, 200), GetSprite("SPR_WaterStream"));
            var meeraPart3Img = CreateImage(screen, "MeeraAvatar", new Vector2(-520, -50), new Vector2(231, 656), GetSprite("SPR_Meera_Entering"));
            if (meeraPart3Img != null) { meeraPart3Img.preserveAspect = true; }

            CreatePromptCard(screen, "Prompt", "Anu has left the washroom...", new Vector2(0, 380), new Vector2(1300, 90), 38);
            CreateFeedbackCard(screen, "Feedback", "", new Vector2(0, -265), new Vector2(1300, 80), 32, Color.yellow);

            GameObject container = CreateContainer(screen, "ChoiceContainer", new Vector2(0, -385), new Vector2(900, 110));
            CreateButton(container, "Btn_TryAgain", "TRY AGAIN", new Vector2(-240, 0), new Vector2(380, 88), new Color(0.85f, 0.38f, 0.20f), 30);
            CreateButton(container, "Btn_ProceedToPart4", "Next: The Soap!", new Vector2(240, 0), new Vector2(380, 88), new Color(0.18f, 0.65f, 0.35f), 30);
        }

        private static void SetupPart4UI(GameObject screen)
        {
            CleanObsoleteScreenObjects(screen);

            // Fullscreen Stretched Background
            CreateImage(screen, "Background", Vector2.zero, Vector2.zero, GetSprite("SPR_Washroom_BG"), true);

            // Pinned Top Header Bar
            CreateHeader(screen, "UNIT 8: EMPTY SOAP!", "Part 4: What Should Meera Do?");

            CreateImage(screen, "SinkImage", new Vector2(0, -20), new Vector2(340, 380), GetSprite("SPR_Sink_Clean"));
            CreateImage(screen, "SoapDispenserImage", new Vector2(-160, 40), new Vector2(220, 320), GetSprite("SPR_Soap_Empty"));
            var meeraPart4Img = CreateImage(screen, "MeeraAvatar", new Vector2(160, -50), new Vector2(580, 754), GetSprite("SPR_Meera_PumpSoap"));
            if (meeraPart4Img != null) { meeraPart4Img.preserveAspect = true; }

            CreatePromptCard(screen, "Prompt", "The soap is finished. What should Meera do?", new Vector2(0, 380), new Vector2(1350, 90), 36);
            CreateFeedbackCard(screen, "Feedback", "", new Vector2(0, -185), new Vector2(1200, 80), 32, Color.yellow);

            GameObject container = CreateContainer(screen, "ChoiceContainer", new Vector2(0, -295), new Vector2(1000, 110));
            CreateButton(container, "Btn_TellTeacher", "Go and Tell a Teacher", new Vector2(-260, 0), new Vector2(440, 88), new Color(0.18f, 0.65f, 0.35f), 28);
            CreateButton(container, "Btn_JustLeave", "Just Leave", new Vector2(260, 0), new Vector2(400, 88), new Color(0.78f, 0.32f, 0.24f), 28);

            CreateButton(screen, "Btn_Finish", "Finish Unit", new Vector2(0, -405), new Vector2(380, 82), new Color(0.20f, 0.60f, 0.90f), 30);
        }

        private static void SetupEndingUI(GameObject screen)
        {
            CleanObsoleteScreenObjects(screen);

            // Fullscreen Stretched Background
            CreateImage(screen, "Background", Vector2.zero, Vector2.zero, GetSprite("SPR_Washroom_BG"), true);

            // Pinned Top Header Bar
            CreateHeader(screen, "UNIT 8: SUMMARY", "Ready for the Next Person!");

            // Title Banner Card
            GameObject titleCard = CreateCard(screen, "TitleCard", new Vector2(0, 335), new Vector2(1250, 100), new Color(0.10f, 0.22f, 0.38f, 0.95f));
            CreateText(titleCard, "TitleText", "READY FOR THE NEXT PERSON!", Vector2.zero, new Vector2(1200, 80), 46, Color.yellow);

            // Stars Container (Using Star Sprites)
            GameObject stars = CreateContainer(screen, "StarsContainer", new Vector2(0, 195), new Vector2(550, 130));
            Sprite emptyStar = GetOrCreateStarSprite(false);
            CreateImage(stars, "Star_1", new Vector2(-150, 0), new Vector2(120, 120), emptyStar);
            CreateImage(stars, "Star_2", new Vector2(0, 0), new Vector2(120, 120), emptyStar);
            CreateImage(stars, "Star_3", new Vector2(150, 0), new Vector2(120, 120), emptyStar);

            // Meera Character Avatar
            var meeraEndImg = CreateImage(screen, "MeeraWavingAvatar", new Vector2(360, -90), new Vector2(250, 650), GetSprite("SPR_Meera_Happy"));
            if (meeraEndImg != null) { meeraEndImg.preserveAspect = true; }

            // Discussion Question Card
            GameObject questionCard = CreateCard(screen, "QuestionCard", new Vector2(-140, -50), new Vector2(960, 180), new Color(0.10f, 0.16f, 0.25f, 0.92f));
            CreateText(questionCard, "DiscussionQuestionText", "Would the next person be happy entering this washroom?", Vector2.zero, new Vector2(900, 140), 42, new Color(0.40f, 0.90f, 1.0f));

            CreateButton(screen, "Btn_Restart", "Play Again", new Vector2(0, -360), new Vector2(400, 90), new Color(0.18f, 0.68f, 0.38f), 34);
        }

        private static void WireControllersSerializedSprites(GameObject p1, GameObject p2, GameObject hw, GameObject p3, GameObject p4, GameObject end)
        {
            // Part 1 Serialized Fields
            if (p1 != null)
            {
                WirePart1Controller(p1);
            }

            // Part 2 Serialized Fields
            if (p2 != null)
            {
                var c2 = p2.GetComponent<U8_Part2_WashroomInsideController_Masters_Activity>();
                if (c2 != null)
                {
                    var so = new SerializedObject(c2);
                    SetSerializedSprite(so, "cubicleOpenSprite", GetSprite("SPR_Cubicle_Open"));
                    SetSerializedSprite(so, "cubicleClosedSprite", GetSprite("SPR_Cubicle_Closed"));
                    SetSerializedSprite(so, "sinkCleanSprite", GetSprite("SPR_Sink_Clean"));
                    SetSerializedSprite(so, "sinkSplashedSprite", GetSprite("SPR_Sink_Splashed"));
                    SetSerializedSprite(so, "anuNormalSprite", GetSprite("SPR_Anu_Normal"));
                    SetSerializedSprite(so, "anuWashingSprite", GetSprite("SPR_Anu_Washing"));
                    SetSerializedSprite(so, "anuWipingSprite", GetSprite("SPR_Anu_Wiping"));
                    so.ApplyModifiedProperties();

                    Transform anuT = p2.transform.Find("AnuInsideAvatar") ?? p2.transform.Find("AnuAvatar");
                    if (anuT != null)
                    {
                        var img = anuT.GetComponent<Image>();
                        if (img != null) { img.preserveAspect = true; }
                    }
                }
            }

            // Handwash Screen Serialized Fields
            if (hw != null)
            {
                var chw = hw.GetComponent<U8_HandwashScreenController_Masters_Activity>();
                if (chw != null)
                {
                    var so = new SerializedObject(chw);
                    SetSerializedSprite(so, "handsSoapySprite", GetSprite("SPR_Hands_Soapy"));
                    SetSerializedSprite(so, "handsSparklingSprite", GetSprite("SPR_Hands_Sparkling"));
                    so.ApplyModifiedProperties();
                }
            }

            // Part 3 Serialized Fields
            if (p3 != null)
            {
                var c3 = p3.GetComponent<U8_Part3_AfterYouController_Masters_Activity>();
                if (c3 != null)
                {
                    var so = new SerializedObject(c3);
                    SetSerializedSprite(so, "meeraEnteringSprite", GetSprite("SPR_Meera_Entering"));
                    SetSerializedSprite(so, "meeraSlippingSprite", GetSprite("SPR_Meera_Slip"));
                    SetSerializedSprite(so, "meeraUnimpressedSprite", GetSprite("SPR_Meera_Unimpressed"));
                    SetSerializedSprite(so, "meeraHappySmileSprite", GetSprite("SPR_Meera_Happy"));
                    SetSerializedSprite(so, "sinkCleanSprite", GetSprite("SPR_Sink_Clean"));
                    SetSerializedSprite(so, "sinkSplashedSprite", GetSprite("SPR_Sink_Splashed"));

                    SetSerializedGameObject(so, "wetFloorPuddleObject", p3.transform.Find("WetFloorPuddleObject")?.gameObject);
                    SetSerializedGameObject(so, "soggyTowelFloorObject", p3.transform.Find("SoggyTowelFloorObject")?.gameObject);
                    SetSerializedGameObject(so, "runningWaterStreamObject", p3.transform.Find("RunningWaterStreamObject")?.gameObject);
                    SetSerializedGameObject(so, "splashedSinkOverlay", p3.transform.Find("SplashedSinkOverlay")?.gameObject ?? p3.transform.Find("SinkSplashed")?.gameObject);
                    
                    Transform sinkT = p3.transform.Find("SinkImage") ?? p3.transform.Find("Sink/SinkImage") ?? p3.transform.Find("Sink");
                    if (sinkT != null)
                    {
                        var sinkProp = so.FindProperty("sinkImage");
                        if (sinkProp != null) sinkProp.objectReferenceValue = sinkT.GetComponent<Image>();
                    }

                    Transform avatarT = p3.transform.Find("MeeraAvatar") ?? p3.transform.Find("Meera_Image");
                    if (avatarT != null)
                    {
                        var avatarProp = so.FindProperty("meeraAvatar");
                        var img = avatarT.GetComponent<Image>();
                        if (avatarProp != null) avatarProp.objectReferenceValue = img;
                        if (img != null) { img.preserveAspect = true; }
                    }

                    so.ApplyModifiedProperties();
                }
            }

            // Part 4 Serialized Fields
            if (p4 != null)
            {
                var c4 = p4.GetComponent<U8_Part4_EmptySoapController_Masters_Activity>();
                if (c4 != null)
                {
                    var so = new SerializedObject(c4);
                    SetSerializedSprite(so, "meeraPumpingSoapSprite", GetSprite("SPR_Meera_PumpSoap"));
                    SetSerializedSprite(so, "meeraShruggingSprite", GetSprite("SPR_Meera_Unimpressed"));
                    SetSerializedSprite(so, "meeraHappySprite", GetSprite("SPR_Meera_Happy"));
                    SetSerializedSprite(so, "soapEmptySprite", GetSprite("SPR_Soap_Empty"));
                    SetSerializedSprite(so, "soapFullRefilledSprite", GetSprite("SPR_Soap_Full"));

                    Transform soapT = p4.transform.Find("SoapDispenserImage") ?? p4.transform.Find("SoapDispenser");
                    if (soapT != null)
                    {
                        var soapProp = so.FindProperty("soapDispenserImage");
                        if (soapProp != null) soapProp.objectReferenceValue = soapT.GetComponent<Image>();
                    }

                    Transform avatarT = p4.transform.Find("MeeraAvatar") ?? p4.transform.Find("Meera_Image");
                    if (avatarT != null)
                    {
                        var avatarProp = so.FindProperty("meeraAvatar");
                        var img = avatarT.GetComponent<Image>();
                        if (avatarProp != null) avatarProp.objectReferenceValue = img;
                        if (img != null) { img.preserveAspect = true; }
                    }

                    so.ApplyModifiedProperties();
                }
            }

            // Ending Screen Serialized Fields
            if (end != null)
            {
                var cend = end.GetComponent<U8_EndingScreen_Masters_Activity>();
                if (cend != null)
                {
                    var so = new SerializedObject(cend);
                    SetSerializedSprite(so, "starEarnedSprite", GetOrCreateStarSprite(true));
                    SetSerializedSprite(so, "starEmptySprite", GetOrCreateStarSprite(false));

                    Transform starsCont = end.transform.Find("StarsContainer") ?? end.transform;
                    SerializedProperty starImagesProp = so.FindProperty("starImages");
                    if (starImagesProp != null)
                    {
                        starImagesProp.ClearArray();
                        for (int i = 1; i <= 3; i++)
                        {
                            Transform starT = starsCont.Find($"Star_{i}") ?? end.transform.Find($"Star_{i}");
                            if (starT != null)
                            {
                                Image img = starT.GetComponent<Image>();
                                if (img != null)
                                {
                                    starImagesProp.InsertArrayElementAtIndex(starImagesProp.arraySize);
                                    starImagesProp.GetArrayElementAtIndex(starImagesProp.arraySize - 1).objectReferenceValue = img;
                                    img.sprite = GetOrCreateStarSprite(false);
                                    img.color = Color.white;
                                    img.preserveAspect = true;
                                }
                            }
                        }
                    }

                    Transform avatarT = end.transform.Find("MeeraWavingAvatar") ?? end.transform.Find("MeeraAvatar");
                    if (avatarT != null)
                    {
                        var avatarProp = so.FindProperty("meeraWavingAvatar");
                        var img = avatarT.GetComponent<Image>();
                        if (avatarProp != null) avatarProp.objectReferenceValue = img;
                        if (img != null) { img.preserveAspect = true; }
                    }

                    Transform btnRestartT = end.transform.Find("Btn_Restart") ?? end.transform.Find("BottomBar/Btn_Restart");
                    if (btnRestartT != null)
                    {
                        var btnProp = so.FindProperty("btnRestartActivity");
                        if (btnProp != null) btnProp.objectReferenceValue = btnRestartT.GetComponent<Button>();
                    }

                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void WirePart1Controller(GameObject p1)
        {
            if (p1 == null) return;
            var c1 = p1.GetComponent<U8_Part1_DoorController_Masters_Activity>();
            if (c1 == null) return;

            var so = new SerializedObject(c1);
            SetSerializedSprite(so, "doorClosedSprite", GetSprite("SPR_Cubicle_Closed"));
            SetSerializedSprite(so, "doorOpenSprite", GetSprite("SPR_Cubicle_Open"));
            SetSerializedSprite(so, "anuNormalSprite", GetSprite("SPR_Anu_Walk_Idle") ?? GetSprite("SPR_Anu_Normal"));
            SetSerializedSprite(so, "anuKnockingSprite", GetSprite("SPR_Anu_Knock_2") ?? GetSprite("SPR_Anu_Knocking"));
            SetSerializedSprite(so, "anuSheepishSprite", GetSprite("SPR_Anu_Sheepish") ?? GetSprite("SPR_Anu_Normal"));

            SetSerializedGameObject(so, "choiceContainer", p1.transform.Find("ChoiceContainer")?.gameObject);

            Transform doorT = p1.transform.Find("WashroomDoor") ?? p1.transform.Find("DoorImage");
            if (doorT != null)
            {
                var doorProp = so.FindProperty("doorImage");
                if (doorProp != null) doorProp.objectReferenceValue = doorT.GetComponent<Image>();
            }

            Transform anuT = p1.transform.Find("AnuAvatar") ?? p1.transform.Find("Anu_Image");
            if (anuT != null)
            {
                var img = anuT.GetComponent<Image>();
                if (img != null)
                {
                    img.preserveAspect = true;
                    var avatarProp = so.FindProperty("anuAvatar");
                    if (avatarProp != null) avatarProp.objectReferenceValue = img;
                }

                var anim = anuT.GetComponent<Animator>();
                if (anim == null) anim = anuT.gameObject.AddComponent<Animator>();
                var ctrlAsset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/SeniorsActivityUnit8/Animations/Anu_AnimatorController.controller");
                if (ctrlAsset != null) anim.runtimeAnimatorController = ctrlAsset;

                var walkCtrl = anuT.GetComponent<U8_AnuWalkingController_Masters_Activity>();
                if (walkCtrl == null) walkCtrl = anuT.gameObject.AddComponent<U8_AnuWalkingController_Masters_Activity>();

                var walkSo = new SerializedObject(walkCtrl);
                var walkSpritesProp = walkSo.FindProperty("walkSprites");
                if (walkSpritesProp != null)
                {
                    walkSpritesProp.ClearArray();
                    for (int i = 1; i <= 4; i++)
                    {
                        Sprite ws = GetSprite($"SPR_Anu_Walk_{i}");
                        walkSpritesProp.InsertArrayElementAtIndex(i - 1);
                        walkSpritesProp.GetArrayElementAtIndex(i - 1).objectReferenceValue = ws;
                    }
                }

                SetSerializedSprite(walkSo, "idleSprite", GetSprite("SPR_Anu_Walk_Idle") ?? GetSprite("SPR_Anu_Normal"));

                var knockSpritesProp = walkSo.FindProperty("knockSprites");
                if (knockSpritesProp != null)
                {
                    knockSpritesProp.ClearArray();
                    for (int i = 1; i <= 3; i++)
                    {
                        Sprite ks = GetSprite($"SPR_Anu_Knock_{i}");
                        knockSpritesProp.InsertArrayElementAtIndex(i - 1);
                        knockSpritesProp.GetArrayElementAtIndex(i - 1).objectReferenceValue = ks;
                    }
                }

                SetSerializedSprite(walkSo, "sheepishSprite", GetSprite("SPR_Anu_Sheepish") ?? GetSprite("SPR_Anu_Normal"));

                var ctrlProp = walkSo.FindProperty("animatorController");
                if (ctrlProp != null) ctrlProp.objectReferenceValue = ctrlAsset;

                var animProp = walkSo.FindProperty("animator");
                if (animProp != null) animProp.objectReferenceValue = anim;

                var rectProp = walkSo.FindProperty("rectTransform");
                if (rectProp != null) rectProp.objectReferenceValue = anuT.GetComponent<RectTransform>();

                var anuImgProp = walkSo.FindProperty("anuImage");
                if (anuImgProp != null) anuImgProp.objectReferenceValue = img;

                walkSo.ApplyModifiedProperties();

                var walkProp = so.FindProperty("anuWalkingController");
                if (walkProp != null) walkProp.objectReferenceValue = walkCtrl;
            }

            Transform promptT = p1.transform.Find("PromptCard/PromptText") ?? p1.transform.Find("PromptText");
            if (promptT != null)
            {
                var promptProp = so.FindProperty("promptText");
                if (promptProp != null) promptProp.objectReferenceValue = promptT.GetComponent<TextMeshProUGUI>();
            }

            Transform feedbackT = p1.transform.Find("FeedbackCard/FeedbackText") ?? p1.transform.Find("FeedbackText");
            if (feedbackT != null)
            {
                var feedbackProp = so.FindProperty("feedbackText");
                if (feedbackProp != null) feedbackProp.objectReferenceValue = feedbackT.GetComponent<TextMeshProUGUI>();
            }

            Transform btnKnockT = p1.transform.Find("ChoiceContainer/Btn_Knock") ?? p1.transform.Find("Btn_Knock");
            if (btnKnockT != null)
            {
                var btnProp = so.FindProperty("btnKnockAndWait");
                if (btnProp != null) btnProp.objectReferenceValue = btnKnockT.GetComponent<Button>();
            }
            Transform btnBangT = p1.transform.Find("ChoiceContainer/Btn_Bang") ?? p1.transform.Find("Btn_Bang");
            if (btnBangT != null)
            {
                var btnProp = so.FindProperty("btnBangLoudly");
                if (btnProp != null) btnProp.objectReferenceValue = btnBangT.GetComponent<Button>();
            }
            Transform btnPushT = p1.transform.Find("ChoiceContainer/Btn_Push") ?? p1.transform.Find("Btn_Push");
            if (btnPushT != null)
            {
                var btnProp = so.FindProperty("btnPushOpen");
                if (btnProp != null) btnProp.objectReferenceValue = btnPushT.GetComponent<Button>();
            }

            so.ApplyModifiedProperties();
        }

        private static void SetSerializedSprite(SerializedObject so, string propertyName, Sprite sprite)
        {
            if (sprite == null) return;
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = sprite;
            }
        }

        private static void SetSerializedGameObject(SerializedObject so, string propertyName, GameObject go)
        {
            if (go == null) return;
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = go;
            }
        }

        private static GameObject CreateContainer(GameObject parent, string name, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        public static GameObject CreateCard(GameObject parent, string name, Vector2 pos, Vector2 size, Color cardColor)
        {
            Transform existing = parent.transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = GetOrCreateRoundedBoxSprite();
            img.type = Image.Type.Sliced;
            img.color = cardColor;
            return go;
        }

        private static Image CreateImage(GameObject parent, string name, Vector2 pos, Vector2 size, Sprite sprite, bool isBackground = false)
        {
            Transform existing = parent.transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();

            if (isBackground)
            {
                go.transform.SetAsFirstSibling();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchoredPosition = pos;
                rt.sizeDelta = size;
            }

            var oldTmp = go.GetComponent<TextMeshProUGUI>();
            if (oldTmp != null) Object.DestroyImmediate(oldTmp);

            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            if (isBackground)
            {
                img.preserveAspect = false;
            }
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(1f, 1f, 1f, 0.2f);
            }
            return img;
        }

        private static RawImage CreateRawImage(GameObject parent, string name, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            RawImage rawImg = go.GetComponent<RawImage>();
            if (rawImg == null) rawImg = go.AddComponent<RawImage>();
            rawImg.color = Color.white;
            return rawImg;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color btnColor, float fontSize = 28)
        {
            Transform existing = parent.transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = GetOrCreateRoundedBoxSprite();
            img.type = Image.Type.Sliced;
            img.color = btnColor;

            Button btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(btnColor, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(btnColor, Color.black, 0.25f);
            btn.colors = colors;

            // Button label
            Transform textTrans = go.transform.Find("Label");
            GameObject textGo = textTrans != null ? textTrans.gameObject : new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            if (textRt == null) textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12, 6);
            textRt.offsetMax = new Vector2(-12, -6);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;

            return btn;
        }

        private static GameObject CreatePromptCard(GameObject parent, string name, string content, Vector2 pos, Vector2 size, float fontSize = 38)
        {
            string cardName = name.EndsWith("Card") ? name : name + "Card";
            GameObject card = CreateCard(parent, cardName, pos, size, new Color(0.08f, 0.14f, 0.22f, 0.94f));

            string textName = name.EndsWith("Text") ? name : name + "Text";
            Transform existingText = card.transform.Find(textName) ?? card.transform.Find("PromptText");
            
            GameObject textGo = existingText != null ? existingText.gameObject : new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(card.transform, false);

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            if (textRt == null) textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(24, 6);
            textRt.offsetMax = new Vector2(-24, -6);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            return card;
        }

        private static GameObject CreateFeedbackCard(GameObject parent, string name, string content, Vector2 pos, Vector2 size, float fontSize = 34, Color? textColor = null)
        {
            string cardName = name.EndsWith("Card") ? name : name + "Card";
            GameObject card = CreateCard(parent, cardName, pos, size, new Color(0.06f, 0.10f, 0.16f, 0.92f));

            string textName = name.EndsWith("Text") ? name : name + "Text";
            Transform existingText = card.transform.Find(textName) ?? card.transform.Find("FeedbackText");

            GameObject textGo = existingText != null ? existingText.gameObject : new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(card.transform, false);

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            if (textRt == null) textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20, 6);
            textRt.offsetMax = new Vector2(-20, -6);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor ?? Color.yellow;
            tmp.enableWordWrapping = true;
            return card;
        }

        private static TextMeshProUGUI CreateText(GameObject parent, string name, string content, Vector2 pos, Vector2 size, float fontSize, Color? color = null)
        {
            Transform existing = parent.transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var oldImg = go.GetComponent<Image>();
            if (oldImg != null) Object.DestroyImmediate(oldImg);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color ?? Color.white;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static void CreateHeader(GameObject screen, string title, string sub)
        {
            Transform existing = screen.transform.Find("HeaderBar");
            GameObject headerCard = existing != null ? existing.gameObject : new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
            headerCard.transform.SetParent(screen.transform, false);

            RectTransform rt = headerCard.GetComponent<RectTransform>() ?? headerCard.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -12f);
            rt.sizeDelta = new Vector2(-40f, 80f);

            Image img = headerCard.GetComponent<Image>() ?? headerCard.AddComponent<Image>();
            img.sprite = GetOrCreateRoundedBoxSprite();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.06f, 0.10f, 0.16f, 0.94f);

            Transform textTrans = headerCard.transform.Find("TitleText");
            GameObject textGo = textTrans != null ? textTrans.gameObject : new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(headerCard.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>() ?? textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(24, 0);
            textRt.offsetMax = new Vector2(-24, 0);

            var tmp = textGo.GetComponent<TextMeshProUGUI>() ?? textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = $"<color=#FFD700><b>{title}</b></color> - <color=#FFFFFF><b>{sub}</b></color>";
            tmp.fontSize = 32;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
        }
    }
}
#endif
