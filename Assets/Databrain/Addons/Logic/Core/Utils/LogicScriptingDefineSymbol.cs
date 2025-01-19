/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Databrain.Logic
{
    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// </summary>
    [InitializeOnLoad]
    public class LogicScriptingDefineSymbol
    {

        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        public static readonly string[] Symbols = new string[] {
         "DATABRAIN_LOGIC",
     };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static LogicScriptingDefineSymbol()
        {
             #if UNITY_2023_2_OR_NEWER
            string definesString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            #else
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            #endif

            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));

            #if UNITY_2023_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), string.Join(";", allDefines.ToArray()));
            #else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
            #endif
        }

    }
}
#endif