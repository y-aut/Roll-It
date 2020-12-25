using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Title, Menu, Play, Create,
}

public static class Scenes
{
    // 現在のScene
    public static SceneType GetActiveScene()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Title Scene":
                return SceneType.Title;
            case "Menu Scene":
                return SceneType.Menu;
            case "Play Scene":
                return SceneType.Play;
            case "Create Scene":
                return SceneType.Create;
            default:
                throw GameException.Unreachable;
        }
    }

    public static void LoadScene(SceneType scene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene((int)scene);
    }
}