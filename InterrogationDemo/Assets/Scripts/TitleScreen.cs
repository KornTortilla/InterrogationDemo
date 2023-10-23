using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        AudioManager.Instance.PlayNewTrack("/MusicRegular/Title");
    }

    public void StartGame()
    {
        GameManager.Instance.StartGame();
        GetComponent<Button>().interactable = false;
    }
}
