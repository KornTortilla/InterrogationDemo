using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;
    using Data.Save;

    public class EvidenceNode : BaseNode
    {
        public InterrogationEvidenceSaveData Evidence { get; set; }
        public TextElement nameField;
        public TextField descField;

        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);

            Text = "Description text.";

            NodeType = NodeType.Evidence;
        }

        public virtual void InitializeEvidence(InterrogationEvidenceSaveData newEvidence)
        {
            Evidence = newEvidence;

            ID = Evidence.ID;
        }

        public void ChangeText()
        {

        }

        public override void Draw()
        {
            #region Title Containter

            nameField = new TextElement()
            {
                text = NodeName
            };

            nameField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__filename-textfield",
                "interro-node__textfield__hidden"
            );

            titleContainer.Insert(0, nameField);
            #endregion 

            #region Input Container
            Port inputPort = this.CreatePort("Previous Node", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            inputContainer.AddToClassList("interro-node__input-container");
            #endregion

            #region Extensions Container
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("interro-node__custom-data-contianer");

            Foldout textFoldout = InterrogationElementUtility.CreateFoldout("Description Text");

            descField = InterrogationElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            descField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );


            textFoldout.Add(descField);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
            #endregion

            RefreshExpandedState();
        }
    }
}


