using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

[InitializeOnLoad]
class BoostScene
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold
            };
        }
    }

    static BoostScene()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI()
    {
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_preAudioPlayOn")))
        {
            if (
                !EditorApplication.isPlaying
                && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                // Store the path of the current scene before entering play mode
                EditorPrefs.SetString("scene_path", SceneManager.GetActiveScene().path);

                EditorSceneManager.OpenScene("Assets/DropMerge/Scenes/Boost.unity");
                EditorApplication.isPlaying = true;
            }
        }
        GUILayout.FlexibleSpace();
    }
}

class BoostSceneExt
{
    [InitializeOnLoadMethod]
    static void InitializeOnLoadMethod()
    {
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    static void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Get the path of the scene that was active before entering play mode
            var path = EditorPrefs.GetString("scene_path");
            EditorPrefs.DeleteKey("scene_path");

            // Load the scene that was active before entering play mode
            if (!string.IsNullOrEmpty(path))
            {
                EditorSceneManager.OpenScene(path);
            }
        }
    }
}
