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

    public partial class CodeGenSettingsProvider
    {
#if LN_LOCALIZATION
        private readonly GUIContent _localizationGui = new("Localization");

        private SerializedProperty _localization;
        private SerializedProperty _localizationNameSpace;
        private SerializedProperty _localizationFilePath;
        private SerializedProperty _localizationTemplate;

        private void LocalizationOnActivate()
        {
            _localization = _serializedSettings.FindProperty("localization");
            _localizationNameSpace = _serializedSettings.FindProperty("localizationNameSpace");
            _localizationFilePath = _serializedSettings.FindProperty("localizationFilePath");
            _localizationTemplate = _serializedSettings.FindProperty("localizationTemplate");
        }
        private void LocalizationOnGUI()
        {
            _localization.boolValue = EditorGUILayout.BeginToggleGroup(_localizationGui, _localization.boolValue);
            if (_localization.boolValue)
            {
                EditorGUILayout.PropertyField(_localizationNameSpace, _nameSpaceGui);
                EditorGUILayout.PropertyField(_localizationFilePath, _filePathGui);
                
                EditorGUILayout.PrefixLabel(_templateGui);
                _localizationTemplate.stringValue =
                        EditorGUILayout.TextArea(_localizationTemplate.stringValue, _maxHeight);
            }
            EditorGUILayout.EndToggleGroup();
#else
            EditorGUILayout.BeginToggleGroup(new GUIContent("Localization package isn't installed."), false);
            EditorGUILayout.EndToggleGroup();
#endif
        }
    }
}