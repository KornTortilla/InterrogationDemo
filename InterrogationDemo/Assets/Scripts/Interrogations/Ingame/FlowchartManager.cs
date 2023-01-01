using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Ingame
{
    using Data;
    using ScriptableObjects;

    public class FlowchartManager : MonoBehaviour
    {
        private InterrogationDialogueManager interrogationManager;
        private List<GameObject> nodeList;
        private GameObject nodeObject;

        private Vector2 startingPos;

        //Creates the flowchart
        public void Initialize(DialogueSO startingDialogue, InterrogationDialogueManager interroManager)
        {
            interrogationManager = interroManager;
            nodeList = new List<GameObject>();
            nodeObject = Resources.Load("Prefabs/FlowchartNode") as GameObject;

            startingPos = startingDialogue.Position;

            AddNodesFromTree(startingDialogue);
        }

        private void AddNodesFromTree(DialogueSO root, GameObject previous = null)
        {
            if (root != null)
            {
                GameObject node = Instantiate(nodeObject, this.transform);
                node.transform.SetAsFirstSibling();
                node.name = root.Name;
                node.GetComponent<FlowchartNode>().Initialize(root, interrogationManager);
                nodeList.Add(node);

                if (previous != null)
                {
                    node.GetComponent<FlowchartNode>().previousNode = previous;
                }

                node.GetComponent<FlowchartNode>().Draw(startingPos);

                foreach (DialogueChoiceData dialogueChoice in root.Choices)
                {
                    AddNodesFromTree(dialogueChoice.NextDialogue, node);
                }
            }
        }

        public void UpdateFlowchart(DialogueSO dialogue, bool advancing)
        {
            if(advancing)
            {
                foreach (GameObject node in nodeList)
                {
                    if (node.GetComponent<FlowchartNode>().dialogue == dialogue)
                    {
                        node.gameObject.SetActive(true);
                        node.GetComponent<FlowchartNode>().ColorHighlight();
                    }
                }
            }
            else
            {
                foreach (GameObject node in nodeList)
                {
                    if (node.GetComponent<FlowchartNode>().dialogue == dialogue)
                    {
                        node.GetComponent<FlowchartNode>().ColorDefault();
                    }
                }
            }
            
        }

        public void RehighlightedNodes(Stack<DialogueSO> dialogueList)
        {
            foreach (GameObject node in nodeList)
            {
                if(dialogueList.Contains(node.GetComponent<FlowchartNode>().dialogue))
                {
                    
                    node.GetComponent<FlowchartNode>().ColorHighlight();
                }
                else
                {
                    node.GetComponent<FlowchartNode>().ColorDefault();
                }
            }
        }
    }
}
