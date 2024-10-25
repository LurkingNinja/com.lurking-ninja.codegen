/***
 * Lurking Ninja CodeGen
 * Copyright (c) 2022-2024 Lurking Ninja.
 *
 * MIT License
 * https://github.com/LurkingNinja/com.lurking-ninja.codegen
 */
namespace LurkingNinja.CodeGen.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public partial class CodeGenSettingsProvider : SettingsProvider
    {
        private const string _SETTINGS_PATH = "Project/Lurking Ninja CodeGen";

        private SerializedObject _serializedSettings;
        private SerializedProperty _autoRun;

        private readonly GUIContent _autoRunGui = new("Run CodeGen automatically");
        
        private readonly GUIContent _nameSpaceGui = new("Namespace");
        private readonly GUIContent _filePathGui = new("Path to generate");
        private readonly GUIContent _templateGui = new("Template");
        private readonly GUIContent _saveGui = new("Save");

        private readonly GUILayoutOption _maxHeight = GUILayout.MaxHeight(100);
        private readonly GUILayoutOption _buttonWidth = GUILayout.Width(100);

        internal static bool IsGenerating { get; private set; }
        
        private CodeGenSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) {}

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            CodeGenSettings.Save();
            _serializedSettings = CodeGenSettings.GetSerializedSettings();
            
            _autoRun = _serializedSettings.FindProperty("autoRun");

#if LN_LOCALIZATION 
            LocalizationOnActivate();
#endif
        }
        
        private Vector2 _scroll = Vector2.zero;
        
        public override void OnGUI(string searchContext)
        {
            AddLine();
            
            _autoRun.boolValue = EditorGUILayout.BeginToggleGroup(_autoRunGui, _autoRun.boolValue);
            EditorGUILayout.EndToggleGroup();
            
            AddLine();
            
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUIStyle.none);
            
            LocalizationOnGUI();

            EditorGUILayout.EndScrollView();

            AddLine();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_saveGui, _buttonWidth)) Apply();

            GUILayout.EndHorizontal();
            
            AddSeparator();
        }

        private void Apply()
        {
            _serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            _serializedSettings.Update();
            CodeGenSettings.Save();
            RunGeneration();
        }

        private static void AddSeparator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
        }
        
        private static void AddLine(float padding = 0f)
        {
            GUILayout.Space(padding / 2);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            GUILayout.Space(padding / 2);
        }

        internal static void RunGeneration()
        {
            IsGenerating = true;
#if LN_LOCALIZATION 
            LocalizationCodeGen.GenerateAll();
#endif
            IsGenerating = false;
        }

        
        [SettingsProvider]
        public static SettingsProvider CreateCodeGenSettingsProvider() => new CodeGenSettingsProvider(_SETTINGS_PATH);
    }
}