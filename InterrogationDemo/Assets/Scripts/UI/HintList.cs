using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HintList : MonoBehaviour
{
    [SerializeField] private GameObject list;

    public void CreateHint(string text)
    {
        GameObject hintPrefab = Resources.Load("Prefabs/Interrogation/Creation/Hint") as GameObject;

        GameObject hintObject = Instantiate(hintPrefab, list.transform);
        hintObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = text;

        LayoutRebuilder.ForceRebuildLayoutImmediate(list.transform.GetComponent<RectTransform>());
    }

    public void Open()
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Close()
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
    }
}
