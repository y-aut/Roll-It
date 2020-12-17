using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Firebase;
using Firebase.Database;

// Firebase Databaseへの読み書きを行う
public static class FirebaseIO
{
    // Firebaseが利用可能かどうか
    public static bool Available { get; set; } = false;

    // Database References
    public static DatabaseReference Root
    {
        set
        {
            Users = value.Child("Users");
            Stages = value.Child("Stages");
        }
    }
    public static DatabaseReference Users { get; set; }
    public static DatabaseReference Stages { get; set; }
    public static DatabaseReference Me => Users.Child(GameData.User.ID.ToString());

    // ユーザーを登録する
    public static async Task RegisterUser(User user)
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        IDType id;
        DatabaseReference entry;
        DataSnapshot val;
        do
        {
            id = IDType.Generate();
            entry = Users.Child(id.ToString());
            val = await entry.GetValueAsync();
        } while (val.HasChildren);

        user.ID = id;
        await entry.SetRawJsonValueAsync(JsonUtility.ToJson(new UserZip(user)));
    }

    // ユーザー名を変更する
    public static async Task ChangeUserName(string newName)
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        await Me.Child("n").SetValueAsync(newName);
        GameData.User.Name = newName;
        GameData.Save();
    }

    // ステージを公開する
    public static async Task PublishStage(Stage stage)
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        IDType id;
        DatabaseReference entry;
        DataSnapshot val;
        do
        {
            id = IDType.Generate();
            entry = Stages.Child(id.ToString());
            val = await entry.GetValueAsync();
        } while (val.HasChildren);

        stage.ID = id;
        stage.PublishedDate = DateTime.Now;
        await entry.SetRawJsonValueAsync(JsonUtility.ToJson(new StageZip(stage)));
        stage.LocalData.IsPublished = true;

        // ユーザーデータのPublishedStagesに追加
        GameData.User.PublishedStages.Add(id);
        var ids = new IDTypeCollection(GameData.User.PublishedStages);
        await Me.Child("p").Child("v")
            .Child((ids.GetRawList().Count - 1).ToString()).SetValueAsync(ids.GetRawList().Last());

        GameData.Save();
    }

    // ステージを非公開にする
    public static async Task UnpublishStage(Stage stage)
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        await Stages.Child(stage.ID.ToString()).RemoveValueAsync();
        stage.LocalData.IsPublished = false;

        // ユーザーデータのPublishedStagesから削除
        GameData.User.PublishedStages.Remove(stage.ID);
        var ids = new IDTypeCollection(GameData.User.PublishedStages);
        await Users.Child(GameData.User.ID.ToString()).Child("p")
            .SetRawJsonValueAsync(JsonUtility.ToJson(ids));

        GameData.Save();
    }

    // 公開中のステージを取得し、actionによる操作を行う
    public static async Task<List<Stage>> GetAllStages()
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        var stages = new List<StageZip>();
        var values = await Stages.GetValueAsync();
        foreach (var i in values.Children)
        {
            stages.Add(JsonUtility.FromJson<StageZip>(i.GetRawJsonValue()));
        }

        return new StageZipCollection(stages).ToStages();
    }

    // 作ったステージのローカルデータを更新
    public static async Task UpdateMyStages()
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        // c(Clear), t(Try), l(Like), d(Dislike)を取得
        var published = GameData.Stages.Where(i => i.LocalData.IsPublished);
        Task<DataSnapshot>[] tasks = new Task<DataSnapshot>[published.Count() * 4];
        for (int i = 0; i < published.Count(); ++i)
        {
            tasks[i * 4 + 0] = Stages.Child(published.ElementAt(i).ID.ToString()).Child("c").GetValueAsync();
            tasks[i * 4 + 1] = Stages.Child(published.ElementAt(i).ID.ToString()).Child("t").GetValueAsync();
            tasks[i * 4 + 2] = Stages.Child(published.ElementAt(i).ID.ToString()).Child("l").GetValueAsync();
            tasks[i * 4 + 3] = Stages.Child(published.ElementAt(i).ID.ToString()).Child("d").GetValueAsync();
        }
        var vals = await Task.WhenAll(tasks);

        for (int i = 0; i < published.Count(); ++i)
        {
            published.ElementAt(i).ClearCount       = int.Parse(vals[i * 4 + 0].Value.ToString());
            published.ElementAt(i).ChallengeCount   = int.Parse(vals[i * 4 + 1].Value.ToString());
            published.ElementAt(i).PosEvaCount      = int.Parse(vals[i * 4 + 2].Value.ToString());
            published.ElementAt(i).NegEvaCount      = int.Parse(vals[i * 4 + 3].Value.ToString());
        }
    }

    // キーに対応する数字を+1する
    private static async Task IncrementCount(IDType id, string key)
    {
        if (!Available) throw new GameException(GameExceptionEnum.FirebaseUnavailable);

        var add = Stages.Child(id.ToString()).Child(key);

        var res = await add.GetValueAsync();
        await add.SetValueAsync(int.Parse(res.Value.ToString()) + 1);
    }

    // ステージのクリア回数を+1する
    public static async Task IncrementClearCount(IDType id)
    {
        await IncrementCount(id, "c");
        GameData.User.LocalData.ClearedIDs.Add(id);
        GameData.Save();
    }

    // ステージのチャレンジ回数を+1する
    public static async Task IncrementChallengeCount(IDType id)
    {
        await IncrementCount(id, "t");
    }

    // ステージの高評価数を+1する
    public static async Task IncrementPosEvaCount(IDType id)
    {
        await IncrementCount(id, "l");
        GameData.User.LocalData.PosEvaIDs.Add(id);
        GameData.Save();
    }

    // ステージの低評価数を+1する
    public static async Task IncrementNegEvaCount(IDType id)
    {
        await IncrementCount(id, "d");
        GameData.User.LocalData.NegEvaIDs.Add(id);
        GameData.Save();
    }
}
