using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Interrogation.Ingame
{
    using ScriptableObjects;

    public class FlowchartNode : MonoBehaviour
    {
        [HideInInspector] public DialogueSO dialogue;
        [HideInInspector] public GameObject previousNode;

        private UILineRenderer uiLiner;
        [SerializeField] private GameObject mainButtonObject;
        [SerializeField] private Button goToButton;
        public GameObject frontConnector;
        public GameObject backConnector;

        [SerializeField] private Color defaultColor;
        [SerializeField] private Color highlightColor;

        public void Initialize(DialogueSO dialogueSO, InterrogationDialogueManager interroManager)
        {
            dialogue = dialogueSO;

            uiLiner = GetComponent<UILineRenderer>();

            mainButtonObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialogue.Name;

            //Ties jump function of interrogation manager to when gotobutton is clicked
            goToButton.onClick.AddListener(() => interroManager.Jump(dialogue));
            //Sets gotobutton off by default
            goToButton.gameObject.SetActive(false);
            //Sets self to be off, to activated when dialogue is reached
            this.gameObject.SetActive(false);
        }

        //Gets called by flowchart manager to reposition each node individually when creating nodes
        public void Draw(Vector2 startingPos)
        {
            //Gets the new position based on the dialogue in-editor position, offset by the starting node's position
            Vector2 newPos = new Vector2(dialogue.Position.x - startingPos.x, -(dialogue.Position.y - startingPos.y));
            //Sets the local position of node to new position
            gameObject.transform.localPosition = newPos;

            if (previousNode != null)
            {
                //Gets the previous node position and adds the front connector's local position of it
                Vector2 previousNodeConnectorPos = previousNode.GetComponent<RectTransform>().localPosition +
                    previousNode.GetComponent<FlowchartNode>().frontConnector.GetComponent<RectTransform>().localPosition;
                //Offsets the previous connect pos by this node's pos
                previousNodeConnectorPos -= newPos;

                //Adds the points off this nodes back connector and the previous node's connector
                uiLiner.points.Add(backConnector.transform.localPosition);
                uiLiner.points.Add(previousNodeConnectorPos);
            }
        }

        //Called when by flowchart manager when advancing dialogue in some form
        public void ColorHighlight()
        {
            ColorNode(highlightColor);
        }

        public void ColorDefault()
        {
            ColorNode(defaultColor);
        }

        private void ColorNode(Color color)
        {
            //Sets the new color for all necessary components
            mainButtonObject.GetComponent<Image>().color = color;
            goToButton.gameObject.GetComponent<Image>().color = color;
            uiLiner.color = color;
        }
    }
}

