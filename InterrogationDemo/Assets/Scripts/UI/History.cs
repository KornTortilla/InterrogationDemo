using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class History : MonoBehaviour
{
    [SerializeField] private GameObject list;

    private string pastName = "";

    private void OnEnable()
    {
        DialogueTextManager.SentenceFinished += AddHistory;
    }

    private void OnDisable()
    {
        DialogueTextManager.SentenceFinished -= AddHistory;
    }

    public void AddHistory(string name, string text)
    {
        GameObject historyPrefab = Resources.Load("Prefabs/Interrogation/Creation/HistoryEntry") as GameObject;

        GameObject historyObject = Instantiate(historyPrefab, list.transform);

        if (name != pastName) historyObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = name;
        historyObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = text;

        pastName = name;

        LayoutRebuilder.ForceRebuildLayoutImmediate(list.transform.GetComponent<RectTransform>());
    }
}
