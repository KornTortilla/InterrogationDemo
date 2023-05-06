using UnityEngine;
using UnityEngine.UIElements;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;

    public class EvidenceNode : BaseNode
    {
        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);

            Text = "Description text.";

            NodeType = NodeType.Evidence;
        }

        public override void Draw()
        {
            #region Title Containter
            TextField dialogueNameTextField = InterrogationElementUtility.CreateTextArea(NodeName, null, callback =>
            {
                TextField target = (TextField)callback.target;

                target.value = callback.newValue.RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(NodeName))
                    {
                        graphView.NameErrorCount++;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(NodeName))
                    {
                        graphView.NameErrorCount--;
                    }
                }

                graphView.RemoveNodeDictionary(this);

                NodeName = callback.newValue;

                graphView.AddNodeDictionary(this);
            });

            dialogueNameTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__filename-textfield",
                "interro-node__textfield__hidden"
            );

            titleContainer.Insert(0, dialogueNameTextField);
            #endregion 

            base.Draw();

            #region Extensions Container
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("interro-node__custom-data-contianer");

            Foldout textFoldout = InterrogationElementUtility.CreateFoldout("Description Text");

            TextField textTextField = InterrogationElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            textTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );


            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
            #endregion

            RefreshExpandedState();
        }
    }
}


