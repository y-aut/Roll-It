using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // 公開日時
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

    // 新規作成
    public Stage()
    {
        LocalData = new StageLocal();
        LocalData.Initialize();
        Structs = new SerializableList<Structure>()
        {
            new Structure(StructureType.Start, new Vector3Int(0,0,0), new Vector3Int(4,1,4), this),
            new Structure(StructureType.Goal, new Vector3Int(0,0,20), new Vector3Int(4,1,4), this),
        };
        AuthorID = GameData.User.ID;
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
        if (item.IsOnlyOne)
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
    public bool CheckSpace(Vector3Int posInt, Vector3Int scaleInt)
    {
        // 各頂点の座標を取得
        var nega = posInt - scaleInt;
        var posi = posInt + scaleInt;

        // Stageの範囲内か
        if (!(nega.IsAllMoreThanOrEqual(-GameConst.STAGE_LIMIT) && posi.IsAllLessThan(GameConst.STAGE_LIMIT)))
            return false;

        // Startの真上にないか
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

    public void Initialize()
    {
    }
}

public enum EvaluationEnum
{
    None, Good, Neutral, Bad,
}