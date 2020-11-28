using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 複数のPrimitiveを一つにまとめた構造体
// 抽象クラスにするとSerializeできないので非抽象クラス
[Serializable]
public class Structure : ISerializationCallbackReceiver
{
    const float BRIDGE_THICKNESS = 0.5f;    // Bridgeの厚み

    [SerializeField]
    private StructureType _type;
    public StructureType Type { get => _type; private set => _type = value; }

    [NonSerialized]
    private List<Primitive> objs;

    // Transform
    // 矢印などの移動やサイズ変更に関するオブジェクトはこの直方体を基準にして設置する
    [SerializeField]
    private Vector3Int _positionInt;
    public Vector3Int PositionInt
    {
        get => _positionInt;
        set
        {
            _positionInt = value;
            UpdateObjects();
        }
    }

    [SerializeField]
    private Vector3Int _localScaleInt;
    public Vector3Int LocalScaleInt
    {
        get => _localScaleInt;
        set
        {
            _localScaleInt = value;
            UpdateObjects();
        }
    }

    [SerializeField]
    private RotationEnum _rotationInt = RotationEnum.IDENTITY;
    public RotationEnum RotationInt
    {
        get => _rotationInt;
        set
        {
            _rotationInt = value;
            UpdateObjects();
        }
    }

    // float型で取得/設定
    public Vector3 Position
    {
        get => (Vector3)PositionInt / GameConst.POSITION_SCALE;
        set => PositionInt = ToPositionInt(value);
    }

    public Vector3 LocalScale
    {
        get => (Vector3)LocalScaleInt / GameConst.LOCALSCALE_SCALE;
        set => LocalScaleInt = ToLocalScaleInt(value);
    }

    public Quaternion Rotation => RotationInt.ToQuaternion();

    // 位置、サイズについて、整数値に変換
    public static Vector3Int ToPositionInt(Vector3 pos) =>
        new Vector3Int(Mathf.RoundToInt(pos.x * GameConst.POSITION_SCALE),
            Mathf.RoundToInt(pos.y * GameConst.POSITION_SCALE),
            Mathf.RoundToInt(pos.z * GameConst.POSITION_SCALE));

    public static Vector3Int ToLocalScaleInt(Vector3 scale) =>
        new Vector3Int(Mathf.RoundToInt(scale.x * GameConst.LOCALSCALE_SCALE),
            Mathf.RoundToInt(scale.y * GameConst.LOCALSCALE_SCALE),
            Mathf.RoundToInt(scale.z * GameConst.LOCALSCALE_SCALE));

    // 位置、サイズについて、float値に変換
    public static Vector3 ToPositionF(Vector3Int pos) => (Vector3)pos / GameConst.POSITION_SCALE;
    public static Vector3 ToLocalScaleF(Vector3Int scale) => (Vector3)scale / GameConst.LOCALSCALE_SCALE;

    // コンストラクタ
    public Structure(StructureType type, Vector3Int pos, Vector3Int scale)
    {
        Type = type;
        _positionInt = pos;
        _localScaleInt = scale;

        SetObjs();
        UpdateObjects();
    }

    // objsにPrimitiveをセットする。位置等はUpdateObjects()で設定するのでしなくて良い
    private void SetObjs() => SetObjs(Type);

    // こちらは直接呼ばない
    private void SetObjs(StructureType type)
    {
        switch (type)
        {
            case StructureType.Floor:
                {
                    var floor = new Primitive(PrimitiveType.Cube);
                    objs = new List<Primitive>() { floor };
                }
                break;
            case StructureType.Start:
                SetObjs(StructureType.Floor);
                break;
            case StructureType.Goal:
                SetObjs(StructureType.Floor);
                break;
            case StructureType.Bridge:
                SetObjs(StructureType.Floor);
                break;
        }

        // Parentをこのオブジェクトに設定
        objs.ForEach(i => i.Parent = this);

    }

    // 位置、サイズに合わせてオブジェクトを更新
    private void UpdateObjects() => UpdateObjects(Type);

    // こちらは直接呼ばない
    private void UpdateObjects(StructureType type)
    {
        switch (type)
        {
            case StructureType.Floor:
                objs[0].Position = Position;
                objs[0].LocalScale = LocalScale;
                break;
            case StructureType.Start:
                UpdateObjects(StructureType.Floor);
                break;
            case StructureType.Goal:
                UpdateObjects(StructureType.Floor);
                break;
            case StructureType.Bridge:
                {
                    objs[0].Position = Position;
                    switch (RotationInt)
                    {
                        case RotationEnum.IDENTITY: // Z-からZ+に下るように配置
                            objs[0].LocalScale = new Vector3(LocalScale.x, BRIDGE_THICKNESS, LocalScale.NewX(0).magnitude);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), new Vector3(0, -LocalScale.y, LocalScale.z));
                            break;
                        case RotationEnum.Y90:  // X-からX+
                            objs[0].LocalScale = new Vector3(LocalScale.NewZ(0).magnitude, BRIDGE_THICKNESS, LocalScale.z);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(LocalScale.x, -LocalScale.y, 0));
                            break;
                        case RotationEnum.Y180: // Z+からZ-
                            objs[0].LocalScale = new Vector3(LocalScale.x, BRIDGE_THICKNESS, LocalScale.NewX(0).magnitude);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), new Vector3(0, LocalScale.y, LocalScale.z));
                            break;
                        case RotationEnum.Y270: // X+からX-
                            objs[0].LocalScale = new Vector3(LocalScale.NewZ(0).magnitude, BRIDGE_THICKNESS, LocalScale.z);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(LocalScale.x, LocalScale.y, 0));
                            break;
                    }
                    // 上面が正確な位置にくるようにY座標を調整
                    objs[0].Position += objs[0].Rotation * Vector3.down * (objs[0].LocalScale.y / 2);
                }
                break;
        }
    }

    // ワールドに生成
    public void Create()
    {
        objs.ForEach(i => i.Create());
    }

    // 各Primitiveにactionを作用させる。Destroy時に必要
    public void ForEach(Action<GameObject> action)
    {
        objs.ForEach(i => i.Act(action));
    }

    // 半透明に
    public void Fade() => objs.ForEach(i => i.Fade());

    // 不透明に
    public void Opaque() => objs.ForEach(i => i.Opaque());

    // オブジェクト選択時、移動を表す矢印を表示する点(X+,Y+,Z+,X-,Y-,Z-)
    public void GetArrowRoots(out List<Vector3> roots)
    {
        roots = new List<Vector3>() {
            Position + new Vector3(LocalScale.x / 2, 0, 0),
            Position + new Vector3(0, LocalScale.y / 2, 0),
            Position + new Vector3(0, 0, LocalScale.z / 2),
            Position - new Vector3(LocalScale.x / 2, 0, 0),
            Position - new Vector3(0, LocalScale.y / 2, 0),
            Position - new Vector3(0, 0, LocalScale.z / 2),
        };
    }

    // オブジェクト選択時、XZ平面上でのサイズ変更を表す辺上の印を表示する点((X+,Z+,X-,Z-)*(Y+,Y-))
    public void GetXZResizeEdges(out List<Vector3> points)
    {
        points = new List<Vector3>() {
            Position + new Vector3(LocalScale.x / 2, LocalScale.y / 2, 0),
            Position + new Vector3(0, LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(-LocalScale.x / 2, LocalScale.y / 2, 0),
            Position + new Vector3(0, LocalScale.y / 2, -LocalScale.z / 2),
            Position + new Vector3(LocalScale.x / 2, -LocalScale.y / 2, 0),
            Position + new Vector3(0, -LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(-LocalScale.x / 2, -LocalScale.y / 2, 0),
            Position + new Vector3(0, -LocalScale.y / 2, -LocalScale.z / 2),
        };
    }

    // オブジェクト選択時、Y方向のサイズ変更を表す辺上の印を表示する点((X+Z+,X-Z+,X-Z-,X+Z-)*(Y+,Y-))
    public void GetYResizeVertexes(out List<Vector3> points)
    {
        points = new List<Vector3>() {
            Position + new Vector3(LocalScale.x / 2, LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(-LocalScale.x / 2, LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(-LocalScale.x / 2, LocalScale.y / 2, -LocalScale.z / 2),
            Position + new Vector3(LocalScale.x / 2, LocalScale.y / 2, -LocalScale.z / 2),
            Position + new Vector3(LocalScale.x / 2, -LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(-LocalScale.x / 2, -LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(-LocalScale.x / 2, -LocalScale.y / 2, -LocalScale.z / 2),
            Position + new Vector3(LocalScale.x / 2, -LocalScale.y / 2, -LocalScale.z / 2),
        };
    }

    // オブジェクト選択時、回転を表す矢印を表示する点
    public Vector3 GetRotateArrowPos()
        => Position + new Vector3(0, LocalScale.y / 2 + 0.5f, 0);


    //// Y方向のサイズ変更が可能か
    //public bool IsYResizable()
    //{
    //    switch (Type)
    //    {
    //        case StructureType.Floor:
    //        case StructureType.Start:
    //        case StructureType.Goal:
    //            return false;
    //        default:
    //            return true;
    //    }
    //}

    // 回転可能か
    public bool IsRotatable => Type == StructureType.Bridge;

    // ステージから削除できるか
    public bool IsDeletable => !(Type == StructureType.Start || Type == StructureType.Goal);

    // Primitiveがクリックされた時にtrueになる
    public bool Clicked { get; set; }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        SetObjs();
        UpdateObjects();
    }
}

// Structureの種類
[Serializable]
public enum StructureType
{
    Floor,      // 壁なし床
    Start,      // スタート
    Goal,       // ゴール
    Bridge,     // 通路（傾く）
}

[Serializable]
public enum RotationEnum
{
    IDENTITY, Y90, Y180, Y270, 
}
