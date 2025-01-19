using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StorageMenuContext
{
    [MenuItem("Game Foundation/Storage/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
