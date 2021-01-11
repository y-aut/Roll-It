using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureItemOperator : MonoBehaviour
{
    public RawImage ImgPreview;
    public RawImage ImgAdditional;
    public GameObject ImgInUse;
    public CanvasGroup canvasGroup;
    private MenuOperator menuOp;
    public StructureGroupViewOperator parent;
    public ChildScrollRect ScrollRect;
    public int StructureNo { get; private set; }

    public StructureItem StructureItem => Prefabs.StructureItemList[StructureNo];

    private Vector3 pointer;

    private bool _inUse = false;
    public bool InUse
    {
        get => _inUse;
        set
        {
            _inUse = value;
            ImgInUse.SetActive(value);
        }
    }

    public void Initialize(int structureNo, MenuOperator _menuOp, StructureGroupViewOperator _parent)
    {
        StructureNo = structureNo;
        menuOp = _menuOp;
        parent = _parent;
        ScrollRect.parentScrollRect = _parent.ScrollRect;
        ImgPreview.texture = Prefabs.StructureItemList[StructureNo].Preview;

        var adt = Prefabs.AdditionalSprites[StructureItem.Type];
        ImgAdditional.gameObject.SetActive(adt != null);
        if (adt) ImgAdditional.texture = adt.texture;

        // 未所持の場合は透過
        if (!GameData.MyStructure[StructureNo])
            canvasGroup.alpha = 0.5f;
    }

    public void OnPointerDown()
    {
        pointer = Input.mousePosition;
    }

    public void OnPointerUp()
    {
        if (pointer == Input.mousePosition) // スクロール中はスルー
            StructurePanelOperator.ShowDialog(menuOp.canvas.transform, this, menuOp);
    }

}
