using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ユーザー情報を表すクラス
public class User
{
    // ユーザーID
    public IDType ID = IDType.Empty;

    // ユーザー名
    public string Name = "";

    // 所持しているコインの数
    public int Coin = 0;

    // 購入したコインの総数
    public int PurchasedCoin = 0;

    // アカウントの状態
    public UserType Type = UserType.None;

    // アカウント作成日時(UTC)
    public DateTime StartDate;

    // 最終ログイン日時(UTC)
    public DateTime LastDate;

    // クリアしたステージ数（重複なし）
    public int ClearCount = 0;

    // 作ったコースが遊ばれた回数
    public int ChallengedCount = 0;

    // 総高評価数
    public int PosEvaCount = 0;

    // お気に入り登録された回数
    public int FavoredCount = 0;

    // 公開したステージ
    public List<IDType> PublishedStages;

    // 使用中のボール
    public int ActiveBallTexture = 0;

    // ローカルデータ
    public UserLocal LocalData;

    public bool IsNotFound { get; private set; } = false;
    public static User NotFound(IDType id) => new User() { ID = id, Name = "Not Found", IsNotFound = true };

    public User()
    {
        StartDate = DateTime.Now;
        PublishedStages = new List<IDType>();
        LocalData = new UserLocal();
    }

    // UserZipからの解凍時に用いる
    public User(IDType id, string name, int coin, int purchased, UserType type, DateTime start, DateTime last,
        int clear, int challenged, int posEva, int favored, IDTypeCollection published, int ballTexture)
    {
        ID = id;
        Name = name;
        Coin = coin;
        PurchasedCoin = purchased;
        Type = type;
        StartDate = start;
        LastDate = last;
        ClearCount = clear;
        ChallengedCount = challenged;
        PosEvaCount = posEva;
        FavoredCount = favored;
        PublishedStages = published.ToList();
        ActiveBallTexture = ballTexture;
    }

    public void Login()
    {
        LastDate = DateTime.Now;
    }

    // ローカルデータとサーバーデータを同期し、更新
    public static void Sync(User local, User server)
    {
        // ID, StartDateはIDの作成時に同期している
        // Nameは同期されていない可能性あり？-- オフラインでの名前変更を許す？
        server.Name = local.Name;
        // Coin, PurchasedCoin, Type, PublishedStagesは変更時に同期している
        // Login()はオフラインでも出来たほうが良いので、LastDateはここで同期する
        server.LastDate = local.LastDate;
        // カウンタ系はここで同期する
        local.ClearCount = server.ClearCount;
        local.ChallengedCount = server.ChallengedCount;
        local.PosEvaCount = server.PosEvaCount;
        local.FavoredCount = server.FavoredCount;
    }
}

// ローカルに保存するユーザーデータ
[Serializable]
public class UserLocal
{
    // クリアしたステージのIDリスト
    [SerializeField]
    public List<IDType> ClearedIDs;

    // 高評価したステージのIDリスト
    [SerializeField]
    public List<IDType> PosEvaIDs;

    // 低評価したステージのIDリスト
    [SerializeField]
    public List<IDType> NegEvaIDs;

    // お気に入りしたユーザー
    [SerializeField]
    public List<IDType> FavorUserIDs;

    public UserLocal()
    {
        ClearedIDs = new List<IDType>();
        PosEvaIDs = new List<IDType>();
        NegEvaIDs = new List<IDType>();
        FavorUserIDs = new List<IDType>();
    }
}

// アカウントのプラン
[Serializable]
public enum UserType
{
    None,           // 無課金
    AdBlock,        // 広告非表示
}

// Userクラスのもつパラメータ
public enum UserParams
{
    ID, Name, Coin, PurchasedCoin, Type, StartDate, LastDate, ClearCount, ChallengedCount,
    PosEvaCount, FavoredCount, PublishedStages, ActiveBallTexture
}