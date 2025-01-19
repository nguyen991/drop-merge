using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DropMerge
{
    public class TouchEffect : MonoBehaviour
    {
        public GameObject touchFx;
        public int poolSize = 10;

        private List<ParticleSystem> pool = new();

        private void Start()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject fx = Instantiate(touchFx, transform);
                pool.Add(fx.GetComponent<ParticleSystem>());
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = -1;

                var fx = pool.Find(f => f.isStopped);
                if (fx != null)
                {
                    fx.transform.position = pos;
                    fx.Play();
                }
            }
        }
    }
}
