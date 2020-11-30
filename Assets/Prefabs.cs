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
    public static GameObject FrameCubePrefab;      // 外枠のキューブのプレハブ

    public static GameObject SlopePrefab;           // Slopeのプレハブ
    public static GameObject ArcPrefab;             // Arcのプレハブ
    public static GameObject ViewpointArrow;        // Viewpointのプレハブ

    public GameObject ArrowPrefabObj;
    public GameObject XZCubePrefabObj;
    public GameObject YCubePrefabObj;
    public GameObject RotateArrowPrefabObj;
    public GameObject FrameCubePrefabObj;
    public GameObject SlopePrefabObj;
    public GameObject ArcPrefabObj;
    public GameObject ViewpointArrowObj;

    public void SetPrefabs()
    {
        ArrowPrefab = ArrowPrefabObj;
        XZCubePrefab = XZCubePrefabObj;
        YCubePrefab = YCubePrefabObj;
        RotateArrowPrefab = RotateArrowPrefabObj;
        FrameCubePrefab = FrameCubePrefabObj;
        SlopePrefab = SlopePrefabObj;
        ArcPrefab = ArcPrefabObj;
        ViewpointArrow = ViewpointArrowObj;
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
