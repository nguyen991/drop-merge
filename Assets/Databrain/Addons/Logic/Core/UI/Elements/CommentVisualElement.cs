/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Helpers;
using Databrain.Logic.Manipulators;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Logic.Elements
{
    public class CommentVisualElement : VisualElement
    {
        public NodeEditor nodeEditor;
        public CommentData commentData;
        private NodeCanvasElement nodeCanvas;

        #if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<GroupVisualElement, UxmlTraits> { }
        #endif
        
        public CommentVisualElement(Vector2 _position, CommentData _commentData, NodeEditor _nodeEditor, NodeCanvasElement _nodeCanvas)
        {
            nodeEditor = _nodeEditor;
            commentData = _commentData;
            nodeCanvas = _nodeCanvas;

            this.transform.position = _position;


            style.position = Position.Absolute;
            style.flexGrow = 0;
            style.borderRightWidth = 1;
            style.borderLeftWidth = 1;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;
            style.flexWrap = Wrap.Wrap;
            style.backgroundColor = new Color(235f / 255f, 208f / 255f, 145f / 255f);
            style.minWidth = 150;
            style.maxWidth = 250;

            DatabrainHelpers.SetPadding(this, 10, 10, 10, 10);

            RegisterCallback<MouseDownEvent>(evt =>
             {
                 if (evt.clickCount == 2)
                 {
                     var _label = this.Q<Label>("comment");
                     _label.style.display = DisplayStyle.None;

                     var _input = this.Q<TextField>("txtField");
                     _input.style.display = DisplayStyle.Flex;
                     _input.value = _commentData.comment;
                 }
             });

            var _commentText = new Label();
            _commentText.name = "comment";
            _commentText.style.whiteSpace = WhiteSpace.Normal;
            _commentText.style.fontSize = 14;
            _commentText.text = _commentData.comment;
            _commentText.style.color = Color.black;
            _commentText.style.flexWrap = Wrap.Wrap;

            var _txt = new TextField();
            _txt.name = "txtField";
            //_txt.multiline = true;
            _txt.style.display = DisplayStyle.None;
            _txt.isDelayed = true;
            _txt.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                var _label = this.Q<Label>("comment");
                _label.style.display = DisplayStyle.Flex;
                _txt.style.display = DisplayStyle.None;

                _commentText.text = _txt.value;
                _commentData.comment = _txt.value;

            });


            Add(_commentText);
            Add(_txt);

            this.AddManipulator(new CommentDragManipulator(this, nodeCanvas, commentData));
        }
    }
}
#endif