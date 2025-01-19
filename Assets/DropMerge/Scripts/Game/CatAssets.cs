using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DropMerge.Game
{
    [System.Serializable]
    public class CatData
    {
        public Sprite icon;
        public GameObject prefab;
        public int score;
        public int randomWeight;
    }

    [CreateAssetMenu(fileName = "Cats", menuName = "DropMerge/Cats")]
    public class CatAssets : ScriptableObject
    {
        public List<CatData> catData;

        public int RandomCat()
        {
            int totalWeight = 0;
            foreach (CatData catData in catData)
            {
                totalWeight += catData.randomWeight;
            }

            int randomWeight = Random.Range(0, totalWeight);
            for (var i = 0; i < catData.Count; i++)
            {
                randomWeight -= catData[i].randomWeight;
                if (randomWeight <= 0)
                {
                    return i;
                }
            }
            return 0;
        }

        public CatData GetCatData(int id)
        {
            if (id < 0 || id >= catData.Count)
            {
                return null;
            }
            return catData[id];
        }

        public int MaxCatId
        {
            get { return catData.Count - 1; }
        }
    }
}
