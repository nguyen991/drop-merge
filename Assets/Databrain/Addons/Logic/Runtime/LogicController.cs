/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Databrain.Logic
{
    [AddComponentMenu("Databrain.Logic/Logic Controller")]
    public class LogicController : MonoBehaviour, IExposedPropertyTable
    {

        public DataLibrary data;

        #if DATABRAIN_1_3
        [DataObjectDropdown("data", includeSubtypes: true, customHeight = 500)]
        #else
        [DataObjectDropdown("data", includeSubtypes: true)]
        #endif
        public LogicGraph graphAsset;
        private LogicGraph runtimeGraphAsset;

        [OdinHide]
        public List<PropertyName> listOfPropNames = new List<PropertyName>();
        [OdinHide]
        public List<UnityEngine.Object> listUnityEngineObjects = new List<UnityEngine.Object>();

        public bool executeOnStart = true;
        private bool executed;
        private bool dataReady = false;

        public UnityEvent<GraphData.GraphProcessingResult> onCompleteUnityEvent;

        // Editor
        [OdinHide]
        public bool expRefFoldout;
        [OdinHide]
        public bool unityEventsFoldout;
        [OdinHide]
        public GameObject activeGameObject;

        public LogicGraph GetRuntimeGraph()
        {
            return runtimeGraphAsset;
        }

        public void ResetRuntimeGraph()
        {
            data.RemoveDataObjectFromRuntime(runtimeGraphAsset);
            runtimeGraphAsset = null;
        }

        public GraphData ExecuteGraph()
        {
           
            if (graphAsset == null)
                return null;

            if (dataReady)
            {         
                // First check if there's already a graph instance
                if (runtimeGraphAsset == null)
                {
                    // When executing a graph from a logic controller
                    // we have to clone the graph to make it unique for this logic controller instance.
                    runtimeGraphAsset = graphAsset.CloneGraph(this.gameObject) as LogicGraph;
                    runtimeGraphAsset.title = runtimeGraphAsset.title + " : " + this.gameObject.name;
                }
                else
                {
                    StopExecution();
                }

                // Initialize node
                runtimeGraphAsset.InitNodes(this);

                runtimeGraphAsset.onCompleteUnityEvent = onCompleteUnityEvent;
                runtimeGraphAsset.ExecuteGraph(this, this.gameObject, this).OnComplete((x) =>{});
            }
            else
            {
                Debug.LogWarning("Data library is not yet ready to execute this graph: " + data.name + " _ at: " + gameObject.name);
            }

            return runtimeGraphAsset;
        }

        /// <summary>
        /// Skips current node and goes to next node
        /// </summary>
        public void SkipToNextNode(int defaultOutputIndex = 0)
        {
            runtimeGraphAsset.SkipToNextNode(defaultOutputIndex);
        }

        /// <summary>
        /// Executes the initial graph directly in the editor
        /// </summary>
        /// <returns></returns>
        public GraphData ExecuteGraphEditor()
        {
            graphAsset.ExecuteGraphEditor(this, this);
            
            return graphAsset;
        }

        public GraphData OnComplete(System.Action<GraphData.GraphProcessingResult> result)
        {

            runtimeGraphAsset.OnComplete(result);

            return runtimeGraphAsset;
        }

        public GraphData CallGraphEvent(string _eventName)
        {
            if (runtimeGraphAsset == null)
            {
                Debug.LogWarning("Warning please make sure graph is running");
                return null;
            }

            for (int i = 0; i < runtimeGraphAsset.graphEvents.Count; i++)
            {
                if (runtimeGraphAsset.graphEvents[i].eventName == _eventName)
                {
                    for (int n = 0; n < runtimeGraphAsset.nodes.Count; n++)
                    {
                        if (runtimeGraphAsset.nodes[n].derivedClassName == "GraphEventListener")
                        {
                            (runtimeGraphAsset.nodes[n] as GraphEventListener).CallEvent(_eventName);
                        }
                    }
                }
            }
            

            return runtimeGraphAsset;
        }

        public void StopExecution(bool _removeFromRuntimeLibrary = false)
        {
            if (runtimeGraphAsset != null)
            {
                runtimeGraphAsset.isRunning = false;

                if (_removeFromRuntimeLibrary)
                {
                    data.RemoveDataObjectFromRuntime(runtimeGraphAsset);
                }
            }
        }


        public void Awake()
        {
            data.RegisterInitializationCallback(DataReady);
        }

        void Start()
        {
            // if (dataReady && !executed  && executeOnStart)
            // {
            //     executed = true;
            //     ExecuteGraph();
            // }
            // else
            // {
            //     if (!executed && executeOnStart)
            //     {
            //         if (data.IsInitialized)
            //         {
            //             executed = true;
            //             dataReady = true;
            //             ExecuteGraph();
            //         }
            //     }
            // }
        }

        void DataReady()
        {
            dataReady = true;
            if (executeOnStart && !executed && graphAsset != null)
            {
                executed = true;
                ExecuteGraph();
            }
        }


        /// <summary>
        /// Set an object to an exposed reference.
        /// </summary>
        /// <param name="_exposedName"></param>
        /// <param name="_object"></param>
        //public void SetExposedReference<T>(string _exposedName, T _object) where T : UnityEngine.Object
        //{
        //    if (PropertyName.IsNullOrEmpty(_exposedName)) return;

        //    for (int i = 0; i < graphAsset.exposedReferences.Count; i++)
        //    {
        //        if (graphAsset.exposedReferences[i].newName == _exposedName)
        //        {
        //            SetReferenceValue(listOfPropNames[i], _object);
        //        }
        //    }
        //}



        public UnityEngine.Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            idValid = false;

            int index = -1;
            index = listOfPropNames.IndexOf(id);
            if (index > -1)
            {
                idValid = true;
                return listUnityEngineObjects[index];
            }

            return null;
        }

        /// <summary>
        /// Set scene reference object.
        /// </summary>
        /// <param name="_referenceName"></param>
        /// <param name="_value"></param>
        public void SetReferenceValue(string _referenceName, UnityEngine.Object _value)
        {
            var _dataObject = data.GetInitialDataObjectByTitle(_referenceName, typeof(SceneComponent));
           
            (this as IExposedPropertyTable).SetReferenceValue(new PropertyName(_dataObject.guid), _value);
            (_dataObject as SceneComponent).exposedReferences.First().referenceObject.ExposedReference.defaultValue = _value;
        }


        public void SetReferenceValue(PropertyName id, UnityEngine.Object value)
        {
            int index = -1;

            if (PropertyName.IsNullOrEmpty(id)) return;


            index = listOfPropNames.IndexOf(id);
            
            if (index > -1)
            {
                listOfPropNames[index] = id;
                listUnityEngineObjects[index] = value;
            }
            else
            {
                listOfPropNames.Add(id);
                listUnityEngineObjects.Add(value);
            }
        }


        public void ClearReferenceValue(PropertyName id)
        {
            int index = -1;
            index = listOfPropNames.IndexOf(id);

            if (index > -1)
            {
                listOfPropNames.RemoveAt(index);
                listUnityEngineObjects.RemoveAt(index);
            }
        }

        // Show runtime nodes gizmos
        public void OnDrawGizmos()
        {
            if (runtimeGraphAsset != null)
            {

                for (int i = 0; i < runtimeGraphAsset.nodes.Count; i ++)
                {
                    if (runtimeGraphAsset.isRunning)
                    {
                        var _runtimeNode = runtimeGraphAsset.nodes[i]; //graphAsset.GetRuntimeNode(graphAsset.nodes[i]);
                        if (_runtimeNode != null)
                        {
                            _runtimeNode.OnDrawNodeGizmos(this.gameObject);
                        }
                    }   
                }
            }
            else
            {
                if (graphAsset != null)
                {
                    for (int i = 0; i < graphAsset.nodes.Count; i ++)
                    {
                        if (graphAsset.nodes[i] != null)
                        {
                            graphAsset.nodes[i].OnDrawNodeGizmos(this.gameObject);
                        }
                    }
                }
            }
        }
    }
}