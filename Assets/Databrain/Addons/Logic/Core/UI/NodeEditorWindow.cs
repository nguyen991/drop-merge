/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Helpers;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;

namespace Databrain.Logic
{
    public class NodeEditorWindow : EditorWindow
    {
        private NodeEditor nodeEditor;
        [SerializeField]
        private static GraphData graphData;

        public void OnEnable()
        {
            this.SetAntiAliasing(4);
        }
        [DidReloadScripts]
        public static void Reload()
        {
            if (HasOpenInstances<NodeEditorWindow>())
            {

                NodeEditorWindow[] _w = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();

                if (_w.Length > 0)
                {
                    if (graphData == null)
                    {

                        // store graph data instance
                        var _graphDataInstance = EditorPrefs.GetInt("Databrain-Logic_GraphDataID");
                        DataObject _graphData = EditorUtility.InstanceIDToObject(_graphDataInstance) as DataObject;

                        _w[0].Setup(_graphData);
                    }
                    else
                    {
                        _w[0].Setup(graphData as DataObject);
                    }
                } 
            }
        }

        public static void Open(DataObject _object)
        {
           
            // store graph data instance
            EditorPrefs.SetInt("Databrain-Logic_GraphDataID", _object.GetInstanceID());

            if (HasOpenInstances<NodeEditorWindow>())
            {
                NodeEditorWindow[] _w = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
                if (_w.Length > 0)
                {
                    _w[0].titleContent = new GUIContent("Logic", DatabrainHelpers.LoadResourceTexture("logic_icon.png"));
                    _w[0].rootVisualElement.Clear();
                    _w[0].Setup(_object);
                    _w[0].Focus();
                }
            }
            else
            {

                var _window = EditorWindow.CreateWindow<NodeEditorWindow>(typeof(NodeEditorWindow));
                _window.titleContent = new GUIContent("Logic", DatabrainHelpers.LoadResourceTexture("logic_icon.png"));
                _window.Setup(_object);
                _window.Focus();
            }


        }

        public void Setup(DataObject _object)
        {
            nodeEditor = new NodeEditor(_object as GraphData);
            graphData = _object as GraphData;

            var _nodeEditor = nodeEditor.GUI(true);
            nodeEditor.openNewWindowButton.SetEnabled(false);

            rootVisualElement.Add(_nodeEditor);
        }

        private void OnDestroy()
        {
            EditorPrefs.SetInt("Databrain-Logic_GraphDataID", -1);
        }
    }
}
#endif