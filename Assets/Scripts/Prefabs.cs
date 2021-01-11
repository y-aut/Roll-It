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
    public static List<StructureItem> StructureItemList;

    // ImgAdditional
    public static EnumCollection<StructureType, Sprite> AdditionalSprites;

    // 各Typeに対応するビットが1であるBoolList
    public static EnumCollection<StructureType, BoolList> TypeBoolList;

    public static GameObject InputBoxPrefab;
    public static GameObject MessageBoxPrefab;
    public static GameObject StageItemPrefab;
    public static GameObject StagePanelPrefab;
    public static GameObject StageViewPrefab;
    public static GameObject NowLoadingPrefab;
    public static GameObject PausePanelPrefab;
    public static GameObject UserPanelPrefab;
    public static GameObject CreateStructureItemPrefab;
    public static GameObject StructurePanelPrefab;
    public static GameObject StructureItemPrefab;
    public static GameObject StructureGroupViewPrefab;
    public static GameObject MachineConfirmPanelPrefab;

    public GameObject ArrowPrefabObj;
    public GameObject XZCubePrefabObj;
    public GameObject YCubePrefabObj;
    public GameObject RotateArrowPrefabObj;
    public GameObject InverseArrowPrefabObj;
    public GameObject FrameCubePrefabObj;
    public GameObject FrameCubeIllegalPrefabObj;
    public GameObject AuxiFacePrefabObj;

    public List<StructureItem> StructureItemListObj;

    public Sprite LiftAdditionalSpriteObj;

    public GameObject InputBoxPrefabObj;
    public GameObject MessageBoxPrefabObj;
    public GameObject StageItemPrefabObj;
    public GameObject StagePanelPrefabObj;
    public GameObject StageViewPrefabObj;
    public GameObject NowLoadingPrefabObj;
    public GameObject PausePanelPrefabObj;
    public GameObject UserPanelPrefabObj;
    public GameObject CreateStructureItemPrefabObj;
    public GameObject StructurePanelPrefabObj;
    public GameObject StructureItemPrefabObj;
    public GameObject StructureGroupViewPrefabObj;
    public GameObject MachineConfirmPanelPrefabObj;

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
        ArrowPrefab = ArrowPrefabObj;
        XZCubePrefab = XZCubePrefabObj;
        YCubePrefab = YCubePrefabObj;
        RotateArrowPrefab = RotateArrowPrefabObj;
        InverseArrowPrefab = InverseArrowPrefabObj;
        FrameCubePrefab = FrameCubePrefabObj;
        FrameCubeIllegalPrefab = FrameCubeIllegalPrefabObj;
        AuxiFacePrefab = AuxiFacePrefabObj;

        StructureItemList = StructureItemListObj;

        AdditionalSprites = new EnumCollection<StructureType, Sprite>(type =>
        {
            if (type == StructureType.Lift) return LiftAdditionalSpriteObj;
            else return null;
        });

        TypeBoolList = new EnumCollection<StructureType, BoolList>(type => new BoolList(StructureItemList.Count));
        for (int i = 0; i < StructureItemList.Count; ++i)
            TypeBoolList[StructureItemList[i].Type][i] = true;

        InputBoxPrefab = InputBoxPrefabObj;
        MessageBoxPrefab = MessageBoxPrefabObj;
        StageItemPrefab = StageItemPrefabObj;
        StagePanelPrefab = StagePanelPrefabObj;
        StageViewPrefab = StageViewPrefabObj;
        NowLoadingPrefab = NowLoadingPrefabObj;
        PausePanelPrefab = PausePanelPrefabObj;
        UserPanelPrefab = UserPanelPrefabObj;
        CreateStructureItemPrefab = CreateStructureItemPrefabObj;
        StructurePanelPrefab = StructurePanelPrefabObj;
        StructureItemPrefab = StructureItemPrefabObj;
        StructureGroupViewPrefab = StructureGroupViewPrefabObj;
        MachineConfirmPanelPrefab = MachineConfirmPanelPrefabObj;

        OpenCurve = OpenCurveObj;
        CloseCurve = CloseCurveObj;
        StructureIndexerSelectCurve = StructureIndexerSelectCurveObj;
        AttitudeToRotationCurve = AttitudeToRotationCurveObj;
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

public static partial class AddMethod
{
    private static EnumCollection<StructureType, List<int>> StructureNos
        = new EnumCollection<StructureType, List<int>>(type => Prefabs.StructureItemList.FindAllIndexes(i => i.Type == type));
    public static List<int> GetStructureNos(this StructureType type) => StructureNos[type];
}