using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public static StageController Instance;
    public static event Action OnActionStart;
    public static event Action OnActionComplete;

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Transform characterListTransform;
    private Dictionary<string, GameObject> characters;

    private Animator currentSpeaker;

    public enum Position
    {
        right, middle, left, offscreenright
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

    public void ChangeBackground(string backgroundName)
    {
        Debug.Log("Changing Background: " + backgroundName);

        Sprite newBackground = Resources.Load<Sprite>("Backgrounds/" + backgroundName);
        background.sprite = newBackground;
    }

    public void AddCharacter(string characterName, float zPosition = 0f)
    {
        //Loads character prefab from name to see if it exists before continuing
        GameObject characterPrefab = Resources.Load("Prefabs/Characters/" + characterName) as GameObject;

        if(characterPrefab)
        {
            GameObject newCharacter = Instantiate(characterPrefab, characterListTransform);

            newCharacter.GetComponent<Actor>().screenName = characterName;

            newCharacter.transform.position = new Vector3(0f, 0f, zPosition);

            characters.Add(characterName, newCharacter);

            Debug.Log("Added: " + characterName);

            StartCoroutine(FadeCharacter(newCharacter, 1f, 0.2f));
        }
    }

    private IEnumerator FadeCharacter(GameObject character, float toValue, float time)
    {
        OnActionStart?.Invoke();

        yield return StartCoroutine(character.GetComponent<Actor>().Fade(toValue, time));

        OnActionComplete?.Invoke();
    }

    public void ShowCharacter(string characterName)
    {
        if (!characters.ContainsKey(characterName))
        {
            return;
        }

        GameObject character = characters[characterName];

        StartCoroutine(FadeCharacter(character, 1f, 0.2f));
    }

    public void HideCharacter(string characterName)
    {
        if (!characters.ContainsKey(characterName))
        {
            return;
        }

        GameObject character = characters[characterName];

        StartCoroutine(FadeCharacter(character, 0f, 0.2f));
    }

    public void PlayAnim(string characterName, string animation)
    {
        //Finds the character with the name and animates it if it exists
        GameObject characterObject = characters[characterName];

        if (characterObject)
        {
            characterObject.GetComponent<Animator>().Play(animation);
        }
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

    public void SetCharacterPosition(string characterName, string positionString)
    {
        //Returns if character doesnt exist
        if(!characters.ContainsKey(characterName))
        {
            return;
        }

        //Gets character and based on position given, parsed into the right enum, places them
        GameObject character = characters[characterName];

        Position positionFromString = Enum.Parse<Position>(positionString);
        float newXValue = character.transform.position.x;
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

            default:
                Debug.LogWarning("Position Enum not found.");
                break;
        }

        SetCharacterXValue(character, newXValue);

        Debug.Log("Set Character to New Position");
    }

    private void SetCharacterXValue(GameObject character, float xValue)
    {
        Vector3 position = character.transform.position;

        character.transform.position = new Vector3(xValue, position.y, position.z);
    }

    public void HintSlide(string characterName,bool goingIn)
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
                character.GetComponent<Actor>().StepIn();
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
}