using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DropMerge.Game;
using GameFoundation.Extensions;
using GameFoundation.FSM;
using GameFoundation.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DropMerge.Popup
{
    public class PopupGameOver : MonoBehaviour
    {
        public RawImage captureImage;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI highScoreText;

        public Button shareButton;
        public Button rankingButton;
        public Button retryButton;

        public List<ParticleSystem> particles;

        public AudioClip congratsSFX;

        private GameModel gameModel;
        private PlayerModel playerModel;
        private Camera captureCamera;

        [Inject]
        private void Inject(
            GameModel gameModel,
            PlayerModel playerModel,
            Camera captureCamera,
            StateMachine stateMachine
        )
        {
            this.gameModel = gameModel;
            this.captureCamera = captureCamera;
            this.playerModel = playerModel;

            shareButton.onClick.AddListener(() => Share());
            rankingButton.onClick.AddListener(() => PopupController.Show(PopupType.LeaderBoard));
            retryButton.onClick.AddListener(() => stateMachine.OnAction(GameStates.Action_Retry));
        }

        public async void OnPopupShow()
        {
            // set score
            scoreText.text = "Score: " + gameModel.Score.Value.ToString("N0");
            highScoreText.text = playerModel.HighScore.Value.ToString("N0");

            // wait for end of frame before capturing image
            await UniTask.WaitForEndOfFrame(this);

            // render camera
            captureCamera.enabled = true;
            captureCamera.Render();

            // get the screen position of the GameObject
            var box = captureCamera.GetComponent<BoxCollider2D>();
            var bottomLeft = captureCamera.WorldToScreenPoint(box.bounds.min);
            var topRight = captureCamera.WorldToScreenPoint(box.bounds.max);

            // create capture rectangle from bottom left to top right
            Rect captureRect =
                new(
                    bottomLeft.x,
                    bottomLeft.y,
                    topRight.x - bottomLeft.x,
                    topRight.y - bottomLeft.y
                );

            // capture the camera image
            var texture = new Texture2D(
                (int)captureRect.width,
                (int)captureRect.height,
                TextureFormat.RGB24,
                false
            );
            texture.ReadPixels(captureRect, 0, 0);
            texture.Apply();
            captureCamera.enabled = false;
            captureImage.texture = texture;

            // play particles
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            particles.ForEach(particle => particle.Play());

            // play sfx
            congratsSFX.PlayOneShot();
        }

        private void Share()
        {
#if UNITY_ANDROID || UNITY_IOS
            new NativeShare()
                .SetSubject("Drop Merge")
                .SetText(
                    "I just scored "
                        + gameModel.Score.Value.ToString("N0")
                        + " in Drop Merge! Can you beat my score?"
                )
                .SetUrl("https://play.google.com/store/apps/details?id=com.unity.dropmerge")
                .Share();
#endif
        }
    }
}
