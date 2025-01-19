using System.Collections;
using System.Collections.Generic;
using GameFoundation.Audio;
using UnityEngine;

namespace GameFoundation.Extensions
{
    public static class AudioClipExt
    {
        public static void PlayOneShot(this AudioClip clip, float volume = 1f)
        {
            AudioManager.Instance?.PlayOneShot(clip, volume);
        }

        public static void PlaySFX(this AudioClip clip, float volume = 1f, bool loop = false)
        {
            AudioManager.Instance?.PlaySFX(clip, volume, loop);
        }

        public static void StopSFX(this AudioClip clip)
        {
            AudioManager.Instance?.StopSFX(clip);
        }
    }
}
