using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public static StageController Instance;

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Transform characterListTransform;
    private Dictionary<string, GameObject> characters;

    private Animator currentSpeaker;

    public enum Position
    {
        right, middle, left
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

    public void AddCharacter(string characterName)
    {
        //Loads character prefab from name to see if it exists before continuing
        GameObject characterPrefab = Resources.Load("Prefabs/Characters/" + characterName) as GameObject;

        if(characterPrefab)
        {
            GameObject newCharacter = Instantiate(characterPrefab, characterListTransform);

            characters.Add(characterName, newCharacter);

            Debug.Log("Added: " + characterName);
        }
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
        switch (positionFromString)
        {
            case Position.left:
                character.transform.position = Vector3.left * 5f;
                break;

            case Position.middle:
                character.transform.position = Vector3.zero;
                break;

            case Position.right:
                character.transform.position = Vector3.right * 5f;
                break;
        }

        Debug.Log("Set Character to New Position");
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
