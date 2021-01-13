using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructurePanelOperator : MonoBehaviour
{
    const int ROTATE_PERIOD = 720;    // 回転周期(f)

    public Button BtnUse;
    public Popup popup;
    private MenuOperator menuOp;
    private StructureItemOperator itemOp;

    public int StructureNo { get; private set; }
    public StructureItem StructureItem => Prefabs.StructureItemList[StructureNo];

    private Structure str;
    private int generation = 0;
    private Vector3 defPos;
    private Quaternion defRot;

    public static void ShowDialog(Transform parent, StructureItemOperator itemOp, MenuOperator menuOp)
    {
        var panel = Instantiate(Prefabs.StructurePanelPrefab, parent, false);
        var script = panel.GetComponent<StructurePanelOperator>();

        script.StructureNo = itemOp.StructureNo;
        script.menuOp = menuOp;
        script.itemOp = itemOp;

        script.Initialize();
        script.popup.Open();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        str.GenerationIncremented(++generation);
        var cam = menuOp.StrPanelCam.transform;

        // カメラをstrを通る鉛直軸を中心に回転
        var qua = Quaternion.Euler(0, 360f * generation / ROTATE_PERIOD, 0);
        cam.rotation = qua * defRot;
        cam.position = qua * (defPos - str.Position) + str.Position;
    }

    private void Initialize()
    {
        BtnUse.gameObject.SetActive(StructureItem.Type == StructureType.Ball && GameData.MyStructure[StructureNo]);
        if (GameData.User.ActiveBallNo == StructureNo) BtnUse.interactable = false;

        // カメラ
        str = new Structure(StructureNo);
        menuOp.SetForStructPanelPreview(str);
        defPos = menuOp.StrPanelCam.transform.position;
        defRot = menuOp.StrPanelCam.transform.rotation;
    }

    public void BtnCloseClicked()
    {
        popup.CloseAndDestroy();
    }

    public void BtnUseClicked()
    {
        // Active Ballを変更
        GameData.User.ActiveBallNo = StructureNo;
        BtnUse.interactable = false;
        itemOp.parent.UpdateActiveBall();   // StructureGroupViewに反映
        menuOp.header.UpdateIcon();
        GameData.Save();
    }
}
