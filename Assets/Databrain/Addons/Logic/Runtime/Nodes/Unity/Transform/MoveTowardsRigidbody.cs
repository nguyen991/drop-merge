using Databrain.Attributes;
using Databrain.Logic.Attributes;
using Databrain.Logic.Utils;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("MoveTowardsRigidbody")]
    [NodeCategory("Unity/Transform")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Smoothly moves the owner object to the target object")]
    public class MoveTowardsRigidbody : NodeData
    {
        //[ExposeToInspector]
        //public float speed;
        [ExposeToInspector]
        public float stopDistance;
        [ExposeToInspector]
        public float smoothTime = 0.3f;

        [DataObjectDropdown(true, sceneComponentType: typeof(Rigidbody))]
        public SceneComponent ownerGameObject;

        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent targetGameObject;

        private float distance;
        private Vector3 velocity = Vector3.zero;

        private Rigidbody _ownerGameObject;
        private GameObject _targetGameObject;

        AsyncHelper asyncHelper;

        public override void ExecuteNode()
        {
            ///////////////////
            if (asyncHelper == null)
            {
                asyncHelper = new AsyncHelper();
            }

            Move();
            CheckDistance();

        }   

        async void CheckDistance()
        {
            if (_ownerGameObject == null)
            {
                _ownerGameObject = ownerGameObject.GetReference<Rigidbody>(this);
            }
            if (_targetGameObject == null)
            {
                _targetGameObject = targetGameObject.GetReference<GameObject>(this);
            }

            do
            {
                distance = Vector3.Distance(_ownerGameObject.transform.position, _targetGameObject.transform.position);

                await asyncHelper.WaitForSeconds(0.2f);

            }while(distance < stopDistance);
        }
        
        async void Move()
        {
            if (_ownerGameObject == null)
            {
                _ownerGameObject = ownerGameObject.GetReference<Rigidbody>(this);
            }
            if (_targetGameObject == null)
            {
                _targetGameObject = targetGameObject.GetReference<GameObject>(this);
            }

            if (_ownerGameObject != null && _targetGameObject != null)
            {
                distance = Vector3.Distance(_ownerGameObject.transform.position, _targetGameObject.transform.position);

                if (distance > stopDistance)
                {

                    do
                    {
                        var _positin = Vector3.SmoothDamp(_ownerGameObject.transform.position, _targetGameObject.transform.position, ref velocity, smoothTime);
                        _ownerGameObject.MovePosition(_positin);

                        await asyncHelper.WaitForFrame();

                    } while (distance > stopDistance);

                }
            }

            ExecuteNextNode(0);
        }
    }
}