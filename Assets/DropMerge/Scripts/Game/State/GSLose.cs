using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DropMerge.Popup;
using GameFoundation.FSM;
using GameFoundation.Mobile;
using GameFoundation.Popup;
using GameFoundation.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DropMerge.Game
{
    public class GSLose : State
    {
        private readonly GameSetting gameSetting;
        private readonly GameModel gameModel;
        private readonly Spawner spawner;
        private readonly PlayerModel playerModel;

        public GSLose(
            GameSetting gameSetting,
            GameModel gameModel,
            PlayerModel playerModel,
            Spawner spawner
        )
        {
            this.gameSetting = gameSetting;
            this.gameModel = gameModel;
            this.spawner = spawner;
            this.playerModel = playerModel;

            AddAction(GameStates.Action_Continue, OnContinue);
            AddAction(GameStates.Action_EndGame, OnEndGame);
            AddAction(GameStates.Action_Retry, Retry);
        }

        public override void OnEnter(string previousState, object data)
        {
            PopupController.Show(PopupType.Lose);
        }

        private async void OnContinue()
        {
            // hide popup
            PopupController.Hide(PopupType.Lose);

            // remove cats
            var removeCats = gameModel
                .Cats.Where(cat => cat.transform.position.y >= gameSetting.limitYContinue)
                .ToList();
            foreach (var cat in removeCats)
            {
                gameModel.Cats.Remove(cat);
                GameObject.Destroy(cat.gameObject);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // change state
            RequestChangeState(GameStates.Playing);
        }

        private void OnEndGame()
        {
            var isHighScore = false;

            // update high score
            if (gameModel.Score.Value > playerModel.HighScore.Value)
            {
                isHighScore = true;
                playerModel.HighScore.Value = gameModel.Score.Value;
            }

            // delete save file
            GameStorage.Instance.Delete(GameModel.SaveFile);

            // hide popup
            PopupController.Hide(PopupType.Lose);

            // disable current cat
            gameModel.CurrentCat.Value.gameObject.SetActive(false);
            spawner.line.gameObject.SetActive(false);

            // show popup
            PopupController.Show(PopupType.GameOver);
        }

        private void Retry()
        {
            // replace scene
            SceneManager.LoadScene(Scenes.Game);
        }
    }
}
