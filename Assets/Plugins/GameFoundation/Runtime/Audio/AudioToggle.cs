using System.Collections;
using System.Collections.Generic;
using GameFoundation.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace GameFoundation.Audio
{
    [RequireComponent(typeof(Toggle))]
    public class AudioToggle : MonoBehaviour
    {
        public AudioClip clip;
        public float volume = 1f;

        private void Start()
        {
            var toggle = GetComponent<Toggle>();
            if (toggle)
            {
                toggle.onValueChanged.AddListener(
                    (value) =>
                    {
                        var _clip = clip ? clip : AudioManager.Instance.buttonSFX;
                        _clip.PlayOneShot(volume);
                    }
                );
            }
        }
    }
}
