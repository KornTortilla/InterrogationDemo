using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;
    using Data.Save;

    public class ProfileNode : BaseNode
    {
        public List<InterrogationEvidenceSaveData> Evidence { get; set; }

        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);

            NodeName = "Profle";

            Text = "Description text.";

            NodeType = NodeType.Profile;

            Evidence = new List<InterrogationEvidenceSaveData>();
        }

        public override void Draw()
        {
            #region Title Containter
            TextField dialogueNameTextField = InterrogationElementUtility.CreateTextField(NodeName, null);

            dialogueNameTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__filename-textfield",
                "interro-node__textfield__hidden"
            );

            titleContainer.Insert(0, dialogueNameTextField);
            #endregion

            #region Main Container
            VisualElement evidenceContainer = new VisualElement();

            Button addEvidenceButton = InterrogationElementUtility.CreateButton("Add Evidence", () =>
            {
                InterrogationEvidenceSaveData newEvidence = new InterrogationEvidenceSaveData()
                {
                    Name = "New Evidence",
                    Text = "Evidence description."
                };

                TextField evidenceNameField = InterrogationElementUtility.CreateTextArea(newEvidence.Name, null, callback =>
                {
                    newEvidence.Name = callback.newValue;
                });

                Foldout evidenceFoldout = InterrogationElementUtility.CreateFoldout("Evidence Text");

                TextField evidenceTextField = InterrogationElementUtility.CreateTextArea(newEvidence.Text, null, callback =>
                {
                    newEvidence.Text = callback.newValue;
                });

                evidenceTextField.AddStyleClasses(
                    "interro-node__textfield",
                    "interro-node__quote-textfield"
                );

                Evidence.Add(newEvidence);

                evidenceContainer.Add(evidenceNameField);

                evidenceFoldout.Add(evidenceTextField);

                evidenceContainer.Add(evidenceFoldout);
            });

            addEvidenceButton.AddToClassList("interro-node__button");

            mainContainer.Insert(1, addEvidenceButton);

            mainContainer.Insert(2, evidenceContainer);
            #endregion

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

        #region Utility
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Debug.Log("bro");

            base.BuildContextualMenu(evt);

            evt.menu.RemoveItemAt(0);

            List<DropdownMenuItem> menus = evt.menu.MenuItems();

            Debug.Log("bro");

            foreach (DropdownMenuItem menu in menus)
            {
                Debug.Log("hi");
                Debug.Log(menu.ToString());
            }
        }
        #endregion
    }
}
