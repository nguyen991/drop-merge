using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using DropMerge.Popup;
using GameFoundation.FSM;
using GameFoundation.Popup;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DropMerge.Game
{
    public class GameHUD : MonoBehaviour
    {
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI scoreText;
        public Image nextImage;
        public Button settingsButton;
        public Button leaderBoardButton;
        public Button scoreButton;
        public GameObject guideImages;

        private StateMachine stateMachine;

        [Inject]
        private void Inject(
            GameModel gameModel,
            PlayerModel playerModel,
            StateMachine stateMachine,
            CatAssets catAssets
        )
        {
            this.stateMachine = stateMachine;

            gameModel
                .Score.Subscribe(score =>
                {
                    scoreText.text = score.ToString("N0");
                })
                .AddTo(this);

            gameModel
                .NextCat.Subscribe(id =>
                {
                    var catData = ProjectContext
                        .Instance.Container.Resolve<CatAssets>()
                        .GetCatData(id);
                    nextImage.sprite = catData?.icon;
                })
                .AddTo(this);

            playerModel
                .HighScore.Subscribe(highScore =>
                {
                    highScoreText.text = highScore.ToString("N0");
                })
                .AddTo(this);

            var images = guideImages.GetComponentsInChildren<Image>();
            for (var i = 1; i < images.Length; i++)
            {
                images[i].sprite = catAssets.GetCatData(i - 1)?.icon;
            }
        }

        private void Start()
        {
            settingsButton.onClick.AddListener(() => PopupController.Show(PopupType.Setting));
            scoreButton.onClick.AddListener(() => PopupController.Show(PopupType.LeaderBoard));
            leaderBoardButton.onClick.AddListener(
                () => PopupController.Show(PopupType.LeaderBoard)
            );
        }

        #region DEBUG
        public void Debug_LoseNow()
        {
            stateMachine.ChangeState(GameStates.Lose);
        }
        #endregion
    }
}
