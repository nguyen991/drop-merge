using System.Collections;
using System.Collections.Generic;
using GameFoundation.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace GameFoundation.Audio
{
    [RequireComponent(typeof(Button))]
    public class AudioButton : MonoBehaviour
    {
        public AudioClip clip;
        public float volume = 1f;

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(() =>
                {
                    var _clip = clip ? clip : AudioManager.Instance.buttonSFX;
                    _clip.PlayOneShot(volume);
                });
            }
        }
    }
}
