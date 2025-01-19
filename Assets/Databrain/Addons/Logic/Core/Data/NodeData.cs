/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Logic.Attributes;
using System.Threading.Tasks;
using UnityEditor;
using Databrain.Logic.StateMachine;
using Databrain.Attributes;
using Databrain.Modules.Import;



#if UNITY_EDITOR
using Databrain.Logic.Elements;
#endif


namespace Databrain.Logic
{

    [System.Serializable]
    public abstract class NodeData : ScriptableObject
    {
        [NodeHideVariable]
        [OdinHide]
        public string guid = "";
        [NodeHideVariable]
        [OdinHide]
        public string derivedClassName;
        [NodeHideVariable]
        [OdinHide]
        public NodeData originalNode;
        [NodeHideVariable]
        [OdinHide]
        public string title;
        [NodeHideVariable]
        [OdinHide]
        public string userTitle;
        [NodeHideVariable]
        [OdinHide]
        public Vector2 position;
        [NodeHideVariable]
        [OdinHide]
        public Vector2 worldPosition;
        [NodeHideVariable]
        [OdinHide]
        public Color color;
        [NodeHideVariable]
        [OdinHide]
        public string[] borderColors;
        [NodeHideVariable]
        [OdinHide]
        public DataLibrary relatedLibraryObject;
        [NodeHideVariable]
        [OdinHide]
        public GraphData graphData;
        
        internal bool nonDeletable = false;

#if UNITY_EDITOR
        public NodeVisualElement nodeVisualElement;
#endif

        [NodeHideVariable]
        [SerializeReference]
        [OdinHide]
        public List<NodeData> connectedNodesOut = new List<NodeData>();
        [NodeHideVariable]
        [OdinHide]
        public List<string> outputs = new List<string>();
        [NodeHideVariable]
        [OdinHide]
        public bool showValues;
        [NodeHideVariable]
        [OdinHide]
        public bool isConnectable;
        [NodeHideVariable]
        [OdinHide]
        public bool isDeleted = false;
        /// <summary>
        /// By default this is true, if this node is in a group
        /// and the group itself is being stopped by a stop group node, then this will be set to false.
        /// </summary>
        [NodeHideVariable]
        [OdinHide]
        public bool canRun = true;

        #region NodeEditorMethods
#if UNITY_EDITOR
        public virtual void EditorInitalize(){ }
        public virtual void EditorInitalize(NodeEditor _nodeEditor, NodeVisualElement _nodeVisualElement){ }
#endif
        public virtual void SelectNode() { }

        public virtual void DeselectNode() { }

        public bool ConnectToNode(int _outputIndex, NodeData _nodeToConnect)
        {
          
            if (!_nodeToConnect.isConnectable)
                return false;


                NodeData _alreadyConnected = null;
               
                if (connectedNodesOut[_outputIndex] != null)
                {
                    if (connectedNodesOut[_outputIndex].guid == _nodeToConnect.guid)
                    {
                        _alreadyConnected = connectedNodesOut[_outputIndex];
                    }
                }
                

                if (_alreadyConnected != null)
                {
                    Debug.Log("already connected");
                }
                else
                {
                    connectedNodesOut[_outputIndex] = _nodeToConnect;
                }

            return true;
        }

#if UNITY_EDITOR
        
        public void SetColor(Color _color)
        {
            if (nodeVisualElement != null)
            {
                nodeVisualElement.SetColor(_color, null);
            }
        }


        public void AddOutput(string _outputName)
        {  
            nodeVisualElement.AddOutput(_outputName);
        }

        public void RemoveOutput(string _outputName)
        {
            nodeVisualElement.RemoveOutput(_outputName);
        }

        public void RemoveOutput(int _outputIndex)
        {
            nodeVisualElement.RemoveOutput(_outputIndex);
        }

        public void ClearOutputs()
        {
            nodeVisualElement.ClearOutputs();
        }

        public void SetHighlightOff()
        {
            if (nodeVisualElement != null)
            {
                nodeVisualElement.SetHighlightOff();
            }
        }

        public bool SetHighlightOn()
        {
            if (nodeVisualElement != null)
            {
                nodeVisualElement.SetHighlightOn();
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        /// Highlight node one time only (does not require to manually call highlight off)
        /// </summary>
        public void HighlightNode()
        {
#if UNITY_EDITOR
            graphData.AddHighlightingNode(this, true);    
#endif
        }

#if UNITY_EDITOR
        public virtual VisualElement CustomGUI()
        {
            return null;
        }

        public virtual VisualElement CustomNodeGUI()
        {
            return null;
        }
#endif       

        #endregion

        #region NodeRuntimeMethods


        public virtual void InitNode() { }


        public abstract void ExecuteNode();

        public void ExecuteNextNode (int _output)
        {
            NodeData _node = this;
            ExecuteNextNodeInternal(_output, out _node);
        }

        public void ExecuteNextNode (int _output, out NodeData _nextNode)
        {
            ExecuteNextNodeInternal(_output, out _nextNode);
        }

        private void ExecuteNextNodeInternal(int _output, out NodeData _nextNode)
        {
            //if (!Application.isPlaying)
            //    return;

            _nextNode = null;
            // graph owner should not be null
            // this means that the graph owner has been destroyed,
            // so we have to stop this graph from running and removing it from the runtime library
            if (graphData.graphOwner == null && graphData.graphOwnerNotNull)
            {
                graphData.isRunning = false;
                graphData.relatedLibraryObject.RemoveDataObjectFromRuntime(graphData);
            }

            if (!graphData.isRunning)
                return;

            if (_output < connectedNodesOut.Count)
            {
                if (connectedNodesOut[_output] != null)
                {
#if UNITY_EDITOR
                    if (connectedNodesOut[_output].nodeVisualElement != null)
                    {
                        try {
                            // nodeVisualElement.SetHighlightOff();
                            // connectedNodesOut[_output].nodeVisualElement.SetHighlightOn();
                            graphData.AddHighlightingNode(connectedNodesOut[_output]);
                        }
                        catch { }
                    }
#endif
                    _nextNode = connectedNodesOut[_output];
                    ExecuteNextNodeAsync(connectedNodesOut[_output]);
                }
                else
                {
#if UNITY_EDITOR
                    try
                    {
                        // nodeVisualElement.SetHighlightOff();
                        // only if its not a state machine node
                        // Debug.Log(this.GetType().Name);
                        if (this.GetType().BaseType != typeof(StateMachineActionNode))
                    {
                        if (this.GetType() != typeof(StateMachineController))
                        {
                            graphData.RemoveHighlightingNode(this);
                        }
                    }
                    }
                    catch { }
#endif

                    // Call on complete only if this node doesn't have any other connected nodes
                    bool _callOnComplete = true;
                    for (int j = 0; j < connectedNodesOut.Count; j ++)
                    {
                        if (j != _output)
                        {
                            if (connectedNodesOut[j] != null)
                            {
                                _callOnComplete = false;
                            }
                        }
                    }

                    if (_callOnComplete)
                    {
                        // Last node
                        graphData.CallOnComplete(this);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                try
                {
                    // nodeVisualElement.SetHighlightOff();
                    if (this.GetType().BaseType != typeof(StateMachineActionNode))
                    {
                        if (this.GetType() != typeof(StateMachineController))
                        {
                            graphData.RemoveHighlightingNode(this);
                        }
                    }
                }
                catch { }           
#endif

                // Last node
                graphData.CallOnComplete(this);
            }

            return;      
        }

        async void ExecuteNextNodeAsync(NodeData _node)
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                double currentFrame = EditorApplication.timeSinceStartup;
                while (currentFrame >= EditorApplication.timeSinceStartup)
                    await Task.Yield();
#endif
            }
            else
            { 
                // wait for one frame
                int currentFrame = Time.frameCount;
                while (currentFrame >= Time.frameCount)
                    await Task.Yield();
            }

            if (_node.canRun)
            {
                graphData.currentExecutingNode = _node;
                _node.ExecuteNode();
            }
            // else
            // {
#if UNITY_EDITOR
                try
                {
                    // nodeVisualElement.SetHighlightOff();
                    if (this.GetType().BaseType != typeof(StateMachineActionNode))
                    {
                        if (this.GetType() != typeof(StateMachineController))
                        {
                            graphData.RemoveHighlightingNode(this);
                        }
                    }
                }
                catch { }
#endif
            // }
        }

        public virtual void OnDrawNodeGizmos(GameObject _logicController){}

#endregion
    }
}