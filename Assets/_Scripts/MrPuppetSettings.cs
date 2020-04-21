using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MrPuppet
{
    class MrPuppetSettings : ScriptableObject
    {
        public const string AssetPath = "Assets/__Config/MrPuppetSettings.asset";

        [SerializeField]
        public string FACSFilePath;

        internal static MrPuppetSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MrPuppetSettings>(AssetPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MrPuppetSettings>();
                settings.FACSFilePath = "/Volumes/GoogleDrive/My Drive/Thinko/Shows/";
                AssetDatabase.CreateAsset(settings, AssetPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static MrPuppetSettings SaveSettings(string ProviderPath){
            var settings = AssetDatabase.LoadAssetAtPath<MrPuppetSettings>(AssetPath);
                settings.FACSFilePath = ProviderPath;

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    class MrPuppetSettingsProvider : SettingsProvider
    {
        private SerializedObject Settings;

        class Styles
        {
            public static GUIContent someString = new GUIContent("FACS File Path");
        }

        public MrPuppetSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            Settings = MrPuppetSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(Settings.FindProperty("FACSFilePath"), Styles.someString);
            
            if (GUI.changed)
            {
                MrPuppetSettings.SaveSettings( Settings.FindProperty("FACSFilePath").stringValue );
            }
            //Settings.ApplyModifiedProperties();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMrPuppetSettingsProvider()
        {
            var provider = new MrPuppetSettingsProvider("Preferences/Mr.Puppet", SettingsScope.User){};

            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }
    }
}