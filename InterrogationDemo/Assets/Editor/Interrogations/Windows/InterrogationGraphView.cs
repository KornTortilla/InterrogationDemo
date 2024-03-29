using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Interrogation.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;
    using Data.Error;
    using Data.Save;

    public class InterrogationGraphView : GraphView
    {
        public InterrogationEditorWindow editorWindow;

        private MiniMap miniMap;

        public ProfileBlackboard profile;

        private readonly SerializableDictionary<string, InterrogationNodeErrorData> nodeDictionary;

        private int nameErrorCount;

        public int NameErrorCount
        {
            get { return nameErrorCount; }
            set 
            {
                nameErrorCount = value; 

                if(nameErrorCount == 0)
                {
                    editorWindow.EnableSaveButton();
                }
                if(nameErrorCount == 1)
                {
                    editorWindow.DisableSaveButton();
                }
            }
        }

        public InterrogationGraphView(InterrogationEditorWindow interroEditorWindow)
        {
            editorWindow = interroEditorWindow;

            nodeDictionary = new SerializableDictionary<string, InterrogationNodeErrorData>();

            AddGridBackground();

            AddBlackboard();

            AddManipulators();

            OnElementsDeleted();
             
            OnGraphViewChanged();

            AddMinimap();

            AddStyles();
        }

        private void AddGridBackground()
        {
            GridBackground grid = new GridBackground();

            grid.StretchToParentSize();

            Insert(0, grid);
        }

        private void AddBlackboard()
        {
            profile = new ProfileBlackboard(this);
        }

        #region Styles
        private void AddStyles()
        {
            this.AddStyleSheet(
                "InterrogationSystem/GraphViewStyles.uss",
                "InterrogationSystem/NodeStyles.uss"
            );
        }
        #endregion

        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateGroupContextualMenu());
            this.AddManipulator(CreateNodeContextualMenu("Add Dialogue Node", NodeType.Dialogue, "Dialogue Name"));
            //this.AddManipulator(CreateNodeContextualMenu("Add Evidence Node", NodeType.Evidence, "Evidence Name"));
        }
        #endregion

        #region Adding Menu Options
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Suspect Group", contentViewContainer.WorldToLocal(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateNodeContextualMenu(string title, NodeType type, string name)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(title, actionEvent => AddElement(CreateNode(contentViewContainer.WorldToLocal(actionEvent.eventInfo.localMousePosition), type, name)))
            );

            return contextualMenuManipulator;
        }
        #endregion

        #region Get Elements
        private SuspectGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            SuspectGroup group = new SuspectGroup(this)
            {
                title = "Lmao"
            };

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));

            return group;
        }

        public BaseNode CreateNode(Vector2 pos, NodeType type, string nodeName, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"Interrogation.Elements.{type}Node");

            BaseNode node = (BaseNode)Activator.CreateInstance(nodeType);

            node.Initialize(this, pos, nodeName);
            
            if(type == NodeType.Dialogue)
            {
                AddNodeDictionary(node);
            }

            if (shouldDraw)
            {
                node.Draw();
            }

            return node;
        }
        #endregion

        #region Compatible Ports
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort.node == port.node) return;

                if (startPort.direction == port.direction) return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion

        private void AddMinimap()
        {
            miniMap = new MiniMap();

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;

            //Minimap Styles, can't be changed with style sheets
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }

        #region Dictionary
        public void AddNodeDictionary(BaseNode node) 
        {
            string nodeName = node.NodeName.ToLower().RemoveWhitespaces();

            if (!nodeDictionary.ContainsKey(nodeName))
            {
                InterrogationNodeErrorData nodeErrorData = new InterrogationNodeErrorData();

                nodeErrorData.NodeList.Add(node);

                nodeDictionary.Add(nodeName, nodeErrorData);

                return;
            }

            List<BaseNode> nodeList = nodeDictionary[nodeName].NodeList;

            nodeList.Add(node);

            Color errorColor = nodeDictionary[nodeName].ErrorData.Color;

            node.SetErrorColor(errorColor);

            if(nodeList.Count == 2)
            {
                NameErrorCount++;
                nodeList[0].SetErrorColor(errorColor);
            }
        }

        public void RemoveNodeDictionary(BaseNode node)
        {
            string nodeName = node.NodeName.ToLower().RemoveWhitespaces();

            List<BaseNode> nodeList = nodeDictionary[nodeName].NodeList;

            nodeList.Remove(node);

            node.ResetColor();

            if(nodeList.Count == 1)
            {
                NameErrorCount--;
                nodeList[0].ResetColor();

                return;
            }

            if(nodeList.Count == 0)
            {
                nodeDictionary.Remove(nodeName);
            }
        }
        #endregion

        #region Callbacks
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                var count = selection.Count;

                List<Edge> edges = new List<Edge>();
                List<BaseNode> nodes = new List<BaseNode>();
                for (var i = count - 1; i >= 0; i--)
                {
                    if (selection[i] is BaseNode node)
                    {
                        nodes.Add(node);
                    }

                    if (selection[i].GetType() == typeof(Edge))
                    {
                        Edge edge = (Edge)selection[i];

                        edges.Add(edge);
                    }
                }

                DeleteElements(edges);

                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].DisconnectAllPorts();

                    if(nodes[i].NodeType == NodeType.Dialogue)
                    {
                        RemoveNodeDictionary(nodes[i]);
                    }

                    RemoveElement(nodes[i]);
                }
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    List<Edge> edgesToRemove = new List<Edge>();

                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        BaseNode nextNode = (BaseNode) edge.input.node;

                        //Check if no children, thus lock port, otherwise normal port
                        if (edge.output.name == "Key")
                        {
                            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData) edge.output.userData;

                            choiceData.KeyIDs.Add(nextNode.ID);
                        }
                        else if (edge.output.name == "Error")
                        {
                            InterrogationErrorSaveData errorData = (InterrogationErrorSaveData)edge.output.userData;

                            errorData.EvidenceIDs.Add(nextNode.ID);
                        }
                        else
                        {
                            if(nextNode.NodeType ==  NodeType.Dialogue)
                            {
                                BaseNode node = (BaseNode)edge.input.node;

                                InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)edge.output.userData;

                                choiceData.NodeID = nextNode.ID;
                            }
                            else
                            {
                                edgesToRemove.Add(edge);
                            }
                        }
                    }

                    foreach(Edge edgeToRemove in edgesToRemove)
                    {
                        Debug.LogError("Choice cannot point to representative node.");

                        changes.edgesToCreate.Remove(edgeToRemove);
                    }
                }


                if(changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach(GraphElement element in changes.elementsToRemove)
                    {
                        if(element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge) element;

                        BaseNode node = (BaseNode)edge.input.node;

                        if (edge.output.name == "Key")
                        {
                            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)edge.output.userData;

                            choiceData.KeyIDs.Remove(node.ID);
                        }
                        else if (edge.output.name == "Error")
                        {
                            InterrogationErrorSaveData errorData = (InterrogationErrorSaveData)edge.output.userData;

                            errorData.EvidenceIDs.Remove(node.ID);
                        }
                        else
                        {
                            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)edge.output.userData;

                            choiceData.NodeID = null;
                        }
                    }
                }

                return changes;
            };
        }
        #endregion

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            nodeDictionary.Clear();

            NameErrorCount = 0;

            profile.Clear();
        }
    }
}