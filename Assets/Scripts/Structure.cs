using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Structureの種類
    追加するときは、Img, CreateOperator.ItemDragged, StructureTypeToCategory, 各種List, 
    SetObjs, UpdateObjects, GenerationIncrementedを更新する
*/
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
    Angle,      // 視点回転の矢印
    Lift,       // リフト
    Ball,       // ボール
    Chopsticks, // 箸
    Jump,       // ジャンプ台
    Box,        // 軽い箱
}

// Structureの種類
public enum Category
{
    Basic,      // 基本
    Moving,     // 動く
    Movable,    // 質量をもつ
    Other,      // 特殊（視点移動など）
}

public static partial class AddMethod
{
    // StructureTypeからCategoryへの変換
    private static List<Category> StructureTypeToCategory = new List<Category>()
    {
        Category.Basic,     // Floor
        Category.Basic,     // Start
        Category.Basic,     // Goal
        Category.Basic,     // Board
        Category.Basic,     // Plate
        Category.Basic,     // Slope
        Category.Basic,     // Arc
        Category.Other,     // Angle
        Category.Moving,    // Lift
        Category.Other,     // Ball
        Category.Basic,     // Chopsticks
        Category.Other,     // Jump
        Category.Movable,   // Box
    };

    public static Category GetCategory(this StructureType type) => StructureTypeToCategory[(int)type];

    // ImgのnameからStructureTypeへの変換
    public static StructureType GetStructureTypeFromImageName(this string imgName)
    {
        return (StructureType)Enum.Parse(typeof(StructureType), imgName.Substring(3));
    }

    // BtnのnameからCategoryへの変換
    public static Category GetCategoryFromButtonName(this string btnName)
    {
        return (Category)Enum.Parse(typeof(Category), btnName.Substring(3));
    }
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
    const float Z_FIGHTING_GAP = 0.001f;    // Z-fightingを防ぐためにズラす

    const float BOARD_THICKNESS = 0.2f;     // Boardの厚み
    const int LIFT_PERIOD = 300;            // Liftの周期(f)
    const float CHOPSTICKS_SPACE = 0.5f;      // Chopsticksの間隔
    const float CHOPSTICKS_DIAMETER = 0.2f;   // Chopsticksの直径

    public const string BALL_NAME = "ball";        // Ballの名前（衝突判定で用いる）

    // 回転可能なStructureType
    public static List<StructureType> RotatableList
        => new List<StructureType>() {
            StructureType.Board,
            StructureType.Slope,
            StructureType.Arc,
            StructureType.Angle,
            StructureType.Chopsticks,
        };

    // リサイズ不可能なStructureType
    public static List<StructureType> NonresizableList
        => new List<StructureType>() {
            StructureType.Ball,
        };

    // Y軸方向にリサイズ不可能なStructureType
    public static List<StructureType> YNonresizableList
        => new List<StructureType>() {
            StructureType.Angle,
        };

    // X軸方向に反転可能なStructureType
    public static List<StructureType> XInversableList
        => new List<StructureType>() {
            StructureType.Chopsticks,
        };

    // Y軸方向に反転可能なStructureType
    public static List<StructureType> YInversableList
        => new List<StructureType>() {
        };

    // Z軸方向に反転可能なStructureType
    public static List<StructureType> ZInversableList
        => new List<StructureType>() {
            StructureType.Chopsticks,
        };

    // 衝突判定をするか
    public static List<StructureType> DetectCollisionList
        => new List<StructureType>() {
            StructureType.Goal,
            StructureType.Angle,
            StructureType.Jump,
        };

    // Position2を使うか
    public static List<StructureType> HasPosition2List
        => new List<StructureType>() {
            StructureType.Lift,
        };

    // ステージに一つしか存在できないか
    public static List<StructureType> OnlyOneList
        => new List<StructureType>() {
            StructureType.Ball,
        };

    // ステージの情報に保存しないStructureType
    public static List<StructureType> UnsavedList
    => new List<StructureType>() {
            StructureType.Ball,
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

    // Liftで、移動方向を表す
    [SerializeField]
    private Vector3Int _moveDirInt;
    public Vector3Int MoveDirInt
    {
        get => _moveDirInt;
        set
        {
            _moveDirInt = value;
            UpdateObjects();
        }
    }

    // Liftで、移動先の位置を表す
    public Vector3Int PositionInt2
    {
        get => PositionInt + MoveDirInt;
        set
        {
            _moveDirInt = value - PositionInt;
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

    public Vector3 MoveDir
    {
        get => (Vector3)MoveDirInt / GameConst.POSITION_SCALE;
        set => MoveDirInt = ToPositionInt(value);
    }

    public Vector3 Position2
    {
        get => (Vector3)PositionInt2 / GameConst.POSITION_SCALE;
        set => PositionInt2 = ToPositionInt(value);
    }

    public Vector3 LocalScale
    {
        get => (Vector3)LocalScaleInt / GameConst.LOCALSCALE_SCALE;
        set => LocalScaleInt = ToLocalScaleInt(value);
    }

    // X軸方向の反転
    [SerializeField]
    private bool _xInversed = false;
    public bool XInversed
    {
        get => _xInversed;
        set
        {
            _xInversed = value;
            UpdateObjects();
        }
    }

    // Y軸方向の反転
    [SerializeField]
    private bool _yInversed = false;
    public bool YInversed
    {
        get => _yInversed;
        set
        {
            _yInversed = value;
            UpdateObjects();
        }
    }

    // Z軸方向の反転
    [SerializeField]
    private bool _zInversed = false;
    public bool ZInversed
    {
        get => _zInversed;
        set
        {
            _zInversed = value;
            UpdateObjects();
        }
    }

    public Quaternion Rotation => RotationInt.ToQuaternion();

    // Z-fightingを防ぐために少しだけY方向にズラしたPosition
    public Vector3 PositionShifted => Position + new Vector3(0, Z_FIGHTING_GAP * (int)Type, 0);
    public Vector3 Position2Shifted => Position2 + new Vector3(0, Z_FIGHTING_GAP * (int)Type, 0);

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

    public Structure(StructureType type, Vector3Int pos, Vector3Int moveDir, Vector3Int scale, Stage parent)
    {
        Type = type;
        _positionInt = pos;
        _moveDirInt = moveDir;
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
                objs = new List<Primitive>() { new Primitive(Prefabs.FloorPrefab) };
                break;
            case StructureType.Start:
                objs = new List<Primitive>() { new Primitive(Prefabs.StartPrefab) };
                break;
            case StructureType.Goal:
                objs = new List<Primitive>() { new Primitive(Prefabs.GoalPrefab), new Primitive(Prefabs.GoalFlagPrefab) };
                break;
            case StructureType.Board:
                objs = new List<Primitive>() { new Primitive(Prefabs.BoardPrefab) };
                break;
            case StructureType.Plate:
                objs = new List<Primitive>() { new Primitive(Prefabs.PlatePrefab) };
                break;
            case StructureType.Slope:
                objs = new List<Primitive>() { new Primitive(Prefabs.SlopePrefab) };
                break;
            case StructureType.Arc:
                objs = new List<Primitive>() { new Primitive(Prefabs.ArcPrefab) };
                break;
            case StructureType.Angle:
                objs = new List<Primitive>() { new Primitive(Prefabs.AngleArrowPrefab) };
                break;
            case StructureType.Lift:
                objs = new List<Primitive>() { new Primitive(Prefabs.LiftPrefab), new Primitive(Prefabs.LiftGoalPrefab, true) };
                break;
            case StructureType.Ball:
                objs = new List<Primitive>() { new Primitive(Prefabs.BallPrefab, true) };
                break;
            case StructureType.Chopsticks:
                objs = new List<Primitive>() { new Primitive(Prefabs.ChopstickPrefab), new Primitive(Prefabs.ChopstickPrefab) };
                break;
            case StructureType.Jump:
                objs = new List<Primitive>() { new Primitive(Prefabs.JumpPrefab) };
                break;
            case StructureType.Box:
                objs = new List<Primitive>() { new Primitive(Prefabs.BoxPrefab, nonKinematic: true) };
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
                objs[0].Position = PositionShifted;
                objs[0].LocalScale = LocalScale;
                break;
            case StructureType.Start:
                UpdateObjects(StructureType.Floor);
                break;
            case StructureType.Goal:
                {
                    UpdateObjects(StructureType.Floor);
                    // GoalFlag
                    objs[1].Position = PositionShifted + new Vector3(0, LocalScale.y / 2, 0);
                    objs[1].LocalScale = Vector3.one;
                }
                break;
            case StructureType.Board:
                {
                    objs[0].Position = PositionShifted;
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
                objs[0].Position = PositionShifted;
                objs[0].LocalScale = LocalScale.NewY(LocalScale.y / 2);
                break;
            case StructureType.Slope:
                {
                    objs[0].Position = PositionShifted;
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
                    objs[0].Position = PositionShifted + objs[0].Rotation * new Vector3(0.05f * objs[0].LocalScale.x, 0.05f * objs[0].LocalScale.y, 0);
                }
                break;
            case StructureType.Angle:
                {
                    objs[0].Rotation = Rotation;
                    objs[0].LocalScale = (Quaternion.Inverse(objs[0].Rotation) * LocalScale).Abs();
                    objs[0].Position = PositionShifted - ToLocalScaleF(new Vector3Int(0, 1, 0)) / 2;
                }
                break;
            case StructureType.Lift:
                {
                    objs[0].LocalScale = LocalScale;
                    
                    if (Stage.ActiveScene == SceneType.Play)
                    {
                        // ω = 2 * Mathf.PI / LIFT_PERIOD
                        objs[0].Position = PositionShifted + ToPositionF(MoveDirInt)
                            * (1 + Mathf.Sin(2 * Mathf.PI * Parent.Generation / LIFT_PERIOD)) / 2;
                    }
                    else
                    {
                        objs[0].Position = PositionShifted;
                        objs[1].LocalScale = LocalScale;
                        objs[1].Position = Position2Shifted;
                    }
                }
                break;
            case StructureType.Ball:
                objs[0].Position = PositionShifted;
                objs[0].LocalScale = Prefabs.BallPrefab.transform.localScale;
                break;
            case StructureType.Chopsticks:
                {
                    // !YInversed, RotateなしのときはX-Y+Z-の頂点からX+Y-Z+の頂点に設置
                    Vector3 r1, r2;
                    switch (RotationInt)
                    {
                        case RotationEnum.IDENTITY:
                            r1 = Position - LocalScale.YMinus() / 2 + ToLocalScaleF(new Vector3Int(1, 0, 0)) / 2;
                            r2 = Position + LocalScale.YMinus() / 2 - ToLocalScaleF(new Vector3Int(1, 0, 0)) / 2;
                            break;
                        case RotationEnum.Y90:
                            r1 = Position + LocalScale.XMinus() / 2 - ToLocalScaleF(new Vector3Int(0, 0, 1)) / 2;
                            r2 = Position - LocalScale.XMinus() / 2 + ToLocalScaleF(new Vector3Int(0, 0, 1)) / 2;
                            break;
                        case RotationEnum.Y180:
                            r1 = Position + LocalScale / 2 - ToLocalScaleF(new Vector3Int(1, 0, 0)) / 2;
                            r2 = Position - LocalScale / 2 + ToLocalScaleF(new Vector3Int(1, 0, 0)) / 2;
                            break;
                        default:    // RotationEnum.Y270:
                            r1 = Position + LocalScale.ZMinus() / 2 + ToLocalScaleF(new Vector3Int(0, 0, 1)) / 2;
                            r2 = Position - LocalScale.ZMinus() / 2 - ToLocalScaleF(new Vector3Int(0, 0, 1)) / 2;
                            break;
                    }
                    if (XInversed)
                    {
                        var tmp = r1.x; r1.x = r2.x; r2.x = tmp;
                    }
                    if (ZInversed)
                    {
                        var tmp = r1.z; r1.z = r2.z; r2.z = tmp;
                    }

                    // r1からr2までの円柱を設置
                    var m = (r1 + r2) / 2;
                    objs[0].Position = m - new Vector3(0, CHOPSTICKS_DIAMETER / 2, 0);
                    // デフォルトではY軸方向
                    objs[0].LocalScale = new Vector3(CHOPSTICKS_DIAMETER, (r1 - m).magnitude, CHOPSTICKS_DIAMETER);
                    objs[0].Rotation = Quaternion.FromToRotation(Vector3.up, r2 - r1);

                    objs[1].Position = objs[0].Position;
                    objs[1].LocalScale = objs[0].LocalScale;
                    objs[1].Rotation = objs[0].Rotation;

                    // 間隔をあける
                    objs[0].Position += Rotation * new Vector3(CHOPSTICKS_SPACE / 2, 0, 0);
                    objs[1].Position += Rotation * new Vector3(-CHOPSTICKS_SPACE / 2, 0, 0);
                }
                break;
            case StructureType.Jump:
                UpdateObjects(StructureType.Floor);
                break;
            case StructureType.Box:
                UpdateObjects(StructureType.Floor);
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
                    if (Stage.ActiveScene == SceneType.Play)
                    {
                        objs[0].Position = Position + ToPositionF(MoveDirInt)
                            * (1 + Mathf.Sin(2 * Mathf.PI * Parent.Generation / LIFT_PERIOD)) / 2;
                    }
                }
                break;
        }
    }

    // ワールドに生成
    public void Create()
    {
        UpdateObjects();

        List<Primitive> range;
        if (Stage.ActiveScene == SceneType.Create) range = objs;
        else range = objs.FindAll(i => !i.CreateOnly);

        range.ForEach(i => i.Create());
        if (DetectsCollision) range.ForEach(i => i.SetCollisionEvent());
    }

    // ワールドから削除
    public void Destroy() => objs.ForEach(i => i.Destroy());

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

    // オブジェクト選択時、X方向の反転を表す矢印を表示する点
    public Vector3 GetXInverseArrowPos()
        => Position + new Vector3(0, 0, -LocalScale.z / 2 - 0.8f);

    // オブジェクト選択時、Y方向の反転を表す矢印を表示する点
    public Vector3 GetYInverseArrowPos()
        => Position + new Vector3(LocalScale.x / 2 + 0.8f, 0, 0);

    // オブジェクト選択時、Z方向の反転を表す矢印を表示する点
    public Vector3 GetZInverseArrowPos()
        => Position + new Vector3(0, -LocalScale.y / 2 - 0.8f, 0);

    // いずれかの方向にサイズ変更が可能か
    public bool IsResizable => !NonresizableList.Contains(Type);

    // Y方向のサイズ変更が可能か
    public bool IsYResizable => !YNonresizableList.Contains(Type);

    // 回転可能か
    public bool IsRotatable => RotatableList.Contains(Type);

    // 各軸方向に反転可能か
    public bool IsXInversable => XInversableList.Contains(Type);
    public bool IsYInversable => YInversableList.Contains(Type);
    public bool IsZInversable => ZInversableList.Contains(Type);

    // 衝突判定を行うか
    public bool DetectsCollision => DetectCollisionList.Contains(Type);

    // Position2を使うか
    public bool HasPosition2 => HasPosition2List.Contains(Type);

    // ステージに一つしか存在できないか
    public bool IsOnlyOne => OnlyOneList.Contains(Type);

    // ステージの情報に保存するか
    public bool IsSaved => !UnsavedList.Contains(Type);

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
        //UpdateObjects();
    }

}

public enum SceneType
{
    Play, Create,
}