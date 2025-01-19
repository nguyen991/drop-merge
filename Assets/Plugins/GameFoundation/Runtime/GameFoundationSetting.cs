using System.Collections;
using System.Collections.Generic;
using GameFoundation.Mobile;
using UnityEngine;

namespace GameFoundation
{
    [CreateAssetMenu(
        fileName = "GameFoundationSetting",
        menuName = "GameFoundation/Setting",
        order = 1
    )]
    public class GameFoundationSetting : ScriptableObject
    {
        [Min(30)]
        public int targetFps = 60;

        [Header("Mobile")]
        public Vector2 designResolution = new Vector2(1080, 1920);

        [Header("Advertise")]
        public AdController.AdConfig admobConfig;

        [Header("Storage")]
        public Storage.GameStorage.DataLayerType storageLayer = Storage
            .GameStorage
            .DataLayerType
            .PlayerPrefs;
    }
}
