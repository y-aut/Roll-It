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

    // 所持しているMoney
    public Money Money = Money.Free;

    // 購入したMoneyの総数
    public Money PurchasedMoney = Money.Free;

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
    public int ActiveBallNo;

    // ローカルデータ
    public UserLocal LocalData;

    public bool IsNotFound { get; private set; } = false;
    public static User NotFound(IDType id) => new User() { ID = id, Name = "Not Found", IsNotFound = true };

    public User()
    {
        StartDate = DateTime.Now;
        PublishedStages = new List<IDType>();
        LocalData = new UserLocal();
        ActiveBallNo = StructureType.Ball.GetStructureNos()[0];
    }

    // UserZipからの解凍時に用いる
    public User(IDType id, string name, Money money, Money purchased, UserType type, DateTime start, DateTime last,
        int clear, int challenged, int posEva, int favored, IDTypeCollection published, int ballNo)
    {
        ID = id;
        Name = name;
        Money = money;
        PurchasedMoney = purchased;
        Type = type;
        StartDate = start;
        LastDate = last;
        ClearCount = clear;
        ChallengedCount = challenged;
        PosEvaCount = posEva;
        FavoredCount = favored;
        PublishedStages = published.ToList();
        ActiveBallNo = ballNo;
    }

    public void Login()
    {
        LastDate = DateTime.Now;
    }

    // ローカルデータとサーバーデータを同期し、更新
    public static void Sync(User local, User server)
    {
        // ID, StartDateはIDの作成時に同期している
        server.Name = local.Name;
        server.ActiveBallNo = local.ActiveBallNo;
        // PublishedStagesは変更時に同期している
        server.Money = local.Money;
        server.PurchasedMoney = local.PurchasedMoney;
        server.Type = local.Type;
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
    ID, Name, Money, PurchasedMoney, Type, StartDate, LastDate, ClearCount, ChallengedCount,
    PosEvaCount, FavoredCount, PublishedStages, ActiveBallNo
}