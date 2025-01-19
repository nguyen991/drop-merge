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
    public class TriggerExitListener : MonoBehaviour
    {
        public List<OnTriggerExit> collisionTriggers = new List<OnTriggerExit>();

        public void OnTriggerExit(Collider _col)
        {
            for (int i = 0; i < collisionTriggers.Count; i++)
            {
                collisionTriggers[i].Trigger(_col);
            }
        }

        public void OnTriggerExit2D(Collider2D _col)
        {
            for (int i = 0; i < collisionTriggers.Count; i++)
            {
                collisionTriggers[i].Trigger2D(_col);
            }
        }
    }
}
