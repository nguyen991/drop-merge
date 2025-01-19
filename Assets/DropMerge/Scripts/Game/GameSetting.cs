using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DropMerge.Game
{
    [CreateAssetMenu(fileName = "GameSetting", menuName = "DropMerge/GameSetting")]
    public class GameSetting : ScriptableObject
    {
        public int warningCount = 10;
        public float limitY = 10;
        public float limitYContinue = 8;
        public Vector2 dragLimit = new Vector2(-4.5f, 4.5f);
    }
}
