using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Firestore;

// Stageを圧縮したクラス
[Serializable]
[FirestoreData]
public class StageZip
{
    // ステージID / 作者ID
    [SerializeField]
    private IDPair id;
    [FirestoreProperty("i")]
    private long ID
    {
        get => id.GetRawVal();
        set => id = new IDPair(value);
    }

    // ステージ名
    [SerializeField]
    private string name;
    [FirestoreProperty("n")]
    private string Name
    {
        get => name;
        set => name = value;
    }

    // 公開日時
    [SerializeField]
    private DateTime publishedDate;
    [FirestoreProperty("p")]
    private Timestamp PublishedDate
    {
        get => Timestamp.FromDateTime(publishedDate);
        set => publishedDate = value.ToDateTime();
    }

    // チャレンジ人数 / クリア人数
    [SerializeField]
    private long challenge_clear;
    [FirestoreProperty("c")]
    private long Challenge_Clear
    {
        get => challenge_clear;
        set => challenge_clear = value;
    }

    // 高評価数 / 低評価数
    // 高評価ランキングは欲しいので高評価を上位ビットに
    [SerializeField]
    private long poseva_negeva;
    [FirestoreProperty("e")]
    private long PosEva_NegEva
    {
        get => poseva_negeva;
        set => poseva_negeva = value;
    }

    // Structures
    [SerializeField]
    private StructureZipCollection structs;
    [FirestoreProperty("s")]
    private List<long> Structs
    {
        get => structs.GetRawList();
        set => structs = new StructureZipCollection(value);
    }

    public StageZip() { }

    public StageZip(Stage src)
    {
        id = new IDPair(src.ID, src.AuthorID);
        name = src.Name;
        publishedDate = src.PublishedDate;
        challenge_clear = AddMethod.Pack(src.ChallengeCount, src.ClearCount);
        poseva_negeva = AddMethod.Pack(src.PosEvaCount, src.NegEvaCount);

        var saved = new List<StructureZip>();
        foreach (var str in src.Structs)
            if (str.IsSaved) saved.Add(new StructureZip(str));
        structs = new StructureZipCollection(saved);
    }

    public Stage ToStage()
    {
        return new Stage(id.ID1, name, id.ID2, publishedDate,
            challenge_clear.GetLower(), challenge_clear.GetUpper(), poseva_negeva.GetUpper(), poseva_negeva.GetLower(), structs);
    }

    // 指定したパラメータをインクリメントした値を返す
    public static long GetIncremented(long v, StageParams par)
    {
        switch (par)
        {
            // 上位32bit
            case StageParams.ChallengeCount:
            case StageParams.PosEvaCount:
                return v.GetUpperIncremented();
            // 下位32bit
            default:
                return v + 1;
        }
    }

    // AuthorIDの値を返す
    public static IDType GetAuthorID(long v) => new IDType((uint)v.GetLower());

    // 指定したカウントパラメータの値を返す
    public static int GetCountValue(long v, StageParams par)
    {
        switch (par)
        {
            // 上位32bit
            case StageParams.ChallengeCount:
            case StageParams.PosEvaCount:
                return v.GetUpper();
            // 下位32bit
            default:
                return v.GetLower();
        }
    }

    // 指定したパラメータが含まれるキー（変数名）を返す
    public static string GetKey(StageParams par)
    {
        switch (par)
        {
            case StageParams.ID:
            case StageParams.AuthorID:
                return "i";
            case StageParams.Name:
                return "n";
            case StageParams.PublishedDate:
                return "p";
            case StageParams.ClearCount:
            case StageParams.ChallengeCount:
                return "c";
            case StageParams.PosEvaCount:
            case StageParams.NegEvaCount:
                return "e";
            case StageParams.Structs:
                return "s";
            default:
                throw GameException.Unreachable;
        }
    }

}

[Serializable]
public class StageZipCollection
{
    [SerializeField]
    private List<StageZip> v;

    public StageZipCollection(List<StageZip> src)
    {
        v = src;
    }

    public static implicit operator List<StageZip>(StageZipCollection col) => col.v;

    public List<Stage> ToStages() => v.Select(i => i.ToStage()).ToList();

}
