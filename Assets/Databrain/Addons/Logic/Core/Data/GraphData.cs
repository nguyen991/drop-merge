/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

using Databrain.Attributes;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using Databrain.Modules.Import;

namespace Databrain.Logic
{
    [DataObjectAddToRuntimeLibrary]
    [DataObjectIcon("nodes", DatabrainColor.Coal)]
    [DataObjectHideAllFields]
    [HideDataObjectType]
    #if DATABRAIN_1_2 || DATABRAIN_1_3
    [DataObjectFirstClassType("Logic", "logic_icon", DatabrainColor.White)]
    #else
    [DataObjectOrder(300)]
    #endif
    public class GraphData : DataObject
    {

        private IExposedPropertyTable exposedProperties;
        public IExposedPropertyTable ExposedPropertyTable
        {
            get { return exposedProperties;  }
        }

        [Hide]
        public bool isRunning;

        [Hide]
        [SerializeReference]
        public List<NodeData> nodes = new List<NodeData>();

        [Hide]
        [SerializeReference]
        public List<GroupData> groups = new List<GroupData>();

        [Hide]
        [SerializeReference]
        public List<CommentData> comments = new List<CommentData>();


        [System.Serializable]
        public class GraphEventsData
        {
            public string eventName;

            public GraphEventsData() { }
        }

        public List<GraphEventsData> graphEvents = new List<GraphEventsData>();

        public class HighlightingNodes
        {
            public NodeData nodeData;
            internal float addedTime;
            internal bool checkTime;
            internal bool isHighlighted;

            public HighlightingNodes(NodeData _node, float _time)
            {
                nodeData = _node;
                addedTime = _time;
            }
        }
        List<HighlightingNodes> highlightingNodes = new List<HighlightingNodes>();

        public class GraphProcessingResult
        {
            public bool boolValue;
            public float floatValue = -Mathf.Infinity;
            public int intValue = int.MaxValue;
            public string stringValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;

            public GraphProcessingResult() { }
            public GraphProcessingResult(
                bool boolValue = false,
                float floatValue = -Mathf.Infinity,
                int intValue = int.MaxValue, 
                string stringValue = null, 
                Vector2? vector2Value = null, 
                Vector3? vector3Value = null)
            {
                this.boolValue = boolValue;
                this.floatValue = floatValue;
                this.intValue = intValue;
                this.stringValue = stringValue;
                if (vector2Value != null)
                {
                    this.vector2Value = (Vector2)vector2Value;
                }
                if (vector3Value != null)
                {
                    this.vector3Value = (Vector3)vector3Value;
                }
                
            }
        }

        public Action<GraphProcessingResult> onComplete;
        public UnityEvent<GraphProcessingResult> onCompleteUnityEvent;

        public GraphProcessingResult graphResult;
        public GameObject graphOwner;
        public LogicController logicController;
        public bool graphOwnerNotNull;
        public GraphParameters graphParameters;
        public NodeData currentExecutingNode;

        public bool editorExecution;

        // editor 
        public bool showMinimap;

        public void SetResult(GraphProcessingResult _result)
        {
            graphResult = _result;
        }

        /// <summary>
        /// Execute Graph directly
        /// </summary>
        /// <param name="_graphOwner"></param>
        public GraphData ExecuteGraph(GameObject _graphOwner)
        {
            return ExecuteGraph(null, _graphOwner);
        }

        /// <summary>
        /// Execute Graph and pass a custom graph parameter class.
        /// Nodes can then override the ExecuteNode(GraphParameters parameters) method to access the class and its values.
        /// </summary>
        /// <param name="_graphParams"></param>
        public GraphData ExecuteGraph(GraphParameters _graphParameters, GameObject _graphOwner = null)
        {
            if (!isRuntimeInstance)
            {
                (runtimeClone as GraphData).graphParameters = _graphParameters;
            }
            else
            {
                graphParameters = _graphParameters;
            }

            return ExecuteGraph(null, _graphOwner, null);
        }

        /// <summary>
        /// Execute the runtime instance of this graph 
        /// </summary>
        public GraphData ExecuteGraph(IExposedPropertyTable _exposedProperties = null, GameObject _graphOwner = null, LogicController _logicController = null)
        {
            exposedProperties = _exposedProperties;
            graphOwner = _graphOwner;
            logicController = _logicController;
            editorExecution = false;


            if (!isRuntimeInstance)
            {
                return (runtimeClone as GraphData).ExecuteGraph(_exposedProperties, _graphOwner, _logicController);
            }
            else
            {
                if (graphOwner != null)
                {
                    graphOwnerNotNull = true;
                }

                isRunning = true;

                CleanupCheck();
      
                // Execute start nodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].derivedClassName == "OnStart")
                    {
                        nodes[i].ExecuteNode();
                    }

                    if (nodes[i].derivedClassName == "OnStartMultiple")
                    {
                        
                        nodes[i].ExecuteNode();
                       
                    }
                }

                #if UNITY_EDITOR
                if (logicController != null)
                {
                    logicController.StartCoroutine(NodeHighlightCheck());
                }
                #endif
            }

            return this;
        }


        public GraphData ExecuteGraphEditor(IExposedPropertyTable _exposedProperties = null, LogicController _logicController = null)
        {
            isRunning = true;
            editorExecution = true;
            logicController = _logicController;

            for (int i = 0; i < nodes.Count; i ++)
            {
                if (nodes[i].derivedClassName == "OnStart")
                {
                    nodes[i].ExecuteNode();
                }

                if (nodes[i].derivedClassName == "OnStartMultiple")
                {
                    nodes[i].ExecuteNode();
                }
            }

            return this;
        }

        public void SkipToNextNode(int defaultOutputIndex = 0)
        {
            if (currentExecutingNode != null)
            {
                currentExecutingNode.ExecuteNextNode(defaultOutputIndex);
            }
        }


        // Constantly checks if the graphowner has not been destroyed.
        // If it has been destroyed we make sure that the  graph stops and that data object also gets removed from the runtime library
        async void CleanupCheck()
        {
            while (isRunning)
            {
                if (graphOwner == null && graphOwnerNotNull)
                {
                    isRunning = false;
                    relatedLibraryObject.RemoveDataObjectFromRuntime(this);
                }

                await Task.Delay(200);
            }
        }

        public GraphData OnComplete(Action<GraphProcessingResult> result)
        {
            onComplete = result;         
            return this;
        }

        public void CallOnComplete(NodeData node)
        {
            // Check if this graph has a OnComplete or ReturnValue node.
            // If yes, we only allow an on complete event call when last node is an OnComplete or ReturnValue node
            bool _hasOnComplete = false;
            for (int n = 0; n < nodes.Count; n ++)
            {
                if (nodes[n].GetType() == typeof(OnComplete) || nodes[n].GetType() == typeof(ReturnValue))
                {
                    _hasOnComplete = true;
                }
            }

            if (_hasOnComplete && node.GetType() != typeof(OnComplete) && node.GetType() != typeof(ReturnValue))
                return;

            if (graphResult == null)
            {
                graphResult = new GraphProcessingResult();
            }
            

            onComplete?.Invoke(graphResult);
            onCompleteUnityEvent?.Invoke(graphResult);
        }

        /// <summary>
        /// Call a local graph event. Use the EventListenerGraphEvent node
        /// to listen to graph events.
        /// </summary>
        /// <param name="_eventName"></param>
        public GraphData CallGraphEvent(string _eventName)
        {
            if (!isRuntimeInstance)
            {
                if (runtimeClone != null)
                {
                    (runtimeClone as GraphData).CallGraphEvent(_eventName);
                }
            }
            else
            {
                for (int i = 0; i < graphEvents.Count; i++)
                {
                    if (graphEvents[i].eventName == _eventName)
                    {
                        for (int n = 0; n < nodes.Count; n++)
                        {
                            if (nodes[n].derivedClassName == "GraphEventListener")
                            {
                                (nodes[n] as GraphEventListener).CallEvent(_eventName);
                            }
                        }
                    }
                }
            }

            return this;
        }


        public void InitNodes(IExposedPropertyTable _exposedProperties = null)
        {
            exposedProperties = _exposedProperties;

            if (!isRuntimeInstance)
            {
                if (runtimeClone != null)
                {
                    (runtimeClone as GraphData).InitNodes();
                }
            }
            else
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].InitNode();
                }
            }
        }

        /// <summary>
        /// Clone this graph to the runtime library
        /// </summary>
        /// <returns></returns>
        public DataObject CloneGraphToRuntime()
        {
           return relatedLibraryObject.CloneDataObjectToRuntime(this);
        }


        public void OnDestroy()
        {
            isRunning = false;
        }

        public void OnDisable()
        {
            isRunning = false;
        }

#if UNITY_EDITOR

        public override List<ScriptableObjectData> CollectObjects()
        {
            List<ScriptableObjectData> _returnList = new List<ScriptableObjectData>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                    continue;

                _returnList.Add(new ScriptableObjectData(nodes[i], nodes[i].title + " : " + nodes[i].userTitle));
            }

            return _returnList;
        }

        public override void OnDelete()
        {
            // Destroy all node assets
            for (int n = 0; n < nodes.Count; n++)
            {
                DestroyImmediate(nodes[n], true);
            }

            // Destroy all groups
            for (int g = 0; g < groups.Count; g++)
            {
                DestroyImmediate(groups[g], true);
            }
        }

        public override void OnDuplicate(DataObject _duplicateFrom)
        {
            var _original = _duplicateFrom as GraphData;

            nodes = new List<NodeData>();

            for (int i = 0; i < _original.nodes.Count; i++)
            {
                if (_original.nodes[i] == null)
                {
                    _original.nodes.RemoveAt(i);
                    continue;
                }

                var _clonedNode = Instantiate(_original.nodes[i]);

                //Debug.Log("clone: " + _data.title + " _ " + _data.guid);
                _clonedNode.guid = _original.nodes[i].guid; // System.Guid.NewGuid().ToString();
                _clonedNode.derivedClassName = _original.nodes[i].derivedClassName;
                _clonedNode.relatedLibraryObject = _original.relatedLibraryObject;
                _clonedNode.graphData = (_original as GraphData);
                //_clonedNode.connectedNodesOut


                nodes.Add(_clonedNode);

                _clonedNode.hideFlags = HideFlags.HideInHierarchy;
                
                AssetDatabase.AddObjectToAsset(_clonedNode, relatedLibraryObject);
                EditorUtility.SetDirty(_clonedNode);
            }


            // Add clones nodes to the runtime graph data nodes
            groups = new List<GroupData>();

            for (int g = 0; g < _original.groups.Count; g++)
            {
                if (_original.groups[g] == null)
                {
                    _original.groups.RemoveAt(g);
                    continue;
                }

                var _clonedGroup = Instantiate(_original.groups[g]);

                _clonedGroup.guid = _original.groups[g].guid;
                _clonedGroup.derivedClassName = _original.groups[g].derivedClassName;
                _clonedGroup.relatedLibraryObject = _original.relatedLibraryObject;
                _clonedGroup.graphData = (_original as GraphData);


                (_clonedGroup as GroupData).size = groups[g].size;
                (_clonedGroup as GroupData).position = groups[g].position;
                (_clonedGroup as GroupData).color = groups[g].color;
                (_clonedGroup as GroupData).assignedNodes = groups[g].assignedNodes;

                groups.Add(_clonedGroup);

                _clonedGroup.hideFlags = HideFlags.HideInHierarchy;


                AssetDatabase.AddObjectToAsset(_clonedGroup, relatedLibraryObject);
                EditorUtility.SetDirty(_clonedGroup);
            }


            // build outputs
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].connectedNodesOut = new List<NodeData>();
                nodes[i].outputs = new List<string>();

                for (int j = 0; j < _original.nodes.Count; j++)
                {
                    if (nodes[i].guid == _original.nodes[j].guid)
                    {
                        for (int o = 0; o < _original.nodes[j].connectedNodesOut.Count; o++)
                        {
                            nodes[i].connectedNodesOut.Add(null);
                        }

                        for (int l = 0; l < _original.nodes[j].outputs.Count; l++)
                        {
                            nodes[i].outputs.Add(_original.nodes[j].outputs[l]);
                        }
                    }
                }

            }



            // Reconnect nodes with clone nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < _original.nodes.Count; j++)
                {
                    if (nodes[i].guid == _original.nodes[j].guid)
                    {
                        for (int o = 0; o < _original.nodes[j].connectedNodesOut.Count; o++)
                        {
                            for (int k = 0; k < nodes.Count; k++)
                            {
                                if (_original.nodes[j].connectedNodesOut[o] != null)
                                {
                                    if (_original.nodes[j].connectedNodesOut[o].guid == nodes[k].guid)
                                    {
                                        nodes[i].connectedNodesOut[o] = (nodes[k]);
                                    }
                                }
                            }
                        }

                    }
                }
            }


            AssetDatabase.SaveAssets();
        }
#endif

        public override void OnEnd()
        {

            isRunning = false;

            DestroyNodes();

            base.OnEnd();
        }


        void DestroyNodes()
        {
            if (!isRuntimeInstance)
                return;

            if (runtimeClone == null)
            {
                //Debug.Log("runtime clone is null");
            }
            else
            {
                // destroy all runtime nodes
                for (int n = 0; n < (runtimeClone as GraphData).nodes.Count; n++)
                {
                    DestroyImmediate((runtimeClone as GraphData).nodes[n], true);
                }
            }
        }

        public NodeData GetRuntimeNode(NodeData _initialNode)
        {
            if (!this.isRuntimeInstance)
            {
                (runtimeClone as GraphData).GetRuntimeNode(_initialNode);
            }
            else
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == null)
                        continue;
                        
                    if (nodes[i].guid == _initialNode.guid)
                    {
                        return nodes[i];
                    }
                }
            }

            return null;
        }

        public override void CloneToRuntimeLibrary(GameObject _ownerGameObject = null)
        {
            CloneGraph();
        }

        public DataObject CloneGraph(GameObject _owner = null)
        {

            base.CloneToRuntimeLibrary(_owner);

            if ((runtimeClone as GraphData) == null)
            {
                return null;
            }
            // Add clones nodes to the runtime graph data nodes
            (runtimeClone as GraphData).nodes = new List<NodeData>();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                {
                    nodes.RemoveAt(i);
                    continue;
                }

                var _clonedNode = Instantiate(nodes[i]);

                //Debug.Log("clone: " + _data.title + " _ " + _data.guid);
                _clonedNode.guid = nodes[i].guid; // System.Guid.NewGuid().ToString();
                _clonedNode.derivedClassName = nodes[i].derivedClassName;
                _clonedNode.relatedLibraryObject = relatedLibraryObject;
                _clonedNode.graphData = (runtimeClone as GraphData);
                _clonedNode.originalNode = nodes[i];

                (runtimeClone as GraphData).nodes.Add(_clonedNode);

                
            }

            // Add clones nodes to the runtime graph data nodes
            (runtimeClone as GraphData).groups = new List<GroupData>();

            for (int g = 0; g < groups.Count; g++)
            {
                if (groups[g] == null)
                {
                    groups.RemoveAt(g);
                    continue;
                }

                var _clonedGroup = Instantiate(groups[g]);

                _clonedGroup.guid = groups[g].guid;
                _clonedGroup.derivedClassName = groups[g].derivedClassName;
                _clonedGroup.relatedLibraryObject = relatedLibraryObject;
                _clonedGroup.graphData = (runtimeClone as GraphData);


                (_clonedGroup as GroupData).size = groups[g].size;
                (_clonedGroup as GroupData).position = groups[g].position;
                (_clonedGroup as GroupData).color = groups[g].color;

                (_clonedGroup as GroupData).assignedNodes = new List<NodeData>();

                for (int p = 0; p < groups[g].assignedNodes.Count; p++)
                {
                    for (int m = 0; m < (runtimeClone as GraphData).nodes.Count; m++)
                    {
                        if ((runtimeClone as GraphData).nodes[m].originalNode == groups[g].assignedNodes[p])
                        {
                            (_clonedGroup as GroupData).assignedNodes.Add((runtimeClone as GraphData).nodes[m]);
                        }
                    }
                }

                
                (runtimeClone as GraphData).groups.Add(_clonedGroup);
            }


            // build outputs
            for (int i = 0; i < (runtimeClone as GraphData).nodes.Count; i++)
            {
                (runtimeClone as GraphData).nodes[i].connectedNodesOut = new List<NodeData>();
                (runtimeClone as GraphData).nodes[i].outputs = new List<string>();

                for (int j = 0; j < nodes.Count; j++)
                {
                    if (nodes[j] == null)
                    {
                        continue;
                    }
                    if ((runtimeClone as GraphData).nodes[i].guid == nodes[j].guid)
                    {
                        for (int o = 0; o < nodes[j].connectedNodesOut.Count; o++)
                        {
                            (runtimeClone as GraphData).nodes[i].connectedNodesOut.Add(null);
                           
                        }

                        for (int l = 0; l < nodes[j].outputs.Count; l++)
                        {
                            (runtimeClone as GraphData).nodes[i].outputs.Add(nodes[j].outputs[l]);
                        }
                    }
                }

            }



            // Reconnect nodes with clone nodes
            for (int i = 0; i < (runtimeClone as GraphData).nodes.Count; i++)
            {
                if ((runtimeClone as GraphData).nodes[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < nodes.Count; j++)
                {
                    if ((runtimeClone as GraphData).nodes[i].guid == nodes[j].guid)
                    {
                        for ( int o = 0; o < nodes[j].connectedNodesOut.Count; o ++)
                        {
                            for (int k = 0; k < (runtimeClone as GraphData).nodes.Count; k++)
                            {
                                if (nodes[j].connectedNodesOut[o] != null)
                                {
                                    if (nodes[j].connectedNodesOut[o].guid == (runtimeClone as GraphData).nodes[k].guid)
                                    {
                                        (runtimeClone as GraphData).nodes[i].connectedNodesOut[o] = ((runtimeClone as GraphData).nodes[k]);
                                    }
                                }   
                            }
                        }
                       
                    }
                }
            }
            
            return runtimeClone;

        }


        public void AddHighlightingNode(NodeData _nodeToHightlight, bool _oneTime = false)
        {

#if UNITY_EDITOR
            var _exist = false; //highlightingNodes.Where(x => x.nodeData == _nodeToHightlight).Select(x => x);
            
            for (int i = 0; i < highlightingNodes.Count; i ++)
            {
                if (highlightingNodes[i].nodeData.guid == _nodeToHightlight.guid)
                {
                    _exist = true;
                }
            }
            
            if (!_exist)
            {
               
                highlightingNodes.Add(new HighlightingNodes(_nodeToHightlight, Time.time));
                var _result = _nodeToHightlight.SetHighlightOn();
                highlightingNodes[highlightingNodes.Count-1].isHighlighted = _result;

                if (_oneTime)
                {
                    highlightingNodes[highlightingNodes.Count-1].checkTime = true;
                }
                else
                {
                    highlightingNodes[highlightingNodes.Count-1].checkTime = false;
                }
            }
#endif
        }

        public void RemoveHighlightingNode(NodeData _node)
        {
#if UNITY_EDITOR
            for (int i = highlightingNodes.Count-1; i >= 0; i --)
            {
                if (highlightingNodes[i].nodeData.guid == _node.guid)
                {
                    if (Time.time <= highlightingNodes[i].addedTime + 0.2f)
                    {
                        highlightingNodes[i].checkTime = true;
                    }
                    else
                    {
                        highlightingNodes[i].nodeData.SetHighlightOff();
                        highlightingNodes.RemoveAt(i); 
                    }
                }
            }
#endif
        }

#if UNITY_EDITOR

        private IEnumerator NodeHighlightCheck()
        {
            while(isRunning)
            {
                for (int i = highlightingNodes.Count-1; i >= 0; i --)
                {
                    if (highlightingNodes[i].checkTime && Time.time > highlightingNodes[i].addedTime + 0.5f)
                    {
                        highlightingNodes[i].nodeData.SetHighlightOff();
                        highlightingNodes.RemoveAt(i);
                    }
                    else
                    {
                        if (!highlightingNodes[highlightingNodes.Count-1].isHighlighted)
                        {
                            var _result = highlightingNodes[highlightingNodes.Count-1].nodeData.SetHighlightOn();
                            highlightingNodes[highlightingNodes.Count-1].isHighlighted = _result;
                        }
                    }
                }
                yield return null;
            }
        }

        public override VisualElement EditorGUI(SerializedObject _serializedObject, DatabrainEditorWindow _editorWindow)
        {
            var _editor = new NodeEditor(this); //, _editorWindow);
            var _nodeEditor = _editor.GUI(false);

            return _nodeEditor;
        }
#endif
    }
}