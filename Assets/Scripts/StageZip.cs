using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Stageを圧縮したクラス
[Serializable]
public class StageZip
{
    // ステージID
    [SerializeField]
    private IDType i;

    // ステージ名
    [SerializeField]
    private string n;

    // 作者ID
    [SerializeField]
    private IDType a;

    // 公開日時
    [SerializeField]
    private DateTime p;

    // クリア人数
    [SerializeField]
    private int c;

    // チャレンジ人数
    [SerializeField]
    private int t;

    // 高評価数
    [SerializeField]
    private int l;

    // 高評価数
    [SerializeField]
    private int d;

    // Structures
    [SerializeField]
    private StructureZipCollection s;

    public StageZip(Stage src)
    {
        i = src.ID;
        n = src.Name;
        a = src.AuthorID;
        p = src.PublishedDate;
        c = src.ClearCount;
        t = src.ChallengeCount;
        l = src.PosEvaCount;
        d = src.NegEvaCount;

        var saved = new List<StructureZip>();
        foreach (var str in src.Structs)
            if (str.IsSaved) saved.Add(new StructureZip(str));
        s = new StructureZipCollection(saved);
    }

    public Stage ToStage()
    {
        return new Stage(i, n, a, p, c, t, l, d, s);
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
