using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Audio;

namespace GameFoundation.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField]
        private bool isPersistent = true;

        [Header("Background")]
        [SerializeField]
        private AudioSource background;

        [Header("SFX")]
        public AudioSource sfx;
        public int sfxPoolSize = 5;

        [Header("Default SFXs")]
        public AudioClip buttonSFX;

        private AudioSource[] sfxPool;

        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (isPersistent)
            {
                if (instance != null && instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            // create sfx pool
            sfxPool = new AudioSource[sfxPoolSize];
            sfxPool[0] = sfx;
            for (int i = 1; i < sfxPoolSize; i++)
            {
                sfxPool[i] = Instantiate(sfx, sfx.transform.parent);
            }
        }

        private void OnDestroy()
        {
            if (isPersistent && instance == this)
            {
                instance = null;
            }
        }

        public void PlayBackground()
        {
            background.Play();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f, bool loop = false)
        {
            var source = sfxPool.Where(source => !source.isPlaying).FirstOrDefault();
            if (source)
            {
                source.clip = clip;
                source.volume = volume;
                source.loop = loop;
                source.Play();
            }
        }

        public void StopSFX(AudioClip clip)
        {
            foreach (var source in sfxPool)
            {
                if (source.isPlaying && source.clip == clip)
                {
                    source.Stop();
                }
            }
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            sfx?.PlayOneShot(clip, volume);
        }

        public void PlayButton()
        {
            PlayOneShot(buttonSFX);
        }

        public void MuteSFX(bool value)
        {
            if (sfxPool == null)
            {
                return;
            }
            foreach (var source in sfxPool)
            {
                source.mute = value;
            }
        }

        public void MuteBackground(bool value)
        {
            background.mute = value;
        }
    }
}
