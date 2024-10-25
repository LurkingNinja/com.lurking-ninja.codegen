/***
 * Lurking Ninja CodeGen
 * Copyright (c) 2022-2024 Lurking Ninja.
 *
 * MIT License
 * https://github.com/LurkingNinja/com.lurking-ninja.codegen
 */
namespace LurkingNinja.CodeGen.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [FilePath("ProjectSettings/CodeGenSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public partial class CodeGenSettings : ScriptableSingleton<CodeGenSettings>
    {
        public static bool AutoRun => instance.autoRun;

        public static event Action<bool> OnAutoRunChanged;
        
        [SerializeField] internal bool autoRun = true;
        
        internal static SerializedObject GetSerializedSettings() => new(instance);

        internal static void InvokeOnAutoRunChanged(bool value) => OnAutoRunChanged?.Invoke(value);
        
        internal static void Save() => instance.Save(true);
        private void OnDisable() => Save();
    }
}