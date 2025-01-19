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
    [NodeTitle("OnTriggerExit")]
    [NodeCategory("Unity/Physics")]
    [NodeOutputs(new string[] { "Trigger Exit" })]
    [NodeDescription("Listen to a trigger exit event happening on the listener object")]
    [NodeColor(DatabrainColor.Gold)]
    [NodeIcon("collisionExit")]
    public class OnTriggerExit : NodeData
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

        public void Trigger(Collider _collider)
        {
            bool _valid = false;
            if (!graphData.isRunning)
                return;


            if (compareWithLayer && !compareWithTag)
            {
                if (((1 << _collider.gameObject.layer) & layerMask) != 0)
                {
                    _valid = true;
                }
            }
            if (!compareWithLayer && compareWithTag)
            {
                if (_collider.gameObject.tag == tag)
                {
                    _valid = true;
                }
            }
            if (compareWithLayer && compareWithTag)
            {
                if (((1 << _collider.gameObject.layer) & layerMask) != 0 && _collider.gameObject.tag == tag)
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

        public void Trigger2D(Collider2D _collider)
        {
            bool _valid = false;
            if (!graphData.isRunning)
                return;


            if (compareWithLayer && !compareWithTag)
            {
                if (((1 << _collider.gameObject.layer) & layerMask) != 0)
                {
                    _valid = true;
                }
            }
            if (!compareWithLayer && compareWithTag)
            {
                if (_collider.gameObject.tag == tag)
                {
                    _valid = true;
                }
            }
            if (compareWithLayer && compareWithTag)
            {
                if (((1 << _collider.gameObject.layer) & layerMask) != 0 && _collider.gameObject.tag == tag)
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
                var _component = _ls.GetComponent<TriggerExitListener>();

                if (_component != null)
                {
                    _component.collisionTriggers.Add(this);
                }
                else
                {
                    _ls.AddComponent<TriggerExitListener>().collisionTriggers.Add(this);
                }
            }
        }
    }
}