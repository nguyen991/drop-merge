using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteMeshConvert
{
    [MenuItem("GameObject/Convert To Sprite Mesh", false)]
    static void UISpriteMeshConvertCommand(MenuCommand menuCommand)
    {
        // The selected GameObject
        GameObject selectedObject = menuCommand.context as GameObject;
        if (selectedObject == null)
        {
            return;
        }

        ConvertToSpriteMesh(selectedObject);
        EditorUtility.SetDirty(selectedObject);
    }

    static void ConvertToSpriteMesh(GameObject selectedObject)
    {
        var images = selectedObject.GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            image.useSpriteMesh = true;
            Debug.Log("Convert To Sprite Mesh: " + image.gameObject.name);
        }
    }
}
