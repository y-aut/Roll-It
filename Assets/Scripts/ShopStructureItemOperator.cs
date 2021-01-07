using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopStructureItemOperator : MonoBehaviour
{
    public RawImage ImgPreview;
    public RawImage ImgAdditional;
    public GameObject ImgSold;
    public GameObject ImgJewel;
    public GameObject ImgCoin;
    public TextMeshProUGUI TxtPrice;
    private MenuOperator menuOp;
    public int StructureNo { get; private set; }
    public bool Sold { get; private set; } = false;

    public StructureItem StructureItem => Prefabs.StructureItemList[StructureNo];

    public void Initialize(int structureNo, MenuOperator _menuOp)
    {
        StructureNo = structureNo;
        menuOp = _menuOp;
        ImgPreview.texture = Prefabs.StructureItemList[StructureNo].Preview;

        var adt = Prefabs.AdditionalSprites[StructureItem.Type];
        ImgAdditional.gameObject.SetActive(adt != null);
        if (adt) ImgAdditional.texture = adt.texture;

        var price = StructureItem.Price;
        if (price.Jewel != 0)
        {
            ImgCoin.SetActive(false);
            TxtPrice.text = price.Jewel.ToString();
        }
        else
        {
            ImgJewel.SetActive(false);
            TxtPrice.text = price.Coin.ToString();
        }
    }

    public void Clicked()
    {
        if (Sold) return;
        StructureItemPanelOperator.ShowDialog(menuOp.canvas.transform, this, menuOp);
    }

    public void SetSold()
    {
        ImgSold.SetActive(true);
        Sold = true;
    }

}
