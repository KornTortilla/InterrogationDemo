using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    public static StageController Instance;
    public static event Action OnActionStart;
    public static event Action OnActionComplete;

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Image curtain;
    [SerializeField] private Transform characterListTransform;
    private Dictionary<string, GameObject> characters;

    private Animator currentSpeaker;

    private int stageActions;

    public enum Position
    {
        right, middle, left, offscreenright, offscreenleft
    }

    private void Awake()
    {
        //Singleton Patter setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        characters = new Dictionary<string, GameObject>();
    }

    public void DropCurtain(float time)
    {
        StartCoroutine(StageAction(FadeCurtain, 1f, time));
    }

    public void PullCurtain(float time)
    {
        StartCoroutine(StageAction(FadeCurtain, 0f, time));
    }

    private IEnumerator FadeCurtain(float toValue, float time)
    {
        float fromValue = curtain.color.a;

        for (float t = 0f; t < 1f; t += Time.deltaTime / time)
        {
            Color newColor = new Color(0, 0, 0, Mathf.Lerp(fromValue, toValue, t));
            curtain.color = newColor;
            yield return null;
        }

        curtain.color = new Color(0, 0, 0, toValue);
    }

    public void ChangeBackground(string backgroundName)
    {
        Debug.Log("Changing Background: " + backgroundName);

        Sprite newBackground = Resources.Load<Sprite>("Backgrounds/" + backgroundName);
        background.sprite = newBackground;
    }

    public void SwitchBackground(string backgroundName, float time)
    {
        StartCoroutine(SwitchingBackground(backgroundName, time));
    }

    private IEnumerator SwitchingBackground(string backgroundName, float time)
    {
        AddStageAction();
        
        yield return StartCoroutine(FadeBackground(0, time));

        ChangeBackground(backgroundName);

        yield return StartCoroutine(FadeBackground(1, time));

        RemoveStageAction();
    }

    public IEnumerator FadeBackground(float toValue, float time)
    {
        float fromValue = background.color.a;

        for (float t = 0f; t < 1f; t += Time.deltaTime / time)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(fromValue, toValue, t));
            background.color = newColor;
            yield return null;
        }

        background.color = new Color(1, 1, 1, toValue);
    }

    public void AddCharacter(string characterName, float zPosition = 0f)
    {
        if(characters.ContainsKey(characterName)) {
            Debug.LogWarning(characterName + " already exists.");

            return;
        }

        //Loads character prefab from name to see if it exists before continuing
        GameObject characterPrefab = Resources.Load("Prefabs/Characters/" + characterName) as GameObject;

        if(characterPrefab)
        {
            GameObject newCharacter = Instantiate(characterPrefab, characterListTransform);

            newCharacter.GetComponent<Actor>().screenName = characterName;

            newCharacter.transform.position = new Vector3(0f, 0f, zPosition);

            characters.Add(characterName, newCharacter);

            Actor actorScript = newCharacter.GetComponent<Actor>();

            StartCoroutine(StageAction(actorScript.Fade, 1f, 0.2f));
        }
    }

    private GameObject GetCharacterFromString(string characterName)
    {
        if (!characters.ContainsKey(characterName))
        {
            Debug.LogError(characterName + " doesn't exist yet.");

            return null;
        }

        return characters[characterName];
    }

    private IEnumerator StageAction(Func<float, float, IEnumerator> actorFunction, float toValue, float time)
    {
        AddStageAction();

        yield return StartCoroutine(actorFunction(toValue, time));

        RemoveStageAction();
    }

    /*
    private IEnumerator SwitchBackground()
    {
        OnActionStart?.Invoke();

        Actor

        OnActionComplete?.Invoke();
    }
    */

    public void ShowCharacter(string characterName)
    {
        GameObject character = GetCharacterFromString(characterName);

        if (character == null) return;

        Actor actorScript = character.GetComponent<Actor>();

        StartCoroutine(StageAction(actorScript.Fade, 1f, 0.2f));
    }

    public void HideCharacter(string characterName)
    {
        GameObject character = GetCharacterFromString(characterName);

        if (character == null) return;

        Actor actorScript = character.GetComponent<Actor>();

        StartCoroutine(StageAction(actorScript.Fade, 0f, 0.2f));
    }

    public void PlayAnim(string characterName, string animation)
    {
        GameObject character = GetCharacterFromString(characterName);

        if (character == null) return;

        character.GetComponent<Animator>().Play(animation);
    }

    public void ChooseSpeaker(string characterName)
    {
        //If the speaker character exists, it becomes the current speaker to used to talk later
        if(characters.ContainsKey(characterName))
        {
            currentSpeaker = characters[characterName].GetComponent<Animator>();
        }
        else
        {
            currentSpeaker = null;
        }
    }

    public void SetCurrentSpeakerTalking(bool isTalking)
    {
        //Sets the bool for the character thats currently set to speak
        if(currentSpeaker)
        {
            currentSpeaker.SetBool("talking", isTalking);
        }
    }

    public float GetCharacterPitch(string characterName)
    {
        GameObject character = GetCharacterFromString(characterName);

        if (character == null) return 0.5f;

        return character.GetComponent<Actor>().voicePitch;
    }

    public void SetCharacterPosition(string characterName, string positionString)
    {
        GameObject character = GetCharacterFromString(characterName);

        if (character == null) return;

        float newXValue = GetPositionValue(character.transform.position.x, positionString);

        SetCharacterXValue(character, newXValue);

        Debug.Log("Set Character to New Position");
    }

    public void MoveCharacter(string characterName, string positionString, float time, bool stageWaits)
    {
        GameObject character = GetCharacterFromString(characterName);

        if (character == null) return;

        float newXValue = GetPositionValue(character.transform.position.x, positionString);

        Actor actorScript = character.GetComponent<Actor>();

        if (stageWaits)
        {
            StartCoroutine(StageAction(actorScript.Slide, newXValue, time));
        }
        else
        {
            StartCoroutine(character.GetComponent<Actor>().Slide(newXValue, time));
        }
    }

    private float GetPositionValue(float characterPosition, string positionString)
    {
        Position positionFromString = Enum.Parse<Position>(positionString);
        float newXValue = characterPosition;
        switch (positionFromString)
        {
            case Position.left:
                newXValue = -5f;
                break;

            case Position.middle:
                newXValue = 0f;
                break;

            case Position.right:
                newXValue = 5f;
                break;

            case Position.offscreenright:
                newXValue = 13f;
                break;

            case Position.offscreenleft:
                newXValue = -13f;
                break;

            default:
                Debug.LogWarning("Position Enum not found.");
                break;
        }

        return newXValue;
    }

    private void SetCharacterXValue(GameObject character, float xValue)
    {
        Vector3 position = character.transform.position;

        character.transform.position = new Vector3(xValue, position.y, position.z);
    }

    public void HintSlide(string characterName, bool goingIn)
    {
        if (!characters.ContainsKey(characterName))
        {
            return;
        }

        Actor partnerScript = characters[characterName].GetComponent<Actor>();

        float placement;
        if (goingIn)
        {
            placement = 5f;

            foreach (GameObject character in characters.Values)
            {
                Actor actorScript = character.GetComponent<Actor>();
                if (actorScript.screenName == characterName)
                {
                    continue;
                }

                actorScript.StepBack();
            }
        }
        else
        {
            
            foreach (GameObject character in characters.Values)
            {
                if(!(character.GetComponent<SpriteRenderer>().color.a == 0))
                {
                    character.GetComponent<Actor>().StepIn();
                }
            }

            placement = 13f;
        }

        StartCoroutine(partnerScript.Slide(placement, 0.25f));
    }

    public void RemoveCharacter(string characterName)
    {
        if (!characters.ContainsKey(characterName))
        {
            return;
        }

        StartCoroutine(characters[characterName].GetComponent<Actor>().Fade(0f, 0.2f));

        StartCoroutine(WaitRemoval(characterName));
    }

    private IEnumerator WaitRemoval(string characterName)
    {
        Actor actorScript = characters[characterName].GetComponent<Actor>();

        yield return new WaitUntil(() => !actorScript.isActing);

        Destroy(characters[characterName]);

        characters.Remove(characterName);
    }

    public void ClearStage()
    {
        foreach(GameObject character in characters.Values)
        {
            Destroy(character);
        }

        characters.Clear();

        background.sprite = null;
    }

    private void AddStageAction()
    {
        if(stageActions == 0)
        {
            OnActionStart?.Invoke();
        }

        stageActions++;
    }

    private void RemoveStageAction()
    {
        stageActions--;

        if (stageActions == 0)
        {
            OnActionComplete?.Invoke();
        }
    }
}