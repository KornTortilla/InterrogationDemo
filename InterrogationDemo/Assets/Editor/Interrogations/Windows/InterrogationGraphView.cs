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

            this.AddManipulator(CreateNodeContextualMenu("Add Dialogue Node", NodeType.Dialogue, "Dialogue Name"));
            this.AddManipulator(CreateNodeContextualMenu("Add Evidence Node", NodeType.Evidence, "Evidence Name"));
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

        #region Adding Node Menu
        private IManipulator CreateNodeContextualMenu(string title, NodeType type, string name)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(title, actionEvent => AddElement(CreateNode(contentViewContainer.WorldToLocal(actionEvent.eventInfo.localMousePosition), type, name)))
            );

            return contextualMenuManipulator;
        }
        #endregion

        #region Get Node
        public BaseNode CreateNode(Vector2 pos, NodeType type, string nodeName, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"Interrogation.Elements.{type}Node");

            BaseNode node = (BaseNode)Activator.CreateInstance(nodeType);

            node.Initialize(this, pos, nodeName);
            AddNodeDictionary(node);

            if (shouldDraw)
            {
                node.Draw();
            }

            return node;
        }

        internal void RemoveElement(TextField tagTextField)
        {
            throw new NotImplementedException();
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

                    RemoveNodeDictionary(nodes[i]);
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
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        BaseNode nextNode = (BaseNode) edge.input.node;

                        //Check if no children, thus lock port, otherwise normal port
                        if (edge.output.name == "Key")
                        {
                            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData) edge.output.userData;

                            choiceData.KeyIDs.Add(nextNode.ID);
                        }
                        else
                        {
                            BaseNode node = (BaseNode)edge.input.node;

                            InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)edge.output.userData;

                            choiceData.NodeID = nextNode.ID;
                        }
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

                        InterrogationChoiceSaveData choiceData = (InterrogationChoiceSaveData)edge.output.userData;

                        if (edge.output.name == "Key")
                        {

                            BaseNode node = (BaseNode)edge.input.node;

                            choiceData.KeyIDs.Remove(node.ID);
                        }
                        else 
                        {
                            
                            choiceData.NodeID = "";
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
        }
    }
}