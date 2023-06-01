using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Elements
{
    using Windows;
    using Utilities;
    using Data.Save;
    

    public class ProfileBlackboard : Blackboard
    {
        private InterrogationGraphView interroGraphView;

        public string defaultErrorResponse = "You were wrong.";
        private TextField errorTextField;

        public List<EvidenceContainer> EvidenceContainers { get; set; }

        public ProfileBlackboard(InterrogationGraphView interrogationGraphView)
        {
            interroGraphView = interrogationGraphView;

            this.title = "Profile";
            this.subTitle = "";
            this.scrollable = true;

            EvidenceContainers = new List<EvidenceContainer>();

            Foldout errorFoldout = InterrogationElementUtility.CreateFoldout("Default Mistake Response");

            errorTextField = InterrogationElementUtility.CreateTextArea(defaultErrorResponse, null, callback =>
            {
                defaultErrorResponse = callback.newValue;
            });

            errorTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            errorFoldout.Add(errorTextField);

            this.Add(errorFoldout);

            this.addItemRequested = actionEvent => { CreateContainer("New Evidence", "Evidence Description."); };

            interroGraphView.Add(this);
        }

        public void CreateContainer(string newName, string newText)
        {
            EvidenceContainer evidenceConainter = new EvidenceContainer(interroGraphView, this, newName, newText);
            EvidenceContainers.Add(evidenceConainter);
        }

        public new void Clear()
        {
            List<VisualElement> elementsToRemove = new List<VisualElement>();

            for (int i = 0; i < this.contentContainer.childCount; i++)
            {
                VisualElement element = this.contentContainer.ElementAt(i);

                if (element is EvidenceContainer)
                {
                    elementsToRemove.Add(element);
                }
            }

            foreach (VisualElement element in elementsToRemove)
            {
                this.Remove(element);
            }

            EvidenceContainers.Clear();

            defaultErrorResponse = "You were wrong.";
            errorTextField.value = defaultErrorResponse;
        }

        public void Load(string defaultError)
        {
            defaultErrorResponse = defaultError;
            errorTextField.value = defaultErrorResponse;
        }
    }
}
