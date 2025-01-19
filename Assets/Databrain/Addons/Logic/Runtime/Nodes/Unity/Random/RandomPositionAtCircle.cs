using Databrain.Attributes;
using Databrain.Blackboard;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("RandomPositionOnCircle")]
    [NodeCategory("Unity/Random")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Returns a random position on a circle")]
    public class RandomPositionOnCircle : NodeData
    {
        public float radius;

        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent centerPoint;

        [DataObjectDropdown(true)]
        public Vector3Variable positionResult;


        public override void ExecuteNode()
        {

            ///////////////////
            var _result = RandomPointOnUnitCircle(radius);

            positionResult.Value = centerPoint.GetReference<GameObject>(this).transform.position + new Vector3(_result.x, 0, _result.y);

            ExecuteNextNode(0);
        }


        Vector2 RandomPointOnUnitCircle(float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2);
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;

            return new Vector2(x, y);

        }
    }
}