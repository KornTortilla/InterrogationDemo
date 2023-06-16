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

        //Called when interrogation manager intializes
        public void Initialize(DialogueInterrogationSO startingDialogue, InterrogationDialogueManager interroManager)
        {
            interrogationManager = interroManager;
            nodeList = new List<GameObject>();
            nodeObject = Resources.Load("Prefabs/Interrogation/Creation/FlowchartNode") as GameObject;

            startingPos = startingDialogue.Position;

            AddNodesFromTree(startingDialogue);
        }

        private void AddNodesFromTree(DialogueInterrogationSO root, GameObject previous = null)
        {
            if (root != null)
            {
                GameObject node = Instantiate(nodeObject, this.transform);
                //Sets as first child in order to keep connecting lines to be below the nodes themselves
                node.transform.SetAsFirstSibling();
                //Renames gameObject to be that of dialogue
                node.name = root.Name;
                //Starts flowchart node script
                node.GetComponent<FlowchartNode>().Initialize(root, interrogationManager);
                nodeList.Add(node);

                if (previous != null)
                {
                    node.GetComponent<FlowchartNode>().previousNode = previous;
                }

                //Places node and draws line for previous connection
                node.GetComponent<FlowchartNode>().Draw(startingPos);

                foreach (DialogueChoiceData dialogueChoice in root.Choices)
                {
                    //Continues on with new dialogues
                    AddNodesFromTree(dialogueChoice.NextDialogue, node);
                }
            }
        }

        //Called when advancing to new dialogue/going back in InterrogationDialogueManager
        public void UpdateFlowchart(DialogueInterrogationSO dialogue, bool advancing)
        {
            //If advancing to new dialogue, get the new dialogue's appropriate node and highlight it
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
            //If going back, get the current dialogue and color its appropriate node the default
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

        //Called when jumping the flowchart by the InterrogationDialogeManager
        public void RehighlightedNodes(Stack<DialogueInterrogationSO> dialogueList)
        {
            //Checks each node if its dialogue is the new previous dialogue list, colors it appropriately
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
