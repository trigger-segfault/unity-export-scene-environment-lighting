// Exportable settings for Window > Rendering > Lighting's Environment tab. AKA
// Skybox settings.

// Allows recording undos when writing back to the scene, and required for
// `SCENEENVLIGHT_SERIALIZED_PROPERTIES`.
#define SCENEENVLIGHT_USE_REFLECTION
// Use SerializedObject to access hidden fields not present in the .NET class.
// Requires: `SCENEENVLIGHT_USE_REFLECTION`.
// Supports: Texture haloTexture, Texture spotCookie
#define SCENEENVLIGHT_SERIALIZED_PROPERTIES

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
#if SCENEENVLIGHT_USE_REFLECTION
using System.Reflection;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TriggerSegfault.editor
{
    /// <summary>
    /// Exportable settings for Window/Rendering/Lighting's Environment tab.
    /// </summary>
    public class SceneEnvironmentLighting : ScriptableObject
    {
        // Set to null to disable custom icon.
        const string AssetIconName = "d_LightingSettings Icon";

        const string FriendlyName = "Scene Environment Lighting";

        const float SecondaryPriority = 10100f;
        const string MenuFolder = "Trigger Segfault/" + FriendlyName + "/";

        const string ImportMenuPath     = "Assets/" + MenuFolder + "Import to Scene";
        const string ExportMenuPath     = "Assets/" + MenuFolder + "Export new from Scene";
        const string OverwriteMenuPath  = "Assets/" + MenuFolder + "Overwrite from Scene";
        const string CopyAssetMenuPath  = "Assets/" + MenuFolder + "Copy Values";
        const string PasteAssetMenuPath = "Assets/" + MenuFolder + "Paste Values";
        const string CopySceneMenuPath  = "Tools/"  + MenuFolder + "Copy Values from Scene";
        const string PasteSceneMenuPath = "Tools/"  + MenuFolder + "Paste Values to Scene";

        const string CreateFileName = nameof(SceneEnvironmentLighting) + ".asset";

        private static string s_scriptFolder;
        private static string ClipboardFilePath => Path.Combine(
            s_scriptFolder ?? "Assets",
            nameof(SceneEnvironmentLighting) + "_Clipboard.asset"
        );

        /// <summary>
        /// Unity is kind enough to strip away underscores when displaying
        /// enums in a popup, so we can use this for the Inspector.
        /// </summary>
        /// <remarks>See: defaultReflectionResolution</remarks>
        public enum ReflectionResolution
        {
            _16   = 16,
            _32   = 32,
            _64   = 64,
            _128  = 128,
            _256  = 256,
            _512  = 512,
            _1024 = 1024,
            _2048 = 2048,
        }


        /// <summary>Other Settings: Fog</summary>
        [Tooltip("Other Settings: Fog")]
        public bool fog;
        /// <summary>Other Settings: Fog > Start (Mode: Linear)</summary>
        /// <remarks>No range.</remarks>
        [Tooltip("Other Settings: Fog > Start (Mode: Linear)")]
        public float fogStartDistance;
        /// <summary>Other Settings: Fog > End (Mode: Linear)</summary>
        /// <remarks>No range.</remarks>
        [Tooltip("Other Settings: Fog > End (Mode: Linear)")]
        public float fogEndDistance;
        /// <summary>Other Settings: Fog > Mode</summary>
        [Tooltip("Other Settings: Fog > Mode")]
        public FogMode fogMode;
        /// <summary>Other Settings: Fog > Color</summary>
        [Tooltip("Other Settings: Fog > Color")]
        public Color fogColor;
        /// <summary>Other Settings: Fog > Density (Mode: Exponential, Exponential Squared)</summary>
        [Tooltip("Other Settings: Fog > Density (Mode: Exponential, Exponential Squared)")]
        [Range(0f, 1f)]
        public float fogDensity;
        /// <summary>Environment: Environment Lighting > Source</summary>
        /// <remarks>Custom is not supported. Defaults to Skybox if set to Custom.</remarks>
        [Tooltip("Environment: Environment Lighting > Source\n" +
                 "Custom is not supported. Defaults to Skybox if set to Custom.")]
        public AmbientMode ambientMode;
        /// <summary>Environment: Environment Lighting > Sky Color (Source: Gradient)</summary>
        [Tooltip("Environment: Environment Lighting > Sky Color (Source: Gradient)")]
        public Color ambientSkyColor;
        /// <summary>Environment: Environment Lighting > Equator Color (Source: Gradient)</summary>
        [Tooltip("Environment: Environment Lighting > Equator Color (Source: Gradient)")]
        public Color ambientEquatorColor;
        /// <summary>Environment: Environment Lighting > Ground Color (Source: Gradient)</summary>
        [Tooltip("Environment: Environment Lighting > Ground Color (Source: Gradient)")]
        public Color ambientGroundColor;
        /// <summary>Environment: Environment Lighting > Intensity Multiplier (Source: Skybox)</summary>
        [Tooltip("Environment: Environment Lighting > Intensity Multiplier (Source: Skybox)")]
        [Range(0f, 8f)]
        public float ambientIntensity;
        /// <summary>Environment: Environment Lighting > Ambient Color (Source: Color)</summary>
        [Tooltip("Environment: Environment Lighting > Ambient Color (Source: Color)")]
        public Color ambientLight;
        /// <summary>Environment: Realtime Shadow Color</summary>
        [Tooltip("Environment: Realtime Shadow Color")]
        public Color subtractiveShadowColor;
        /// <summary>Environment: Skybox Material</summary>
        [Tooltip("Environment: Skybox Material")]
        public Material skybox;
        /// <summary>Environment: Sun Source (represents <c>sun</c>)</summary>
        [Tooltip("Environment: Sun Source\n" +
                 "Path to GameObject with Sun's Light component. " +
                 "Requires starting with '/' for absolute path.")]
        public string sunPath = string.Empty;
        /// <summary>Environment: Sun Source (stored in <c>sunPath</c>)</summary>
        public Light sun
        {
            get => GetComponentInScene<Light>(sunPath);
            set => sunPath = GetComponentScenePath(value) ?? string.Empty;
        }
        /// <summary>
        /// Public, and not obsolete, but not present in editor window. This
        /// is automatically adjusted by the Unity Editor when setting other
        /// properties.
        /// </summary>
        [Tooltip("This data is automatically adjusted by the editor when " +
                 "setting other properties.")]
        public SphericalHarmonicsL2 ambientProbe;
        /// <summary>Environment: Environment Reflections > Cubemap (Source: Skybox)</summary>
        [Tooltip("Environment: Environment Reflections > Cubemap (Source: Skybox)")]
        public Texture customReflectionTexture;
        /// <summary>Environment: Environment Reflections > Compression</summary>
        /// <remarks>LightmapEditorSettings property.</remarks>
        [Tooltip("Environment: Environment Reflections > Compression")]
        public ReflectionCubemapCompression reflectionCubemapCompression;
        /// <summary>Environment: Environment Reflections > Intensity Multiplier</summary>
        [Tooltip("Environment: Environment Reflections > Intensity Multiplier")]
        [Range(0f, 1f)]
        public float reflectionIntensity;
        /// <summary>Environment: Environment Reflections > Bounces</summary>
        [Tooltip("Environment: Environment Reflections > Bounces")]
        [Range(1f, 5f)]
        public int reflectionBounces;
        /// <summary>Environment: Environment Reflections > Source</summary>
        [Tooltip("Environment: Environment Reflections > Source")]
        public DefaultReflectionMode defaultReflectionMode;
        /// <summary>Environment: Environment Reflections > Resolution (Source: Skybox)</summary>
        /// <remarks>Only powers of two. Floored if not power of two.</remarks>
        [Tooltip("Environment: Environment Reflections > Resolution (Source: Skybox)")]
        public ReflectionResolution defaultReflectionResolution;
#if SCENEENVLIGHT_USE_REFLECTION && SCENEENVLIGHT_SERIALIZED_PROPERTIES
        /// <summary>Other Settings: Halo Texture</summary>
        /// <remarks>
        /// RenderSettings serialized: m_HaloTexture<para/>
        /// Claimed to be a <c>Texture2D</c>, but accepts <c>Texture</c>.
        /// </remarks>
        [Tooltip("Other Settings: Halo Texture")]
        public Texture haloTexture;
#endif
        /// <summary>Other Settings: Halo Strength</summary>
        [Tooltip("Other Settings: Halo Strength")]
        [Range(0f, 1f)]
        public float haloStrength;
        /// <summary>Other Settings: Flare Strength</summary>
        [Tooltip("Other Settings: Flare Strength")]
        [Range(0f, 1f)]
        public float flareStrength;
        /// <summary>Other Settings: Flare Fade Speed</summary>
        [Tooltip("Other Settings: Flare Fade Speed")]
        /// <remarks>No range.</remarks>
        public float flareFadeSpeed;
#if SCENEENVLIGHT_USE_REFLECTION && SCENEENVLIGHT_SERIALIZED_PROPERTIES
        /// <summary>Other Settings: Spot Cookie</summary>
        /// <remarks>
        /// RenderSettings serialized: m_SpotCookie<para/>
        /// Claimed to be a <c>Texture2D</c>, but accepts <c>Texture</c>.
        /// </remarks>
        [Tooltip("Other Settings: Spot Cookie")]
        public Texture spotCookie;
#endif


        #region Menus
        [MenuItem(ImportMenuPath, false, secondaryPriority = SecondaryPriority + 0f)]
        private static void MenuImportToScene()
        {
            var settings = GetSelectedAsset();
            if (settings != null)
            {
                settings.Write(recordUndo: true);
            }
        }

        [MenuItem(ExportMenuPath, false, secondaryPriority = SecondaryPriority + 1f)]
        private static void MenuExportNewFromScene()
        {
            string folderPath = GetSelectedFolder();
            if (folderPath != null)
            {
                var settings = ScriptableObject.CreateInstance<SceneEnvironmentLighting>();
                try
                {
                    settings.Read(recordUndo: false);
                    ProjectWindowUtil.CreateAsset(
                        settings, Path.Combine(folderPath, CreateFileName)
                    );
                }
                catch
                {
                    DestroyImmediate(settings);
                    throw;
                }
            }
        }

        [MenuItem(OverwriteMenuPath, false, secondaryPriority = SecondaryPriority + 2f)]
        private static void MenuOverwiteFromScene()
        {
            var settings = GetSelectedAsset();
            if (settings != null)
            {
                settings.Read(recordUndo: true);
                // Call SetDirty here if recordUndo is false.
                //EditorUtility.SetDirty(settings);
            }
        }

        [MenuItem(CopyAssetMenuPath, false, secondaryPriority = SecondaryPriority + 3f)]
        private static void MenuCopyFromAsset()
        {
            SetClipboardAsset(GetSelectedAsset());
        }

        [MenuItem(PasteAssetMenuPath, false, secondaryPriority = SecondaryPriority + 4f)]
        private static void MenuPasteToAsset()
        {
            CopyAsset(GetClipboardAsset(), GetSelectedAsset(), recordUndo: true);
        }

        [MenuItem(CopySceneMenuPath, false, secondaryPriority = SecondaryPriority + 0f)]
        private static void MenuCopyFromScene()
        {
            var settings = ScriptableObject.CreateInstance<SceneEnvironmentLighting>();
            try
            {
                settings.Read(recordUndo: false);
                SetClipboardAsset(settings);
            }
            catch
            {
                DestroyImmediate(settings);
                throw;
            }
        }

        [MenuItem(PasteSceneMenuPath, false, secondaryPriority = SecondaryPriority + 1f)]
        private static void MenuPasteToScene()
        {
            var clipboard = GetClipboardAsset();
            if (clipboard != null)
            {
                clipboard.Write(recordUndo: true);
            }
        }

        [MenuItem(ExportMenuPath, true)]
        private static bool MenuValidateHasSelectedFolder()
        {
            return GetSelectedFolder() != null;
        }

        [MenuItem(ImportMenuPath, true)]
        [MenuItem(OverwriteMenuPath, true)]
        [MenuItem(CopyAssetMenuPath, true)]
        private static bool MenuValidateHasSelectedAsset()
        {
            return GetSelectedAsset() != null;
        }

        [MenuItem(PasteSceneMenuPath, true)]
        private static bool MenuValidateHasClipboard()
        {
            return AssetDatabase.GetMainAssetTypeAtPath(ClipboardFilePath)
                == typeof(SceneEnvironmentLighting);
        }

        [MenuItem(PasteAssetMenuPath, true)]
        private static bool MenuValidateHasSelectedAssetAndClipboard()
        {
            return MenuValidateHasSelectedAsset() && MenuValidateHasClipboard();
        }

        private static string GetSelectedFolder()
        {
            if (Selection.activeObject != null)
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    try
                    {
                        path = ProjectWindowUtil.GetContainingFolder(path);
                    }
                    catch {}
                }
                if (AssetDatabase.IsValidFolder(path))
                {
                    return path;
                }
            }
            return null;
        }

        private static SceneEnvironmentLighting GetSelectedAsset()
        {
            if (Selection.activeObject != null &&
                Selection.activeObject is SceneEnvironmentLighting settings)
            {
                return settings;
            }
            return null;
        }

        private static SceneEnvironmentLighting GetClipboardAsset()
        {
            return AssetDatabase.LoadAssetAtPath<SceneEnvironmentLighting>(ClipboardFilePath);
        }

        private static void SetClipboardAsset(SceneEnvironmentLighting settings)
        {
            if (settings != null)
            {
                if (AssetDatabase.Contains(settings))
                {
                    // Not sure if this is necessary. It's unclear if creating
                    // an asset from an asset file that already exists is bad
                    // practice or not.
                    var newSettings = ScriptableObject.CreateInstance<SceneEnvironmentLighting>();
                    try
                    {
                        CopyAsset(settings, newSettings, recordUndo: false);
                    }
                    catch
                    {
                        DestroyImmediate(newSettings);
                        throw;
                    }
                    settings = newSettings;
                }
                AssetDatabase.CreateAsset(settings, ClipboardFilePath);
            }
        }

        private static void CopyAsset(
            SceneEnvironmentLighting source, SceneEnvironmentLighting dest, bool recordUndo
        )
        {
            if (source != null && dest != null)
            {
                if (recordUndo)
                {
                    Undo.RegisterCompleteObjectUndo(
                    //Undo.RecordObject(
                        dest, "Replace " + FriendlyName
                    );
                }
                string origName = dest.name;
                HideFlags origHideFlags = dest.hideFlags;
                try
                {
                    EditorUtility.CopySerialized(source, dest);
                    if (recordUndo)
                    {
                        EditorUtility.SetDirty(dest);
                    }
                }
                finally
                {
                    dest.name = origName;
                    dest.hideFlags = origHideFlags;
                }
            }
        }
        #endregion

        #region Validate/Read/Write
        private static readonly AmbientMode[] InvalidAmbientModes = { AmbientMode.Custom };
        void OnValidate()
        {
            this.fogDensity          = Math.Clamp(this.fogDensity,          0f, 1f);
            this.ambientIntensity    = Math.Clamp(this.ambientIntensity,    0f, 8f);
            this.reflectionIntensity = Math.Clamp(this.reflectionIntensity, 0f, 1f);
            this.reflectionBounces   = Math.Clamp(this.reflectionBounces,   1,  5);
            this.haloStrength        = Math.Clamp(this.haloStrength,        0f, 1f);
            this.flareStrength       = Math.Clamp(this.flareStrength,       0f, 1f);
            this.defaultReflectionResolution =
                (ReflectionResolution)FloorPowerOfTwo((int)this.defaultReflectionResolution, 16, 2048);
            this.fogMode =
                ValidateEnum(this.fogMode, FogMode.ExponentialSquared);
            this.ambientMode =
                ValidateEnum(this.ambientMode, AmbientMode.Skybox, InvalidAmbientModes);
            this.reflectionCubemapCompression =
                ValidateEnum(this.reflectionCubemapCompression, ReflectionCubemapCompression.Auto);
            this.defaultReflectionMode =
                ValidateEnum(this.defaultReflectionMode, DefaultReflectionMode.Skybox);
        }

        /// <summary>
        /// Read settings from <c>RenderSettings</c> and
        /// <c>LightmapEditorSettings</c> classes.
        /// </summary>
        public void Read(bool recordUndo)
        {
            if (recordUndo)
            {
                Undo.RegisterCompleteObjectUndo(
                //Undo.RecordObject(
                    this, "Replace " + FriendlyName
                );
            }

            this.fog                         = RenderSettings.fog;
            this.fogStartDistance            = RenderSettings.fogStartDistance;
            this.fogEndDistance              = RenderSettings.fogEndDistance;
            this.fogMode                     = RenderSettings.fogMode;
            this.fogColor                    = RenderSettings.fogColor;
            this.fogDensity                  = RenderSettings.fogDensity;
            this.ambientMode                 = RenderSettings.ambientMode;
            this.ambientSkyColor             = RenderSettings.ambientSkyColor;
            this.ambientEquatorColor         = RenderSettings.ambientEquatorColor;
            this.ambientGroundColor          = RenderSettings.ambientGroundColor;
            this.ambientIntensity            = RenderSettings.ambientIntensity;
            this.ambientLight                = RenderSettings.ambientLight;
            this.subtractiveShadowColor      = RenderSettings.subtractiveShadowColor;
            this.skybox                      = RenderSettings.skybox;
            this.sun                         = RenderSettings.sun;
            this.ambientProbe                = RenderSettings.ambientProbe;
            this.customReflectionTexture     = RenderSettings.customReflectionTexture;
            this.reflectionIntensity         = RenderSettings.reflectionIntensity;
            this.reflectionBounces           = RenderSettings.reflectionBounces;
            this.defaultReflectionMode       = RenderSettings.defaultReflectionMode;
            this.defaultReflectionResolution = (ReflectionResolution)RenderSettings.defaultReflectionResolution;
            this.haloStrength                = RenderSettings.haloStrength;
            this.flareStrength               = RenderSettings.flareStrength;
            this.flareFadeSpeed              = RenderSettings.flareFadeSpeed;

#if SCENEENVLIGHT_USE_REFLECTION && SCENEENVLIGHT_SERIALIZED_PROPERTIES
            // haloTexture
            // spotCookie
            ReadWriteHaloTextureSpotCookie(this, GetRenderSettings(), read: true);
#endif

            this.reflectionCubemapCompression = LightmapEditorSettings.reflectionCubemapCompression;

            if (recordUndo)
            {
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Write settings to <c>RenderSettings</c> and
        /// <c>LightmapEditorSettings</c> classes.
        /// </summary>
        public void Write(bool recordUndo)
        {
            // Create a copy for validation, so that invalid values aren't
            // changed in the original file (for version control reasons).
            var valid = ScriptableObject.CreateInstance<SceneEnvironmentLighting>();
            try
            {
            CopyAsset(this, valid, recordUndo: false);
            //EditorUtility.CopySerialized(this, valid);
            valid.OnValidate();

#if SCENEENVLIGHT_USE_REFLECTION
#if SCENEENVLIGHT_SERIALIZED_PROPERTIES
            // Required to write serialized properties, always get.
            var renderSettingsObject = GetRenderSettings();
#else
            var renderSettingsObject = recordUndo ? GetRenderSettings() : null;
#endif
            var recordObjects = recordUndo ? new List<UnityEngine.Object>() : null;
            if (recordUndo)
            {
                var lightmapSettingsObject = GetLightmapSettings();
                if (renderSettingsObject != null)
                {
                    recordObjects.Add(renderSettingsObject);
                }
                if (lightmapSettingsObject != null)
                {
                    recordObjects.Add(lightmapSettingsObject);
                }
                if (recordObjects.Count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(
                    //Undo.RecordObjects(
                        recordObjects.ToArray(), "Apply " + FriendlyName
                    );
                }
            }
#endif // SCENEENVLIGHT_USE_REFLECTION

            RenderSettings.fog                         = valid.fog;
            RenderSettings.fogStartDistance            = valid.fogStartDistance;
            RenderSettings.fogEndDistance              = valid.fogEndDistance;
            RenderSettings.fogMode                     = valid.fogMode;
            RenderSettings.fogColor                    = valid.fogColor;
            RenderSettings.fogDensity                  = valid.fogDensity;
            RenderSettings.ambientMode                 = valid.ambientMode;
            RenderSettings.ambientSkyColor             = valid.ambientSkyColor;
            RenderSettings.ambientEquatorColor         = valid.ambientEquatorColor;
            RenderSettings.ambientGroundColor          = valid.ambientGroundColor;
            RenderSettings.ambientIntensity            = valid.ambientIntensity;
            RenderSettings.ambientLight                = valid.ambientLight;
            RenderSettings.subtractiveShadowColor      = valid.subtractiveShadowColor;
            RenderSettings.skybox                      = valid.skybox;
            RenderSettings.sun                         = valid.sun;
            RenderSettings.ambientProbe                = valid.ambientProbe;
            RenderSettings.customReflectionTexture     = valid.customReflectionTexture;
            RenderSettings.reflectionIntensity         = valid.reflectionIntensity;
            RenderSettings.reflectionBounces           = valid.reflectionBounces;
            RenderSettings.defaultReflectionMode       = valid.defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = (int)valid.defaultReflectionResolution;
            RenderSettings.haloStrength                = valid.haloStrength;
            RenderSettings.flareStrength               = valid.flareStrength;
            RenderSettings.flareFadeSpeed              = valid.flareFadeSpeed;

#if SCENEENVLIGHT_USE_REFLECTION && SCENEENVLIGHT_SERIALIZED_PROPERTIES
            // haloTexture
            // spotCookie
            ReadWriteHaloTextureSpotCookie(valid, renderSettingsObject, read: false);
#endif

            LightmapEditorSettings.reflectionCubemapCompression = valid.reflectionCubemapCompression;

#if SCENEENVLIGHT_USE_REFLECTION
            if (recordUndo && recordObjects != null)
            {
                foreach (var recordObject in recordObjects)
                {
                    EditorUtility.SetDirty(recordObject);
                }
            }
#endif
            }
            finally
            {
                DestroyImmediate(valid);
            }
        }

#if SCENEENVLIGHT_USE_REFLECTION && SCENEENVLIGHT_SERIALIZED_PROPERTIES
        private static void ReadWriteHaloTextureSpotCookie(
            SceneEnvironmentLighting settings,
            UnityEngine.Object renderSettingsObject,
            bool read
        )
        {
            using var serialized = renderSettingsObject != null
                    ? new SerializedObject(renderSettingsObject)
                    : null;
            using var haloTextureProp = serialized?.FindProperty("m_HaloTexture");
            using var spotCookieProp  = serialized?.FindProperty("m_SpotCookie");

            if (haloTextureProp != null || spotCookieProp != null)
            {
                serialized.Update();
                if (haloTextureProp != null)
                {
                    if (read)
                    {
                        settings.haloTexture = haloTextureProp.objectReferenceValue
                            as Texture;
                    }
                    else
                    {
                        haloTextureProp.objectReferenceValue = settings.haloTexture;
                    }
                }
                if (spotCookieProp != null)
                {
                    if (read)
                    {
                        settings.spotCookie = spotCookieProp.objectReferenceValue
                            as Texture;
                    }
                    else
                    {
                        spotCookieProp.objectReferenceValue = settings.spotCookie;
                    }
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
#endif // SCENEENVLIGHT_USE_REFLECTION && SCENEENVLIGHT_SERIALIZED_PROPERTIES

#if SCENEENVLIGHT_USE_REFLECTION
        private static bool s_internalMethodsLoaded = false;
        private static MethodInfo s_getRenderSettings;
        private static MethodInfo s_getLightmapSettings;
        private static void LoadInternalMethods()
        {
            if (!s_internalMethodsLoaded)
            {
                s_internalMethodsLoaded = true;
                const BindingFlags Flags = BindingFlags.Static
                                            | BindingFlags.NonPublic;
                s_getRenderSettings   = typeof(UnityEngine.RenderSettings)
                                        .GetMethod("GetRenderSettings", Flags);
                s_getLightmapSettings = typeof(UnityEditor.LightmapEditorSettings)
                                        .GetMethod("GetLightmapSettings", Flags);
            }
        }

        private static UnityEngine.Object GetRenderSettings()
        {
            LoadInternalMethods();
            return s_getRenderSettings?.Invoke(null, Array.Empty<object>())
                as UnityEngine.Object;
        }

        private static UnityEngine.Object GetLightmapSettings()
        {
            LoadInternalMethods();
            return s_getLightmapSettings?.Invoke(null, Array.Empty<object>())
                as UnityEngine.Object;
        }
#endif // SCENEENVLIGHT_USE_REFLECTION
        #endregion

        #region Helpers
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += AssignAssetIconAndFindScriptFolder;
        }

        private static void AssignAssetIconAndFindScriptFolder()
        {
            var dummyObject = ScriptableObject.CreateInstance<SceneEnvironmentLighting>();
            try
            {
                var script = MonoScript.FromScriptableObject(dummyObject);
                if (script != null)
                {
                    string path = AssetDatabase.GetAssetPath(script);
                    if (path != null)
                    {
                        s_scriptFolder = ProjectWindowUtil.GetContainingFolder(path);
                        var importer = AssetImporter.GetAtPath(path) as MonoImporter;
                        if (importer != null && AssetIconName != null)
                        {
                            var content = EditorGUIUtility.IconContent(AssetIconName);
                            if (content != null && content.image != null &&
                                content.image is Texture2D icon)
                            {
                                importer.SetIcon(icon);
                            }
                        }
                    }
                }
            }
            finally
            {
                DestroyImmediate(dummyObject);
            }
        }

        private static T GetComponentInScene<T>(string path) where T : Component
        {
            if (!string.IsNullOrEmpty(path))
            {
                var gameObject = GameObject.Find(path);
                if (gameObject != null)
                {
                    return gameObject.GetComponent<T>();
                }
            }
            return null;
        }

        private static string GetComponentScenePath(Component component)
        {
            if (component != null && component.transform != null)
            {
                var pathParts = new List<string>();
                Transform transform = component.transform;
                do
                {
                    pathParts.Insert(0, transform.name);
                    transform = transform.parent;
                } while (transform != null);
                // This is an absolute path, so prefix with '/'.
                return "/" + string.Join('/', pathParts);
            }
            return null;
        }

        private static T ValidateEnum<T>(T value, T defaultValue)  where T : Enum
        {
            return Enum.IsDefined(typeof(T), value) ? value : defaultValue;
        }

        private static T ValidateEnum<T>(T value, T defaultValue, params T[] invalidValues)
            where T : Enum
        {
            return -1 == Array.IndexOf(invalidValues, value)
                ? ValidateEnum(value, defaultValue) : defaultValue;
        }

        private static int FloorPowerOfTwo(int value, int lower, int upper)
        {
            for (int x = lower; x < upper; x *= 2)
            {
                if (value < x * 2)
                {
                    return x;
                }
            }
            return upper;
        }
        #endregion
    }
}

#endif
