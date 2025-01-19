using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DropMerge.Game
{
    [CreateAssetMenu(fileName = "GameAudio", menuName = "DropMerge/GameAudio")]
    public class GameAudio : ScriptableObject
    {
        public AudioClip dropSFX;
        public AudioClip mergeSFX;
        public AudioClip powerSFX;
    }
}
