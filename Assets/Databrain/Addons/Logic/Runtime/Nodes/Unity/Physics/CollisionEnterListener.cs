/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Logic
{
    public class CollisionEnterListener : MonoBehaviour
    {
        public List<OnCollisionEnter> collisionTriggers = new List<OnCollisionEnter>();

        public void OnCollisionEnter(Collision _col)
        {
            for (int i = 0; i < collisionTriggers.Count; i++)
            {
                collisionTriggers[i].Collision(_col);
            }
        }
    }
}