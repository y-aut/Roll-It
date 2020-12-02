using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Structureの種類
// 追加するときは、Img, CreateOperator.ItemDragged, 各種List, SetObjs, UpdateObjects, GenerationIncrementedを更新する
[Serializable]
public enum StructureType
{
    Floor,      // 壁なし床
    Start,      // スタート
    Goal,       // ゴール
    Board,      // 通路（傾く）
    Plate,      // 円盤
    Slope,      // 三角柱の坂
    Arc,        // 円弧状の坂
    Viewpoint,  // 視点回転の矢印
    Lift,       // リフト

}

[Serializable]
public enum RotationEnum
{
    IDENTITY, Y90, Y180, Y270, NB,
}

// 複数のPrimitiveを一つにまとめた構造体
// 抽象クラスにするとSerializeできないので非抽象クラス
[Serializable]
public class Structure
{
    const float BOARD_THICKNESS = 0.2f;     // Boardの厚み
    const float LIFT_THICKNESS = 0.2f;      // Liftの厚み

    const int LIFT_PERIOD = 300;            // Liftの周期(f)

    // 回転可能なStructureType
    public static List<StructureType> RotatableList
        => new List<StructureType>() {
            StructureType.Board,
            StructureType.Slope,
            StructureType.Arc,
            StructureType.Viewpoint,
        };

    // Y軸方向にリサイズ可能なStructureType
    public static List<StructureType> YNonresizableList
        => new List<StructureType>() {
            StructureType.Viewpoint,
        };

    // Y軸方向に反転可能なStructureType
    public static List<StructureType> YInversableList
        => new List<StructureType>() {
            StructureType.Lift,
        };

    // 衝突判定をするか
    public static List<StructureType> DetectCollisionList
        => new List<StructureType>() {
            StructureType.Viewpoint,
        };

    // 動的オブジェクトか
    public static List<StructureType> DynamicList
        => new List<StructureType>() {
            StructureType.Lift,
        };

    [SerializeField]
    private StructureType _type;
    public StructureType Type { get => _type; private set => _type = value; }

    [NonSerialized]
    private List<Primitive> objs;

    [NonSerialized]
    private Stage _parent;
    public Stage Parent { get => _parent; set => _parent = value; }

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
    private Vector3Int _localScaleInt;      // Inverseの情報を含むので、負の値になりうる
    public Vector3Int LocalScaleInt         // 負の値にならない
    {
        get => _localScaleInt.Abs();
        set
        {
            if (YInversed)
                _localScaleInt = value.YMinus();
            else
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

    // 反転はLocalScaleの符号で表す
    public bool YInversed
    {
        get => _localScaleInt.y < 0;
        set
        {
            if (YInversed == value) return;
            _localScaleInt = _localScaleInt.YMinus();
        }
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
    public Structure(StructureType type, Vector3Int pos, Vector3Int scale, Stage parent)
    {
        Type = type;
        _positionInt = pos;
        _localScaleInt = scale;
        Parent = parent;

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
                objs = new List<Primitive>() { new Primitive(PrimitiveType.Cube) };
                break;
            case StructureType.Start:
                SetObjs(StructureType.Floor);
                break;
            case StructureType.Goal:
                SetObjs(StructureType.Floor);
                break;
            case StructureType.Board:
                SetObjs(StructureType.Floor);
                break;
            case StructureType.Plate:
                objs = new List<Primitive>() { new Primitive(PrimitiveType.Cylinder) };
                break;
            case StructureType.Slope:
                objs = new List<Primitive>() { new Primitive(Prefabs.SlopePrefab) };
                break;
            case StructureType.Arc:
                objs = new List<Primitive>() { new Primitive(Prefabs.ArcPrefab) };
                break;
            case StructureType.Viewpoint:
                objs = new List<Primitive>() { new Primitive(Prefabs.ViewpointArrow) };
                break;
            case StructureType.Lift:
                SetObjs(StructureType.Floor);
                objs[0].AddRigidbody(1f, true);
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
            case StructureType.Board:
                {
                    objs[0].Position = Position;
                    switch (RotationInt)
                    {
                        case RotationEnum.IDENTITY: // X+からX-に下るように配置
                            objs[0].LocalScale = new Vector3(LocalScale.NewZ(0).magnitude, BOARD_THICKNESS, LocalScale.z);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(LocalScale.x, LocalScale.y, 0));
                            break;
                        case RotationEnum.Y90: // Z-からZ+
                            objs[0].LocalScale = new Vector3(LocalScale.x, BOARD_THICKNESS, LocalScale.NewX(0).magnitude);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), new Vector3(0, -LocalScale.y, LocalScale.z));
                            break;
                        case RotationEnum.Y180:  // X-からX+
                            objs[0].LocalScale = new Vector3(LocalScale.NewZ(0).magnitude, BOARD_THICKNESS, LocalScale.z);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(LocalScale.x, -LocalScale.y, 0));
                            break;
                        case RotationEnum.Y270: // Z+からZ-
                            objs[0].LocalScale = new Vector3(LocalScale.x, BOARD_THICKNESS, LocalScale.NewX(0).magnitude);
                            objs[0].Rotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), new Vector3(0, LocalScale.y, LocalScale.z));
                            break;
                    }
                    // 上面が正確な位置にくるようにY座標を調整
                    objs[0].Position += objs[0].Rotation * Vector3.down * (objs[0].LocalScale.y / 2);
                }
                break;
            case StructureType.Plate:
                objs[0].Position = Position;
                objs[0].LocalScale = LocalScale.NewY(LocalScale.y / 2);
                break;
            case StructureType.Slope:
                {
                    objs[0].Position = Position;
                    // Quaternion.Euler(90, 0, 0)をかけるとX+からX-に下る
                    objs[0].Rotation = Rotation * Quaternion.Euler(90, 0, 0);
                    objs[0].LocalScale = (Quaternion.Inverse(objs[0].Rotation) * LocalScale).Abs();
                }
                break;
            case StructureType.Arc:
                {
                    // Quaternion.Euler(180, 0, 0)をかけるとX+からX-に下る
                    objs[0].Rotation = Rotation * Quaternion.Euler(180, 0, 0);
                    objs[0].LocalScale = (Quaternion.Inverse(objs[0].Rotation) * LocalScale).Abs();
                    // 内面が正確な位置にくるようにY座標を調整
                    objs[0].Position = Position + objs[0].Rotation * new Vector3(0.05f * objs[0].LocalScale.x, 0.05f * objs[0].LocalScale.y, 0);
                }
                break;
            case StructureType.Viewpoint:
                {
                    objs[0].Rotation = Rotation;
                    objs[0].LocalScale = (Quaternion.Inverse(objs[0].Rotation) * LocalScale).Abs();
                    objs[0].Position = Position - ToLocalScaleF(new Vector3Int(0, 1, 0)) / 2;
                }
                break;
            case StructureType.Lift:
                {
                    objs[0].LocalScale = new Vector3(LocalScale.x, LIFT_THICKNESS, LocalScale.z);
                    // y = Asin(ωt): A = LocalScale.y / 2, ω = 2 * Mathf.PI / LIFT_PERIOD
                    objs[0].Position = Position
                        + new Vector3(0, LocalScale.y * (YInversed ? -1 : 1) / 2 * Mathf.Sin(2 * Mathf.PI * Parent.Generation / LIFT_PERIOD)
                        - LIFT_THICKNESS / 2, 0);
                }
                break;
        }
    }

    // Generationに合わせてオブジェクトを更新
    public void GenerationIncremented() => GenerationIncremented(Type);

    // こちらは直接呼ばない
    private void GenerationIncremented(StructureType type)
    {
        switch (type)
        {
            case StructureType.Lift:
                {
                    objs[0].MovePosition(Position
                        + new Vector3(0, LocalScale.y * (YInversed ? -1 : 1) / 2 * Mathf.Sin(2 * Mathf.PI * Parent.Generation / LIFT_PERIOD)
                        - LIFT_THICKNESS / 2, 0));
                }
                break;
        }
    }

    // ワールドに生成
    public void Create()
    {
        objs.ForEach(i => i.Create());
        if (DetectsCollision) objs.ForEach(i => i.SetCollisionEvent());
    }

    // ワールドから削除
    public void Destroy() => objs.ForEach(i => i.Destroy());

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
        => Position + new Vector3(0, LocalScale.y / 2 + 1f, 0);

    // オブジェクト選択時、Y方向の反転を表す矢印を表示する点
    public Vector3 GetYInverseArrowPos()
        => Position + new Vector3(LocalScale.x / 2 + 0.8f, 0, 0);


    // Y方向のサイズ変更が可能か
    public bool IsYResizable => !YNonresizableList.Contains(Type);

    // 回転可能か
    public bool IsRotatable => RotatableList.Contains(Type);

    // Y軸方向に反転可能か
    public bool IsYInversable => YInversableList.Contains(Type);

    // 衝突判定を行うか
    public bool DetectsCollision => DetectCollisionList.Contains(Type);

    // 動的オブジェクトか
    public bool IsDynamic => DynamicList.Contains(Type);

    // ステージから削除できるか
    public bool IsDeletable => !(Type == StructureType.Start || Type == StructureType.Goal);

    // Primitiveがクリックされた時にtrueになる
    public bool Clicked { get; set; }

    // ボールと衝突した時にtrueになる
    public bool Collided { get; set; }

    // Stageから呼ばれる
    public void OnAfterDeserialize()
    {
        SetObjs();
        UpdateObjects();
    }
}
