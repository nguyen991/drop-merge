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
    public class TriggerEnterListener : MonoBehaviour
    {
        public List<OnTriggerEnter> collisionTriggers = new List<OnTriggerEnter>();

        public void OnTriggerEnter(Collider _col)
        {
            for (int i = 0; i < collisionTriggers.Count; i++)
            {
                collisionTriggers[i].Trigger(_col);
            }
        }

        public void OnTriggerEnter2D(Collider2D _col) 
        {
            for (int i = 0; i < collisionTriggers.Count; i++)
            {
                collisionTriggers[i].Trigger2D(_col);
            }    
        }
    }
}

