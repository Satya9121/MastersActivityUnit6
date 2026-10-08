#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Googolplex.Unit8
{
    public static class U8_WipeVideoContainerSceneSetup
    {
        private const string ScenePath = "Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity";

        [MenuItem("Googolplex/Unit 8/Setup Wipe Video Container in Scene", false, 25)]
        public static void EnsureWipeVideoContainerInScene()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[U8_WipeVideoContainerSceneSetup] Setup cannot be run during Play Mode.");
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            bool isCorrectScene = activeScene.IsValid() && (activeScene.path == ScenePath || activeScene.name == "SeniorsActivity_unit8");

            if (!isCorrectScene)
            {
                if (System.IO.File.Exists(ScenePath))
                {
                    activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
                else
                {
                    return;
                }
            }

            // Find U8_Screen_02_Part2Inside
            GameObject part2Screen = GameObject.Find("U8_Screen_02_Part2Inside");
            if (part2Screen == null)
            {
                var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (var t in allTransforms)
                {
                    if (t.name == "U8_Screen_02_Part2Inside" && t.gameObject.scene == activeScene)
                    {
                        part2Screen = t.gameObject;
                        break;
                    }
                }
            }

            if (part2Screen == null)
            {
                Debug.LogWarning("[U8_WipeVideoContainerSceneSetup] U8_Screen_02_Part2Inside not found in scene.");
                return;
            }

            var controller = part2Screen.GetComponent<U8_Part2_WashroomInsideController_Masters_Activity>();
            if (controller == null)
            {
                Debug.LogWarning("[U8_WipeVideoContainerSceneSetup] Controller component not found on U8_Screen_02_Part2Inside.");
                return;
            }

            // Check if WipeVideoContainer already exists under part2Screen
            Transform containerT = part2Screen.transform.Find("WipeVideoContainer");
            GameObject containerGo = containerT != null ? containerT.gameObject : null;

            bool createdNew = false;
            if (containerGo == null)
            {
                // Create WipeVideoContainer root
                containerGo = new GameObject("WipeVideoContainer", typeof(RectTransform), typeof(CanvasGroup));
                containerGo.transform.SetParent(part2Screen.transform, false);
                containerGo.transform.SetAsLastSibling();

                RectTransform rt = containerGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;

                var cg = containerGo.GetComponent<CanvasGroup>();
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = false;

                // Child 1: Background dark overlay
                GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bgGo.transform.SetParent(containerGo.transform, false);
                RectTransform bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
                bgRt.anchoredPosition = Vector2.zero;
                bgRt.localScale = Vector3.one;

                Image bgImg = bgGo.GetComponent<Image>();
                bgImg.color = new Color(0.04f, 0.06f, 0.1f, 1f);
                bgImg.raycastTarget = true;

                // Child 2: WipeVideoDisplay
                GameObject displayGo = new GameObject("WipeVideoDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter), typeof(VideoPlayer));
                displayGo.transform.SetParent(containerGo.transform, false);

                RectTransform displayRt = displayGo.GetComponent<RectTransform>();
                displayRt.anchorMin = new Vector2(0.5f, 0.5f);
                displayRt.anchorMax = new Vector2(0.5f, 0.5f);
                displayRt.pivot = new Vector2(0.5f, 0.5f);
                displayRt.sizeDelta = new Vector2(1080f, 720f);
                displayRt.anchoredPosition = Vector2.zero;
                displayRt.localScale = Vector3.one;

                var rawImg = displayGo.GetComponent<RawImage>();
                rawImg.color = Color.white;
                rawImg.raycastTarget = false;

                var fitter = displayGo.GetComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fitter.aspectRatio = 16f / 9f;

                var player = displayGo.GetComponent<VideoPlayer>();
                player.playOnAwake = false;
                player.renderMode = VideoRenderMode.RenderTexture;
                player.audioOutputMode = VideoAudioOutputMode.None;
                player.isLooping = false;

                VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit8/Art/Wiping sink video.mp4");
                if (clip == null)
                {
                    clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit6/Art/Wiping sink video.mp4");
                }
                player.clip = clip;

                // Deactivated by default so it only displays during Step 7 wipe video
                containerGo.SetActive(false);

                Undo.RegisterCreatedObjectUndo(containerGo, "Create WipeVideoContainer in Scene");
                createdNew = true;
            }

            // Ensure child components are found
            RawImage childRawImage = containerGo.GetComponentInChildren<RawImage>(true);
            VideoPlayer childPlayer = containerGo.GetComponentInChildren<VideoPlayer>(true);
            AspectRatioFitter childFitter = containerGo.GetComponentInChildren<AspectRatioFitter>(true);

            // Wire SerializedObject references on controller
            SerializedObject so = new SerializedObject(controller);
            var propContainer = so.FindProperty("wipeVideoContainer");
            var propRawImage = so.FindProperty("wipeVideoRawImage");
            var propPlayer = so.FindProperty("wipeVideoPlayer");
            var propFitter = so.FindProperty("wipeVideoAspectFitter");
            var propClip = so.FindProperty("wipingSinkVideoClip");

            bool modified = false;
            if (propContainer != null && propContainer.objectReferenceValue != containerGo)
            {
                propContainer.objectReferenceValue = containerGo;
                modified = true;
            }
            if (propRawImage != null && propRawImage.objectReferenceValue != childRawImage)
            {
                propRawImage.objectReferenceValue = childRawImage;
                modified = true;
            }
            if (propPlayer != null && propPlayer.objectReferenceValue != childPlayer)
            {
                propPlayer.objectReferenceValue = childPlayer;
                modified = true;
            }
            if (propFitter != null && propFitter.objectReferenceValue != childFitter)
            {
                propFitter.objectReferenceValue = childFitter;
                modified = true;
            }
            if (propClip != null && propClip.objectReferenceValue == null)
            {
                propClip.objectReferenceValue = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit8/Art/Wiping sink video.mp4") ??
                                                AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/SeniorsActivityUnit6/Art/Wiping sink video.mp4");
                modified = true;
            }

            if (modified)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            }

            if (createdNew || modified)
            {
                EditorUtility.SetDirty(part2Screen);
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
            }

            Debug.Log($"[U8_WipeVideoContainerSceneSetup] WipeVideoContainer verified in scene hierarchy (created: {createdNew}, modified: {modified}).");
        }
    }
}
#endif
