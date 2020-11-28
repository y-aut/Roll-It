using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ステージはマス目によって構成される。
[Serializable]
public class Stage
{
    [SerializeField]
    private int _id;
    public int ID { get => _id; private set => _id = value; }
    public static int MaxID { get; set; }

    [SerializeField]
    private SerializableList<Structure> Structs;
    // Structs[0]はスタート、[1]はゴール

    public Structure Start => Structs[0];
    public Structure Goal => Structs[1];

    public Stage()
    {
        
    }

    // FromJsonは引数なしのコンストラクタを自動で呼び出すので、新規ステージの初期化部は分ける
    public void Initialize()
    {
        ID = ++MaxID;
        Structs = new SerializableList<Structure>()
        {
            new Structure(StructureType.Start, new Vector3Int(0,0,0), new Vector3Int(1,1,1)),
            new Structure(StructureType.Goal, new Vector3Int(0,0,10), new Vector3Int(1,1,1)),
        };
    }

    public void Create()
    {
        Structs.ForEach(i => i.Create());
    }

    public void ForEach(Action<GameObject> action)
    {
        Structs.ForEach(i => i.ForEach(action));
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

}
