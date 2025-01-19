/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Logic
{
    public class CollisionExitListener : MonoBehaviour
    {
        public List<OnCollisionExit> collisionTriggers = new List<OnCollisionExit>();

        public void OnCollisionExit(Collision _col)
        {
            for (int i = 0; i < collisionTriggers.Count; i++)
            {
                collisionTriggers[i].Collision(_col);
            }
        }
    }
}
