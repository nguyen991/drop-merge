/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;
using Databrain.Logic.Manipulators;


namespace Databrain.Logic.Elements
{
    public class GroupVisualElement : VisualElement
    {

        public GraphData graphData;
        public GroupData groupData;
        public NodeEditor nodeEditor;
        public Vector2 targetStartPosition;

        private VisualElement nodeCanvasContainer;
        private NodeCanvasElement nodeCanvas;
        //private Foldout valuesFoldout;
        private ColorField colorField;
        private Button title;
        private VisualElement resizeDrag;

        #if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<GroupVisualElement, UxmlTraits> { }
        #endif
        
        public GroupVisualElement() { }

        public GroupVisualElement(GroupData _groupData, GraphData _graphData, VisualElement _nodeCanvasContainer, NodeCanvasElement _nodeCanvas, NodeEditor _nodeEditor)
        {
            this.name = _groupData.guid;
            groupData = _groupData;
            graphData = _graphData;
            nodeEditor = _nodeEditor;
            nodeCanvasContainer = _nodeCanvasContainer;
            nodeCanvas = _nodeCanvas;

            style.position = Position.Absolute;
            style.flexGrow = 1;
            style.borderRightWidth = 1;
            style.borderLeftWidth = 1;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;

            // Load visual asset of the group
            var _visualAsset = DatabrainHelpers.GetVisualAsset("LogicAssetsPath.cs", "groupVisualAsset.uxml");
            _visualAsset.CloneTree(this);


            resizeDrag = this.Q<VisualElement>("sizeDragger");
            DatabrainHelpers.SetCursor(resizeDrag, MouseCursor.ResizeUpLeft);

            colorField = this.Q<ColorField>("color");
            title = this.Q<Button>("title");
            title.text = groupData.title;

            title.RegisterCallback<ClickEvent>(x =>
            {
                var _popup = new ChangeGroupNamePopup(this, groupData.title);
                ChangeGroupNamePopup.ShowPanel(title.worldBound.position, _popup);

            });

            colorField.RegisterValueChangedCallback(change =>
            {
                if (change.newValue != change.previousValue)
                {
                    var _newColor = new Color(colorField.value.r, colorField.value.g, colorField.value.b, colorField.value.a);
                    groupData.groupColor = _newColor;

                    SetColor(_newColor);
                }

            });

            // Set random group color
            if (groupData.groupColor == new Color(0f, 0f, 0f, 0f))
            {
                var _groupColor = new Color(UnityEngine.Random.Range(70f, 200f) / 255f, UnityEngine.Random.Range(70f, 200f) / 255f, UnityEngine.Random.Range(70f, 200f) / 255f, 255f / 255f);
                SetColor(_groupColor);
            }
            else
            {
                SetColor(groupData.groupColor);
            }
  
            if (_groupData.assignedNodes.Count > 0)
            {

                var _boundRect = CalculateRectToFitAll(_groupData.assignedNodes);

                // Add a bit space around
                _boundRect = new Rect(_boundRect.x - 50, _boundRect.y - 50, _boundRect.width + 100, _boundRect.height + 100);

                if (float.IsNaN(_boundRect.width) || float.IsNaN(_boundRect.height))
                {
                    if (!float.IsNaN(groupData.size.x) || !float.IsNaN(groupData.size.y))
                    {
                        this.style.width = groupData.size.x;
                        this.style.height = groupData.size.y;
                       
                    }

                    this.transform.position = groupData.position;
                }
                else
                {
                    this.style.width = _boundRect.width;
                    this.style.height = _boundRect.height;
                    this.transform.position = new Vector3(_boundRect.position.x, _boundRect.position.y, 0);

                }

                groupData.position = this.transform.position;
                if (!float.IsNaN(_boundRect.width) || !float.IsNaN(_boundRect.height))
                {
                    groupData.size = new Vector2(_boundRect.width, _boundRect.height);
                }
              
            }

            this.AddManipulator(new GroupDragManipulator(this, nodeCanvas, groupData));
            this.AddManipulator(new GroupResizeManipulator(this, resizeDrag));

        }



        Rect CalculateRectToFitAll(List<NodeData> _nodes)
        {
            Rect rectToFit = this.layout;
            bool reachedFirstChild = false;

            _nodes.ForEach(ge =>
            {

                if (!reachedFirstChild)
                {
                    if (ge != null)
                    {
                        if (ge.nodeVisualElement != null)
                        {
                            rectToFit = ge.nodeVisualElement.ChangeCoordinatesTo(nodeCanvasContainer, ge.nodeVisualElement.layout);
                            reachedFirstChild = true;
                        }
                    }
                }
                else
                {
                    if (ge != null)
                    {
                        if (ge.nodeVisualElement != null)
                        {
                            rectToFit = Utils.LogicHelpers.Encompass(rectToFit, ge.nodeVisualElement.ChangeCoordinatesTo(nodeCanvasContainer, ge.nodeVisualElement.layout));
                        }
                    }

                }

            });


            return rectToFit;
        }

        public void RenameGroup(string _newGroupName)
        {
            groupData.title = _newGroupName;
            title = this.Q<Button>("title");
            title.text = _newGroupName;
        }

        public void SetColor(Color groupColor)
        {
            style.backgroundColor = new Color(groupColor.r, groupColor.g, groupColor.b, 10f / 255f);
            style.borderBottomColor = groupColor;
            style.borderTopColor = groupColor;
            style.borderLeftColor = groupColor;
            style.borderRightColor = groupColor;
            style.unityTextOutlineColor = groupColor;
            //resizeDrag.style.backgroundColor = groupColor;
            resizeDrag.style.borderRightColor = groupColor;
            resizeDrag.style.borderBottomColor = groupColor;

            colorField.value = groupColor;
            title.style.color = groupColor;
        }

        Rect GetVector2BoundRect(Vector2[] _points)
        {
            var _minX = _points.Min(p => p.x);
            var _minY = _points.Min(p => p.y);
            var _maxX = _points.Max(p => p.x);
            var _maxY = _points.Max(p => p.y);

            return new Rect((_minX) - 50, (_minY) - 50, (_maxX - _minX) + 100, (_maxY - _minY) + 100);
        }
    }


    public class ChangeGroupNamePopup : PopupWindowContent
    {
        static Vector2 position;
        static string groupName;
        GroupVisualElement group;

        public static void ShowPanel(Vector2 _pos, ChangeGroupNamePopup _panel)
        {
            position = _pos;
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 22);
        }

        public override void OnGUI(Rect rect)
        {
        }

        public override void OnOpen()
        {
            var _root = editorWindow.rootVisualElement;
            _root.style.flexDirection = FlexDirection.Row;

            var _textInput = new TextField();
            _textInput.style.flexGrow = 1;
            _textInput.value = groupName;

            var _renameButton = new Button();
            _renameButton.text = "Rename";
            _renameButton.RegisterCallback<ClickEvent>(click =>
            {
                group.RenameGroup(_textInput.value);
                editorWindow.Close();
            });

            _root.Add(_textInput);
            _root.Add(_renameButton);

        }


        public ChangeGroupNamePopup(GroupVisualElement _group, string _groupName)
        {
            group = _group;
            groupName = _groupName;
        }
    }
}
#endif