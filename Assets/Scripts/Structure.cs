using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Structureの種類
    追加時に更新するもの:
    CreateOperator.ItemDragged, StructureTypeToCategory, 各種List, StructureOrder,
    SetObjs, UpdateObjects, GenerationIncremented, SetForPreview, SetForPanelPreview
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
    Gate,       // ゲート


    NB,
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
    private static readonly List<Category> StructureTypeToCategory = new List<Category>()
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
        Category.Moving,    // Gate
    };

    public static Category GetCategory(this StructureType type) => StructureTypeToCategory[(int)type];

}

[Serializable]
public enum RotationEnum
{
    IDENTITY, Y90, Y180, Y270, NB,
}

// 複数のPrimitiveを一つにまとめた構造体
public class Structure
{
    public const float Z_FIGHTING_GAP = 0.001f;    // Z-fightingを防ぐためにズラす

    const float BOARD_THICKNESS = 0.2f;     // Boardの厚み
    const int LIFT_PERIOD = 300;            // Liftの周期(f)
    const float CHOPSTICKS_SPACE = 0.5f;      // Chopsticksの間隔
    const float CHOPSTICKS_DIAMETER = 0.2f;   // Chopsticksの直径
    const float GATE_TOP_HEIGHT = 0.3f;     // Gateの上に突き出た部分の長さ

    public const string BALL_NAME = "ball";        // Ballの名前（衝突判定で用いる）

    // LocalScaleの最大サイズ
    public const int LOCALSCALE_LIMIT = 1024;
    // MoveDirの最大サイズ
    public const int MOVEDIR_LIMIT = 64;

    // Previewの撮影位置
    static readonly Vector3 PREVIEW_POS = Vector3.one * GameConst.STAGE_LIMIT * GameConst.POSITION_SCALE * 2;
    static readonly Vector3Int PREVIEW_POSINT = Structure.ToPositionInt(PREVIEW_POS);

    // StructureTypeの表示順（ゲーム内での順序）
    public static readonly List<StructureType> StructureOrder = new List<StructureType>()
    {
        StructureType.Ball,
        StructureType.Floor,
        StructureType.Start,
        StructureType.Goal,
        StructureType.Board,
        StructureType.Plate,
        StructureType.Slope,
        StructureType.Arc,
        StructureType.Angle,
        StructureType.Lift,
        StructureType.Chopsticks,
        StructureType.Jump,
        StructureType.Box,
        StructureType.Gate,
    };

    public int No { get; private set; }
    public StructureItem Item => Prefabs.StructureItemList[No];
    public StructureType Type => Item.Type;

    private List<Primitive> objs;

    public Stage Parent { get; set; }

    // Transform
    // 矢印などの移動やサイズ変更に関するオブジェクトはこの直方体を基準にして設置する
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
    private Vector3Int _moveDirInt;
    public Vector3Int MoveDirInt
    {
        get => _moveDirInt;
        set
        {
            if (!value.IsAllBetween(-MOVEDIR_LIMIT, MOVEDIR_LIMIT)) return;
            _moveDirInt = value;
            UpdateObjects();
        }
    }

    // Liftで、移動先の位置を表す
    public Vector3Int PositionInt2
    {
        get => PositionInt + MoveDirInt;
        set => MoveDirInt = value - PositionInt;
    }

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

    public int Tag { get; private set; } = 0;

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
    public Structure(int no, Vector3Int pos, Vector3Int scale, Stage parent)
    {
        No = no;
        _positionInt = pos;
        _localScaleInt = scale;
        Parent = parent;

        SetObjs();
        UpdateObjects();
    }

    public Structure(int no, Vector3Int pos, Vector3Int moveDir, Vector3Int scale, Stage parent)
    {
        No = no;
        _positionInt = pos;
        _moveDirInt = moveDir;
        _localScaleInt = scale;
        Parent = parent;

        SetObjs();
        UpdateObjects();
    }

    public Structure(Structure src, Stage parent)
        : this(src.No, src._positionInt, src._localScaleInt, src._moveDirInt,
              src._rotationInt, src._xInversed, src._yInversed, src._zInversed, src.Tag, parent)
    { }

    // プレビューをキャプチャする用に初期化する
    public Structure(int no)
    {
        No = no;
        SetObjs();
    }

    // StructureZipから解凍するときに用いる

    // Ver 0
    public Structure(int no, Vector3Int pos, Vector3Int scale,
    Vector3Int moveDir, RotationEnum rot, bool xInv, bool yInv, bool zInv, int tag, Stage parent)
    {
        No = no;
        _positionInt = pos;
        _localScaleInt = scale;
        _moveDirInt = moveDir;
        _rotationInt = rot;
        _xInversed = xInv;
        _yInversed = yInv;
        _zInversed = zInv;
        Tag = tag;
        Parent = parent;

        SetObjs();
        UpdateObjects();
    }

    // objsにPrimitiveをセットする。位置等はUpdateObjects()で設定するのでしなくて良い
    private void SetObjs()
    {
        // PrefabsにあるPrefabを一つずつ生成する
        objs = Item.Prefabs.Select(i => new Primitive(i)).ToList();

        switch (Type)
        {
            case StructureType.Lift:
                objs[1].CreateOnly = true;  // Lift Goal
                break;
            case StructureType.Ball:
                objs[0].CreateOnly = true;  // Ball
                break;
            case StructureType.Chopsticks:
                objs.Add(new Primitive(Item.Prefabs[0]));   // 複製
                break;
            case StructureType.Box:
                objs[0].NonKinematic = true;
                break;
            case StructureType.Gate:
                objs.InsertRange(0, new List<Primitive>() {
                    new Primitive(Item.Prefabs[0]), new Primitive(Item.Prefabs[0]), new Primitive(Item.Prefabs[0])
                });
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
                    
                    if (Scenes.GetActiveScene() == SceneType.Play)
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
                objs[0].LocalScale = Vector3.one * GameConst.BALL_SCALE;
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
            case StructureType.Gate:
                {
                    // Xが横幅
                    // 柱
                    // Rotationが0のときは、X+Z-とX-Z-の隅に置く
                    objs[0].Position = ToPositionF(PositionInt + LocalScaleInt.Scaled(1, 0, -1));
                    objs[0].Rotation = Quaternion.Euler(0, 0, 180);
                    objs[1].Position = ToPositionF(PositionInt + LocalScaleInt.Scaled(-1, 0, -1));
                    objs[0].LocalScale = objs[1].LocalScale = new Vector3(1, LocalScale.y, 1);
                    objs[2].Position = objs[0].Position + new Vector3(0, (LocalScale.y + GATE_TOP_HEIGHT) / 2, 0);
                    objs[3].Position = objs[1].Position + new Vector3(0, (LocalScale.y + GATE_TOP_HEIGHT) / 2, 0);
                    // 梁
                    objs[4].Position = ToPositionF(PositionInt + LocalScaleInt.Scaled(0, 1, -1));
                    objs[4].LocalScale = new Vector3(LocalScale.x - 1, 1, 1);
                    // 扉
                    objs[5].Position = ToPositionF(PositionInt - LocalScaleInt.ZCast());
                    objs[5].LocalScale = new Vector3(LocalScale.x - 0.75f, LocalScale.y, 0.25f);
                }
                break;
        }
    }

    // Preview用に位置やサイズを設定し、撮影位置を返す
    // 撮影直前に呼び出す
    public (Vector3 pos, Quaternion rot) SetForPreview()
    {
        var res = SetForPreview(Type);
        // LookRotationだと少し上向きになるのでカメラを下にずらす
        if (Type != StructureType.Ball)
            res.rot *= Quaternion.Euler(5, 0, 0);
        return res;
    }

    // こちらは直接呼ばない
    private (Vector3 pos, Quaternion rot) SetForPreview(StructureType type)
    {
        Vector3 camPos; Quaternion camRot;
        switch (type)
        {
            case StructureType.Floor:
                objs[0].Position = PREVIEW_POS;
                objs[0].LocalScale = new Vector3(4, 0.5f, 4);
                camPos = PREVIEW_POS + new Vector3(-5.12f, 3.01f, -6.82f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Start:
                (camPos, camRot) = SetForPreview(StructureType.Floor);
                break;
            case StructureType.Goal:
                (camPos, camRot) = SetForPreview(StructureType.Floor);
                break;
            case StructureType.Board:
                Position = PREVIEW_POS;
                LocalScale = new Vector3(2, 2, 2);
                RotationInt = RotationEnum.Y270;
                UpdateObjects();
                camPos = PREVIEW_POS + new Vector3(-3.66f, 1.99f, -3.41f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Plate:
                (camPos, camRot) = SetForPreview(StructureType.Floor);
                break;
            case StructureType.Slope:
                Position = PREVIEW_POS;
                LocalScale = new Vector3(1, 1, 1);
                RotationInt = RotationEnum.Y270;
                UpdateObjects();
                camPos = PREVIEW_POS + new Vector3(-1.16f, 0.03f, -1.98f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Arc:
                Position = PREVIEW_POS;
                LocalScale = new Vector3(1, 1, 1);
                RotationInt = RotationEnum.Y270;
                UpdateObjects();
                camPos = PREVIEW_POS + new Vector3(-1.18f, 0.27f, -1.92f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Angle:
                objs.Add(new Primitive(Prefabs.StructureItemList[StructureType.Floor.GetStructureNos()[0]].Prefabs[0], this));
                objs[0].Position = PREVIEW_POS;
                objs[0].LocalScale = new Vector3(1, 1, 1);
                objs[1].Position = PREVIEW_POS + new Vector3(0, -0.25f, 0);
                objs[1].LocalScale = new Vector3(1.5f, 0.5f, 1.5f);
                camPos = PREVIEW_POS + new Vector3(-1.33f, 2.77f, -1.14f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Lift:
                objs.RemoveAt(objs.Count - 1);
                objs[0].Position = PREVIEW_POS;
                objs[0].LocalScale = new Vector3(2, 0.5f, 2);
                camPos = PREVIEW_POS + new Vector3(-3.69f, 1.45f, -2.51f);
                camRot = new Quaternion(0.135986447f, 0.48212409f, -0.0760462657f, 0.862137556f);
                break;
            case StructureType.Ball:
                objs[0].CreateOnly = false;
                objs[0].Position = PREVIEW_POS;
                objs[0].LocalScale = Vector3.one * 0.7f;
                camPos = PREVIEW_POS + new Vector3(-0.852f, 0.588f, -0.864f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Chopsticks:
                Position = PREVIEW_POS;
                LocalScale = new Vector3(2, 2, 2);
                UpdateObjects();
                camPos = PREVIEW_POS + new Vector3(0.85f, 0.19f, 4.18f);
                camRot = new Quaternion(0.00350445928f, -0.996310771f, 0.0502598435f, 0.069473289f);
                break;
            case StructureType.Jump:
                objs[0].Position = PREVIEW_POS;
                objs[0].LocalScale = new Vector3(2, 0.5f, 1);
                camPos = PREVIEW_POS + new Vector3(-2.12f, 1.48f, -2.18f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Box:
                objs[0].Position = PREVIEW_POS;
                objs[0].LocalScale = new Vector3(1, 1, 1);
                camPos = PREVIEW_POS + new Vector3(-1.45f, 1.53f, -1.77f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Gate:
                camPos = PREVIEW_POS + new Vector3(-1.45f, 1.53f, -1.77f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            default:
                throw GameException.Unreachable;
        }

        return (camPos, camRot);
    }

    // StructureItemPanelのPreview用に位置やサイズを設定し、撮影位置を返す
    // GenerationIncremented()を呼び出すので、このStructureのPositionInt等も設定する
    public (Vector3 pos, Quaternion rot) SetForPanelPreview()
    {
        var res = SetForPanelPreview(Type);
        UpdateObjects();
        return res;
    }

    // こちらは直接呼ばない
    private (Vector3 pos, Quaternion rot) SetForPanelPreview(StructureType type)
    {
        Vector3 camPos; Quaternion camRot;
        switch (type)
        {
            case StructureType.Floor:
                PositionInt = PREVIEW_POSINT;
                LocalScaleInt = new Vector3Int(4, 1, 4);
                camPos = PREVIEW_POS + new Vector3(0, 2, -3);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Start:
                (camPos, camRot) = SetForPanelPreview(StructureType.Floor);
                break;
            case StructureType.Goal:
                (camPos, camRot) = SetForPanelPreview(StructureType.Floor);
                break;
            case StructureType.Board:
                PositionInt = PREVIEW_POSINT;
                LocalScaleInt = new Vector3Int(4, 4, 4);
                RotationInt = RotationEnum.Y270;
                UpdateObjects();
                camPos = PREVIEW_POS + new Vector3(-3.66f, 1.99f, -3.41f);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Plate:
                (camPos, camRot) = SetForPanelPreview(StructureType.Floor);
                break;
            case StructureType.Slope:
                (camPos, camRot) = SetForPanelPreview(StructureType.Board);
                break;
            case StructureType.Arc:
                (camPos, camRot) = SetForPanelPreview(StructureType.Board);
                break;
            case StructureType.Angle:
                (camPos, camRot) = SetForPanelPreview(StructureType.Floor);
                break;
            case StructureType.Lift:
                PositionInt = PREVIEW_POSINT;
                LocalScaleInt = new Vector3Int(4, 1, 4);
                MoveDirInt = new Vector3Int(0, 5, 0);
                camPos = (Position + Position2) / 2 + new Vector3(0, 2, -4);
                camRot = Quaternion.LookRotation((Position + Position2) / 2 - camPos);
                break;
            case StructureType.Ball:
                objs[0].CreateOnly = false;
                PositionInt = PREVIEW_POSINT;
                camPos = PREVIEW_POS + new Vector3(0, 2, -3) * 0.4f;
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Chopsticks:
                PositionInt = PREVIEW_POSINT;
                LocalScaleInt = new Vector3Int(1, 1, 4);
                RotationInt = RotationEnum.Y180;
                camPos = PREVIEW_POS + new Vector3(0, 2, -3);
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Jump:
                (camPos, camRot) = SetForPanelPreview(StructureType.Floor);
                break;
            case StructureType.Box:
                PositionInt = PREVIEW_POSINT;
                LocalScaleInt = new Vector3Int(1, 1, 1);
                camPos = PREVIEW_POS + new Vector3(0, 2, -3) * 0.4f;
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            case StructureType.Gate:
                PositionInt = PREVIEW_POSINT;
                LocalScaleInt = new Vector3Int(1, 1, 1);
                camPos = PREVIEW_POS + new Vector3(0, 2, -3) * 0.4f;
                camRot = Quaternion.LookRotation(PREVIEW_POS - camPos);
                break;
            default:
                throw GameException.Unreachable;
        }

        UpdateObjects();
        return (camPos, camRot);
    }

    // Generationに合わせてオブジェクトを更新
    public void GenerationIncremented(int generation = -1)
    {
        if (generation == -1) generation = Parent.Generation;

        switch (Type)
        {
            case StructureType.Lift:
                {
                    if (Scenes.GetActiveScene() != SceneType.Create)
                    {
                        objs[0].Position = Position + ToPositionF(MoveDirInt)
                            * (1 + Mathf.Sin(2 * Mathf.PI * generation / LIFT_PERIOD)) / 2;
                    }
                }
                break;
        }
    }

    // ワールドに生成
    public void Create()
    {
        UpdateObjects();
        Collided = false;

        List<Primitive> range;
        if (Scenes.GetActiveScene() == SceneType.Create) range = objs;
        else range = objs.FindAll(i => !i.CreateOnly);

        range.ForEach(i => i.Create());
        if (Type.DetectsCollision()) range.ForEach(i => i.SetCollisionEvent());
    }

    public void CreateForPreview()
    {
        Collided = false;
        objs.FindAll(i => !i.CreateOnly).ForEach(i => i.Create());
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
    // 補助面を表示する面
    public static CubeFace GetArrowFace(int index)
    {
        switch (index % 3)
        {
            case 0: return CubeFace.X;
            case 1: return CubeFace.Y;
            default: return CubeFace.Z;
        }
    }

    // オブジェクト選択時、X軸上でのサイズ変更を表す辺上の印を表示する点((X+,X-)*(Y+,Y-))
    public void GetXResizeEdges(out List<Vector3> points)
    {
        points = new List<Vector3>() {
            Position + new Vector3(LocalScale.x / 2, LocalScale.y / 2, 0),
            Position + new Vector3(-LocalScale.x / 2, LocalScale.y / 2, 0),
            Position + new Vector3(LocalScale.x / 2, -LocalScale.y / 2, 0),
            Position + new Vector3(-LocalScale.x / 2, -LocalScale.y / 2, 0),
        };
    }

    // オブジェクト選択時、Z軸上でのサイズ変更を表す辺上の印を表示する点((Z+,Z-)*(Y+,Y-))
    public void GetZResizeEdges(out List<Vector3> points)
    {
        points = new List<Vector3>() {
            Position + new Vector3(0, LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(0, LocalScale.y / 2, -LocalScale.z / 2),
            Position + new Vector3(0, -LocalScale.y / 2, LocalScale.z / 2),
            Position + new Vector3(0, -LocalScale.y / 2, -LocalScale.z / 2),
        };
    }

    // 補助面を表示する面
    public static CubeFace GetXResizeFace(int index) => index % 2 == 0 ? CubeFace.XP : CubeFace.XN;
    public static CubeFace GetZResizeFace(int index) => index % 2 == 0 ? CubeFace.ZP : CubeFace.ZN;

    // オブジェクト選択時、Y方向のサイズ変更を表す辺上の印を表示する点((X+Z+,X-Z+,X-Z-,X+Z-)*(Y+,Y-))
    public void GetYResizeVertexes(out List<Vector3> points)
    {
        var scaleHalf = LocalScale / 2;
        points = new List<Vector3>() {
            Position + new Vector3(scaleHalf.x, scaleHalf.y, scaleHalf.z),
            Position + new Vector3(-scaleHalf.x, scaleHalf.y, scaleHalf.z),
            Position + new Vector3(-scaleHalf.x, scaleHalf.y, -scaleHalf.z),
            Position + new Vector3(scaleHalf.x, scaleHalf.y, -scaleHalf.z),
            Position + new Vector3(scaleHalf.x, -scaleHalf.y, scaleHalf.z),
            Position + new Vector3(-scaleHalf.x, -scaleHalf.y, scaleHalf.z),
            Position + new Vector3(-scaleHalf.x, -scaleHalf.y, -scaleHalf.z),
            Position + new Vector3(scaleHalf.x, -scaleHalf.y, -scaleHalf.z),
        };
    }
    // 補助面を表示する面
    public static CubeFace GetYResizeFace(int index) => index < 4 ? CubeFace.YP : CubeFace.YN;

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

    // Primitiveがクリックされた時にtrueになる
    public bool Clicked { get; set; }

    // ボールと衝突した時にtrueになる
    public bool Collided { get; set; }

}

public static partial class AddMethod
{
    // 最小サイズが(1,1,1)以外のもの
    private static readonly Dictionary<StructureType, Vector3Int> MinSizeDict
        = new Dictionary<StructureType, Vector3Int>() {
            { StructureType.Gate, new Vector3Int(2, 1, 1) },
        };
    public static Vector3Int MinSize(this StructureType type)
        => MinSizeDict.ContainsKey(type) ? MinSizeDict[type] : Vector3Int.one;

    // 回転可能なStructureType
    private static readonly List<StructureType> RotatableList
        = new List<StructureType>() {
            StructureType.Board,
            StructureType.Slope,
            StructureType.Arc,
            StructureType.Angle,
            StructureType.Chopsticks,
            StructureType.Gate,
        };

    // リサイズ不可能なStructureType
    private static readonly List<StructureType> NonresizableList
        = new List<StructureType>() {
            StructureType.Ball,
        };

    // Y軸方向にリサイズ不可能なStructureType
    private static readonly List<StructureType> YNonresizableList
        = new List<StructureType>() {
            StructureType.Angle,
        };

    // XZ軸の一方向にのみリサイズ可能なStructureType（Rotationが0のときはX軸のみにResizable）
    private static readonly List<StructureType> XZEitherNonresizableList
        = new List<StructureType>() {
            StructureType.Gate,
        };

    // X軸方向に反転可能なStructureType
    private static readonly List<StructureType> XInversableList
        = new List<StructureType>() {
            StructureType.Chopsticks,
        };

    // Y軸方向に反転可能なStructureType
    private static readonly List<StructureType> YInversableList
        = new List<StructureType>()
        {
        };

    // Z軸方向に反転可能なStructureType
    private static readonly List<StructureType> ZInversableList
        = new List<StructureType>() {
            StructureType.Chopsticks,
        };

    // 衝突判定をするか
    private static readonly List<StructureType> DetectCollisionList
        = new List<StructureType>() {
            StructureType.Goal,
            StructureType.Angle,
            StructureType.Jump,
        };

    // Position2を使うか
    private static readonly List<StructureType> HasPosition2List
        = new List<StructureType>() {
            StructureType.Lift,
        };

    // ステージに一つしか存在できないか
    private static readonly List<StructureType> OnlyOneList
        = new List<StructureType>() {
            StructureType.Ball,
        };

    // ステージの情報に保存しないStructureType
    private static readonly List<StructureType> UnsavedList
        = new List<StructureType>() {
            StructureType.Ball,
        };

    // Create画面の下に表示しないStructureType
    private static readonly List<StructureType> HideInItemViewList
        = new List<StructureType>() {
            StructureType.Start,
            StructureType.Goal,
            StructureType.Ball,
        };

    // Gallery画面に表示しないStructureType
    private static readonly List<StructureType> HideInGalleryList
        = new List<StructureType>() {
            StructureType.Start,
            StructureType.Goal,
        };

    // 他のStructureの上に置くのが推奨されるStructureType
    private static readonly List<StructureType> ShouldBeOnStructureList
        = new List<StructureType>() {
            StructureType.Ball,
            StructureType.Angle,
            StructureType.Box,
        };

    // 削除不可のStructureType
    private static readonly List<StructureType> UndeletableList
        = new List<StructureType>() {
            StructureType.Start,
            StructureType.Goal,
        };

    // いずれかの方向にサイズ変更が可能か
    public static bool IsResizable(this StructureType type) => !NonresizableList.Contains(type);

    // Y方向のサイズ変更が可能か
    public static bool IsYResizable(this StructureType type) => !YNonresizableList.Contains(type);

    // X方向のサイズ変更が可能か
    public static bool IsXResizable(this StructureType type, RotationEnum rot) =>
        !XZEitherNonresizableList.Contains(type) ||
        (rot == RotationEnum.IDENTITY || rot == RotationEnum.Y180);

    // Z方向のサイズ変更が可能か
    public static bool IsZResizable(this StructureType type, RotationEnum rot) =>
        !XZEitherNonresizableList.Contains(type) ||
        (rot == RotationEnum.Y90 || rot == RotationEnum.Y270);

    // 回転可能か
    public static bool IsRotatable(this StructureType type) => RotatableList.Contains(type);

    // 各軸方向に反転可能か
    public static bool IsXInversable(this StructureType type) => XInversableList.Contains(type);
    public static bool IsYInversable(this StructureType type) => YInversableList.Contains(type);
    public static bool IsZInversable(this StructureType type) => ZInversableList.Contains(type);

    // 衝突判定を行うか
    public static bool DetectsCollision(this StructureType type) => DetectCollisionList.Contains(type);

    // Position2を使うか
    public static bool HasPosition2(this StructureType type) => HasPosition2List.Contains(type);

    // ステージに一つしか存在できないか
    public static bool IsOnlyOne(this StructureType type) => OnlyOneList.Contains(type);

    // ステージの情報に保存するか
    public static bool IsSaved(this StructureType type) => !UnsavedList.Contains(type);

    // Create画面の下に表示するか
    public static bool ShowInItemView(this StructureType type) => !HideInItemViewList.Contains(type);

    // Gallery画面に表示するか
    public static bool ShowInGallery(this StructureType type) => !HideInGalleryList.Contains(type);

    // 他のStructureの上に置くのが推奨されるか
    public static bool ShouldBeOnStructure(this StructureType type) => ShouldBeOnStructureList.Contains(type);

    // ステージから削除できるか
    public static bool IsDeletable(this StructureType type) => !UndeletableList.Contains(type);

}