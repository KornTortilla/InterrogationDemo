using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;

    public class RepNode : BaseNode
    {
        public TextElement nameField;
        private Port inputPort;

        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);
        }

        public override void Draw()
        {
            #region Title Containter

            nameField = new TextElement()
            {
                text = NodeName
            };

            nameField.AddStyleClasses(
                "interro-node__evidence-text"
            );

            titleContainer.Insert(0, nameField);
            #endregion 

            #region Input Container
            inputPort = this.CreatePort("Previous Node", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            inputContainer.AddToClassList("interro-node__input-container");
            #endregion

            RefreshExpandedState();
        }
    }
}


