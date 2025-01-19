using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CandyCoded.HapticFeedback;
using Cysharp.Threading.Tasks;
using GameFoundation.Extensions;
using GameFoundation.FSM;
using GameFoundation.Storage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DropMerge.Game
{
    public class GSPlaying : State
    {
        private readonly GameModel gameModel;
        private readonly Spawner spawner;
        private readonly GameSetting gameSetting;
        private readonly GameAudio gameAudio;
        private readonly PlayerModel playerModel;

        private CancellationTokenSource cancellation;
        private bool touched = false;

        public GSPlaying(
            GameModel gameModel,
            Spawner spawner,
            GameSetting gameSetting,
            GameAudio gameAudio,
            PlayerModel playerModel
        )
        {
            this.gameModel = gameModel;
            this.spawner = spawner;
            this.gameSetting = gameSetting;
            this.gameAudio = gameAudio;
            this.playerModel = playerModel;

            AddAction<(Cat, Cat)>(GameStates.Action_Collider, OnCollider);
            AddAction(GameStates.Action_Retry, Retry);
            AddAction(
                GameStates.Action_SaveGame,
                () => GameStorage.Instance.Save(GameModel.SaveFile, gameModel.GetSerializeData())
            );
        }

        public override void OnEnter(string previousState, object data)
        {
            cancellation = new CancellationTokenSource();
        }

        public override void OnExit(string nextState)
        {
            cancellation?.Dispose();
        }

        public override void OnDestroy()
        {
            cancellation?.Dispose();
        }

        public override void OnUpdate()
        {
            if (gameModel.CurrentCat.Value && !IsPointerOverUIObject())
                Drag();

            // Debug
#if UNITY_EDITOR
            // change state lose if press L
            if (Input.GetKeyDown(KeyCode.L))
            {
                RequestChangeState(GameStates.Lose);
            }
#endif
        }

        private void Drag()
        {
            if (Input.GetMouseButton(0) && gameModel.CurrentCat.Value)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                gameModel.CurrentTransform.position = new Vector3(
                    Mathf.Clamp(mousePosition.x, gameSetting.dragLimit.x, gameSetting.dragLimit.y),
                    gameModel.CurrentTransform.position.y,
                    gameModel.CurrentTransform.position.z
                );
                spawner.line.position = new Vector3(
                    gameModel.CurrentTransform.position.x,
                    spawner.line.position.y,
                    spawner.line.position.z
                );
                touched = true;
            }
            else if (touched)
            {
                touched = false;
                gameModel.Cats.Add(gameModel.CurrentCat.Value);
                gameModel.CurrentCat.Value.SetActive(true);
                gameModel.CurrentCat.Value = null;

                // play audio
                gameAudio.dropSFX.PlaySFX();

                // delay spawn next cat
                UniTask
                    .Delay(TimeSpan.FromSeconds(0.5), cancellationToken: cancellation.Token)
                    .ContinueWith(() =>
                    {
                        // check bounds
                        CheckBounds();

                        // spawn next cat
                        gameModel.CurrentCat.Value = spawner.SpawnOnTop(gameModel.NextCat.Value);
                        gameModel.NextCat.Value = spawner.RandomNext();
                    });
            }
        }

        private bool IsPointerOverUIObject()
        {
            // Check for mouse input
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            // Check for touch input
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnCollider((Cat cat1, Cat cat2) data)
        {
            var (cat1, cat2) = data;
            if (!cat1 || !cat1.IsActive || !cat2 || !cat2.IsActive)
            {
                return;
            }

            // increase score
            gameModel.Score.Value += cat1.score;

            // get center position
            var centerPosition = (cat1.transform.position + cat2.transform.position) / 2;

            // destroy cats
            cat1.SetActive(false);
            cat2.SetActive(false);
            gameModel.Cats.Remove(cat1);
            gameModel.Cats.Remove(cat2);
            GameObject.Destroy(cat1.gameObject);
            GameObject.Destroy(cat2.gameObject);

            // spawn new cat
            var newCat = spawner.Spawn(cat1.catId + 1, centerPosition, Vector3.zero);
            if (newCat)
            {
                gameModel.Cats.Add(newCat);

                // play audio
                gameAudio.mergeSFX.PlaySFX();
            }
            else
            {
                gameAudio.powerSFX.PlaySFX();
            }

            // haptic feedback
            if (playerModel.Vibration.Value)
            {
                HapticFeedback.LightFeedback();
            }
        }

        private void CheckBounds()
        {
            // warning line
            var warningLimit = false;
            foreach (var cat in gameModel.Cats)
            {
                cat.Warning(false);
                if (cat.transform.position.y >= gameSetting.limitYContinue && cat.IsIdle())
                {
                    warningLimit = true;
                }
            }
            spawner.limitLine.SetBool("warning", warningLimit);

            // warning cat
            var outCats = gameModel
                .Cats.Where(cat =>
                    cat.IsActive && cat.transform.position.y >= gameSetting.limitY && cat.IsIdle()
                )
                .ToList();

            if (outCats.Count > 0)
            {
                outCats.ForEach(cat => cat.Warning(true));
                gameModel.WarningCount.Value += 1;
                if (gameModel.WarningCount.Value >= gameSetting.warningCount)
                {
                    RequestChangeState(GameStates.Lose);
                }
                else
                {
                    UniTask
                        .Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellation.Token)
                        .ContinueWith(CheckBounds);
                }
            }
            else
            {
                gameModel.WarningCount.Value = 0;
            }
        }

        private void Retry()
        {
            // replace scene
            SceneManager.LoadScene(Scenes.Game);
        }
    }
}
