using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    //Emum of all scenes, needs to updated as scenes get added
    public enum Scene
    {
        Interrogation, Cutscene, Background
    }

    public static void Load(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
