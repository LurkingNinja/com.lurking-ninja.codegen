using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LurkingNinja.CodeGen.Editor
{
    // To detect creation and saving an asset. We do not care about moving.
    public class OnAssetPostProcessor : AssetPostprocessor
    {
        private static readonly Dictionary<Type, List<Action<Object>>> _changeCallbacks = new();
        internal static readonly Dictionary<Type, List<Action<Object>>> DeleteCallbacks = new();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                foreach (var keyValue in _changeCallbacks)
                {
                    var asset = AssetDatabase.LoadAssetAtPath(path, keyValue.Key);
                    if (asset is null) continue;
                    foreach (var action in keyValue.Value)
                    {
                        action?.Invoke(asset);
                    }
                }
            }
        }

        public static void AddListener(Type key, Action<Object> changeCallback, Action<Object> deleteCallback)
        {
            AddChangeListener(key, changeCallback);
            AddDeletionListener(key, deleteCallback);
        }

        public static void RemoveListener(Type key, Action<Object> changeCallback, Action<Object> deleteCallback)
        {
            RemoveChangeListener(key, changeCallback);
            RemoveDeletionListener(key, deleteCallback);
        }
        
        public static void AddChangeListener(Type key, Action<Object> callback)
        {
            if (!_changeCallbacks.ContainsKey(key)) _changeCallbacks[key] = new List<Action<Object>>();
            if (_changeCallbacks[key].Contains(callback)) return;
            _changeCallbacks[key].Add(callback);
        }

        public static void RemoveChangeListener(Type key, Action<Object> callback)
        {
            if (!_changeCallbacks.ContainsKey(key)) return;
            _changeCallbacks[key].Remove(callback);
        }

        public static void AddDeletionListener(Type key, Action<Object> callback)
        {
            if (!DeleteCallbacks.ContainsKey(key)) DeleteCallbacks[key] = new List<Action<Object>>();
            if (DeleteCallbacks[key].Contains(callback)) return;
            DeleteCallbacks[key].Add(callback);
        }

        public static void RemoveDeletionListener(Type key, Action<Object> callback)
        {
            if (!DeleteCallbacks.ContainsKey(key)) return;
            DeleteCallbacks[key].Remove(callback);
        }
    }

    // To detect asset removal.
    public class CustomAssetModificationProcessor : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions rao)
        {
            foreach (var keyValue in OnAssetPostProcessor.DeleteCallbacks)
            {
                var asset = AssetDatabase.LoadAssetAtPath(path, keyValue.Key);
                if (asset is null) continue;
                foreach (var action in keyValue.Value)
                {
                    action?.Invoke(asset);
                }
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
}