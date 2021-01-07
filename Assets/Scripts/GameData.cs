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
    const string MYSTRUCTURE_STRING = "MyStructure";

    // 作成したステージ
    public static List<Stage> Stages { get; private set; }
    // ユーザー情報
    public static User User { get; private set; }
    // 所持しているStructure
    public static BoolList MyStructure { get; private set; }

    public static void Save()
    {
        var zip = new StageZipCollection(Stages.Select(i => new StageZip(i)).ToList());
        PlayerPrefs.SetString(STAGES_STRING, JsonUtility.ToJson(zip));
        PlayerPrefs.SetString(STAGELOCALS_STRING, JsonUtility.ToJson(Stages.Select(i => i.LocalData).ToSerializableList()));
        PlayerPrefs.SetString(USER_STRING, JsonUtility.ToJson(new UserZip(User)));
        PlayerPrefs.SetString(USERLOCAL_STRING, JsonUtility.ToJson(User.LocalData));
        PlayerPrefs.SetString(MYSTRUCTURE_STRING, JsonUtility.ToJson(MyStructure));
    }

    public static bool Load()
    {
        bool success = true;

        if (PlayerPrefs.HasKey(STAGES_STRING))
        {
            try
            {
                Stages = JsonUtility.FromJson<StageZipCollection>(PlayerPrefs.GetString(STAGES_STRING)).ToStages();

                var locals = JsonUtility.FromJson<SerializableList<StageLocal>>(PlayerPrefs.GetString(STAGELOCALS_STRING));
                for (int i = 0; i < Stages.Count; ++i)
                    Stages[i].LocalData = locals[i];
            }
            catch (System.Exception)
            {
                success = false;
            }
            finally
            {
                if (Stages == null)
                    Stages = new List<Stage>();
                else
                {
                    foreach (var stage in Stages)
                    {
                        if (stage.LocalData == null)
                            stage.LocalData = new StageLocal();
                    }
                }
            }
        }
        else
        {
            Stages = new List<Stage>();
        }

        if (PlayerPrefs.HasKey(USER_STRING))
        {
            try
            {
                User = JsonUtility.FromJson<UserZip>(PlayerPrefs.GetString(USER_STRING)).ToUser();
                User.LocalData = JsonUtility.FromJson<UserLocal>(PlayerPrefs.GetString(USERLOCAL_STRING));
            }
            catch (System.Exception)
            {
                success = false;
            }
            finally
            {
                if (User == null)
                    User = new User();
                else if (User.LocalData == null)
                    User.LocalData = new UserLocal();
            }
        }
        else
        {
            User = new User();
        }

        if (PlayerPrefs.HasKey(MYSTRUCTURE_STRING))
        {
            try
            {
                MyStructure = JsonUtility.FromJson<BoolList>(PlayerPrefs.GetString(MYSTRUCTURE_STRING));
                // アイテムの数の変化を調整
                MyStructure.Count = Prefabs.StructureItemList.Count;
            }
            catch (System.Exception)
            {
                success = false;
            }
            finally
            {
                if (MyStructure is null) InitializeMyStructure();
            }
        }
        else
        {
            InitializeMyStructure();
        }

        User.Login();

        return success;
    }

    private static void InitializeMyStructure()
    {
        MyStructure = new BoolList(Prefabs.StructureItemList.Count, i => Prefabs.StructureItemList[i].Price == Money.Default);
    }

}
