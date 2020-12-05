using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ステージはマス目によって構成される。
[Serializable]
public class Stage : ISerializationCallbackReceiver
{
    [SerializeField]
    private int _id;
    public int ID { get => _id; private set => _id = value; }
    public static int MaxID { get; set; }

    [SerializeField]
    private string _name = "New Stage";
    public string Name { get => _name; set => _name = value; }

    // ステージが生成されてから何フレーム経過したか
    // Operatorから毎フレーム更新する
    public int Generation { get; private set; }
    public void IncrementGeneration()
    {
        ++Generation;
        Structs.ForEach(i => i.GenerationIncremented());
    }

    [SerializeField]
    private SerializableList<Structure> Structs;
    // Structs[0]はスタート、[1]はゴール

    public Structure Start => Structs[0];
    public Structure Goal => Structs[1];

    // FromJsonは引数なしのコンストラクタを自動で呼び出すので、新規ステージの初期化部は分ける
    public void Initialize()
    {
        ID = ++MaxID;
        Structs = new SerializableList<Structure>()
        {
            new Structure(StructureType.Start, new Vector3Int(0,0,0), new Vector3Int(4,1,4), this),
            new Structure(StructureType.Goal, new Vector3Int(0,0,20), new Vector3Int(4,1,4), this),
        };
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
        
    }

    public void OnAfterDeserialize()
    {
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
}
