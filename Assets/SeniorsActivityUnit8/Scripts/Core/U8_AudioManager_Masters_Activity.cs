using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Googolplex.Unit8
{
    [System.Serializable]
    public struct U8_SoundEntry_Masters_Activity
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    public class U8_AudioManager_Masters_Activity : MonoBehaviour
    {
        private static U8_AudioManager_Masters_Activity _instance;
        public static U8_AudioManager_Masters_Activity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<U8_AudioManager_Masters_Activity>();
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<U8_AudioManager_Masters_Activity>();
                    }
                    if (_instance != null && !_instance.isInitialized)
                    {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Dedicated Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource ambSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voSource;

        [Header("Explicit Voice Clips (Direct Safe Mapping)")]
        [SerializeField] private AudioClip vo_U8_01; // "Anu needs the washroom"
        [SerializeField] private AudioClip vo_U8_02; // "The door is closed What should Anu do"
        [SerializeField] private AudioClip vo_U8_03; // "Knock and wait"
        [SerializeField] private AudioClip vo_U8_04; // "Now what next"
        [SerializeField] private AudioClip vo_U8_05; // "Do not forget to flush"
        [SerializeField] private AudioClip vo_U8_06; // "Now wash your hands Keep tapping"
        [SerializeField] private AudioClip vo_U8_07; // "Keep going"
        [SerializeField] private AudioClip vo_U8_08; // "All clean One star"
        [SerializeField] private AudioClip vo_U8_09; // "Turn the tap off"
        [SerializeField] private AudioClip vo_U8_10; // "Towel in the bin"
        [SerializeField] private AudioClip vo_U8_11; // "Wipe the sink"
        [SerializeField] private AudioClip vo_U8_12; // "Here comes Meera"
        [SerializeField] private AudioClip vo_U8_13; // "Oh dear Shall we try again"
        [SerializeField] private AudioClip vo_U8_14; // "The soap is finished What should Meera do"
        [SerializeField] private AudioClip vo_U8_15; // "Three stars Ready for the next person"
        [SerializeField] private AudioClip vo_U8_16; // "Would the next person be happy"
        [SerializeField] private AudioClip vo_U8_ANU_1; // "Sorry"
        [SerializeField] private AudioClip vo_U8_MEE_1; // "Maam the soap is finished"
        [SerializeField] private AudioClip vo_U8_MEE_2; // "VO_U8_MEE_2"
        [SerializeField] private AudioClip vo_U8_VOICE_1; // "Just a minute"

        [Header("Audio Clips Library")]
        [SerializeField] private List<U8_SoundEntry_Masters_Activity> sounds = new List<U8_SoundEntry_Masters_Activity>();

        private readonly Dictionary<string, U8_SoundEntry_Masters_Activity> soundDict = new Dictionary<string, U8_SoundEntry_Masters_Activity>(StringComparer.OrdinalIgnoreCase);
        private bool isInitialized = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            Debug.Log("[U8 DEBUG] AudioManager Instance initialized");

            EnsureAudioListener();
            InitializeAudioSources();

#if UNITY_EDITOR
            AutoPopulateClipsInEditor();
#endif

            Debug.Log("[U8 DEBUG] Building VO dictionary");
            BuildSoundDictionary();
            PreloadAllAudioClips();

            Debug.Log($"[U8 DEBUG] VO dictionary count = {soundDict.Count}");

            bool hasVO01 = soundDict.TryGetValue("VO_U8_01", out var e01) && e01.clip != null;
            Debug.Log($"[U8 DEBUG] Contains VO_U8_01 = {(hasVO01 ? "TRUE" : "FALSE")}");
            if (hasVO01)
            {
                Debug.Log($"[U8 DEBUG] VO_U8_01 clip = {e01.clip.name}");
            }
            else
            {
                Debug.Log("[U8 DEBUG] VO_U8_01 IS NOT IN DICTIONARY");
            }

            bool hasVO02 = soundDict.TryGetValue("VO_U8_02", out var e02) && e02.clip != null;
            Debug.Log($"[U8 DEBUG] Contains VO_U8_02 = {(hasVO02 ? "TRUE" : "FALSE")}");
            if (hasVO02)
            {
                Debug.Log($"[U8 DEBUG] VO_U8_02 clip = {e02.clip.name}");
            }
            else
            {
                Debug.Log("[U8 DEBUG] VO_U8_02 IS NOT IN DICTIONARY");
            }

            // U8 Audio Validation Report
            int validCount = 0;
            int missingCount = 0;
            List<string> missingIds = new List<string>();

            if (sounds != null)
            {
                foreach (var s in sounds)
                {
                    if (s.clip != null) validCount++;
                    else
                    {
                        missingCount++;
                        missingIds.Add(s.id);
                    }
                }
            }

            Debug.Log($"[U8 AUDIO VALIDATION]\nTotal library entries: {(sounds != null ? sounds.Count : 0)}\nEntries with valid AudioClip: {validCount}\nEntries still missing: {missingCount}");
            if (missingIds.Count > 0)
            {
                Debug.LogWarning($"[U8 AUDIO VALIDATION] Missing IDs: {string.Join(", ", missingIds)}");
            }

            Debug.Log($"[U8 Audio] AudioManager Initialized on {Application.platform} | Registered Keys: {soundDict.Count} | Serialized Clips: {sounds.Count} | Listener Vol: {AudioListener.volume}");
        }

        public void EnsureAudioSourcesOnMainCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = FindFirstObjectByType<Camera>();
                if (mainCam == null) mainCam = FindObjectOfType<Camera>();
            }

            GameObject camHost = mainCam != null ? mainCam.gameObject : gameObject;

            // Find or create the 4 AudioSources on the camera host
            AudioSource[] existingSources = camHost.GetComponents<AudioSource>();
            if (existingSources.Length >= 4)
            {
                bgmSource = existingSources[0];
                ambSource = existingSources[1];
                sfxSource = existingSources[2];
                voSource = existingSources[3];
            }
            else
            {
                if (bgmSource == null) bgmSource = camHost.AddComponent<AudioSource>();
                if (ambSource == null) ambSource = camHost.AddComponent<AudioSource>();
                if (sfxSource == null) sfxSource = camHost.AddComponent<AudioSource>();
                if (voSource == null) voSource = camHost.AddComponent<AudioSource>();
            }

            ConfigureSource(bgmSource, 0.45f, true);
            ConfigureSource(ambSource, 0.35f, true);
            ConfigureSource(sfxSource, 1.0f, false);
            ConfigureSource(voSource, 1.0f, false);

            EnsureAudioListener();
        }

        private void InitializeAudioSources()
        {
            var uniqueSources = new HashSet<AudioSource>();

            if (bgmSource != null) uniqueSources.Add(bgmSource);
            if (ambSource != null && !uniqueSources.Contains(ambSource)) uniqueSources.Add(ambSource);
            else ambSource = null;
            if (sfxSource != null && !uniqueSources.Contains(sfxSource)) uniqueSources.Add(sfxSource);
            else sfxSource = null;
            if (voSource != null && !uniqueSources.Contains(voSource)) uniqueSources.Add(voSource);
            else voSource = null;

            if (bgmSource == null) bgmSource = CreateDedicatedSource("U8_BGM_Source");
            if (ambSource == null) ambSource = CreateDedicatedSource("U8_AMB_Source");
            if (sfxSource == null) sfxSource = CreateDedicatedSource("U8_SFX_Source");
            if (voSource == null) voSource = CreateDedicatedSource("U8_VO_Source");

            ConfigureSource(bgmSource, 0.45f, true);
            ConfigureSource(ambSource, 0.35f, true);
            ConfigureSource(sfxSource, 1.0f, false);
            ConfigureSource(voSource, 1.0f, false);
        }

        private AudioSource CreateDedicatedSource(string name)
        {
            // First check if a child with this name already exists
            Transform existingChild = transform.Find(name);
            if (existingChild != null)
            {
                AudioSource existingSrc = existingChild.GetComponent<AudioSource>();
                if (existingSrc != null) return existingSrc;
                return existingChild.gameObject.AddComponent<AudioSource>();
            }

            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);
            AudioSource src = child.AddComponent<AudioSource>();
            return src;
        }

        private void ConfigureSource(AudioSource src, float defaultVol, bool loop)
        {
            if (src == null) return;
            src.enabled = true;
            src.playOnAwake = false;
            src.loop = loop;
            src.volume = defaultVol;
            src.mute = false;
            src.spatialBlend = 0f; // 2D Full stereo sound
            src.bypassEffects = false;
            src.bypassListenerEffects = false;
            src.bypassReverbZones = false;
            src.outputAudioMixerGroup = null; // Direct master routing
        }

        public void EnsureAudioListener()
        {
            AudioListener.volume = 1.0f;
            AudioListener.pause = false;

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = FindFirstObjectByType<Camera>();
                if (mainCam == null) mainCam = FindObjectOfType<Camera>();
            }

            GameObject host = mainCam != null ? mainCam.gameObject : gameObject;
            AudioListener listener = host.GetComponent<AudioListener>();
            if (listener == null) listener = host.AddComponent<AudioListener>();
            listener.enabled = true;

            // Ensure only one listener is active in the scene
            AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            foreach (var l in allListeners)
            {
                if (l != listener) l.enabled = false;
            }

            if (listener == null || !listener.enabled)
            {
                Debug.LogError("[U8 Audio] AUDIO LISTENER MISSING or disabled!");
            }
        }

        public void PlayButtonClick()
        {
            PlaySFX("SFX_Bubble");
        }

        public void BuildSoundDictionary()
        {
            soundDict.Clear();
            if (sounds != null)
            {
                foreach (var entry in sounds)
                {
                    if (entry.clip == null) continue;

                    if (!string.IsNullOrEmpty(entry.id))
                    {
                        RegisterEntryKey(entry.id, entry);
                    }

                    RegisterEntryKey(entry.clip.name, entry);

                    string mappedId = MapFileNameToSoundId(entry.clip.name);
                    if (!string.IsNullOrEmpty(mappedId))
                    {
                        RegisterEntryKey(mappedId, entry);
                    }

                    if (!string.IsNullOrEmpty(entry.id))
                    {
                        string mappedFromId = MapFileNameToSoundId(entry.id);
                        if (!string.IsNullOrEmpty(mappedFromId))
                        {
                            RegisterEntryKey(mappedFromId, entry);
                        }
                    }
                }
            }
        }

        private void RegisterEntryKey(string key, U8_SoundEntry_Masters_Activity entry)
        {
            if (string.IsNullOrEmpty(key) || entry.clip == null) return;

            soundDict[key] = entry;

            string normalized = key.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
            if (!string.IsNullOrEmpty(normalized) && !soundDict.ContainsKey(normalized))
            {
                soundDict[normalized] = entry;
            }
        }

        public void PreloadAllAudioClips()
        {
            if (sounds == null) return;
            int preloadedCount = 0;
            foreach (var entry in sounds)
            {
                if (entry.clip != null)
                {
                    if (entry.clip.loadState != AudioDataLoadState.Loaded)
                    {
                        entry.clip.LoadAudioData();
                    }
                    preloadedCount++;
                }
            }
            Debug.Log($"[U8 Audio] Preloaded {preloadedCount} audio clips into memory for instant Android & Editor playback.");
        }

        private bool TryGetSoundEntry(string soundId, out U8_SoundEntry_Masters_Activity entry)
        {
            entry = default;
            if (string.IsNullOrEmpty(soundId)) return false;

            // 1. Direct dictionary lookup
            if (soundDict.TryGetValue(soundId, out entry) && entry.clip != null)
            {
                EnsureClipLoaded(entry.clip);
                return true;
            }

            // 2. Normalized lookup
            string cleanQuery = soundId.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
            if (soundDict.TryGetValue(cleanQuery, out entry) && entry.clip != null)
            {
                EnsureClipLoaded(entry.clip);
                return true;
            }

            // 3. Check mapped ID lookup
            string mapped = MapFileNameToSoundId(soundId);
            if (!string.IsNullOrEmpty(mapped) && soundDict.TryGetValue(mapped, out entry) && entry.clip != null)
            {
                EnsureClipLoaded(entry.clip);
                return true;
            }

            // 4. Search serialized sounds list
            if (sounds != null)
            {
                foreach (var s in sounds)
                {
                    if (s.clip == null) continue;

                    string sIdClean = (s.id ?? "").Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
                    string sClipClean = (s.clip.name ?? "").Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");

                    if (sIdClean == cleanQuery || sClipClean == cleanQuery ||
                        sIdClean.Contains(cleanQuery) || cleanQuery.Contains(sIdClean) ||
                        sClipClean.Contains(cleanQuery) || cleanQuery.Contains(sClipClean))
                    {
                        entry = s;
                        soundDict[soundId] = entry;
                        EnsureClipLoaded(entry.clip);
                        return true;
                    }
                }
            }

#if UNITY_EDITOR
            // 5. On-demand editor asset database lookup
            AudioClip onDemand = LoadClipOnDemand(soundId);
            if (onDemand != null)
            {
                entry = new U8_SoundEntry_Masters_Activity { id = soundId, clip = onDemand, volume = 1f };
                RegisterEntryKey(soundId, entry);
                RegisterEntryKey(onDemand.name, entry);
                if (!string.IsNullOrEmpty(mapped)) RegisterEntryKey(mapped, entry);
                EnsureClipLoaded(entry.clip);
                return true;
            }
#endif

            return false;
        }

        private void EnsureClipLoaded(AudioClip clip)
        {
            if (clip != null && clip.loadState != AudioDataLoadState.Loaded)
            {
                clip.LoadAudioData();
            }
        }

#if UNITY_EDITOR
        public void AutoPopulateClipsInEditor()
        {
            if (sounds == null) sounds = new List<U8_SoundEntry_Masters_Activity>();
            List<U8_SoundEntry_Masters_Activity> validSounds = new List<U8_SoundEntry_Masters_Activity>();
            HashSet<string> registered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var s in sounds)
            {
                if (!string.IsNullOrEmpty(s.id) && s.clip != null)
                {
                    registered.Add(s.id);
                    validSounds.Add(s);
                }
            }
            sounds = validSounds;

            string[] searchFolders = new[] { 
                "Assets/SeniorsActivityUnit8/SFX", 
                "Assets/SeniorsActivityUnit8/Audio/U8_MastersActivity_audios",
                "Assets/SeniorsActivityUnit8/Audio"
            };

            foreach (string folder in searchFolders)
            {
                if (!System.IO.Directory.Exists(folder)) continue;

                string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip == null) continue;

                    string rawName = clip.name;
                    if (!registered.Contains(rawName))
                    {
                        registered.Add(rawName);
                        sounds.Add(new U8_SoundEntry_Masters_Activity { id = rawName, clip = clip, volume = 1f });
                    }

                    string id = MapFileNameToSoundId(rawName);
                    if (!string.IsNullOrEmpty(id) && !registered.Contains(id))
                    {
                        registered.Add(id);
                        sounds.Add(new U8_SoundEntry_Masters_Activity { id = id, clip = clip, volume = 1f });
                    }
                }
            }

            BuildSoundDictionary();
            Debug.Log($"[U8 Audio] Auto-populated audio clips: total {sounds.Count} clips loaded into library.");
        }

        private AudioClip LoadClipOnDemand(string soundId)
        {
            List<string> validFolders = new List<string>();
            string[] searchFolders = new[] { 
                "Assets/SeniorsActivityUnit8/SFX", 
                "Assets/SeniorsActivityUnit8/Audio/U8_MastersActivity_audios",
                "Assets/SeniorsActivityUnit8/Audio"
            };
            foreach (var f in searchFolders)
            {
                if (System.IO.Directory.Exists(f)) validFolders.Add(f);
            }

            if (validFolders.Count == 0) return null;

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", validFolders.ToArray());
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, soundId, StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
                string mappedId = MapFileNameToSoundId(fileName);
                if (!string.IsNullOrEmpty(mappedId) && string.Equals(mappedId, soundId, StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }
            return null;
        }
#endif

        public static string MapFileNameToSoundId(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            if (fileName.StartsWith("SFX_") || fileName.StartsWith("AMB_") || fileName.StartsWith("MUS_") || fileName.StartsWith("VO_"))
                return fileName;

            string lower = fileName.ToLower().Trim();

            // Teacher / Narrator VO
            if (lower.Contains("anu needs the washroom")) return "VO_U8_01";
            if (lower.Contains("the door is closed what should") || lower.Contains("door is closed")) return "VO_U8_02";
            if (lower.Contains("knock and wait")) return "VO_U8_03";
            if (lower.Contains("now what next")) return "VO_U8_04";
            if (lower.Contains("do not forget to flush") || lower.Contains("forget to flush")) return "VO_U8_05";
            if (lower.Contains("now wash your hands") || lower.Contains("keep tapping")) return "VO_U8_06";
            if (lower.Contains("keep going")) return "VO_U8_07";
            if (lower.Contains("all clean one star") || lower.Contains("all clean")) return "VO_U8_08";
            if (lower.Contains("turn the tap off")) return "VO_U8_09";
            if (lower.Contains("towel in the bin")) return "VO_U8_10";
            if (lower.Contains("wipe the sink")) return "VO_U8_11";
            if (lower.Contains("here comes meera")) return "VO_U8_12";
            if (lower.Contains("shall we try again") || lower.Contains("oh dear")) return "VO_U8_13";
            if (lower.Contains("the soap is finished what should") || lower.Contains("soap is finished what")) return "VO_U8_14";
            if (lower.Contains("three stars ready for the next") || lower.Contains("ready for the next person")) return "VO_U8_15";
            if (lower.Contains("would the next person be happy")) return "VO_U8_16";

            // Character Voices
            if (lower.Contains("sorry")) return "VO_U8_ANU_1";
            if (lower.Contains("maam the soap is finished") || lower.Contains("ma'am the soap is finished")) return "VO_U8_MEE_1";
            if (lower.Contains("meera slip") || lower.Contains("meera startle") || lower.Contains("gasp")) return "VO_U8_MEE_2";
            if (lower.Contains("just a minute")) return "VO_U8_VOICE_1";

            // Music & Ambience
            if (lower.Contains("handwash song") && lower.Contains("inst")) return "MUS_HandwashSong_Inst";
            if (lower.Contains("handwash song") || lower.Contains("handwashing")) return "MUS_HandwashSong";
            if (lower.Contains("win") || lower.Contains("celebration")) return "MUS_Win";
            if (lower.Contains("washroom") && lower.Contains("amb")) return "AMB_Washroom";

            // SFX
            if (lower.Contains("two polite knocks") || lower.Contains("knock")) return "SFX_Knock";
            if (lower.Contains("heavy bangs") || lower.Contains("bang")) return "SFX_BangDoor";
            if (lower.Contains("bolted door rattling") || lower.Contains("rattle")) return "SFX_BoltRattle";
            if (lower.Contains("bolt sliding shut") || lower.Contains("bolt click")) return "SFX_BoltClick";
            if (lower.Contains("toilet flushing") || lower.Contains("flush")) return "SFX_Flush";
            if (lower.Contains("tap turning on") || lower.Contains("tap on")) return "SFX_TapOn";
            if (lower.Contains("tap squeaking shut") || lower.Contains("tap off")) return "SFX_TapOff";
            if (lower.Contains("soap dispenser pumping once") || lower.Contains("soap pump")) return "SFX_SoapPump";
            if (lower.Contains("nothing in it") || lower.Contains("soap empty")) return "SFX_SoapEmpty";
            if (lower.Contains("wet hand-rubbing") || lower.Contains("scrub")) return "SFX_Scrub";
            if (lower.Contains("bubble pop") || lower.Contains("bubble")) return "SFX_Bubble";
            if (lower.Contains("germ") || lower.Contains("squeak")) return "SFX_GermOff";
            if (lower.Contains("paper towel pulled") || lower.Contains("towel pull")) return "SFX_TowelPull";
            if (lower.Contains("paper landing in") || lower.Contains("bin drop")) return "SFX_BinDrop";
            if (lower.Contains("quick wipe") || lower.Contains("wipe")) return "SFX_Wipe";
            if (lower.Contains("foot slipping") || lower.Contains("slip")) return "SFX_Slip";
            if (lower.Contains("sparkle")) return "SFX_Sparkle";
            if (lower.Contains("star chime") || lower.Contains("star")) return "SFX_Star";
            if (lower.Contains("clapping") || lower.Contains("clap")) return "SFX_Clap";
            if (lower.Contains("party popper") || lower.Contains("confetti")) return "SFX_Confetti";

            return null;
        }

        private void CheckAudioHealth(AudioSource src, string category)
        {
            if (src == null)
            {
                Debug.LogError($"[U8 Audio] AUDIO SOURCE MISSING for {category}!");
                return;
            }
            if (!src.enabled)
            {
                src.enabled = true;
            }
            if (src.mute)
            {
                Debug.LogWarning($"[U8 Audio] AUDIO MUTED: Source '{src.name}' ({category}) was muted, unmuting now.");
                src.mute = false;
            }
            if (src.volume <= 0f)
            {
                Debug.LogWarning($"[U8 Audio] AUDIO VOLUME ZERO: Source '{src.name}' ({category}) volume was 0, resetting to audible level.");
                src.volume = (category == "BGM" || category == "Ambience") ? 0.45f : 1.0f;
            }
            if (AudioListener.volume <= 0f)
            {
                Debug.LogWarning($"[U8 Audio] AUDIO LISTENER VOLUME ZERO: resetting AudioListener.volume to 1.0.");
                AudioListener.volume = 1.0f;
            }
            if (AudioListener.pause)
            {
                Debug.LogWarning($"[U8 Audio] AUDIO LISTENER PAUSED: unpausing AudioListener.");
                AudioListener.pause = false;
            }
        }

        public void PlaySFX(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            Debug.Log($"[U8 Audio] AUDIO REQUESTED: SFX '{soundId}'");

            if (sfxSource == null || !sfxSource.enabled) InitializeAudioSources();
            CheckAudioHealth(sfxSource, "SFX");

            if (TryGetSoundEntry(soundId, out U8_SoundEntry_Masters_Activity entry) && entry.clip != null && sfxSource != null)
            {
                float vol = entry.volume > 0 ? entry.volume : 1.0f;
                Debug.Log($"[U8 Audio] AUDIO CLIP FOUND: '{entry.clip.name}' (Len: {entry.clip.length:F2}s, LoadState: {entry.clip.loadState})");
                sfxSource.PlayOneShot(entry.clip, vol);
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: SFX '{entry.clip.name}' length={entry.clip.length:F2}s volume={vol} mute={sfxSource.mute} spatialBlend={sfxSource.spatialBlend} listenerVol={AudioListener.volume}");
            }
            else
            {
                Debug.LogError($"[U8 Audio] MISSING CLIP: '{soundId}'");
            }
        }

        public AudioClip GetExplicitVOClip(string voId)
        {
            if (string.IsNullOrEmpty(voId)) return null;
            string clean = voId.Trim().ToUpperInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");

            if (clean == "VOU801" || clean.Contains("ANUNEEDS") || (clean.Contains("WASHROOM") && !clean.Contains("AMB")))
                if (vo_U8_01 != null) return vo_U8_01;

            if (clean == "VOU802" || clean.Contains("DOORISCLOSED"))
                if (vo_U8_02 != null) return vo_U8_02;

            if (clean == "VOU803" || clean.Contains("KNOCKANDWAIT"))
                if (vo_U8_03 != null) return vo_U8_03;

            if (clean == "VOU804" || clean.Contains("NOWWHATNEXT"))
                if (vo_U8_04 != null) return vo_U8_04;

            if (clean == "VOU805" || clean.Contains("FORGETTOFLUSH"))
                if (vo_U8_05 != null) return vo_U8_05;

            if (clean == "VOU806" || clean.Contains("NOWWASHYOURHANDS") || clean.Contains("KEEPTAPPING"))
                if (vo_U8_06 != null) return vo_U8_06;

            if (clean == "VOU807" || clean.Contains("KEEPGOING"))
                if (vo_U8_07 != null) return vo_U8_07;

            if (clean == "VOU808" || clean.Contains("ALLCLEAN") || clean.Contains("ONESTAR"))
                if (vo_U8_08 != null) return vo_U8_08;

            if (clean == "VOU809" || clean.Contains("TURNTAPOFF") || clean.Contains("TURNTHETAPOFF"))
                if (vo_U8_09 != null) return vo_U8_09;

            if (clean == "VOU810" || clean.Contains("TOWELINTHEBIN") || clean.Contains("TOWELINBIN"))
                if (vo_U8_10 != null) return vo_U8_10;

            if (clean == "VOU811" || clean.Contains("WIPETHESINK") || clean.Contains("WIPESINK"))
                if (vo_U8_11 != null) return vo_U8_11;

            if (clean == "VOU812" || clean.Contains("HERECOMESMEERA"))
                if (vo_U8_12 != null) return vo_U8_12;

            if (clean == "VOU813" || clean.Contains("SHALLWETRYAGAIN") || clean.Contains("OHDEAR"))
                if (vo_U8_13 != null) return vo_U8_13;

            if (clean == "VOU814" || clean.Contains("THESOAPISFINISHED") || clean.Contains("SOAPISFINISHED"))
                if (vo_U8_14 != null) return vo_U8_14;

            if (clean == "VOU815" || clean.Contains("THREESTARS") || clean.Contains("READYFORTHENEXT"))
                if (vo_U8_15 != null) return vo_U8_15;

            if (clean == "VOU816" || clean.Contains("WOULDTHENEXTPERSON"))
                if (vo_U8_16 != null) return vo_U8_16;

            if (clean == "VOU8ANU1" || clean == "SORRY")
                if (vo_U8_ANU_1 != null) return vo_U8_ANU_1;

            if (clean == "VOU8MEE1" || clean.Contains("MAAMTHESOAP"))
                if (vo_U8_MEE_1 != null) return vo_U8_MEE_1;

            if (clean == "VOU8MEE2" || clean.Contains("MEERAGASP") || clean.Contains("MEERASLIP"))
                if (vo_U8_MEE_2 != null) return vo_U8_MEE_2;

            if (clean == "VOU8VOICE1" || clean.Contains("JUSTAMINUTE"))
                if (vo_U8_VOICE_1 != null) return vo_U8_VOICE_1;

            return null;
        }

        public void PlayVO(string voId)
        {
            Debug.Log("[U8 FINAL TEST] Requested VO = " + voId);

            if (string.IsNullOrEmpty(voId)) return;

            if (voSource == null || !voSource.enabled) InitializeAudioSources();
            CheckAudioHealth(voSource, "VO");

            AudioClip clip = GetExplicitVOClip(voId);

            if (clip == null && TryGetSoundEntry(voId, out U8_SoundEntry_Masters_Activity entry))
            {
                clip = entry.clip;
            }

            Debug.Log("[U8 FINAL TEST] Resolved clip = " + (clip == null ? "NULL" : clip.name));

            if (clip != null && voSource != null)
            {
                EnsureClipLoaded(clip);
                voSource.Stop();
                voSource.clip = clip;
                voSource.volume = 1f;
                voSource.loop = false;
                voSource.spatialBlend = 0f;
                voSource.mute = false;
                voSource.Play();
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: VO '{clip.name}' length={clip.length:F2}s volume={voSource.volume} mute={voSource.mute} spatialBlend={voSource.spatialBlend} listenerVol={AudioListener.volume}");
            }
            else
            {
                Debug.LogError($"[U8 Audio] MISSING CLIP: '{voId}'");
            }
        }

        public void StopVO()
        {
            if (voSource != null) voSource.Stop();
        }

        public void PlayBGM(string bgmId, bool loop = true)
        {
            if (string.IsNullOrEmpty(bgmId)) return;
            Debug.Log($"[U8 Audio] AUDIO REQUESTED: BGM '{bgmId}' (Loop={loop})");

            if (bgmSource == null || !bgmSource.enabled) InitializeAudioSources();
            CheckAudioHealth(bgmSource, "BGM");

            if (TryGetSoundEntry(bgmId, out U8_SoundEntry_Masters_Activity entry) && entry.clip != null && bgmSource != null)
            {
                if (bgmSource.clip == entry.clip && bgmSource.isPlaying) return;

                Debug.Log($"[U8 Audio] AUDIO CLIP FOUND: '{entry.clip.name}' (Len: {entry.clip.length:F2}s, LoadState: {entry.clip.loadState})");
                bgmSource.Stop();
                bgmSource.clip = entry.clip;
                bgmSource.loop = loop;
                bgmSource.volume = entry.volume > 0 ? entry.volume : 0.45f;
                bgmSource.Play();
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: BGM '{entry.clip.name}' length={entry.clip.length:F2}s volume={bgmSource.volume} loop={loop} mute={bgmSource.mute} spatialBlend={bgmSource.spatialBlend} listenerVol={AudioListener.volume}");
            }
            else
            {
                Debug.LogError($"[U8 Audio] MISSING CLIP: '{bgmId}'");
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null) bgmSource.Stop();
        }

        public void PlayAmbience(string ambId, bool loop = true)
        {
            if (string.IsNullOrEmpty(ambId)) return;
            Debug.Log($"[U8 Audio] AUDIO REQUESTED: Ambience '{ambId}' (Loop={loop})");

            if (ambSource == null || !ambSource.enabled) InitializeAudioSources();
            CheckAudioHealth(ambSource, "Ambience");

            if (TryGetSoundEntry(ambId, out U8_SoundEntry_Masters_Activity entry) && entry.clip != null && ambSource != null)
            {
                if (ambSource.clip == entry.clip && ambSource.isPlaying) return;

                Debug.Log($"[U8 Audio] AUDIO CLIP FOUND: '{entry.clip.name}' (Len: {entry.clip.length:F2}s, LoadState: {entry.clip.loadState})");
                ambSource.Stop();
                ambSource.clip = entry.clip;
                ambSource.loop = loop;
                ambSource.volume = entry.volume > 0 ? entry.volume : 0.35f;
                ambSource.Play();
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: Ambience '{entry.clip.name}' length={entry.clip.length:F2}s volume={ambSource.volume} loop={loop} mute={ambSource.mute} spatialBlend={ambSource.spatialBlend} listenerVol={AudioListener.volume}");
            }
            else
            {
                Debug.LogError($"[U8 Audio] MISSING CLIP: '{ambId}'");
            }
        }

        public void StopAmbience()
        {
            if (ambSource != null) ambSource.Stop();
        }

        public AudioClip GetClip(string soundId)
        {
            AudioClip explicitClip = GetExplicitVOClip(soundId);
            if (explicitClip != null) return explicitClip;

            if (TryGetSoundEntry(soundId, out U8_SoundEntry_Masters_Activity entry) && entry.clip != null)
            {
                return entry.clip;
            }
            Debug.LogError($"[U8 Audio] MISSING CLIP (GetClip): '{soundId}'");
            return null;
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            if (sfxSource == null || !sfxSource.enabled) InitializeAudioSources();
            CheckAudioHealth(sfxSource, "SFX");
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: Direct SFX '{clip.name}' length={clip.length:F2}s volume={volume} mute={sfxSource.mute}");
            }
        }

        public void PlayVO(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            if (voSource == null || !voSource.enabled) InitializeAudioSources();
            CheckAudioHealth(voSource, "VO");
            if (voSource != null)
            {
                voSource.Stop();
                voSource.clip = clip;
                voSource.volume = volume;
                voSource.loop = false;
                voSource.Play();
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: Direct VO '{clip.name}' length={clip.length:F2}s volume={volume} mute={voSource.mute}");
            }
        }

        public void PlayBGM(AudioClip clip, bool loop = true, float volume = 0.45f)
        {
            if (clip == null) return;
            if (bgmSource == null || !bgmSource.enabled) InitializeAudioSources();
            CheckAudioHealth(bgmSource, "BGM");
            if (bgmSource != null)
            {
                if (bgmSource.clip == clip && bgmSource.isPlaying) return;
                bgmSource.Stop();
                bgmSource.clip = clip;
                bgmSource.loop = loop;
                bgmSource.volume = volume;
                bgmSource.Play();
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: Direct BGM '{clip.name}' length={clip.length:F2}s volume={volume} loop={loop} mute={bgmSource.mute}");
            }
        }

        public void PlayAmbience(AudioClip clip, bool loop = true, float volume = 0.35f)
        {
            if (clip == null) return;
            if (ambSource == null || !ambSource.enabled) InitializeAudioSources();
            CheckAudioHealth(ambSource, "Ambience");
            if (ambSource != null)
            {
                if (ambSource.clip == clip && ambSource.isPlaying) return;
                ambSource.Stop();
                ambSource.clip = clip;
                ambSource.loop = loop;
                ambSource.volume = volume;
                ambSource.Play();
                Debug.Log($"[U8 Audio] AUDIO PLAY CALLED: Direct Ambience '{clip.name}' length={clip.length:F2}s volume={volume} loop={loop} mute={ambSource.mute}");
            }
        }
    }
}
