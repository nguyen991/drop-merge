using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine;
using Databrain.Logic.Utils;

namespace Databrain.Logic
{
    [NodeTitle("MoveTowards")]
    [NodeCategory("Unity/Transform")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Smoothly moves the owner object to the target object")]
    public class MoveTowards : NodeData
    {

        [ExposeToInspector]
        public float stopDistance;
        [ExposeToInspector]
        public float smoothTime = 0.3f;

        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent movingGameObject;
        [InfoBox("If moving object is a rigidbody, set this to true.")]
        public bool isRigidbody;

        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent targetGameObject;

        private float distance;
        private Vector3 velocity = Vector3.zero;

        private Rigidbody _movingRigidbodyGameObject;
        private GameObject _movingGameObject;
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
            if (_movingGameObject == null && movingGameObject != null)
            {
                _movingGameObject = movingGameObject.GetReference<GameObject>(this);
            }
            if (_targetGameObject == null && targetGameObject != null)
            {
                _targetGameObject = targetGameObject.GetReference<GameObject>(this);
            }

            if (_movingGameObject != null && _targetGameObject != null)
            {
                do
                {
                    distance = Vector3.Distance(_movingGameObject.transform.position, _targetGameObject.transform.position);

                    await asyncHelper.WaitForSeconds(0.2f);

                } while (distance > stopDistance && graphData.isRunning);
            }
        }
        
        async void Move()
        {
            if (_movingGameObject == null && movingGameObject != null)
            {
                _movingGameObject = movingGameObject.GetReference<GameObject>(this);
            }
            if (_targetGameObject == null && targetGameObject != null)
            {
                _targetGameObject = targetGameObject.GetReference<GameObject>(this);
            }

            if (_movingRigidbodyGameObject == null && _movingGameObject != null)
            {
                _movingRigidbodyGameObject = _movingGameObject.GetComponent<Rigidbody>();
            }

            if (_movingGameObject != null && _targetGameObject != null)
            {
                distance = Vector3.Distance(_movingGameObject.transform.position, _targetGameObject.transform.position);

                if (distance > stopDistance)
                {
                    if (!isRigidbody)
                    {
                        do
                        {
                            _movingGameObject.transform.position = Vector3.SmoothDamp(_movingGameObject.transform.position, _targetGameObject.transform.position, ref velocity, smoothTime);

                            await asyncHelper.WaitForFrame();


                        } while (distance > stopDistance && graphData.isRunning);
                    }
                    else
                    {
                        do
                        {
                            var _position = Vector3.SmoothDamp(_movingGameObject.transform.position, _targetGameObject.transform.position, ref velocity, smoothTime);
                            _movingRigidbodyGameObject.MovePosition(_position);

                            await asyncHelper.WaitForFrame();

                        } while (distance > stopDistance && graphData.isRunning);
                    }
                }
            }

            ExecuteNextNode(0);
        }
    }
}