using System.Collections;
using System.Collections.Generic;
using GameFoundation.FSM;
using GameFoundation.Storage;
using UnityEngine;

namespace DropMerge.Game
{
    public class GSInit : State
    {
        private readonly Spawner spawner;
        private readonly GameModel gameModel;

        public GSInit(GameModel gameModel, Spawner spawner)
        {
            this.gameModel = gameModel;
            this.spawner = spawner;
        }

        public override void OnEnter(string previousState, object data)
        {
            // load saved game
            var gameSaveData = GameStorage.Instance.Load<GameModel.GameModelData>(
                GameModel.SaveFile
            );
            if (gameSaveData != null)
            {
                gameModel.LoadSerializeData(gameSaveData);

                // spawn first cat
                var cat = spawner.SpawnOnTop(gameSaveData.CurrentCat);
                gameModel.CurrentCat.Value = cat;

                // spawn all cats
                foreach (var catData in gameSaveData.Cats)
                {
                    var newCat = spawner.Spawn(
                        catData.Id,
                        catData.Position.ToVector3(),
                        new Vector3(0, 0, catData.Rotation)
                    );
                    newCat.appearAnimation = false;
                    gameModel.Cats.Add(newCat);
                }
            }
            else
            {
                // spawn first cat
                var cat = spawner.SpawnOnTop(gameModel.NextCat.Value);
                gameModel.CurrentCat.Value = cat;

                // set next cat
                gameModel.NextCat.Value = spawner.RandomNext();
            }

            // change state to playing
            RequestChangeState(GameStates.Playing);
        }
    }
}
