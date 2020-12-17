using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Menu, Play, Create, Select, Result,
}

public static class Scenes
{
    // 現在のScene
    public static SceneType GetActiveScene()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Menu Scene":
                return SceneType.Menu;
            case "Play Scene":
                return SceneType.Play;
            case "Create Scene":
                return SceneType.Create;
            case "Select Scene":
                return SceneType.Select;
            case "Result Scene":
                return SceneType.Result;
            default:
                throw GameException.Unreachable;
        }
    }

    public static void LoadScene(SceneType scene)
    {
        SceneManager.LoadScene((int)scene);
    }
}