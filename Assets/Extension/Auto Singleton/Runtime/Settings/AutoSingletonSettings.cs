using System;
using AutoSingleton;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "Auto Singleton Settings", menuName = "Auto Singleton/Settings")]
    public class AutoSingletonSettings : ScriptableObject
    {
        public string[] ExcludedManagers => excludedManagers;
        
        public bool ShowDebugCustomManager => showDebugCustomManager;
        
        [BoxGroup("Managers")]
        [SerializeField, ReorderableList, TypeDropDown(typeof(Singleton))]
        protected string[] excludedManagers;

        [BoxGroup("Debug")]
        [SerializeField]
        protected bool showDebugCustomManager = true;

        private const string KAssetName = "AutoManagerSettings";

        public static AutoSingletonSettings CurrentSettings =>
            HasSettingAsset ? Resources.Load<AutoSingletonSettings>(KAssetName) : DefaultSettings;

        public static bool HasSettingAsset => Resources.Load<AutoSingletonSettings>(KAssetName) != null;
        
        public static AutoSingletonSettings DefaultSettings
        {
            get
            {
                if (_defaultSettings == null)
                    _defaultSettings = CreateDefaultSettings();
                return _defaultSettings;
            }
        }

        private static AutoSingletonSettings _defaultSettings;

        private static AutoSingletonSettings CreateDefaultSettings()
        {
            AutoSingletonSettings defaultAsset = CreateInstance<AutoSingletonSettings>();
    
            defaultAsset.excludedManagers = Array.Empty<string>();
            return defaultAsset;
        }
    }
