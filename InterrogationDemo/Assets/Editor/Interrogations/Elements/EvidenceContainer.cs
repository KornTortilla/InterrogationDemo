using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;

    public class EvidenceContainer : VisualElement
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public List<EvidenceRepNode> EvidenceRepNodes { get; set; }

        public EvidenceContainer(InterrogationGraphView interroGraphView, ProfileBlackboard board, string newName, string newText)
        {

            ID = Guid.NewGuid().ToString();
            Name = newName;
            Text = newText;
            EvidenceRepNodes = new List<EvidenceRepNode>();

            this.style.width = board.style.width;

            VisualElement textContainer = new VisualElement();
            VisualElement creationContianer = new VisualElement();
            VisualElement movementContainer = new VisualElement();

            TextField evidenceNameField = InterrogationElementUtility.CreateTextArea(Name, null, callback =>
            {
                Name = callback.newValue;

                foreach (EvidenceRepNode eNode in EvidenceRepNodes)
                {
                    eNode.NodeName = callback.newValue;
                    eNode.nameField.text = callback.newValue;
                }
            });

            evidenceNameField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__filename-textfield",
                "interro-node__textfield__hidden"
            );

            Foldout evidenceFoldout = InterrogationElementUtility.CreateFoldout("Evidence Description");

            TextField evidenceTextField = InterrogationElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            evidenceTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            Button addEvidenceNodeButton = InterrogationElementUtility.CreateButton("+", () =>
            {
                Vector2 pos = new Vector2(interroGraphView.contentViewContainer.contentRect.width / 2, interroGraphView.contentViewContainer.contentRect.width / 4);

                EvidenceRepNode eNode = (EvidenceRepNode)interroGraphView.CreateNode(interroGraphView.contentViewContainer.WorldToLocal(pos), NodeType.EvidenceRep, Name);
                eNode.InitializeParent(this);

                interroGraphView.AddElement(eNode);
            });

            Button deleteEvidenceButton = InterrogationElementUtility.CreateButton("X", () =>
            {
                board.EvidenceContainers.Remove(this);

                board.Remove(this);

                foreach(EvidenceRepNode eNode in EvidenceRepNodes)
                {
                    eNode.DisconnectAllPorts();
                    interroGraphView.RemoveElement(eNode);
                }
            });

            Button moveUpButton = InterrogationElementUtility.CreateButton("Up", () =>
            {
                int position = board.IndexOf(this);

                if(position > 4)
                {
                    board.Insert(position - 1, this);

                    int containerPosition = board.EvidenceContainers.IndexOf(this);
                    board.EvidenceContainers.Remove(this);
                    board.EvidenceContainers.Insert(containerPosition - 1, this);

                    int newPosition = board.EvidenceContainers.IndexOf(this);
                    board.subTitle = newPosition.ToString();
                }
            });

            Button moveDownButton = InterrogationElementUtility.CreateButton("Down", () =>
            {
                int position = board.IndexOf(this);

                board.Insert(position + 1, this);

                int containerPosition = board.EvidenceContainers.IndexOf(this);
                board.EvidenceContainers.Remove(this);
                board.EvidenceContainers.Insert(containerPosition + 1, this);
            });

            addEvidenceNodeButton.AddToClassList("interro-node__button");
            deleteEvidenceButton.AddToClassList("interro-node__button");
            moveUpButton.AddToClassList("interro-node__button");
            moveDownButton.AddToClassList("interro-node__button");
            creationContianer.AddToClassList("interro-node__evidence-button");
            movementContainer.AddToClassList("interro-node__evidence-button");
            this.AddToClassList("interro-node__evidence-container");

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

            board.contentContainer.Add(this);
        }
    }
}