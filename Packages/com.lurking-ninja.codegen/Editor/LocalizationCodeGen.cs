/***
 * Lurking Ninja CodeGen
 * Copyright (c) 2022-2024 Lurking Ninja.
 *
 * MIT License
 * https://github.com/LurkingNinja/com.lurking-ninja.codegen
 */

using UnityEngine;

#if LN_LOCALIZATION
namespace LurkingNinja.CodeGen.Editor
{
    using UnityEditor;
    using UnityEditor.Localization;
    using UnityEditor.Localization.Search;
    using UnityEngine.Localization.Tables;
    using UnityEditor.Search;
#if LN_EDITOR_COROUTINES
    using Unity.EditorCoroutines.Editor;
#endif
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    public static class LocalizationCodeGen
    {
        private const string _FILE_REPLACEMENT = "_Shared_Data";
        
        private static readonly Type _SHARED_TABLE_DATA_TYPE = typeof(SharedTableData);
        
#if LN_EDITOR_COROUTINES
        private static EditorCoroutine _runningCoroutine;
#endif

        [InitializeOnLoadMethod]
        private static void Awake()
        {
            if (CodeGenSettings.AutoRun) EventSubscribe();
            CodeGenSettings.OnAutoRunChanged += EventSubscribe;
        }

        private static void EventSubscribe(bool turnOn = true)
        {
#if LN_EDITOR_COROUTINES
            if (turnOn && CodeGenSettings.LocalizationEnabled)
                LocalizationEditorSettings.EditorEvents.TableEntryModified += ModifyCallback;
            else
                LocalizationEditorSettings.EditorEvents.TableEntryModified -= ModifyCallback;
#endif
            if (turnOn && CodeGenSettings.LocalizationEnabled)
            {
                OnAssetPostProcessor.Add(_SHARED_TABLE_DATA_TYPE, ModifyCallback, DeleteCallback);
                AssemblyReloadEvents.beforeAssemblyReload += Destructor;

                return;
            }
            
            OnAssetPostProcessor.Remove(_SHARED_TABLE_DATA_TYPE, ModifyCallback, DeleteCallback);
            AssemblyReloadEvents.beforeAssemblyReload -= Destructor;
        }

        private static void Destructor() => EventSubscribe(false);

#if LN_EDITOR_COROUTINES
        private static void ModifyCallback(SharedTableData.SharedTableEntry entry)
        {
            if (_runningCoroutine != null) EditorCoroutineUtility.StopCoroutine(_runningCoroutine);
            
            _runningCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(ModifyCallbackCoroutine(entry));
        }
        
        private static IEnumerator ModifyCallbackCoroutine(SharedTableData.SharedTableEntry entry)
        {
            yield return new EditorWaitForSeconds(2.0f);
            
            EditorCoroutineUtility.StopCoroutine(_runningCoroutine);

            var search = SearchService.Request($"st: tr(en):{entry.Key}", SearchFlags.Synchronous);
            
             if (search.Count == 0 || search[0].data is not TableEntrySearchData tableData) yield break;

            DoModifyCallback(tableData.Collection.SharedData);
        }
#endif 
        private static void ModifyCallback(OnProcess callbackData) =>
            DoModifyCallback(callbackData.Target as SharedTableData);

        public static void GenerateAll()
        {
#if LN_EDITOR_COROUTINES
            EditorCoroutineUtility.StartCoroutineOwnerless(DoGenerateAll());
#endif
        }

        private static IEnumerator DoGenerateAll()
        {
            foreach (var tableCollection in LocalizationEditorSettings.GetStringTableCollections())
            {
                foreach (var stringTable in tableCollection.StringTables)
                {
                    if (stringTable.SharedData is not null) DoModifyCallback(stringTable.SharedData);

                    yield return null;
                }
            }
        }

        private static void DoModifyCallback(SharedTableData sharedTableData) =>
            CodegenUtil.WriteFile(
                sharedTableData.TableCollectionName.Replace(_FILE_REPLACEMENT, string.Empty),
                CodeGenSettings.LocalizationFilePath,
                GenerateFileContent(sharedTableData));


        private static void DeleteCallback(OnProcess callbackData) =>
            CodegenUtil.DeleteFile(callbackData.Path, CodeGenSettings.LocalizationFilePath);
        
        private static bool IsSmart(StringTableCollection tableCollection, long id) =>
            tableCollection != null && tableCollection.StringTables
                .Select(stable => stable.GetEntry(id)).Any(tableEntry => tableEntry.IsSmart);
        
        private static string GenerateFileContent(SharedTableData table)
        {Debug.Log("doing...");
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(table.TableCollectionNameGuid);

            var sb = new StringBuilder();

            foreach (var entry in table.Entries)
            {
                sb.AppendFormat("\t\t\tpublic static string {0}", CodegenUtil.KeyToCSharp(entry.Key));
                var isSmart = IsSmart(tableCollection, entry.Id);
                if (isSmart) sb.Append("(List<object> o)");
                sb.AppendFormat(" => LocalizationSettings.StringDatabase.GetLocalizedString(NAME, {0}", entry.Id);
                if(isSmart) sb.Append(", o");
                sb.Append(");");
                sb.Append(Environment.NewLine);
            }
            
            var withNamespace = string.IsNullOrEmpty(CodeGenSettings.LocalizationNameSpace)
                ? sb.ToString()
                : $"namespace {CodeGenSettings.LocalizationNameSpace} {sb}";
            
            return string.Format(CodeGenSettings.LocalizationTemplate,
                /*{0}*/DateTime.Now,
                /*{1}*/CodegenUtil.KeyToCSharp(tableCollection.TableCollectionName),
                /*{2}*/table.TableCollectionName,
                /*{3}*/withNamespace);
        }
    }
}
#endif