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

        public string partnerName = "";
        public string introText = "We're intro-ing.";
        public string noHintResponse = "Sorry, I got nothing.";
        public string defaultErrorResponse = "You were wrong.";

        private TextField partnerNameField;
        private TextField introTextField;
        private TextField noHintTextField;
        private TextField errorTextField;

        public List<EvidenceContainer> EvidenceContainers { get; set; }

        public ProfileBlackboard(InterrogationGraphView interrogationGraphView)
        {
            interroGraphView = interrogationGraphView;

            this.title = "Profile";
            this.subTitle = "";
            this.scrollable = true;

            EvidenceContainers = new List<EvidenceContainer>();

            partnerNameField = InterrogationElementUtility.CreateTextField(partnerName, "Partner Name:", callback =>
            {
                partnerName = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            Foldout introFoldout = InterrogationElementUtility.CreateFoldout("Intro Text");

            introTextField = InterrogationElementUtility.CreateTextArea(introText, null, callback =>
            {
                introText = callback.newValue;
            });

            Foldout noHintFoldout = InterrogationElementUtility.CreateFoldout("No Hint Response");

            noHintTextField = InterrogationElementUtility.CreateTextArea(noHintResponse, null, callback =>
            {
                noHintResponse = callback.newValue;
            });

            Foldout errorFoldout = InterrogationElementUtility.CreateFoldout("Default Mistake Response");

            errorTextField = InterrogationElementUtility.CreateTextArea(defaultErrorResponse, null, callback =>
            {
                defaultErrorResponse = callback.newValue;
            });

            introTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            noHintTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            errorTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            introFoldout.Add(introTextField);
            noHintFoldout.Add(noHintTextField);
            errorFoldout.Add(errorTextField);

            this.Add(partnerNameField);
            this.Add(introFoldout);
            this.Add(noHintFoldout);
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

            partnerName = "";
            partnerNameField.value = partnerName;

            introText = "We're intro-ing.";
            introTextField.value = introText;

            noHintResponse = "Sorry, I got nothing.";
            noHintTextField.value = noHintResponse;

            defaultErrorResponse = "You were wrong.";
            errorTextField.value = defaultErrorResponse;
        }

        public void Load(string pName, string intro, string noHint, string defaultError)
        {
            partnerName = pName;
            partnerNameField.value = partnerName;

            introText = intro;
            introTextField.value = introText;

            noHintResponse = noHint;
            noHintTextField.value = noHintResponse;

            defaultErrorResponse = defaultError;
            errorTextField.value = defaultErrorResponse;
        }
    }
}
