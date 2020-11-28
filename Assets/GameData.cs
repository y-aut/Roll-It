using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static SerializableList<Stage> MyStages { get; private set; }

    public static void Save()
    {
        PlayerPrefs.SetString("MyStages", JsonUtility.ToJson(MyStages));
        PlayerPrefs.SetInt("StageMaxID", Stage.MaxID);

    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey("MyStages"))
            MyStages = JsonUtility.FromJson<SerializableList<Stage>>(PlayerPrefs.GetString("MyStages"));
        else
            MyStages = new SerializableList<Stage>();

        if (PlayerPrefs.HasKey("StageMaxID"))
            Stage.MaxID = PlayerPrefs.GetInt("StageMaxID");

    }

}
