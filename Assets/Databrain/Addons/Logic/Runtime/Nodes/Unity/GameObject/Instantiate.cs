using Databrain.Attributes;
using Databrain.Blackboard;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Instantiate")]
    [NodeCategory("Unity/GameObject")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("This is a node description")]
    public class Instantiate : NodeData
    {
        public GameObject prefab;

        public bool useGameObjectPosition;

        [EnableBy("useGameObjectPosition")]
        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent spawnPosition;

        public bool useVector3Position;
        [EnableBy("useVector3Position")]
        [DataObjectDropdown(true)]
        public Vector3Variable vector3Position;

        public Vector3 offset;

        public override void ExecuteNode()
        {
            ///////////////////

            if (useGameObjectPosition)
            {
                var _spawnPoint = spawnPosition.GetReference<GameObject>(this);
                Instantiate(prefab, _spawnPoint.transform.position + offset, _spawnPoint.transform.rotation);
            }
            else
            {
                if (vector3Position != null)
                {
                    Instantiate(prefab, vector3Position.Value + offset, Quaternion.identity);
                }
            }

            ExecuteNextNode(0);
        }   
    }
}