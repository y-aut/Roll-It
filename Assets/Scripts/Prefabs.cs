using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 起動時にプレハブをロードし、static変数に代入
public class Prefabs : MonoBehaviour
{
    public static GameObject ArrowPrefab;          // 移動用矢印
    public static GameObject XZCubePrefab;         // XZリサイズ用キューブ
    public static GameObject YCubePrefab;          // Yリサイズ用キューブ
    public static GameObject RotateArrowPrefab;    // 回転用矢印
    public static GameObject InverseArrowPrefab;   // 反転用矢印
    public static GameObject FrameCubePrefab;      // 外枠のキューブのプレハブ
    public static GameObject FrameCubeIllegalPrefab;      // 設置不可能な場所での外枠のキューブのプレハブ

    // Structures
    public static GameObject BallPrefab;            // Ballのプレハブ
    public static GameObject FloorPrefab;           // Floorのプレハブ
    public static GameObject StartPrefab;           // Startのプレハブ
    public static GameObject GoalPrefab;            // Goalのプレハブ
    public static GameObject GoalFlagPrefab;        // Goalの旗のプレハブ
    public static GameObject BoardPrefab;           // Boardのプレハブ
    public static GameObject PlatePrefab;           // Plateのプレハブ
    public static GameObject SlopePrefab;           // Slopeのプレハブ
    public static GameObject ArcPrefab;             // Arcのプレハブ
    public static GameObject AngleArrowPrefab;      // Angleのプレハブ
    public static GameObject LiftPrefab;            // Liftのプレハブ
    public static GameObject LiftGoalPrefab;        // Liftの行き先のプレハブ
    public static GameObject ChopstickPrefab;       // Chopsticksのプレハブ
    public static GameObject JumpPrefab;            // Jumpのプレハブ
    public static GameObject BoxPrefab;             // Boxのプレハブ

    public static GameObject InputBoxPrefab;        // InputBoxのプレハブ
    public static GameObject MessageBoxPrefab;      // MessageBoxのプレハブ
    public static GameObject StageItemPrefab;       // StageItemのプレハブ
    public static GameObject NowLoadingPrefab;      // NowLoadingのプレハブ
    public static GameObject PausePanelPrefab;      // PausePanelのプレハブ

    public GameObject ArrowPrefabObj;
    public GameObject XZCubePrefabObj;
    public GameObject YCubePrefabObj;
    public GameObject RotateArrowPrefabObj;
    public GameObject InverseArrowPrefabObj;
    public GameObject FrameCubePrefabObj;
    public GameObject FrameCubeIllegalPrefabObj;

    public GameObject BallPrefabObj;
    public GameObject FloorPrefabObj;
    public GameObject StartPrefabObj;
    public GameObject GoalPrefabObj;
    public GameObject GoalFlagPrefabObj;
    public GameObject BoardPrefabObj;
    public GameObject PlatePrefabObj;
    public GameObject SlopePrefabObj;
    public GameObject ArcPrefabObj;
    public GameObject AngleArrowPrefabObj;
    public GameObject LiftPrefabObj;
    public GameObject LiftGoalPrefabObj;
    public GameObject ChopstickPrefabObj;
    public GameObject JumpPrefabObj;
    public GameObject BoxPrefabObj;

    public GameObject InputBoxPrefabObj;
    public GameObject MessageBoxPrefabObj;
    public GameObject StageItemPrefabObj;
    public GameObject NowLoadingPrefabObj;
    public GameObject PausePanelPrefabObj;

    // ポップアップウィンドウ表示時のLocalScaleの曲線
    public static AnimationCurve OpenCurve;
    public static AnimationCurve CloseCurve;
    // StageItem.ShowDetail時のHeightの曲線
    public static AnimationCurve ShowDetailCurve;
    // StructureIndexerの選択切り替え時のWidthの曲線
    public static AnimationCurve StructureIndexerSelectCurve;

    public AnimationCurve OpenCurveObj;
    public AnimationCurve CloseCurveObj;
    public AnimationCurve ShowDetailCurveObj;
    public AnimationCurve StructureIndexerSelectCurveObj;

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

        InputBoxPrefab = InputBoxPrefabObj;
        MessageBoxPrefab = MessageBoxPrefabObj;
        StageItemPrefab = StageItemPrefabObj;
        NowLoadingPrefab = NowLoadingPrefabObj;
        PausePanelPrefab = PausePanelPrefabObj;

        OpenCurve = OpenCurveObj;
        CloseCurve = CloseCurveObj;
        ShowDetailCurve = ShowDetailCurveObj;
        StructureIndexerSelectCurve = StructureIndexerSelectCurveObj;
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
