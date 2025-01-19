/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("OnCollisionEnter")]
    [NodeCategory("Unity/Physics")]
    [NodeOutputs(new string[] {"Collision"})]
    [NodeDescription("Listen to a on collision enter event happening on the listener object")]
    [NodeColor(DatabrainColor.Gold)]
    [NodeIcon("collision")]
    [NodeSize(200, 150)]
    public class OnCollisionEnter : NodeData
    {
        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent listener;

      
        [Title("Compare with", DatabrainColor.White)]
        public bool compareWithLayer = true;
        [EnableBy("compareWithLayer")]
        public LayerMask layerMask;

        public bool compareWithTag;
        [Tag]
        [EnableBy("compareWithTag")]
        public string tag;


        public override void InitNode(){}


        public void Collision(Collision _collision)
        {
            if (!graphData.isRunning)
                return;

            bool _valid = false;

            if (compareWithLayer && !compareWithTag)
            {
                if (((1 << _collision.gameObject.layer) & layerMask) != 0)
                {
                    _valid = true;
                }
            }
            if (!compareWithLayer && compareWithTag)
            {
                if (_collision.gameObject.tag == tag)
                {
                    _valid = true;
                }
            }
            if (compareWithLayer && compareWithTag)
            {
                if (((1 << _collision.gameObject.layer) & layerMask) != 0 && _collision.gameObject.tag == tag)
                {
                    _valid = true;
                }
            }
            if (!compareWithLayer && !compareWithTag)
            {
                _valid = true;
            }

            if (_valid)
            {
                ExecuteNextNode(0);
            }
        }

        public override void ExecuteNode() 
        {
            var _ls = listener.GetReference<GameObject>(this);
            if (_ls != null)
            {
                var _component = _ls.GetComponent<CollisionEnterListener>();

                if (_component != null)
                {
                    _component.collisionTriggers.Add(this);
                }
                else
                {
                    _ls.AddComponent<CollisionEnterListener>().collisionTriggers.Add(this);
                }
            }
        }
    }
}