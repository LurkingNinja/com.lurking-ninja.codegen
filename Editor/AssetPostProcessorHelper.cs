using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LurkingNinja.CodeGen.Editor
{
    public static class AssetPostProcessorHelper
    {
        public static string GetFullPath(string fileName, string path) =>
            $"{Application.dataPath}/../{path}{fileName}.cs";

        public static string GetPath(string fileName, string path) =>
            $"{path}{fileName}.cs";

        public static string KeyToCSharpWithoutAt(string key) => KeyToCSharp(key, false);
		
        public static string KeyToCSharp(string key, bool addAt = true)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Key cannot be empty or null.");
            
            var outKey = "";
            if (!char.IsLetter(key[0]) && key[0] != '_')
                outKey = $"_{key}";
            outKey = key.Where(t1 => char.IsLetterOrDigit(t1) || t1 != '_').Aggregate(outKey, (current, t1) => current + t1);
            outKey = addAt ? $"@{outKey}" : outKey;
            var isValidIdentifier = new Microsoft.CSharp.CSharpCodeProvider().IsValidIdentifier(outKey);
            if (!isValidIdentifier)
                throw new ArgumentOutOfRangeException(nameof(key),
                    "Key should be resolvable into a valid C# identifier.");
            return outKey;
        }

        public static string GetFileName(string fileName) =>
            Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_");

        public static void DeleteFile(string fileName, string path) =>
            AssetDatabase.DeleteAsset(GetPath(GetFileName(fileName), path));

        public static void WriteFile(string fileName, string path, string content)
        {
            fileName = GetFileName(fileName);
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