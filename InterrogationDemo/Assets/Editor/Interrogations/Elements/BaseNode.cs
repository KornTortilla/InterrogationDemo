using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;

    public class BaseNode : Node
    {
        public string ID { get; set; }
        public string NodeName { get; set; }
        public string Text { get; set; }
        public NodeType NodeType { get; set; }

        protected InterrogationGraphView graphView;
        private Color defaultBackgroundColor;

        public virtual void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            NodeName = nodeName;

            graphView = interroGraphView;

            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            SetPosition(new Rect(pos, Vector2.zero));

            mainContainer.AddToClassList("interro-node__main-container");
            extensionContainer.AddToClassList("interro-node__extension-container");
        }

        public virtual void Draw()
        {

        }

        #region Utility
        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        public void DisconnectPorts(VisualElement container)
        {
            foreach (VisualElement element in container.Children())
            {
                if(element is Port port)
                {
                    if (!port.connected)
                    {
                        continue;
                    }

                    graphView.DeleteElements(port.connections);

                    continue;
                }

                foreach(Port ports in element.Children())
                {
                    if (!ports.connected)
                    {
                        continue;
                    }

                    graphView.DeleteElements(ports.connections);
                }
            }
        }

        public void SetErrorColor(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetColor()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
        #endregion
    }
}

