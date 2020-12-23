using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Firebase;
using Firebase.Firestore;

// Firebaseへの読み書きを行う
public static class FirebaseIO
{
    // Firebaseが利用可能かどうか
    public static bool Available { get; set; } = false;

    // Collection Reference
    public static void SetReference()
    {
        Root = FirebaseFirestore.DefaultInstance;
        Users = Root.Collection("Users");
        Stages = Root.Collection("Stages");
    }

    private static FirebaseFirestore Root { get; set; }
    private static CollectionReference Users { get; set; }
    private static CollectionReference Stages { get; set; }
    private static DocumentReference Me => Users.Document(GameData.User.ID.ToString());

    // ユーザーを登録する
    public static async Task RegisterUser(User user)
    {
        if (!Available) throw GameException.FirebaseUnavailable;

        IDType id;
        DocumentReference entry;
        DocumentSnapshot val;
        do
        {
            id = IDType.Generate();
            entry = Users.Document(id.ToString());
            val = await entry.GetSnapshotAsync();
        } while (val.Exists);

        user.ID = id;
        await entry.SetAsync(new UserZip(user));
    }

    // ユーザー名を変更する
    public static async Task ChangeUserName(string newName)
    {
        if (!Available) throw GameException.FirebaseUnavailable;

        await Me.UpdateAsync(new Dictionary<string, object>()
            { { UserZip.GetKey(UserParams.Name), newName } });
        GameData.User.Name = newName;
        GameData.Save();
    }

    // ステージを公開する
    public static async Task PublishStage(Stage stage)
    {
        if (!Available) throw GameException.FirebaseUnavailable;

        IDType id;
        DocumentReference entry;
        DocumentSnapshot val;
        do
        {
            id = IDType.Generate();
            entry = Stages.Document(id.ToString());
            val = await entry.GetSnapshotAsync();
        } while (val.Exists);

        stage.ID = id;
        stage.PublishedDate = DateTime.Now.ToUniversalTime();
        await entry.SetAsync(new StageZip(stage));
        stage.LocalData.IsPublished = true;

        // ユーザーデータのPublishedStagesに追加
        GameData.User.PublishedStages.Add(id);

        // PublishedStagesをすべて更新する
        var ids = new IDTypeCollection(GameData.User.PublishedStages);
        await Me.UpdateAsync(UserZip.GetKey(UserParams.PublishedStages), ids.GetRawList());

        GameData.Save();
    }

    // ステージを非公開にする
    public static async Task UnpublishStage(Stage stage)
    {
        if (!Available) throw GameException.FirebaseUnavailable;

        await Stages.Document(stage.ID.ToString()).DeleteAsync();
        stage.LocalData.IsPublished = false;

        // ユーザーデータのPublishedStagesから削除
        GameData.User.PublishedStages.Remove(stage.ID);

        // PublishedStagesをすべて更新する
        var ids = new IDTypeCollection(GameData.User.PublishedStages);
        await Me.UpdateAsync(UserZip.GetKey(UserParams.PublishedStages), ids.GetRawList());

        GameData.Save();
    }

    // 直近に取得した最後のステージ
    private static DocumentSnapshot LastSnapShot;

    // 公開中のステージをkeyの値が大きい順にLIMIT個を取得する（0ページ目）
    public static async Task<List<Stage>> GetStagesAtFirstPage(string key)
    {
        if (!Available) throw GameException.FirebaseUnavailable;
        
        var stages = new List<StageZip>();
        var values = await Stages.OrderByDescending(key).Limit(StageViewContentOperator.STAGE_LIMIT).GetSnapshotAsync();
        if (values.Count != 0) LastSnapShot = values.Last();

        foreach (var i in values.Documents)
        {
            stages.Add(i.ConvertTo<StageZip>());
        }

        return new StageZipCollection(stages).ToStages();
    }

    // 次のページを取得する
    public static async Task<List<Stage>> GetStagesAtNextPage(string key)
    {
        if (!Available) throw GameException.FirebaseUnavailable;
        
        var stages = new List<StageZip>();
        var values = await Stages.OrderByDescending(key).StartAfter(LastSnapShot)
            .Limit(StageViewContentOperator.STAGE_LIMIT).GetSnapshotAsync();
        LastSnapShot = values.Last();

        foreach (var i in values.Documents)
        {
            stages.Add(i.ConvertTo<StageZip>());
        }

        return new StageZipCollection(stages).ToStages();
    }

    // 公開中のステージをkeyの値が最も小さいもののIDを取得する
    // ステージが一つも存在しないときは、Emptyを返す
    public static async Task<IDType> GetLastStageID(string key)
    {
        if (!Available) throw GameException.FirebaseUnavailable;
        
        var value = await Stages.OrderBy(key).Limit(1).GetSnapshotAsync();

        if (value.Count == 0) return IDType.Empty;
        else return value.First().ConvertTo<StageZip>().ToStage().ID;
    }

    // 作ったステージのローカルデータを更新
    public static async Task UpdateMyStages()
    {
        if (!Available) throw GameException.FirebaseUnavailable;

        // 公開中のステージを全て取得
        var published = GameData.Stages.Where(i => i.LocalData.IsPublished);
        var tasks = new List<Task<DocumentSnapshot>>();

        foreach (var stage in published)
            tasks.Add(Stages.Document(stage.ID.ToString()).GetSnapshotAsync());
        var vals = await Task.WhenAll(tasks);

        for (int i = 0; i < published.Count(); ++i)
        {
            var stage = vals[i].ConvertTo<StageZip>().ToStage();
            published.ElementAt(i).ClearCount = stage.ClearCount;
            published.ElementAt(i).ChallengeCount = stage.ChallengeCount;
            published.ElementAt(i).PosEvaCount = stage.PosEvaCount;
            published.ElementAt(i).NegEvaCount = stage.NegEvaCount;
        }
    }

    // ユーザーデータを同期
    public static async Task SyncUser()
    {
        if (!Available) throw GameException.FirebaseUnavailable;

        var user = (await Me.GetSnapshotAsync()).ConvertTo<UserZip>().ToUser();
        User.Sync(GameData.User, user);
        await Me.SetAsync(new UserZip(user));
    }

    // ステージのIDからステージを取得
    public static async Task<Stage> GetStage(IDType stageID)
    {
        var res = await Stages.Document(stageID.ToString()).GetSnapshotAsync();
        return res.ConvertTo<StageZip>().ToStage();
    }

    // ユーザーIDからユーザーを取得
    public static async Task<User> GetUser(IDType userID)
    {
        var res = await Users.Document(userID.ToString()).GetSnapshotAsync();
        if (!res.Exists) return User.NotFound;
        return res.ConvertTo<UserZip>().ToUser();
    }

    // ステージのキーに対応する数字を+1する
    private static async Task IncrementCount(IDType stageID, StageParams par)
    {
        await Stages.Document(stageID.ToString()).UpdateAsync(StageZip.GetKey(par), FieldValue.Increment(1));
    }

    // Userのパラメータに対応する数字を+1する
    private static async Task IncrementUserCount(IDType userID, UserParams par)
    {
        await Users.Document(userID.ToString()).UpdateAsync(UserZip.GetKey(par), FieldValue.Increment(1));
    }

    // Userのパラメータに対応する数字を-1する
    private static async Task DecrementUserCount(IDType userID, UserParams par)
    {
        await Users.Document(userID.ToString()).UpdateAsync(UserZip.GetKey(par), FieldValue.Increment(-1));
    }

    // ステージのクリア回数を+1する
    public static async Task IncrementClearCount(Stage stage)
    {
        ++stage.ClearCount;
        if (GameData.User.LocalData.ClearedIDs.Contains(stage.ID))
        {
            await IncrementCount(stage.ID, StageParams.ClearCount);
        }
        else
        {
            await Task.WhenAll(IncrementCount(stage.ID, StageParams.ClearCount),
                IncrementUserCount(GameData.User.ID, UserParams.ClearCount));
            GameData.User.LocalData.ClearedIDs.Add(stage.ID);
        }

        GameData.Save();
    }

    // ステージのチャレンジ回数を+1する
    public static async Task IncrementChallengeCount(Stage stage)
    {
        ++stage.ChallengeCount;
        await Task.WhenAll(IncrementCount(stage.ID, StageParams.ChallengeCount),
            IncrementUserCount(stage.AuthorID, UserParams.ChallengedCount));
    }

    // ステージの高評価数を+1する
    public static async Task IncrementPosEvaCount(Stage stage)
    {
        ++stage.PosEvaCount;
        await Task.WhenAll(IncrementCount(stage.ID, StageParams.PosEvaCount),
            IncrementUserCount(stage.AuthorID, UserParams.PosEvaCount));
        
        GameData.User.LocalData.PosEvaIDs.Add(stage.ID);
        GameData.Save();
    }

    // ステージの低評価数を+1する
    public static async Task IncrementNegEvaCount(Stage stage)
    {
        ++stage.NegEvaCount;
        await IncrementCount(stage.ID, StageParams.NegEvaCount);

        GameData.User.LocalData.NegEvaIDs.Add(stage.ID);
        GameData.Save();
    }

    // ユーザーをお気に入りに登録する
    public static async Task IncrementFavoredCount(User user)
    {
        ++user.FavoredCount;
        await IncrementUserCount(user.ID, UserParams.FavoredCount);

        GameData.User.LocalData.FavorUserIDs.Add(user.ID);
        GameData.Save();
    }

    // ユーザーをお気に入りから解除する
    public static async Task DecrementFavoredCount(User user)
    {
        --user.FavoredCount;
        await DecrementUserCount(user.ID, UserParams.FavoredCount);

        GameData.User.LocalData.FavorUserIDs.Remove(user.ID);
        GameData.Save();
    }
}
