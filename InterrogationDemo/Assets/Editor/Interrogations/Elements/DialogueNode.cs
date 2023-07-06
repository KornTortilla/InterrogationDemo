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
    using Data.Save;

    public class DialogueNode : BaseNode
    {
        public List<InterrogationChoiceSaveData> Choices { get; set; }
        public List<InterrogationErrorSaveData> Errors { get; set; }
        public List<DialogueRepNode> DialogueRepNodes { get; set; }

        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);

            Text = "Dialogue text.";

            Choices = new List<InterrogationChoiceSaveData>();

            InterrogationChoiceSaveData defaultChoice = new InterrogationChoiceSaveData()
            {
                Text = "New Choice",
                KeyIDs = new List<string>(),
                Type = OutputType.Choice
            };

            Choices.Add(defaultChoice);

            Errors = new List<InterrogationErrorSaveData>();

            DialogueRepNodes = new List<DialogueRepNode>();

            NodeType = NodeType.Dialogue;
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

            Button addDialogueRepButton = InterrogationElementUtility.CreateButton("+", () =>
            {
                Vector2 pos = new Vector2(graphView.contentViewContainer.contentRect.width / 2, graphView.contentViewContainer.contentRect.width / 4);

                DialogueRepNode dNode = (DialogueRepNode)graphView.CreateNode(graphView.contentViewContainer.WorldToLocal(pos), NodeType.DialogueRep, NodeName);
                dNode.InitializeParent(this);

                graphView.AddElement(dNode);
            });

            titleContainer.Insert(0, dialogueNameTextField);

            titleButtonContainer.Add(addDialogueRepButton);
            #endregion 

            #region Input Container
            Port inputPort = this.CreatePort("Previous Node", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            inputContainer.AddToClassList("interro-node__input-container");
            #endregion

            #region Main Container
            Button addChoiceButton = InterrogationElementUtility.CreateButton("Add Choice", () =>
            {
                InterrogationChoiceSaveData newChoice = new InterrogationChoiceSaveData()
                {
                    Text = "New Choice",
                    KeyIDs = new List<string>(),
                    Type = OutputType.Choice
                };

                Port choicePort = CreateChoice(newChoice);

                Choices.Add(newChoice);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("interro-node__button");

            mainContainer.Insert(1, addChoiceButton);

            Button addLockButton = InterrogationElementUtility.CreateButton("Add Lock", () =>
            {
                InterrogationChoiceSaveData newLock = new InterrogationChoiceSaveData()
                {
                    Text = "New Lock",
                    KeyIDs = new List<string>(),
                    Type = OutputType.Lock
                };

                CreateLock(newLock);

                Choices.Add(newLock);
            });

            addLockButton.AddToClassList("interro-node__button");

            mainContainer.Insert(2, addLockButton);

            VisualElement errorContainer = new VisualElement();

            Button addErrorButton = InterrogationElementUtility.CreateButton("Add Error Response", () =>
            {
                InterrogationErrorSaveData errorData = new InterrogationErrorSaveData()
                {
                    Text = "You were wrong.",
                    EvidenceIDs = new List<string>()
                };

                CreateError(errorData);

                Errors.Add(errorData);
            });

            addErrorButton.AddToClassList("interro-node__button");

            mainContainer.Insert(3, addErrorButton);

            mainContainer.Insert(5, errorContainer);
            #endregion

            #region Output Container
            foreach (InterrogationChoiceSaveData choice in Choices)
            {
                if(choice.Type == OutputType.Choice)
                {
                    Port choicePort = CreateChoice(choice);

                    outputContainer.Add(choicePort);
                }
                else
                {
                    CreateLock(choice);
                }
            }

            outputContainer.AddToClassList("interro-node__output-container");
            #endregion

            #region Extensions Container
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("interro-node__custom-data-contianer");

            Foldout textFoldout = InterrogationElementUtility.CreateFoldout("Dialogue Text");

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

            foreach (InterrogationErrorSaveData error in Errors)
            {
                CreateError(error);
            }
            #endregion

            RefreshExpandedState();
        }

        #region Creation
        private Port CreateChoice(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.name = "Choice";

            choicePort.userData = userData;

            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)userData;

            TextField choiceTextField = InterrogationElementUtility.CreateTextArea(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            Button deleteChoiceButton = InterrogationElementUtility.CreateButton("X", () =>
            {
                if (choicePort.connected) graphView.DeleteElements(choicePort.connections);

                Choices.Remove(choiceData);

                graphView.RemoveElement(choicePort);
            });

            choiceTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__choice-textfield",
                "interro-node__textfield__hidden"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            choicePort.AddToClassList("interro-node__output-port");

            return choicePort;
        }

        private void CreateLock(object userData)
        {
            Port lockPathPort = this.CreatePort();

            lockPathPort.name = "LockPath";

            Port keyPort = this.CreatePort("Key", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);

            keyPort.name = "Key";

            lockPathPort.userData = userData;

            keyPort.userData = userData;

            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)userData;

            TextField lockPathTextField = InterrogationElementUtility.CreateTextArea(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            Button deleteChoiceButton = InterrogationElementUtility.CreateButton("X", () =>
            {
                if (lockPathPort.connected) graphView.DeleteElements(lockPathPort.connections);
                if (keyPort.connected) graphView.DeleteElements(keyPort.connections);

                Choices.Remove(choiceData);

                graphView.RemoveElement(lockPathPort);
                graphView.RemoveElement(keyPort);
            });

            Foldout hintFoldout = InterrogationElementUtility.CreateFoldout("Hint Text");

            TextField hintTextField = InterrogationElementUtility.CreateTextArea(choiceData.Hint, null, callback =>
            {
                choiceData.Hint = callback.newValue;
            });

            hintTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );


            lockPathTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__choice-textfield",
                "interro-node__textfield__hidden"
            );

            lockPathPort.Add(lockPathTextField);
            lockPathPort.Add(deleteChoiceButton);

            hintFoldout.Add(hintTextField);
            keyPort.Add(hintFoldout);

            keyPort.AddToClassList("interro-node__hint-port");

            outputContainer.Add(lockPathPort);
            outputContainer.Add(keyPort);
        }

        private void CreateError(object userData)
        {
            VisualElement newError = new VisualElement();

            Port errorPort = this.CreatePort();
            errorPort.name = "Error";
            errorPort.userData = userData;

            InterrogationErrorSaveData errorData = (InterrogationErrorSaveData)userData;

            Foldout errorFoldout = InterrogationElementUtility.CreateFoldout("Error Response Text");

            TextField errorTextField = InterrogationElementUtility.CreateTextArea(errorData.Text, null, callback =>
            {
                errorData.Text = callback.newValue;
            });

            errorTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            Button deleteErrorButton = InterrogationElementUtility.CreateButton("X", () =>
            {
                if (errorPort.connected) graphView.DeleteElements(errorPort.connections);

                Errors.Remove(errorData);

                extensionContainer.Remove(newError);
            });

            errorFoldout.Add(errorTextField);
            errorPort.Add(deleteErrorButton);

            newError.Add(errorFoldout);
            newError.Add(errorPort);

            extensionContainer.Add(newError);
        }
        #endregion

        #region Utility
        public override void DisconnectAllPorts()
        {
            base.DisconnectAllPorts();

            foreach(VisualElement child in extensionContainer.Children())
            {
                DisconnectPorts(child);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectPorts(inputContainer));
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectPorts(outputContainer));

            base.BuildContextualMenu(evt);
        }
        #endregion
    }
}

