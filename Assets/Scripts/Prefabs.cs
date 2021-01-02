using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 起動時にプレハブをロードし、static変数に代入
public class Prefabs : MonoBehaviour
{
    public static GameObject ArrowPrefab;           // 移動用矢印
    public static GameObject XZCubePrefab;          // XZリサイズ用キューブ
    public static GameObject YCubePrefab;           // Yリサイズ用キューブ
    public static GameObject RotateArrowPrefab;     // 回転用矢印
    public static GameObject InverseArrowPrefab;    // 反転用矢印
    public static GameObject FrameCubePrefab;       // 外枠のキューブのプレハブ
    public static GameObject FrameCubeIllegalPrefab;      // 設置不可能な場所での外枠のキューブのプレハブ
    public static GameObject AuxiFacePrefab;        // 補助面のプレハブ

    // Structures
    public static List<GameObject> BallPrefab;
    public static List<GameObject> FloorPrefab;
    public static List<GameObject> StartPrefab;
    public static List<GameObject> GoalPrefab;
    public static List<GameObject> GoalFlagPrefab;
    public static List<GameObject> BoardPrefab;
    public static List<GameObject> PlatePrefab;
    public static List<GameObject> SlopePrefab;
    public static List<GameObject> ArcPrefab;
    public static List<GameObject> AngleArrowPrefab;
    public static List<GameObject> LiftPrefab;
    public static List<GameObject> LiftGoalPrefab;
    public static List<GameObject> ChopstickPrefab;
    public static List<GameObject> JumpPrefab;
    public static List<GameObject> BoxPrefab;

    // ImgAdditional
    public static EnumCollection<StructureType, Sprite> AdditionalSprites;

    public static GameObject InputBoxPrefab;
    public static GameObject MessageBoxPrefab;
    public static GameObject StageItemPrefab;
    public static GameObject StagePanelPrefab;
    public static GameObject StageViewPrefab;
    public static GameObject NowLoadingPrefab;
    public static GameObject PausePanelPrefab;
    public static GameObject UserPanelPrefab;
    public static GameObject StructureItemPrefab;

    public GameObject ArrowPrefabObj;
    public GameObject XZCubePrefabObj;
    public GameObject YCubePrefabObj;
    public GameObject RotateArrowPrefabObj;
    public GameObject InverseArrowPrefabObj;
    public GameObject FrameCubePrefabObj;
    public GameObject FrameCubeIllegalPrefabObj;
    public GameObject AuxiFacePrefabObj;

    public List<GameObject> BallPrefabObj;
    public List<GameObject> FloorPrefabObj;
    public List<GameObject> StartPrefabObj;
    public List<GameObject> GoalPrefabObj;
    public List<GameObject> GoalFlagPrefabObj;
    public List<GameObject> BoardPrefabObj;
    public List<GameObject> PlatePrefabObj;
    public List<GameObject> SlopePrefabObj;
    public List<GameObject> ArcPrefabObj;
    public List<GameObject> AngleArrowPrefabObj;
    public List<GameObject> LiftPrefabObj;
    public List<GameObject> LiftGoalPrefabObj;
    public List<GameObject> ChopstickPrefabObj;
    public List<GameObject> JumpPrefabObj;
    public List<GameObject> BoxPrefabObj;

    public Sprite LiftAdditionalSpriteObj;

    public GameObject InputBoxPrefabObj;
    public GameObject MessageBoxPrefabObj;
    public GameObject StageItemPrefabObj;
    public GameObject StagePanelPrefabObj;
    public GameObject StageViewPrefabObj;
    public GameObject NowLoadingPrefabObj;
    public GameObject PausePanelPrefabObj;
    public GameObject UserPanelPrefabObj;
    public GameObject StructureItemPrefabObj;

    // ポップアップウィンドウ表示時のLocalScaleの曲線
    public static AnimationCurve OpenCurve;
    public static AnimationCurve CloseCurve;
    // StructureIndexerの選択切り替え時のWidthの曲線
    public static AnimationCurve StructureIndexerSelectCurve;
    // デバイスを傾けた角度に対する視点回転の割合
    public static AnimationCurve AttitudeToRotationCurve;

    public AnimationCurve OpenCurveObj;
    public AnimationCurve CloseCurveObj;
    public AnimationCurve StructureIndexerSelectCurveObj;
    public AnimationCurve AttitudeToRotationCurveObj;

    public void SetPrefabs()
    {
        BallPrefab = BallPrefabObj;
        ArrowPrefab = ArrowPrefabObj;
        XZCubePrefab = XZCubePrefabObj;
        YCubePrefab = YCubePrefabObj;
        RotateArrowPrefab = RotateArrowPrefabObj;
        InverseArrowPrefab = InverseArrowPrefabObj;
        FrameCubePrefab = FrameCubePrefabObj;
        FrameCubeIllegalPrefab = FrameCubeIllegalPrefabObj;
        AuxiFacePrefab = AuxiFacePrefabObj;

        FloorPrefab = FloorPrefabObj;
        StartPrefab = StartPrefabObj;
        GoalPrefab = GoalPrefabObj;
        GoalFlagPrefab = GoalFlagPrefabObj;
        BoardPrefab = BoardPrefabObj;
        PlatePrefab = PlatePrefabObj;
        SlopePrefab = SlopePrefabObj;
        ArcPrefab = ArcPrefabObj;
        AngleArrowPrefab = AngleArrowPrefabObj;
        LiftPrefab = LiftPrefabObj;
        LiftGoalPrefab = LiftGoalPrefabObj;
        ChopstickPrefab = ChopstickPrefabObj;
        JumpPrefab = JumpPrefabObj;
        BoxPrefab = BoxPrefabObj;

        AdditionalSprites = new EnumCollection<StructureType, Sprite>(type =>
        {
            if (type == StructureType.Lift) return LiftAdditionalSpriteObj;
            else return null;
        });

        InputBoxPrefab = InputBoxPrefabObj;
        MessageBoxPrefab = MessageBoxPrefabObj;
        StageItemPrefab = StageItemPrefabObj;
        StagePanelPrefab = StagePanelPrefabObj;
        StageViewPrefab = StageViewPrefabObj;
        NowLoadingPrefab = NowLoadingPrefabObj;
        PausePanelPrefab = PausePanelPrefabObj;
        UserPanelPrefab = UserPanelPrefabObj;
        StructureItemPrefab = StructureItemPrefabObj;

        OpenCurve = OpenCurveObj;
        CloseCurve = CloseCurveObj;
        StructureIndexerSelectCurve = StructureIndexerSelectCurveObj;
        AttitudeToRotationCurve = AttitudeToRotationCurveObj;
    }

    public static int GetTextureCount(StructureType type)
    {
        switch (type)
        {
            case StructureType.Floor:
                return FloorPrefab.Count;
            case StructureType.Start:
                return StartPrefab.Count;
            case StructureType.Goal:
                return GoalPrefab.Count;
            case StructureType.Board:
                return BoardPrefab.Count;
            case StructureType.Plate:
                return PlatePrefab.Count;
            case StructureType.Slope:
                return SlopePrefab.Count;
            case StructureType.Arc:
                return ArcPrefab.Count;
            case StructureType.Angle:
                return AngleArrowPrefab.Count;
            case StructureType.Lift:
                return LiftPrefab.Count;
            case StructureType.Ball:
                return BallPrefab.Count;
            case StructureType.Chopsticks:
                return ChopstickPrefab.Count;
            case StructureType.Jump:
                return JumpPrefab.Count;
            case StructureType.Box:
                return BoxPrefab.Count;
            default:
                throw GameException.Unreachable;
        }
    }

    private static bool firstTime = true;

    private void Awake()
    {
        if (firstTime)
        {
            firstTime = false;
            SetPrefabs();
        }
    }

}
