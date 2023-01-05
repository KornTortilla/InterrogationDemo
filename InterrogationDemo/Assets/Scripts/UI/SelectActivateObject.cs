using UnityEngine;
using UnityEngine.EventSystems;

//Activates different game object to activate and deactivate on button click
public class SelectActivateObject : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public GameObject activatedObject;

    public void OnSelect(BaseEventData eventData)
    {
        activatedObject.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        activatedObject.gameObject.SetActive(false);
    }
}
