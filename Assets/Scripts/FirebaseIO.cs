using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    // 非同期の読み込みが完了したか
    public static bool LoadFinished;
    // 非同期で読み込んだリスト
    public static List<Stage> AnswerStages;

    // ユーザーを登録する
    public static async Task<bool> RegisterUser(User user)
    {
        if (!Available) return false;

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
        var _ = entry.SetRawJsonValueAsync(JsonUtility.ToJson(user));

        return true;
    }

    // ステージを公開する
    public static async Task<bool> PublishStage(Stage stage)
    {
        if (!Available) return false;

        LoadFinished = false;

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
        await entry.SetRawJsonValueAsync(JsonUtility.ToJson(stage));
        stage.LocalData.IsPublished = true;

        LoadFinished = true;

        return true;
    }

    // ステージを非公開にする
    public static async Task<bool> UnpublishStage(Stage stage)
    {
        if (!Available) return false;

        LoadFinished = false;

        await Stages.Child(stage.ID.ToString()).RemoveValueAsync();
        stage.LocalData.IsPublished = false;

        LoadFinished = true;

        return true;
    }

    // 公開中のステージを取得し、actionによる操作を行う
    public static bool GetAllStages()
    {
        if (!Available) return false;

        AnswerStages = new List<Stage>();
        LoadFinished = false;
        Stages.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                foreach (var i in task.Result.Children)
                {
                    AnswerStages.Add(JsonUtility.FromJson<Stage>(i.GetRawJsonValue()));
                }
                LoadFinished = true;
            }
        });

        return true;
    }

    // ステージのチャレンジ回数を+1する
    public static async void IncrementChallengeCount(IDType id)
    {
        if (!Available) return;

        LoadFinished = false;
        var add = Stages.Child(id.ToString()).Child("ChallengeCount");

        var res = await add.GetValueAsync();
        var _ = add.SetValueAsync(int.Parse(res.Value.ToString()) + 1);
    }

    // ステージのクリア回数を+1する
    public static async void IncrementClearCount(IDType id)
    {
        if (!Available) return;

        LoadFinished = false;
        var add = Stages.Child(id.ToString()).Child("ClearCount");

        var res = await add.GetValueAsync();
        var _ = add.SetValueAsync(int.Parse(res.Value.ToString()) + 1);
    }
}
