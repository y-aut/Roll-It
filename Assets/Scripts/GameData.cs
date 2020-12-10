using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameData
{
    const string STAGES_STRING = "Stages";
    const string STAGELOCALS_STRING = "StageLocals";
    const string USER_STRING = "User";
    const string USERLOCAL_STRING = "UserLocal";

    // 作成したステージ
    public static SerializableList<Stage> Stages { get; private set; }
    // ユーザー情報
    public static User User { get; private set; }

    public static void Save()
    {
        PlayerPrefs.SetString(STAGES_STRING, JsonUtility.ToJson(Stages));
        PlayerPrefs.SetString(STAGELOCALS_STRING, JsonUtility.ToJson(Stages.Select(i => i.LocalData)));
        PlayerPrefs.SetString(USER_STRING, JsonUtility.ToJson(User));
        PlayerPrefs.SetString(USERLOCAL_STRING, JsonUtility.ToJson(User.LocalData));
    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey(STAGES_STRING))
        {
            Stages = JsonUtility.FromJson<SerializableList<Stage>>(PlayerPrefs.GetString(STAGES_STRING));

            var locals = JsonUtility.FromJson<SerializableList<StageLocal>>(PlayerPrefs.GetString(STAGELOCALS_STRING));
            for (int i = 0; i < Stages.Count; ++i)
                Stages[i].LocalData = locals[i];
        }
        else
        {
            Stages = new SerializableList<Stage>();
        }

        if (PlayerPrefs.HasKey(USER_STRING))
        {
            User = JsonUtility.FromJson<User>(PlayerPrefs.GetString(USER_STRING));
            User.LocalData = JsonUtility.FromJson<UserLocal>(PlayerPrefs.GetString(USERLOCAL_STRING));
        }
        else
        {
            User = new User();
            User.Initialize();
        }
        User.Login();
    }

}
