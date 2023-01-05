using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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

        private static Dictionary<string, BaseNode> loadedNodes;

        public static void Intialize(InterrogationGraphView interroGraphView, string graphName)
        {
            graphView = interroGraphView;
            nodes = new List<BaseNode>();

            graphFileName = graphName;
            containerFolderPath = $"Assets/Scripts/Interrogations/Ingame/Files/{graphFileName}";

            createdSOs = new Dictionary<string, ScriptableObject>();

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

            DialogueSOManager dialogueManagerData = CreateAsset<DialogueSOManager>(containerFolderPath, graphFileName);

            dialogueManagerData.Initialize(graphFileName);

            SaveNodes(graphData, dialogueManagerData);

            SaveAsset(graphData);
            SaveAsset(dialogueManagerData);
        }

        private static void SaveNodes(InterrogationGraphSaveDataSO graphData, DialogueSOManager dialogueManagerData)
        {
            List<string> dialogueNodeNames = new List<string>();
            List<string> evidenceNodeNames = new List<string>();

            foreach (BaseNode node in nodes)
            {
                graphData.Nodes.Add(SaveNodesToGraph(node));

                SaveNodesToDialogue(node, dialogueManagerData);

                if(node.NodeType == NodeType.Dialogue) dialogueNodeNames.Add(node.NodeName);
                else evidenceNodeNames.Add(node.NodeName);
            }

            UpdateDialogueConnections();

            UpdateOldNodes(dialogueNodeNames, evidenceNodeNames, graphData);
        }

        private static InterrogationNodeSaveData SaveNodesToGraph(BaseNode node)
        {
            List<InterrogationChoiceSaveData> choices = null;

            if (node.NodeType == NodeType.Dialogue)
            {
                DialogueNode dialogueNode = (DialogueNode) node;

                choices = CloneNodeChoices(dialogueNode.Choices);
            }

            InterrogationNodeSaveData nodeData = new InterrogationNodeSaveData()
            {
                ID = node.ID,
                Name = node.NodeName,
                Choices = choices,
                Text = node.Text,
                Position = node.GetPosition().position,
                Type = node.NodeType
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
                    Type = choice.Type
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        private static void SaveNodesToDialogue(BaseNode node, DialogueSOManager dialogueManagerData)
        {
            if(node.NodeType == NodeType.Dialogue)
            {
                DialogueSO dialogueData;

                DialogueNode dialogueNode = (DialogueNode)node;

                dialogueData = CreateAsset<DialogueSO>($"{containerFolderPath}/Dialogue", node.NodeName.RemoveWhitespaces());

                dialogueManagerData.DialogueList.Add(dialogueData);

                dialogueData.Initialize(
                    node.NodeName,
                    node.Text,
                    ConvertNodeChoices(dialogueNode.Choices),
                    node.GetPosition().position
                );

                createdSOs.Add(node.ID, dialogueData);

                SaveAsset(dialogueData);
            }
            else
            {
                EvidenceSO evidenceData;

                evidenceData = CreateAsset<EvidenceSO>($"{containerFolderPath}/Evidence", node.NodeName.RemoveWhitespaces());

                dialogueManagerData.EvidenceList.Add(evidenceData);

                evidenceData.Initialize(
                    node.NodeName,
                    node.Text
                );

                createdSOs.Add(node.ID, evidenceData);

                SaveAsset(evidenceData);
            }
        }

        private static List<DialogueChoiceData> ConvertNodeChoices(List<InterrogationChoiceSaveData> nodeChoices) 
        {
            List<DialogueChoiceData> dialogueChoices = new List<DialogueChoiceData>();

            foreach(InterrogationChoiceSaveData nodeChoice in nodeChoices)
            {
                DialogueChoiceData choiceData = new DialogueChoiceData()
                {
                    Text = nodeChoice.Text,
                    Keys = new List<ScriptableObject>()
                };

                if (choiceData.Keys.Count == 0) choiceData.Opened = true;

                else choiceData.Opened = false;

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void UpdateDialogueConnections()
        {
            foreach(BaseNode node in nodes)
            {
               if(node.NodeType == NodeType.Dialogue)
               {
                    DialogueNode dialogueNode = (DialogueNode)node;

                    DialogueSO dialogueData = (DialogueSO)createdSOs[dialogueNode.ID];

                    for (int i = 0; i < dialogueNode.Choices.Count; i++)
                    {
                        InterrogationChoiceSaveData nodeChoice = dialogueNode.Choices[i];

                        if (!string.IsNullOrEmpty(nodeChoice.NodeID))
                        {
                            if(createdSOs[nodeChoice.NodeID] is DialogueSO nextDialogueData)
                            {
                                dialogueData.Choices[i].NextDialogue = nextDialogueData;
                            }
                        }

                        if (nodeChoice.KeyIDs != null)
                        {
                            foreach (string ID in nodeChoice.KeyIDs)
                            {
                                dialogueData.Choices[i].Keys.Add(createdSOs[ID]);
                            }
                        }

                        SaveAsset(dialogueData);
                    }

                    Port port = (Port)node.inputContainer.Children().First();

                    if (port.connected)
                    {
                        foreach (Edge edge in port.connections)
                        {
                            if (edge.output.name != "Key")
                            {
                                BaseNode previoudNode = (BaseNode)edge.output.node;

                                dialogueData.PreviousDialogue = (DialogueSO) createdSOs[previoudNode.ID];

                                SaveAsset(dialogueData);
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateOldNodes(List<string> currentDialogueNames, List<string> currentEvidenceNames, InterrogationGraphSaveDataSO graphData)
        {
            if(graphData.OldDialogueNames != null && graphData.OldDialogueNames.Count != 0) {
                List<string> nodesToRemove = graphData.OldDialogueNames.Except(currentDialogueNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Dialogue", nodeToRemove.RemoveWhitespaces());
                }
            }

            graphData.OldDialogueNames = new List<string>(currentDialogueNames);

            if (graphData.OldEvidenceNames != null && graphData.OldEvidenceNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldEvidenceNames.Except(currentEvidenceNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Evidence", nodeToRemove.RemoveWhitespaces());
                }
            }

            graphData.OldEvidenceNames = new List<string>(currentEvidenceNames);
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

            LoadNodes(graphData.Nodes);
            LoadNodeConnections();
        }

        private static void LoadNodes(List<InterrogationNodeSaveData> nodes)
        {
            foreach(InterrogationNodeSaveData nodeData in nodes)
            {
                List<InterrogationChoiceSaveData> choices;
                if (nodeData.Type == NodeType.Dialogue)
                {
                    choices = CloneNodeChoices(nodeData.Choices);
                }
                else
                {
                    choices = null;
                }

                BaseNode node = graphView.CreateNode(nodeData.Position, nodeData.Type, nodeData.Name, false);

                node.ID = nodeData.ID;
                node.Text = nodeData.Text;

                if(nodeData.Type == NodeType.Dialogue)
                {
                    DialogueNode dialogueNode = (DialogueNode)node;
                    dialogueNode.Choices = choices;

                    node = (BaseNode)dialogueNode;
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
            CreateFolder("Assets/Scripts/Interrogations/Ingame", "Files");
            CreateFolder("Assets/Scripts/Interrogations/Ingame/Files", graphFileName);

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