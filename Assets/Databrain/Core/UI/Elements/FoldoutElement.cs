/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using Databrain.Attributes;
using Databrain.Helpers;

using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.UI.Elements
{
    public class FoldoutElement : VisualElement
    {
        private VisualElement header;
        private VisualElement arrow;
        private Label title;
        private VisualElement container;

        private float totalHeight = 0;
        private Texture2D arrowRight;
        private Texture2D arrowDown;

        public Action<bool> onFoldout;

        private bool _value;
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OpenFoldout(_value);
            }
        }

        private List<VisualElement> items = new List<VisualElement>();

        public FoldoutElement(string _title, Action<bool> _onFoldoutCallback, params Action[] _additionalButtons)
        {
            totalHeight = 0;
            arrowRight = DatabrainHelpers.LoadIcon("arrowRight");
            arrowDown = DatabrainHelpers.LoadIcon("arrowDown");
            onFoldout = _onFoldoutCallback;

            style.flexGrow = 1;

            items = new List<VisualElement>();

            header = new VisualElement();

            header.style.marginTop = 5;
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = DatabrainColor.Grey.GetColor();
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = DatabrainColor.DarkGrey.GetColor();

            arrow = new VisualElement();
            arrow.style.backgroundImage = arrowRight;
            arrow.style.width = 24;
            arrow.style.height = 24;

            title = new Label();
            title.text = _title;
            title.style.marginLeft = 10;
            title.style.unityTextAlign = TextAnchor.MiddleLeft;

            container = new VisualElement();
            container.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) };
            container.style.overflow = Overflow.Hidden;
            container.style.display = DisplayStyle.None;
            DatabrainHelpers.SetBorder(container, 1, DatabrainColor.Grey.GetColor());


            
            header.RegisterCallback<ClickEvent>(click => 
            {
                 OpenFoldout(container.style.display == DisplayStyle.None ? true : false);
            });

            header.RegisterCallback<MouseEnterEvent>(enter => 
            {
                header.style.backgroundColor = DatabrainColor.LightGrey.GetColor();
            });

            header.RegisterCallback<MouseLeaveEvent>(enter => 
            {
                header.style.backgroundColor = DatabrainColor.Grey.GetColor();
            });

            header.Add(arrow);
            header.Add(title);

            Add(header);
            Add(container);


        }

        public void SetLabel(string _label)
        {
            title.text = _label;
        }

        public void AddContent(VisualElement _content)
        {
            items.Add(_content);
            this.Add(_content);
            this.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            this.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);

            for (int i = 0; i < items.Count; i++)
            {
                totalHeight += items[i].resolvedStyle.height;
                container.Add(items[i]);
            }
        }


        public void OpenFoldout(bool _open)
        {
            if (_open)
            {
                _value = true;
                container.style.display = DisplayStyle.Flex;
                DatabrainHelpers.SetPadding(container, 5, 5, 5, 5);
                arrow.style.backgroundImage = arrowDown;
            }
            else
            {
                _value = false;
                container.style.display = DisplayStyle.None;
                DatabrainHelpers.SetPadding(container, 0, 0, 0, 0);
                arrow.style.backgroundImage = arrowRight; 
            }

            onFoldout?.Invoke(_open);
        }
    }
}
#endif