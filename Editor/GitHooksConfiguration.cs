using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;


namespace GitHooksUtility
{
    [CreateAssetMenu(menuName = "GitHooks/Configuration", fileName = "GitHooksConfiguration")]
    public class GitHooksConfiguration : ScriptableObject, ISerializationCallbackReceiver
    {
        public bool enabled;
        public List<Object> hooks = new();

        [System.NonSerialized]
        public List<string> assetPaths = new();

        public bool IsConfiguration => Configuration == this;
        public bool IsConfigurationTemplate => ConfigurationTemplate == this;
        //private const string INSTALLED_KEY = "com.ersagun.git-hooks-utility.installed";
        private const string CONFIGURATION_FILE = "GitHooksConfiguration.asset";
        private const string HOOKS_FOLDER = "GitHooks";



        void OnEnable()
        {
            if (IsConfiguration)
                PostProcessImportAsset.onPostProcessAllAssets += OnAssetsChanged;
            RefreshAssetsList();
        }

        void OnDisable()
        {
            if (IsConfiguration)
                PostProcessImportAsset.onPostProcessAllAssets -= OnAssetsChanged;
        }

        void OnAssetsChanged(IEnumerable<string> changedAssets)
        {
            if (!IsConfiguration)
            {
                return;
            }
            if (!enabled)
            {
                return;
            }
            foreach (var changedAsset in changedAssets)
            {
                if (assetPaths.Contains(changedAsset))
                {
                    Debug.Log("Change in hook assets detected");
                    CopyHooks();
                    return;
                }
            }
        }

        private static void CopyHooks()
        {
            Debug.Log("Installing git hooks");
            if(Configuration.enabled)
            {
                GitHookResolver.assetPaths = Configuration.assetPaths;
                GitHookResolver.MoveGitHooksToFolder();
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {

        }
        void OnValidate()
        {
            RefreshAssetsList();
        }

        private void RefreshAssetsList()
        {
            var newAssets = new List<string>();
            bool assetsChanged = hooks.Count != assetPaths.Count;
            foreach (var hook in hooks)
            {
                string asset = "";
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(hook, out string guid, out long _))
                {
                    asset = AssetDatabase.GUIDToAssetPath(guid);
                }
                newAssets.Add(asset);
                if (!assetPaths.Contains(asset))
                {
                    assetsChanged = true;
                }
            }
            assetPaths.Clear();
            assetPaths.AddRange(newAssets);
            if (assetsChanged)
            {
                CopyHooks();
            }
        }

        public static void Setup()
        {
            if (ConfigurationTemplate == null)
            {
                PostProcessImportAsset.OnPostProcessOnce((_) => Setup());
                return;

            }
            if (!IsConfigurationCopied())
            {
                FixSerialization();
                CopyConfiguration();
            }
        }

        private static void FixSerialization()
        {
            if(!GitSerializationResolver.CheckGitSerialization())
                GitSerializationResolver.SetGitSerialization();
        }

        private static bool copyingConfiguration = false;

        private static void CopyConfiguration()
        {
            if (copyingConfiguration) return;
            copyingConfiguration = true;
            if (!AssetDatabase.IsValidFolder("Assets/Editor Default Resources/" + HOOKS_FOLDER))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Editor Default Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }
                AssetDatabase.CreateFolder("Assets/Editor Default Resources", HOOKS_FOLDER);
            }


            var configurationTemplate = ConfigurationTemplate;
            var configuration = CreateInstance<GitHooksConfiguration>();

            foreach (var hook in configurationTemplate.hooks)
            {

                string filePath = HOOKS_FOLDER + "/" + hook.name;
                string path = "Assets/Editor Default Resources/" + filePath;


                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(hook, out string guid, out long localId))
                {

                    AssetDatabase.ActiveRefreshImportMode = AssetDatabase.RefreshImportMode.InProcess;
                    AssetDatabase.CopyAsset(AssetDatabase.GUIDToAssetPath(guid), path);


                    PostProcessImportAsset.OnPostProcessAssetOnce(path, (IEnumerable<string> files) =>
                    {
                        var newHook = AssetDatabase.LoadAssetAtPath<Object>(path);
                        //newHook = EditorGUIUtility.Load(filePath);
                        configuration.hooks.Add(newHook);
                        Debug.Log(Configuration == configuration);
                        Debug.Log(newHook);
                        EditorUtility.SetDirty(configuration);
                        AssetDatabase.SaveAssets();
                    });

                    AssetDatabase.ImportAsset(path);
                }
            }

            configuration.enabled = true;


            AssetDatabase.CreateAsset(configuration, "Assets/Editor Default Resources/" + CONFIGURATION_FILE);
            copyingConfiguration = false;

            PostProcessImportAsset.OnPostProcessAssetOnce("Assets/Editor Default Resources/" + CONFIGURATION_FILE, (_) => CopyHooks());
        }

        public static bool IsConfigurationCopied()
        {
            return Configuration != null;
        }

        public static GitHooksConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = (GitHooksConfiguration)AssetDatabase.LoadAssetAtPath<GitHooksConfiguration>("Assets/Editor Default Resources/" + CONFIGURATION_FILE);
                }
                return configuration;
            }
        }

        private static GitHooksConfiguration configuration;
        private static GitHooksConfiguration configurationTemplate;
        public static GitHooksConfiguration ConfigurationTemplate
        {
            get
            {
                if (configurationTemplate == null)
                {
                    configurationTemplate = AssetDatabase.LoadAssetAtPath<GitHooksConfiguration>("Packages/com.ersagun.git-hooks-utility/Editor/ConfigurationTemplate.asset");
                }
                if (configurationTemplate == null)
                {
                    foreach (var config in Resources.FindObjectsOfTypeAll<GitHooksConfiguration>())
                    {
                        if (config.name == "ConfigurationTemplate")
                        {
                            configurationTemplate = config;
                            break;
                        }
                    }
                }

                return configurationTemplate;
            }
        }

    }






    public class PostProcessImportAsset : AssetPostprocessor
    {

        public static UnityEngine.Events.UnityAction<IEnumerable<string>> onPostProcessAllAssets;
        public static void OnPostProcessOnce(UnityEngine.Events.UnityAction<IEnumerable<string>> action)
        {
            void Callback(IEnumerable<string> args)
            {
                action.Invoke(args);
                onPostProcessAllAssets -= Callback;
            }
            onPostProcessAllAssets += Callback;
        }

        public static void OnPostProcessAssetOnce(string assetPath, UnityEngine.Events.UnityAction<IEnumerable<string>> action)
        {
            void Callback(IEnumerable<string> args)
            {
                if (args.Contains(assetPath))
                {
                    action.Invoke(args);
                    onPostProcessAllAssets -= Callback;
                }
            }
            onPostProcessAllAssets += Callback;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            onPostProcessAllAssets?.Invoke(new List<string>(importedAssets).Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths));
        }
    }

}