using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ステージはマス目によって構成される。
[Serializable]
public class Stage : ISerializationCallbackReceiver
{
    // ローカルデータ
    [NonSerialized]
    public StageLocal LocalData;

    // ステージID（公開時に一意のIDを取得する）
    [SerializeField]
    public IDType ID;

    // ステージ名
    [SerializeField]
    public string Name = "New Stage";

    // 作者ID
    [SerializeField]
    private IDType AuthorID;

    // 公開日時
    [SerializeField]
    public DateTime PublishedDate;

    // クリア人数
    [SerializeField]
    public int ClearCount;

    // チャレンジ人数
    [SerializeField]
    public int ChallengeCount;

    // 各評価の人数
    [SerializeField]
    public int PosEvaCount; // Positive Evaluation Count
    [SerializeField]
    public int NeuEvaCount; // Neutral Evaluation Count
    [SerializeField]
    public int NegEvaCount; // Negative Evaluation Count

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

    [NonSerialized]
    private List<Structure> Structs;
    [SerializeField]
    private SerializableList<Structure> SavedStructs;   // 保存されるStructures

    // Structs[0]はスタート、[1]はゴール
    public Structure Start => Structs[0];
    public Structure Goal => Structs[1];
    public Structure Ball => Structs.Find(i => i.Type == StructureType.Ball);

    // FromJsonは引数なしのコンストラクタを自動で呼び出すので、新規ステージの初期化部は分ける
    public void Initialize()
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

    public void OnBeforeSerialize()
    {
        // 保存しないStructureは除外
        SavedStructs = new SerializableList<Structure>();
        foreach (var i in Structs)
            if (i.IsSaved) SavedStructs.Add(i);
    }

    public void OnAfterDeserialize()
    {
        Structs = SavedStructs;
        Structs.ForEach(i => { i.Parent = this; i.OnAfterDeserialize(); });
    }

    // posにscaleのオブジェクトを設置可能かどうか
    public bool CheckSpace(Vector3Int posInt, Vector3Int scaleInt)
    {
        // 各頂点の座標を取得
        var nega = posInt - scaleInt;
        var posi = posInt + scaleInt;

        // Stageの範囲内か
        if ((nega - new Vector3Int(GameConst.X_NLIMIT, GameConst.Y_NLIMIT, GameConst.Z_NLIMIT)).NegativeExists()
            || (new Vector3Int(GameConst.X_PLIMIT, GameConst.Y_PLIMIT, GameConst.Z_PLIMIT) - posi).NegativeExists())
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