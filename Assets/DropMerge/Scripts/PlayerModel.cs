using System.Collections;
using System.Collections.Generic;
using GameFoundation.Audio;
using GameFoundation.Storage;
using UniRx;

namespace DropMerge
{
    public class PlayerModel
    {
        public ReactiveProperty<int> HighScore { get; private set; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<bool> Audio { get; private set; } =
            new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> Sound { get; private set; } =
            new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> Vibration { get; private set; } =
            new ReactiveProperty<bool>(true);

        public PlayerModel()
        {
            GameStorage.Instance.AutoSave("player", this);
        }

        public void UpdateSetting()
        {
            Audio.Subscribe(
                (value) =>
                {
                    AudioManager.Instance?.MuteBackground(!value);
                }
            );
            Sound.Subscribe(
                (value) =>
                {
                    AudioManager.Instance?.MuteSFX(!value);
                }
            );
        }
    }
}
