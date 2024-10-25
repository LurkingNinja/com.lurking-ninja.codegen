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
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.CSharp;
    using UnityEditor;
    using UnityEngine;

    public static class CodegenUtil
    {
#if !LN_EDITOR_COROUTINES
        [InitializeOnLoadMethod]
        private static void InstallEditorCoroutines() =>
            UnityEditor.PackageManager.Client.Add("com.unity.editorcoroutines");
#endif

        private static string GetFullPath(string fileName, string path) =>
            $"{Application.dataPath}/../{path}{fileName}.cs";

        private static string GetPath(string fileName, string path) =>
            $"{path}{fileName}.cs";

        public static string KeyToCSharpWithoutAt(string key) => KeyToCSharp(key, false);

        private static bool HasLeadingDigit(string value) => (value[0] >= '0' && value[0] <= '9');

        public static string KeyToCSharp(string key, bool addAt = true)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Key cannot be empty or null.");

            var cSharpCodeProvider = new CSharpCodeProvider();

            // Removing "NNN - " prefixes with any numbers. 
            key = Regex.Replace(key, @"^[0-9]*\s-\s", string.Empty);
            // Replacing spaces and - with underscores.
            key = key.Replace(" ", "_").Replace("-", "_");

            if (HasLeadingDigit(key)) key = $"_{key}";
            
            // Attempt to create a C# compatible identifier from the key.
            var outKey = cSharpCodeProvider.CreateValidIdentifier(key);
            // Adding @ unless explicitly unwanted.
            outKey = addAt ? $"@{outKey}" : outKey;

            // Attempt to check if the result is a valid C# identifier.
            return !cSharpCodeProvider.IsValidIdentifier(outKey)
                    ? throw new ArgumentOutOfRangeException(nameof(key),
                            $"Key should be resolvable into a valid C# identifier: {outKey}")
                    : outKey;
        }

        private static string GetFilename(string fileName) =>
                Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_");

        public static void DeleteFile(string fileName, string path) =>
            AssetDatabase.DeleteAsset(GetPath(GetFilename(fileName), path));

        public static void WriteFile(string fileName, string path, string content)
        {
            fileName = GetFilename(fileName);
            var genPath = GetFullPath(fileName, path);
            var folderOnly = Path.GetDirectoryName(genPath);
            if (!Directory.Exists(folderOnly) && folderOnly != null)
                Directory.CreateDirectory(folderOnly);
            using var writer = new StreamWriter(genPath, false);
            writer.WriteLine(content);
            AssetDatabase.ImportAsset($"{path}{fileName}.cs");
        }
    }
}