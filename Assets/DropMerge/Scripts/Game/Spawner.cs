using System;
using System.Collections;
using System.Collections.Generic;
using GameFoundation.FSM;
using UnityEngine;
using Zenject;

namespace DropMerge.Game
{
    public class Spawner : MonoBehaviour
    {
        public Transform line;
        public Animator limitLine;

        private CatAssets catAssets;
        private StateMachine stateMachine;

        [Inject]
        private void Inject(CatAssets catAssets, StateMachine stateMachine)
        {
            this.catAssets = catAssets;
            this.stateMachine = stateMachine;
        }

        public int RandomNext()
        {
            return catAssets.RandomCat();
        }

        public Cat SpawnOnTop(int catId)
        {
            var cat = Spawn(
                catId,
                new Vector3(line.position.x, transform.position.y, transform.position.z),
                Vector3.zero
            );
            cat.SetActive(false);
            line.gameObject.SetActive(true);
            return cat;
        }

        public Cat Spawn(int catId, Vector3 position, Vector3 rotation)
        {
            if (catId >= catAssets.MaxCatId)
            {
                return null;
            }

            if (catId < 0)
            {
                catId = catAssets.RandomCat();
            }

            var catData = catAssets.GetCatData(catId);
            if (catData == null)
            {
                return null;
            }

            var catGO = Instantiate(catData.prefab, position, Quaternion.Euler(rotation));
            var cat = catGO.GetComponent<Cat>();
            cat.SetData(catId, catData);
            cat.OnCollider += (cat1, cat2) =>
                stateMachine.OnAction(GameStates.Action_Collider, (cat1, cat2));
            return cat;
        }
    }
}
