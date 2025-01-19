using System.Collections;
using System.Collections.Generic;
using GameFoundation.Audio;
using GameFoundation.FSM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DropMerge.Popup
{
    public class PopupSetting : MonoBehaviour
    {
        [Header("Toggles Group")]
        public Toggle soundToggle;
        public Toggle audioToggle;
        public Toggle vibrationToggle;

        [Header("Buttons Group")]
        public Button retryButton;

        [Inject]
        public void Inject(PlayerModel playerModel, StateMachine stateMachine)
        {
            // subscribe to player model
            playerModel
                .Audio.Subscribe(audio =>
                {
                    audioToggle.isOn = audio;
                })
                .AddTo(this);
            playerModel
                .Sound.Subscribe(sound =>
                {
                    soundToggle.isOn = sound;
                })
                .AddTo(this);
            playerModel
                .Vibration.Subscribe(vibration => vibrationToggle.isOn = vibration)
                .AddTo(this);

            // add toggle listeners
            audioToggle.onValueChanged.AddListener(value =>
            {
                playerModel.Audio.Value = value;
            });
            soundToggle.onValueChanged.AddListener(value =>
            {
                playerModel.Sound.Value = value;
            });
            vibrationToggle.onValueChanged.AddListener(value =>
                playerModel.Vibration.Value = value
            );

            // add button listeners
            retryButton.onClick.AddListener(() => stateMachine.OnAction(GameStates.Action_Retry));
        }
    }
}
