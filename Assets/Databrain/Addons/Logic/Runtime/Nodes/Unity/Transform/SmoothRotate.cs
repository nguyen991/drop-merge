using Databrain.Attributes;
using Databrain.Logic.Attributes;
using Databrain.Logic.Utils;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("SmoothRotate")]
    [NodeCategory("Unity/Transform")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Smoothly rotates the owner object to face the target object")]
    public class SmoothRotate : NodeData
    {
        [ExposeToInspector]
        public float rotateSpeed = 10f;
        public float finishTolerance = 0.01f;

        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent rotatingGameObject;
        [InfoBox("If rotating object is a rigidbody, set this to true.")]
        public bool isRigidbody;

        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent targetGameObject;

        private Rigidbody _rotatingRigidbody;
        private GameObject _rotatingGameObject;
        private GameObject _targetGameObject;
        AsyncHelper asyncHelper;

        public override void ExecuteNode()
        {
            ///////////////////
            if (asyncHelper == null)
            {
                asyncHelper = new AsyncHelper();
            }

            Rotate();
        }   

        async void Rotate()
        {
            if (_rotatingGameObject == null && rotatingGameObject != null)
            {
                _rotatingGameObject = rotatingGameObject.GetReference<GameObject>(this);
            }
            if (_targetGameObject == null && targetGameObject != null)
            {
                _targetGameObject = targetGameObject.GetReference<GameObject>(this);
            }

            if (_rotatingRigidbody == null && _rotatingGameObject != null)
            {
                _rotatingRigidbody = _rotatingGameObject.GetComponent<Rigidbody>();
            }

            if (_rotatingGameObject != null && _targetGameObject != null)
            {

                if (!isRigidbody)
                {
                    Quaternion targetRotation = Quaternion.identity;
                    do
                    {
                        Vector3 targetDirection = (_targetGameObject.transform.position - _rotatingGameObject.transform.position).normalized;

                        if (targetDirection != Vector3.zero)
                        {
                            targetRotation = Quaternion.LookRotation(targetDirection);
                            _rotatingGameObject.transform.rotation = Quaternion.RotateTowards(_rotatingGameObject.transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
                        }

                        await asyncHelper.WaitForFrame();

                    } while (_rotatingGameObject != null && Quaternion.Angle(_rotatingGameObject.transform.rotation, targetRotation) > finishTolerance);
                }
                else
                {
                    Quaternion targetRotation = Quaternion.identity;
                    do
                    {
                        Vector3 targetDirection = (_targetGameObject.transform.position - _rotatingGameObject.transform.position).normalized;

                        if (targetDirection != Vector3.zero)
                        {
                            targetRotation = Quaternion.LookRotation(targetDirection);
                            var _rotate = Quaternion.RotateTowards(_rotatingGameObject.transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);


                            _rotatingRigidbody.MoveRotation(_rotate);


                        }

                        await asyncHelper.WaitForFrame();

                    } while (_rotatingGameObject != null && Quaternion.Angle(_rotatingGameObject.transform.rotation, targetRotation) > finishTolerance);

                }
            }
         

            ExecuteNextNode(0);
        }
    }
}