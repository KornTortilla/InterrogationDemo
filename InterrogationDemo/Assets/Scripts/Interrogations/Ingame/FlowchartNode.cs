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

            goToButton.onClick.AddListener(() => interroManager.Jump(dialogue));
            //goToButton.GetComponent<Button>().onClick.AddListener(() => manager.GetComponent<CanvasController>().SetActiveAlternate());
            goToButton.gameObject.SetActive(false);

            //this.gameObject.SetActive(false);
        }

        public void Draw(Vector2 startingPos)
        {
            Vector2 newPos = new Vector2(dialogue.Position.x - startingPos.x, -(dialogue.Position.y - startingPos.y));
            gameObject.transform.localPosition = newPos;

            if (previousNode != null)
            {
                Vector2 previousNodeConnectorPos = previousNode.GetComponent<RectTransform>().localPosition + 
                    previousNode.GetComponent<FlowchartNode>().frontConnector.GetComponent<RectTransform>().localPosition;
                previousNodeConnectorPos -= newPos;

                uiLiner.points.Add(backConnector.transform.localPosition);
                uiLiner.points.Add(previousNodeConnectorPos);
            }
        }

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
            mainButtonObject.GetComponent<Image>().color = color;
            goToButton.gameObject.GetComponent<Image>().color = color;
            uiLiner.color = color;
        }
    }
}

