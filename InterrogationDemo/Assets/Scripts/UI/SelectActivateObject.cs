using UnityEngine;
using UnityEngine.EventSystems;

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
