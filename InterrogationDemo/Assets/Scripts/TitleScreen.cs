using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    private void Awake()
    {
        AudioManager.Instance.StartNewMusic("/MusicRegular/Title");
    }

    public void StartGame(Button button)
    {
        GameManager.Instance.StartGame();
        button.interactable = false;
    }
}
