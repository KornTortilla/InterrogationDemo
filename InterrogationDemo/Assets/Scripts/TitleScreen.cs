using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGame()
    {
        GameManager.Instance.StartGame();
        GetComponent<Button>().interactable = false;
    }
}
