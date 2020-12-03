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
    public static GameObject YInverseArrowPrefab;  // Y反転用矢印
    public static GameObject FrameCubePrefab;      // 外枠のキューブのプレハブ
    public static GameObject FrameCubeIllegalPrefab;      // 設置不可能な場所での外枠のキューブのプレハブ

    // Structures
    public static GameObject FloorPrefab;           // Floorのプレハブ
    public static GameObject StartPrefab;           // Startのプレハブ
    public static GameObject GoalPrefab;            // Goalのプレハブ
    public static GameObject GoalFlagPrefab;        // Goalの旗のプレハブ
    public static GameObject BoardPrefab;           // Boardのプレハブ
    public static GameObject SlopePrefab;           // Slopeのプレハブ
    public static GameObject ArcPrefab;             // Arcのプレハブ
    public static GameObject AngleArrowPrefab;      // Angleのプレハブ
    public static GameObject LiftPrefab;            // Liftのプレハブ
    public static GameObject LiftGoalPrefab;        // Liftの行き先のプレハブ

    public static GameObject InputBoxPrefab;        // InputBoxのプレハブ

    public GameObject ArrowPrefabObj;
    public GameObject XZCubePrefabObj;
    public GameObject YCubePrefabObj;
    public GameObject RotateArrowPrefabObj;
    public GameObject YInverseArrowPrefabObj;
    public GameObject FrameCubePrefabObj;
    public GameObject FrameCubeIllegalPrefabObj;

    public GameObject FloorPrefabObj;
    public GameObject StartPrefabObj;
    public GameObject GoalPrefabObj;
    public GameObject GoalFlagPrefabObj;
    public GameObject BoardPrefabObj;
    public GameObject SlopePrefabObj;
    public GameObject ArcPrefabObj;
    public GameObject AngleArrowPrefabObj;
    public GameObject LiftPrefabObj;
    public GameObject LiftGoalPrefabObj;

    public GameObject InputBoxPrefabObj;

    // ポップアップウィンドウ表示時のLocalScaleの曲線
    public static AnimationCurve OpenCurve;
    public static AnimationCurve CloseCurve;

    public AnimationCurve OpenCurveObj;
    public AnimationCurve CloseCurveObj;

    public void SetPrefabs()
    {
        ArrowPrefab = ArrowPrefabObj;
        XZCubePrefab = XZCubePrefabObj;
        YCubePrefab = YCubePrefabObj;
        RotateArrowPrefab = RotateArrowPrefabObj;
        YInverseArrowPrefab = YInverseArrowPrefabObj;
        FrameCubePrefab = FrameCubePrefabObj;
        FrameCubeIllegalPrefab = FrameCubeIllegalPrefabObj;

        FloorPrefab = FloorPrefabObj;
        StartPrefab = StartPrefabObj;
        GoalPrefab = GoalPrefabObj;
        GoalFlagPrefab = GoalFlagPrefabObj;
        BoardPrefab = BoardPrefabObj;
        SlopePrefab = SlopePrefabObj;
        ArcPrefab = ArcPrefabObj;
        AngleArrowPrefab = AngleArrowPrefabObj;
        LiftPrefab = LiftPrefabObj;
        LiftGoalPrefab = LiftGoalPrefabObj;

        InputBoxPrefab = InputBoxPrefabObj;

        OpenCurve = OpenCurveObj;
        CloseCurve = CloseCurveObj;
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
