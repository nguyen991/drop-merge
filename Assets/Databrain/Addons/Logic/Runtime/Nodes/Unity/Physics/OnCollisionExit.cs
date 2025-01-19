/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;

using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("OnCollisionExit")]
    [NodeCategory("Unity/Physics")]
    [NodeOutputs(new string[] { "Collision Exit" })]
    [NodeDescription("Listen to a on collision exit event happening on the listener object")]
    [NodeColor(DatabrainColor.Gold)]
    [NodeIcon("collisionExit")]
    [NodeSize(210, 150)]
    public class OnCollisionExit : NodeData
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
                var _component = _ls.GetComponent<CollisionExitListener>();

                if (_component != null)
                {
                    _component.collisionTriggers.Add(this);
                }
                else
                {
                    _ls.AddComponent<CollisionExitListener>().collisionTriggers.Add(this);
                }
            }
        }
    }
}