using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateStructureItemOperator : MonoBehaviour
{
    public RawImage ImgPreview;
    public GameObject ImgTriangle;
    public RawImage ImgAdditional;
    public int StructureNo { get; private set; }
    private bool IsClickable;
    private CreateOperator CreateOp;

    public StructureItem StructureItem => Prefabs.StructureItemList[StructureNo];

    public void Initialize(CreateOperator createOp, int structureNo, bool isClickable)
    {
        CreateOp = createOp;
        StructureNo = structureNo;
        IsClickable = isClickable;
        ImgTriangle.SetActive(IsClickable);
        ImgPreview.texture = StructureItem.Preview;

        var adt = Prefabs.AdditionalSprites[Prefabs.StructureItemList[StructureNo].Type];
        ImgAdditional.gameObject.SetActive(adt != null);
        if (adt) ImgAdditional.texture = adt.texture;
    }

    public void InitializeForBall(CreateOperator createOp, int structureNo)
    {
        CreateOp = createOp;
        StructureNo = structureNo;
        IsClickable = false;
        ImgPreview.texture = StructureItem.Preview;
    }

    public void Clicked()
    {
        if (!IsClickable) return;
        CreateOp.TextureList.Show(this);
    }

    public void Dragged()
    {
        CreateOp.ItemDragged(this);
    }

    public void Released()
    {
        CreateOp.ItemReleased();
    }

}
