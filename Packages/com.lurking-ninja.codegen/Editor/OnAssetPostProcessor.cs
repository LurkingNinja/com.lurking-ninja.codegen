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
    using System.Collections.Generic;
    using UnityEditor;
    using Object = UnityEngine.Object;

    public struct OnProcess
    {
        public Object Target;
        public string Path;
    }
    
    public class OnAssetPostProcessor : AssetPostprocessor
    {
        private static readonly Dictionary<Type, List<Action<OnProcess>>> _CHANGE_CALLBACKS = new();
        internal static readonly Dictionary<Type, List<Action<OnProcess>>> DELETE_CALLBACKS = new();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                foreach (var keyValue in _CHANGE_CALLBACKS)
                {
                    var asset = AssetDatabase.LoadAssetAtPath(path, keyValue.Key);
                    if (asset is null) continue;

                    var callback = new OnProcess { Target = asset, Path = path };
                    
                    foreach (var action in keyValue.Value) action?.Invoke(callback);
                }
            }
        }

        public static void Add(Type key, Action<OnProcess> changeCallback, Action<OnProcess> deleteCallback)
        {
            AddChange(key, changeCallback);
            AddDeletion(key, deleteCallback);
        }

        public static void Remove(Type key, Action<OnProcess> changeCallback, Action<OnProcess> deleteCallback)
        {
            RemoveChange(key, changeCallback);
            RemoveDeletion(key, deleteCallback);
        }

        private static void AddChange(Type key, Action<OnProcess> callback)
        {
            if (!_CHANGE_CALLBACKS.ContainsKey(key)) _CHANGE_CALLBACKS[key] = new List<Action<OnProcess>>();
            if (_CHANGE_CALLBACKS[key].Contains(callback)) return;
            
            _CHANGE_CALLBACKS[key].Add(callback);
        }

        private static void RemoveChange(Type key, Action<OnProcess> callback)
        {
            if (_CHANGE_CALLBACKS.TryGetValue(key, out var changeCallback))
                changeCallback.Remove(callback);
        }

        private static void AddDeletion(Type key, Action<OnProcess> callback)
        {
            if (!DELETE_CALLBACKS.ContainsKey(key)) DELETE_CALLBACKS[key] = new List<Action<OnProcess>>();
            if (DELETE_CALLBACKS[key].Contains(callback)) return;

            DELETE_CALLBACKS[key].Add(callback);
        }

        private static void RemoveDeletion(Type key, Action<OnProcess> callback)
        {
            if (DELETE_CALLBACKS.TryGetValue(key, out var deleteCallback))
                deleteCallback.Remove(callback);
        }
    }

    // To detect asset removal.
    public class CustomAssetModificationProcessor : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions rao)
        {
            foreach (var keyValue in OnAssetPostProcessor.DELETE_CALLBACKS)
            {
                var asset = AssetDatabase.LoadAssetAtPath(path, keyValue.Key);
                if (asset is null) continue;

                var callback = new OnProcess { Target = asset, Path = path };
                
                foreach (var action in keyValue.Value) action?.Invoke(callback);
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}