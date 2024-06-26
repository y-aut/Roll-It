﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// ステージはマス目によって構成される。
public class Stage
{
    // ローカルデータ
    public StageLocal LocalData;

    // ステージID（公開時に一意のIDを取得する）
    public IDType ID;

    // ステージ名
    public string Name = "New Stage";

    // 作者ID
    public IDType AuthorID;

    // 公開日時(UTC)
    public DateTime PublishedDate;

    // クリア人数
    public int ClearCount;

    // チャレンジ人数
    public int ChallengeCount;

    // 各評価の人数
    public int PosEvaCount; // Positive Evaluation Count
    public int NegEvaCount; // Negative Evaluation Count
    public int NeuEvaCount => ClearCount - (PosEvaCount + NegEvaCount);

    // ストラクチャ
    public List<Structure> Structs;

    // クリア率
    public float ClearRate => ChallengeCount == 0 ? 0f : (float)ClearCount / ChallengeCount;

    // 高評価率
    public float PosRate => (float)PosEvaCount / (PosEvaCount + NeuEvaCount + NegEvaCount);

    // ステージが生成されてから何フレーム経過したか
    // Operatorから毎フレーム更新する
    public int Generation { get; private set; }
    public void IncrementGeneration()
    {
        ++Generation;
        Structs.ForEach(i => i.GenerationIncremented());
    }

    // Structs[0]はスタート、[1]はゴール
    public Structure Start => Structs[0];
    public Structure Goal => Structs[1];
    public Structure Ball => Structs.Find(i => i.Type == StructureType.Ball);

    public bool IsNotFound { get; private set; } = false;
    public static Stage NotFound(IDType id) => new Stage() { ID = id, Name = "Not Found", IsNotFound = true };

    // 新規作成
    public Stage()
    {
        LocalData = new StageLocal();
        Structs = new List<Structure>()
        {
            new Structure(StructureType.Start.GetStructureNos()[0], new Vector3Int(0, 0, 0), new Vector3Int(4, 1, 4), this),
            new Structure(StructureType.Goal.GetStructureNos()[0], new Vector3Int(0, 0, 20), new Vector3Int(4, 1, 4), this),
        };
        AuthorID = GameData.User.ID;
    }

    public Stage(Stage src)
    {
        ID = src.ID;
        Name = src.Name;
        AuthorID = src.AuthorID;
        PublishedDate = src.PublishedDate;
        ClearCount = src.ClearCount;
        ChallengeCount = src.ChallengeCount;
        PosEvaCount = src.PosEvaCount;
        NegEvaCount = src.NegEvaCount;
        Structs = src.Structs.Select(i => new Structure(i, this)).ToList();
        if (src.LocalData != null)
            LocalData = new StageLocal(src.LocalData);
    }

    // StageZipから解凍するときに用いる
    public Stage(IDType id, string name, IDType authorID, DateTime published, int clear, int challenge, int pos, int neg, StructureZipCollection strs)
    {
        ID = id;
        Name = name;
        AuthorID = authorID;
        PublishedDate = published;
        ClearCount = clear;
        ChallengeCount = challenge;
        PosEvaCount = pos;
        NegEvaCount = neg;
        Structs = strs.ToStructures(this);
    }

    public void Create()
    {
        Structs.ForEach(i => i.Create());
        Generation = 0;
    }

    public void Destroy()
    {
        Structs.ForEach(i => i.Destroy());
    }

    public void Add(Structure item)
    {
        if (item.Type.IsOnlyOne())
        {
            var rest = Structs.Find(i => i.Type == item.Type);
            if (rest != null)
            {
                rest.Destroy();
                Structs.Remove(rest);
            }
        }
        Structs.Add(item);
    }

    public void Delete(Structure item)
    {
        Structs.Remove(item);
    }

    // 各種カウンタをリセット
    public void ResetCount()
    {
        ClearCount = ChallengeCount = PosEvaCount = NegEvaCount = 0;
    }

    // 0でないカウンタが存在するか
    public bool CountNonZero()
    {
        return ClearCount != 0 || ChallengeCount != 0 || PosEvaCount != 0 || NegEvaCount != 0;
    }

    // クリックされたStructureを返す
    public Structure ClickedStructure()
    {
        foreach (var i in Structs)
        {
            if (i.Clicked) return i;
        }
        return null;
    }

    // 衝突したStructureを返す
    public List<Structure> CollidedStructures() => Structs.FindAll(i => i.Collided);

    // posにscaleのオブジェクトを設置可能かどうか
    public bool CheckSpaceFor(Structure str)
    {
        // 各頂点の座標を取得
        var nega = str.PositionInt - str.LocalScaleInt;
        var posi = str.PositionInt + str.LocalScaleInt;

        // Stageの範囲内か
        if (!(nega.IsAllMoreThanOrEqual(-GameConst.STAGE_LIMIT) && posi.IsAllLessThan(GameConst.STAGE_LIMIT)))
            return false;

        // Startの真上にないか
        if (str.Type == StructureType.Ball) return true;
        Vector3Int nlim = Start.PositionInt - new Vector3Int(1, -Start.LocalScaleInt.y, 1);
        Vector3Int plim = Start.PositionInt + new Vector3Int(1, 8, 1);
        // xyz各方向からみて全てで重なりがあれば、直方体同士が重なっている
        if ((posi - nlim).IsAllPositive() && (plim - nega).IsAllPositive()) return false;

        return true;
    }

    // ゲームオーバーになる高さ
    public float GameOverY => Structs.Min(i => i.Position.y - i.LocalScale.y / 2) - 5f;

}

[Serializable]
public class StageLocal
{
    // 公開しているか
    [SerializeField]
    public bool IsPublished = false;

    // クリアチェックを完了したか
    [SerializeField]
    public bool IsClearChecked = false;

    public StageLocal() { }

    public StageLocal(StageLocal src)
    {
        IsPublished = src.IsPublished;
        IsClearChecked = src.IsClearChecked;
    }
}

public enum EvaluationEnum
{
    None, Good, Neutral, Bad,
}

// Stageクラスのもつパラメータ
public enum StageParams
{
    ID, Name, AuthorID, PublishedDate, ClearCount, ChallengeCount, PosEvaCount, NegEvaCount, Structs
}