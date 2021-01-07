using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

// ユーザー情報を圧縮したクラス
[Serializable]
[FirestoreData]
public class UserZip
{
    // ユーザーID
    [SerializeField]
    private IDType id;
    [FirestoreProperty("i")]
    private long ID
    {
        get => id.GetRawVal();
        set => id = new IDType((uint)value);
    }

    // ユーザー名
    [SerializeField]
    private string name;
    [FirestoreProperty("n")]
    private string Name
    {
        get => name;
        set => name = value;
    }

    // Money
    [SerializeField]
    private Money money;
    [FirestoreProperty("m")]
    private long Money_
    {
        get => money.Pack();
        set => money = Money.Unpack(value);
    }

    // Purchased Money
    [SerializeField]
    private Money purchased;
    [FirestoreProperty("M")]
    private long Purchased
    {
        get => purchased.Pack();
        set => purchased = Money.Unpack(value);
    }

    // アカウントの状態
    [SerializeField]
    private UserType userType;
    [FirestoreProperty("t")]
    private int UserType
    {
        get => (int)userType;
        set => userType = (UserType)value;
    }

    // アカウント作成日時
    [SerializeField]
    private DateTime startDate;
    [FirestoreProperty("s")]
    private Timestamp StartDate
    {
        get => Timestamp.FromDateTime(startDate);
        set => startDate = value.ToDateTime();
    }

    // 最終ログイン日時
    [SerializeField]
    private DateTime lastDate;
    [FirestoreProperty("l")]
    private Timestamp LastDate
    {
        get => Timestamp.FromDateTime(lastDate);
        set => lastDate = value.ToDateTime();
    }

    // クリアしたステージ数（重複なし）/ 作ったコースが遊ばれた回数
    [SerializeField]
    private long clear_challenged;
    [FirestoreProperty("c")]
    private long Clear_Challenged
    {
        get => clear_challenged;
        set => clear_challenged = value;
    }

    // 総高評価数 / お気に入り登録された回数
    // 高評価ランキングは欲しいので高評価を上位ビットに
    [SerializeField]
    private long poseva_favored;
    [FirestoreProperty("f")]
    private long PosEva_Favored
    {
        get => poseva_favored;
        set => poseva_favored = value;
    }

    // 公開したステージ
    [SerializeField]
    private IDTypeCollection published;
    [FirestoreProperty("p")]
    private List<long> Published
    {
        get => published.GetRawList();
        set => published = new IDTypeCollection(value);
    }

    // 使用中のボール
    [SerializeField]
    public int activeBallTexture = 0;
    [FirestoreProperty("b")]
    private int ActiveBallTexture
    {
        get => activeBallTexture;
        set => activeBallTexture = value;
    }

    public UserZip() { }

    public UserZip(User src)
    {
        id = src.ID;
        name = src.Name;
        money = src.Money;
        purchased = src.PurchasedMoney;
        userType = src.Type;
        startDate = src.StartDate;
        lastDate = src.LastDate;
        clear_challenged = AddMethod.Pack(src.ClearCount, src.ChallengedCount);
        poseva_favored = AddMethod.Pack(src.PosEvaCount, src.FavoredCount);
        published = new IDTypeCollection(src.PublishedStages);
    }

    public User ToUser()
    {
        return new User(id, name, money, purchased, userType, startDate, lastDate,
            clear_challenged.GetUpper(), clear_challenged.GetLower(), poseva_favored.GetUpper(), poseva_favored.GetLower(),
            published, activeBallTexture);
    }

    // 指定したパラメータをvだけ増やすのに足すべき値
    public static long GetIncrementValue(long v, UserParams par)
    {
        switch (par)
        {
            // 上位32bit
            case UserParams.ClearCount:
            case UserParams.PosEvaCount:
                return v << 32;
            // 下位32bit
            default:
                return v;
        }
    }

    // 指定したパラメータが含まれるキーを返す
    public static string GetKey(UserParams par)
    {
        switch (par)
        {
            case UserParams.ID:
                return "i";
            case UserParams.Name:
                return "n";
            case UserParams.Money:
                return "m";
            case UserParams.PurchasedMoney:
                return "M";
            case UserParams.Type:
                return "t";
            case UserParams.StartDate:
                return "s";
            case UserParams.LastDate:
                return "l";
            case UserParams.ClearCount:
            case UserParams.ChallengedCount:
                return "c";
            case UserParams.PosEvaCount:
            case UserParams.FavoredCount:
                return "f";
            case UserParams.PublishedStages:
                return "p";
            case UserParams.ActiveBallNo:
                return "b";
            default:
                throw GameException.Unreachable;
        }
    }
}

