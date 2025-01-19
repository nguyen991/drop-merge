using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.DesignerServices;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace DropMerge.Game
{
    public class GameModel
    {
        public static readonly string SaveFile = "game.sav";

        public ReactiveProperty<int> Score { get; private set; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> NextCat { get; private set; } = new ReactiveProperty<int>(-1);

        public ReactiveProperty<Cat> CurrentCat { get; private set; } = new ReactiveProperty<Cat>();

        public ReactiveCollection<Cat> Cats { get; private set; } = new ReactiveCollection<Cat>();

        public ReactiveProperty<int> WarningCount { get; private set; } =
            new ReactiveProperty<int>(0);

        public Transform CurrentTransform
        {
            get => CurrentCat.Value.transform;
        }

        public GameModelData GetSerializeData()
        {
            var cats = new CatData[Cats.Count];
            for (var i = 0; i < Cats.Count; i++)
            {
                cats[i] = new CatData
                {
                    Id = Cats[i].catId,
                    Position = new Vec3(Cats[i].transform.position),
                    Rotation = Cats[i].transform.rotation.eulerAngles.z
                };
            }

            return new GameModelData
            {
                Score = Score.Value,
                NextCat = NextCat.Value,
                CurrentCat = CurrentCat.Value.catId,
                Cats = cats
            };
        }

        public void LoadSerializeData(GameModelData data)
        {
            Score.Value = data.Score;
            NextCat.Value = data.NextCat;
        }

        public class GameModelData
        {
            public int Score;
            public int NextCat;
            public int CurrentCat;
            public CatData[] Cats;
        }

        public struct CatData
        {
            public int Id;
            public Vec3 Position;
            public float Rotation;
        }

        public struct Vec3
        {
            public float x;
            public float y;

            // public float z;

            public Vec3(Vector3 vector)
            {
                x = vector.x;
                y = vector.y;

                // round to 2 decimal places
                x = (float)Math.Round(x, 2);
                y = (float)Math.Round(y, 2);
            }

            public readonly Vector3 ToVector3()
            {
                return new Vector3(x, y, 0);
            }
        }
    }
}
