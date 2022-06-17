using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace GitHooksUtility
{
    [InitializeOnLoad]
    public static class GitHooksUtility
    {

        // This works on load
        static GitHooksUtility() {
            GitHooksConfiguration.Setup();
        }

        [MenuItem("Tools/Git Hooks Utility/Select Configuration Asset", priority = 1)]
        static void SelectConfigurationAsset()
        {
            Selection.activeObject = GitHooksConfiguration.Configuration;
        }

    }
}

