/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using Databrain.Logic.Utils;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("MoveForward")]
    [NodeCategory("Unity/Transform")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Moves an object along its forward axis")]
    public class MoveForward : NodeData
    {
        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent ownerGameObject;

        [InfoBox("-1 for infinite movement")]
        public float moveDuration = 1;
        [ExposeToInspector]
        public float movementSpeed = 10;
        private float startTime;

        AsyncHelper asyncHelper;

        public override void ExecuteNode()
        {
            ///////////////////

            startTime = Time.time;

            if (asyncHelper == null)
            {
                asyncHelper = new AsyncHelper();
            }

            Move();

        }   


        async void Move()
        {
            var _obj = ownerGameObject.GetReference<GameObject>(this);

            if (moveDuration > -1)
            {
                while (Time.time < startTime + moveDuration)
                {
                    _obj.transform.position += _obj.transform.forward * Time.deltaTime * movementSpeed;

                    // await Await.NextUpdate();
                    await asyncHelper.WaitForFrame();
                }
            }
            else
            {
                while (true)
                {
                    _obj.transform.position += _obj.transform.forward * Time.deltaTime * movementSpeed;

                    // await Await.NextUpdate();
                    await asyncHelper.WaitForFrame();
                }
            }


            ExecuteNextNode(0);
        }
    }
}