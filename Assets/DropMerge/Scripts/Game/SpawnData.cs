using Databrain;
using Databrain.Attributes;
using UnityEngine;

namespace DropMerge.Game
{
    public class SpawnData : DataObject
    {
        public int score;
        public int randomWeight;

        [HorizontalLine, ShowAssetPreview]
        public GameObject prefab;
    }
}
