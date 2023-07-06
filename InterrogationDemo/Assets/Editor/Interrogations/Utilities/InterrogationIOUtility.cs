using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Utilities
{
    using Windows;
    using Elements;
    using Enumerations;
    using Data;
    using Data.Save;
    using ScriptableObjects;
    

    public class InterrogationIOUtility
    {
        private static InterrogationGraphView graphView;
        private static List<BaseNode> nodes;

        private static string graphFileName;
        private static string containerFolderPath;

        private static Dictionary<string, ScriptableObject> createdSOs;

        private static Dictionary<string, BaseNode> savedNodes;

        private static Dictionary<string, EvidenceContainer> loadedEvidence;
        private static Dictionary<string, BaseNode> loadedNodes;

        public static void Intialize(InterrogationGraphView interroGraphView, string graphName)
        {
            graphView = interroGraphView;
            nodes = new List<BaseNode>();

            graphFileName = graphName;
            containerFolderPath = $"Assets/Resources/InterrogationFiles/{graphFileName}";

            createdSOs = new Dictionary<string, ScriptableObject>();

            savedNodes = new Dictionary<string, BaseNode>();

            loadedEvidence = new Dictionary<string, EvidenceContainer>();
            loadedNodes = new Dictionary<string, BaseNode>();
        }

        #region Save Methods
        public static void Save()
        {
            CreateStaticFolders();

            //Grabs all the nodes and puts them in a list
            GetElementFromGraphView();

            InterrogationGraphSaveDataSO graphData = CreateAsset<InterrogationGraphSaveDataSO>("Assets/Editor/Interrogations/SavedGraphs", graphFileName);

            graphData.Intialize(graphFileName);

            DialogueManagerInterrogationSO dialogueSOManager = CreateAsset<DialogueManagerInterrogationSO>(containerFolderPath, graphFileName);

            dialogueSOManager.Initialize(graphFileName);

            List<string> evidenceNames = new List<string>();
            List<string> dialogueNames = new List<string>();

            SaveProfile(graphData, dialogueSOManager, evidenceNames);
            SaveNodes(graphData, dialogueSOManager, dialogueNames);
            UpdateOldNames(evidenceNames, dialogueNames, graphData);

            SaveAsset(graphData);
            SaveAsset(dialogueSOManager);
        }

        private static void SaveProfile(InterrogationGraphSaveDataSO graphData, DialogueManagerInterrogationSO dialogueSOManager, List<string> evidenceNames)
        {
            ProfileBlackboard profile = graphView.profile;

            graphData.PartnerName = profile.partnerName;
            graphData.IntroText = profile.introText;
            graphData.NoHintResponse = profile.noHintResponse;
            graphData.DefaultErrorResponse = profile.defaultErrorResponse;

            foreach (EvidenceContainer evidenceContainer in profile.EvidenceContainers)
            {
                graphData.Evidence.Add(SaveProfileEvidenceToGraph(evidenceContainer));
                SaveProfileEvidenceToDialogue(evidenceContainer, dialogueSOManager);

                evidenceNames.Add(evidenceContainer.Name);
            }

            dialogueSOManager.PartnerName = profile.partnerName;
            dialogueSOManager.IntroText = profile.introText;
            dialogueSOManager.NoHintResponse = profile.noHintResponse;
            dialogueSOManager.DefaultErrorResponse = profile.defaultErrorResponse;

            SaveAsset(dialogueSOManager);
        }

        private static InterrogationEvidenceSaveData SaveProfileEvidenceToGraph(EvidenceContainer evidenceContainer)
        {
            InterrogationEvidenceSaveData evidenceData = new InterrogationEvidenceSaveData()
            {
                ID = evidenceContainer.ID,
                Name = evidenceContainer.Name,
                Text = evidenceContainer.Text
            };

            return evidenceData;
        }

        private static void SaveProfileEvidenceToDialogue(EvidenceContainer evidenceContainer, DialogueManagerInterrogationSO dialogueSOManager)
        {
            EvidenceInterrogationSO evidenceSO;

            evidenceSO = CreateAsset<EvidenceInterrogationSO>($"{containerFolderPath}/Evidence", evidenceContainer.Name.RemoveWhitespaces());

            dialogueSOManager.EvidenceList.Add(evidenceSO);

            evidenceSO.Initialize(
                evidenceContainer.Name,
                evidenceContainer.Text
            );

            createdSOs.Add(evidenceContainer.ID, evidenceSO);

            SaveAsset(evidenceSO);
        }

        private static void SaveNodes(InterrogationGraphSaveDataSO graphData, DialogueManagerInterrogationSO dialogueSOManager, List<string> dialogueNames)
        {
            foreach (BaseNode node in nodes)
            {
                if(node.NodeType == NodeType.Dialogue)
                {
                    graphData.DialogueNodes.Add(SaveDialogueNodesToGraph((DialogueNode)node));

                    SaveNodesToDialogue(node, dialogueSOManager);

                    dialogueNames.Add(node.NodeName);

                    savedNodes.Add(node.ID, node);
                }
                else
                {
                    graphData.RepNodes.Add(SaveRepNodesToGraph((RepNode)node));

                    savedNodes.Add(node.ID, node);
                }
            }

            UpdateDialogueConnections();
        }

        private static InterrogationDialogueNodeSaveData SaveDialogueNodesToGraph(DialogueNode node)
        {
            List<InterrogationChoiceSaveData> choices = CloneNodeChoices(node.Choices); ;
            List<InterrogationErrorSaveData> errors = CloneNodeErrors(node.Errors);

            InterrogationDialogueNodeSaveData nodeData = new InterrogationDialogueNodeSaveData()
            {
                ID = node.ID,
                Name = node.NodeName,
                Choices = choices,
                Errors = errors,
                Text = node.Text,
                Position = node.GetPosition().position
            };

            return nodeData;
        }

        private static InterrogationRepNodeSaveData SaveRepNodesToGraph(RepNode node)
        {
            string parentID;

            if(node.NodeType == NodeType.DialogueRep)
            {
                DialogueRepNode dRepNode = (DialogueRepNode)node;

                parentID = dRepNode.DialogueNode.ID;
            }
            else
            {
                EvidenceRepNode eRepNode = (EvidenceRepNode)node;

                parentID = eRepNode.EvidenceContainer.ID;
            }

            InterrogationRepNodeSaveData nodeData = new InterrogationRepNodeSaveData()
            {
                ID = node.ID,
                Name = node.NodeName,
                ParentID = parentID,
                Position = node.GetPosition().position,
                NodeType = node.NodeType
            };

            return nodeData;
        }

        private static List<InterrogationChoiceSaveData> CloneNodeChoices(List<InterrogationChoiceSaveData> nodeChoices)
        {
            List<InterrogationChoiceSaveData> choices = new List<InterrogationChoiceSaveData>();

            foreach (InterrogationChoiceSaveData choice in nodeChoices)
            {
                InterrogationChoiceSaveData choiceData = new InterrogationChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID,
                    KeyIDs = choice.KeyIDs,
                    Hint = choice.Hint,
                    Type = choice.Type
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        private static List<InterrogationErrorSaveData> CloneNodeErrors(List<InterrogationErrorSaveData> nodeErrors)
        {
            List<InterrogationErrorSaveData> errors = new List<InterrogationErrorSaveData>();

            foreach (InterrogationErrorSaveData error in nodeErrors)
            {
                InterrogationErrorSaveData errorData = new InterrogationErrorSaveData()
                {
                    Text = error.Text,
                    EvidenceIDs = error.EvidenceIDs
                };

                errors.Add(errorData);
            }

            return errors;
        }

        private static void SaveNodesToDialogue(BaseNode node, DialogueManagerInterrogationSO dialogueSoManager)
        {
            if(node.NodeType == NodeType.Dialogue)
            {
                DialogueInterrogationSO dialogueSO;

                DialogueNode dialogueNode = (DialogueNode)node;

                dialogueSO = CreateAsset<DialogueInterrogationSO>($"{containerFolderPath}/Dialogue", node.NodeName.RemoveWhitespaces());

                dialogueSoManager.DialogueList.Add(dialogueSO);

                dialogueSO.Initialize(
                    node.NodeName,
                    node.Text,
                    ConvertNodeChoices(dialogueNode.Choices),
                    ConvertNodeErrors(dialogueNode.Errors),
                    node.GetPosition().position
                );

                createdSOs.Add(node.ID, dialogueSO);

                SaveAsset(dialogueSO);
            }
        }

        private static List<DialogueChoiceData> ConvertNodeChoices(List<InterrogationChoiceSaveData> nodeChoices)
        {
            List<DialogueChoiceData> dialogueChoices = new List<DialogueChoiceData>();

            foreach (InterrogationChoiceSaveData nodeChoice in nodeChoices)
            {
                DialogueChoiceData choiceData = new DialogueChoiceData()
                {
                    Text = nodeChoice.Text,
                    Keys = new List<ScriptableObject>(),
                    Hint = nodeChoice.Hint
                };

                if (choiceData.Keys.Count == 0) choiceData.Opened = true;

                else choiceData.Opened = false;

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static List<DialogueErrorData> ConvertNodeErrors(List<InterrogationErrorSaveData> nodeErrors)
        {
            List<DialogueErrorData> dialogueErrors = new List<DialogueErrorData>();

            foreach (InterrogationErrorSaveData nodeError in nodeErrors)
            {
                DialogueErrorData errorData = new DialogueErrorData()
                {
                    Text = nodeError.Text,
                    Evidence = new List<ScriptableObject>()
                };

                dialogueErrors.Add(errorData);
            }

            return dialogueErrors;
        }

        private static void UpdateDialogueConnections()
        {
            foreach(BaseNode node in nodes)
            {
               if(node.NodeType == NodeType.Dialogue)
               {
                    DialogueNode dialogueNode = (DialogueNode)node;

                    DialogueInterrogationSO dialogueSO = (DialogueInterrogationSO)createdSOs[dialogueNode.ID];

                    for (int i = 0; i < dialogueNode.Choices.Count; i++)
                    {
                        InterrogationChoiceSaveData nodeChoice = dialogueNode.Choices[i];

                        if (!string.IsNullOrEmpty(nodeChoice.NodeID))
                        {
                            if(createdSOs[nodeChoice.NodeID] is DialogueInterrogationSO nextDialogueData)
                            {
                                dialogueSO.Choices[i].NextDialogue = nextDialogueData;
                            }
                        }

                        if (nodeChoice.KeyIDs != null)
                        {
                            foreach (string ID in nodeChoice.KeyIDs)
                            {
                                if(savedNodes[ID].NodeType ==  NodeType.DialogueRep)
                                {
                                    DialogueRepNode dRepNode = (DialogueRepNode)savedNodes[ID];

                                    dialogueSO.Choices[i].Keys.Add(createdSOs[dRepNode.DialogueNode.ID]);
                                }
                                else if(savedNodes[ID].NodeType == NodeType.EvidenceRep)
                                {
                                    EvidenceRepNode eRepNode = (EvidenceRepNode)savedNodes[ID];

                                    dialogueSO.Choices[i].Keys.Add(createdSOs[eRepNode.EvidenceContainer.ID]);
                                }
                                else
                                {
                                    dialogueSO.Choices[i].Keys.Add(createdSOs[ID]);
                                }
                            }
                        }

                        SaveAsset(dialogueSO);
                    }

                    for (int i = 0; i < dialogueNode.Errors.Count; i++)
                    {
                        InterrogationErrorSaveData errorData = dialogueNode.Errors[i];

                        if (errorData.EvidenceIDs != null)
                        {
                            foreach (string ID in errorData.EvidenceIDs)
                            {
                                if (savedNodes[ID].NodeType == NodeType.DialogueRep)
                                {
                                    DialogueRepNode dRepNode = (DialogueRepNode)savedNodes[ID];

                                    dialogueSO.Errors[i].Evidence.Add(createdSOs[dRepNode.DialogueNode.ID]);
                                }
                                else if(savedNodes[ID].NodeType == NodeType.EvidenceRep)
                                {
                                    EvidenceRepNode eRepNode = (EvidenceRepNode)savedNodes[ID];

                                    dialogueSO.Errors[i].Evidence.Add(createdSOs[eRepNode.EvidenceContainer.ID]);
                                }
                                else
                                {
                                    dialogueSO.Errors[i].Evidence.Add(createdSOs[ID]);
                                }
                            }
                        }

                        SaveAsset(dialogueSO);
                    }

                    Port port = (Port)node.inputContainer.Children().First();

                    if (port.connected)
                    {
                        foreach (Edge edge in port.connections)
                        {
                            if (edge.output.name != "Key")
                            {
                                BaseNode previoudNode = (BaseNode)edge.output.node;

                                dialogueSO.PreviousDialogue = (DialogueInterrogationSO) createdSOs[previoudNode.ID];

                                SaveAsset(dialogueSO);
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateOldNames(List<string> currentEvidenceNames, List<string> currentDialogueNames, InterrogationGraphSaveDataSO graphData)
        {
            if (graphData.OldEvidenceNames != null && graphData.OldEvidenceNames.Count != 0)
            {
                List<string> objectsToRemove = graphData.OldEvidenceNames.Except(currentEvidenceNames).ToList();

                foreach (string objectToRemove in objectsToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Evidence", objectToRemove.RemoveWhitespaces());
                }
            }

            graphData.OldEvidenceNames = new List<string>(currentEvidenceNames);

            if (graphData.OldDialogueNames != null && graphData.OldDialogueNames.Count != 0) {
                List<string> nodesToRemove = graphData.OldDialogueNames.Except(currentDialogueNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Dialogue", nodeToRemove.RemoveWhitespaces());
                }
            }

            graphData.OldDialogueNames = new List<string>(currentDialogueNames);
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        #endregion

        #region Load Methods
        public static void Load()
        {
            InterrogationGraphSaveDataSO graphData = LoadAsset<InterrogationGraphSaveDataSO>("Assets/Editor/Interrogations/SavedGraphs", graphFileName);

            if(graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Couldn't load the file!",
                    "The file at the following path:\n\n" +
                    $"Assets/Scripts/Interrogations/GraphEditor/SavedGraphs/{graphFileName}\n\n" +
                    "Make sure you have the right file name!",
                    "Whatever."
                );

                return;
            }

            InterrogationEditorWindow.UpdateFileName(graphData.FileName);

            graphView.profile.Load(graphData.PartnerName, graphData.IntroText, graphData.NoHintResponse, graphData.DefaultErrorResponse);
            LoadProfileEvidence(graphData.Evidence);
            LoadDialogueNodes(graphData.DialogueNodes);
            LoadRepNodes(graphData.RepNodes);
            LoadNodeConnections();
        }

        private static void LoadProfileEvidence(List<InterrogationEvidenceSaveData> evidence)
        {
            foreach(InterrogationEvidenceSaveData evidenceData in evidence)
            {
                EvidenceContainer evidenceConainter = new EvidenceContainer(graphView, graphView.profile, evidenceData.Name, evidenceData.Text);

                evidenceConainter.ID = evidenceData.ID;

                graphView.profile.EvidenceContainers.Add(evidenceConainter);

                loadedEvidence.Add(evidenceConainter.ID, evidenceConainter);
            }
        }

        private static void LoadDialogueNodes(List<InterrogationDialogueNodeSaveData> nodes)
        {
            foreach(InterrogationDialogueNodeSaveData nodeData in nodes)
            {
                List<InterrogationChoiceSaveData> choices;
                List<InterrogationErrorSaveData> errors;

                choices = CloneNodeChoices(nodeData.Choices);
                errors = CloneNodeErrors(nodeData.Errors);

                BaseNode node = graphView.CreateNode(nodeData.Position, NodeType.Dialogue, nodeData.Name, false);

                node.ID = nodeData.ID;
                node.Text = nodeData.Text;

                DialogueNode dialogueNode = (DialogueNode)node;
                dialogueNode.Choices = choices;
                dialogueNode.Errors = errors;

                node = (BaseNode)dialogueNode;

                node.Draw();

                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);
            }
        }

        private static void LoadRepNodes(List<InterrogationRepNodeSaveData> nodes)
        {
            foreach(InterrogationRepNodeSaveData nodeData in nodes)
            {
                BaseNode node = graphView.CreateNode(nodeData.Position, nodeData.NodeType, nodeData.Name, false);

                node.ID = nodeData.ID;

                if(node.NodeType == NodeType.DialogueRep)
                {
                    DialogueRepNode dRepNode = (DialogueRepNode)node;

                    dRepNode.InitializeParent((DialogueNode)loadedNodes[nodeData.ParentID]);

                    node = (BaseNode)dRepNode;
                }
                else
                {
                    EvidenceRepNode eRepNode = (EvidenceRepNode)node;

                    eRepNode.InitializeParent((EvidenceContainer)loadedEvidence[nodeData.ParentID]);

                    node = (BaseNode)eRepNode;
                }

                node.Draw();

                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);
            }
        }

        private static void LoadNodeConnections()
        {
            foreach(KeyValuePair<string, BaseNode> loadedNode in loadedNodes)
            {
                foreach(Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData) choicePort.userData;

                    if(string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        continue;
                    }

                    if (choicePort.name != "Key")
                    {
                        BaseNode nextNode = loadedNodes[choiceData.NodeID];

                        Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                        Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                        graphView.AddElement(edge);
                    }
                    else
                    {
                        if(choiceData.KeyIDs != null)
                        {
                            for (int i = 0; i < choiceData.KeyIDs.Count; i++)
                            {
                                BaseNode nextNode = loadedNodes[choiceData.KeyIDs[i]];

                                Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                                Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                                graphView.AddElement(edge);
                            }
                        }
                    }
                }

                foreach (VisualElement parent in loadedNode.Value.extensionContainer.Children())
                {
                    foreach(VisualElement element in parent.Children())
                    {
                        if (element is Port)
                        {
                            Port errorPort = (Port)element;

                            InterrogationErrorSaveData errorData = (InterrogationErrorSaveData)errorPort.userData;

                            if (errorData.EvidenceIDs != null)
                            {
                                for (int i = 0; i < errorData.EvidenceIDs.Count; i++)
                                {
                                    BaseNode nextNode = loadedNodes[errorData.EvidenceIDs[i]];

                                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                                    Edge edge = errorPort.ConnectTo(nextNodeInputPort);

                                    graphView.AddElement(edge);
                                }
                            }
                        }
                    }
                }

                loadedNode.Value.RefreshPorts();
            }
        }
        #endregion

        #region Folder Methods
        private static void CreateStaticFolders()
        {
            //Graph Data
            CreateFolder("Assets/Editor/Interrogations", "SavedGraphs");

            //Ingame Data
            CreateFolder("Assets/Resources/InterrogationFiles", graphFileName);

            CreateFolder(containerFolderPath, "Dialogue");
            CreateFolder(containerFolderPath, "Evidence");
        }

        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }

        private static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }
        #endregion

        #region Get Methods
        private static void GetElementFromGraphView()
        {
            graphView.graphElements.ForEach((Action<GraphElement>)(element =>
            {
                if(element is BaseNode node)
                {
                    nodes.Add(node);

                    return;
                }
            }));
        }
        #endregion
    }
}