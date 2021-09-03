using System.Collections.Generic;
using UnityEditor;

namespace AddressablesMasterDevelopment.Editor
{
    internal static class UseUniTaskAsyncModel
    {
        private const string uniTaskSymbol = "USE_UNITASK";

    
        [MenuItem("Tools/Addressables Master/UniTask/On")]
        internal static void IncludeToSymbols()
        {
            if (DefineActivity())
            {
                EditorUtility.DisplayDialog("UniTask is supported",
                    "UniTask is already used", "OK");
            }
            else
            {
                if (EditorUtility.DisplayDialog("Use UniTask Async Model?",
                    "If UniTask support is enabled then all asynchronous code will use the UniTask async operations", "Yes", "No"))
                {
                    DefineSymbols(true);
                }
            }
        }
        [MenuItem("Tools/Addressables Master/UniTask/Off")]
        internal static void ExcludeFromSymbols()
        {
            if (!DefineActivity())
            {
                EditorUtility.DisplayDialog("UniTask support is already turned off",
                    "The .NET default async model is used", "OK");
            }
            else
            {
                if (EditorUtility.DisplayDialog("Disable UniTask support?",
                    "This action will cause the use of the standard .NET async model", "Yes", "No"))
                {
                    DefineSymbols(false);
                }
            }
        }

        private static bool DefineActivity()
        {
            BuildTargetGroup currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
        
            List<string> scriptingSymbols = 
                new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Split(';'));

            return scriptingSymbols.Contains(uniTaskSymbol);
        }
        
        private static void DefineSymbols(bool state)
        {
            BuildTargetGroup currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
        
            List<string> scriptingSymbols = 
                new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Split(';'));
        
            if (state)
            {
                if (!scriptingSymbols.Contains(uniTaskSymbol)) scriptingSymbols.Add(uniTaskSymbol);
                else return;
            }
            else
            {
                if (scriptingSymbols.Contains(uniTaskSymbol)) scriptingSymbols.Remove(uniTaskSymbol);
                else return;
            }

            string finalScriptingSymbols = string.Join(";", scriptingSymbols.ToArray()); 
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, finalScriptingSymbols);
        }
    }
}