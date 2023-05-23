using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;
    using Data.Save;

    public class EvidenceContainer : GraphElement
    {
        List<EvidenceNode> EvidenceNodes { get; set; }

        public void Initialize(InterrogationGraphView interroGraphView, VisualElement board)
        {
            this.style.width = board.style.width;

            VisualElement textContainer = new VisualElement();
            VisualElement creationContianer = new VisualElement();
            VisualElement movementContainer = new VisualElement();

            EvidenceNodes = new List<EvidenceNode>();

            InterrogationEvidenceSaveData evidenceData = new InterrogationEvidenceSaveData()
            {
                Name = "New Evidence",
                ID = Guid.NewGuid().ToString(),
                Text = "Evidence description."
            };

            TextField evidenceNameField = InterrogationElementUtility.CreateTextArea(evidenceData.Name, null, callback =>
            {
                evidenceData.Name = callback.newValue;

                foreach (EvidenceNode eNode in EvidenceNodes)
                {
                    eNode.nameField.text = callback.newValue;
                }
            });

            evidenceNameField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__filename-textfield",
                "interro-node__textfield__hidden"
            );

            Foldout evidenceFoldout = InterrogationElementUtility.CreateFoldout("Evidence Description");

            TextField evidenceTextField = InterrogationElementUtility.CreateTextArea(evidenceData.Text, null, callback =>
            {
                evidenceData.Text = callback.newValue;

                foreach (EvidenceNode eNode in EvidenceNodes)
                {
                    eNode.descField.value = callback.newValue;
                }
            });

            evidenceTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            Button addEvidenceNodeButton = InterrogationElementUtility.CreateButton("+", () =>
            {
                EvidenceNode eNode = (EvidenceNode)interroGraphView.CreateNode(interroGraphView.contentViewContainer.WorldToLocal(Vector2.zero), NodeType.Evidence, evidenceData.Name);
                eNode.InitializeEvidence(evidenceData);

                EvidenceNodes.Add(eNode);

                interroGraphView.AddElement(eNode);
            });

            Button deleteEvidenceButton = InterrogationElementUtility.CreateButton("X", () =>
            {
                interroGraphView.Evidence.Remove(evidenceData);

                board.Remove(this);
            });

            Button moveUpButton = InterrogationElementUtility.CreateButton("Up", () =>
            {
                int position = board.IndexOf(this);

                if(position > 0)
                {
                    board.Insert(position - 1, this);
                }
            });

            Button moveDownButton = InterrogationElementUtility.CreateButton("Down", () =>
            {
                int position = board.IndexOf(this);

                board.Insert(position + 1, this);
            });

            addEvidenceNodeButton.AddToClassList("interro-node__button");
            deleteEvidenceButton.AddToClassList("interro-node__button");
            moveUpButton.AddToClassList("interro-node__button");
            moveDownButton.AddToClassList("interro-node__button");
            creationContianer.AddToClassList("interro-node__evidence-button");
            movementContainer.AddToClassList("interro-node__evidence-button");
            this.AddToClassList("interro-node__evidence-container");

            interroGraphView.Evidence.Add(evidenceData);

            evidenceFoldout.Add(evidenceTextField);

            textContainer.Add(evidenceNameField);
            textContainer.Add(evidenceFoldout);

            creationContianer.Add(addEvidenceNodeButton);
            creationContianer.Add(deleteEvidenceButton);

            movementContainer.Add(moveUpButton);
            movementContainer.Add(moveDownButton);

            this.Add(textContainer);
            this.Add(creationContianer);
            this.Add(movementContainer);

            board.Add(this);
        }
    }
}