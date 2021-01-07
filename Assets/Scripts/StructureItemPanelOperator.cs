using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureItemPanelOperator : MonoBehaviour
{
    const int ROTATE_PERIOD = 720;    // 回転周期(f)

    public GameObject ImgJewel;
    public GameObject ImgCoin;
    public TextMeshProUGUI TxtPrice;
    public Button BtnPurchase;
    public Popup popup;
    private MenuOperator menuOp;
    private Transform parent;
    private ShopStructureItemOperator shopItem;

    public int StructureNo { get; private set; }
    public StructureItem StructureItem => Prefabs.StructureItemList[StructureNo];

    private Structure str;
    private int generation = 0;
    private Vector3 defPos;
    private Quaternion defRot;

    public static void ShowDialog(Transform parent, ShopStructureItemOperator shopItem, MenuOperator menuOp)
    {
        var panel = Instantiate(Prefabs.StructureItemPanelPrefab, parent, false);
        var script = panel.GetComponent<StructureItemPanelOperator>();

        script.StructureNo = shopItem.StructureNo;
        script.parent = parent;
        script.menuOp = menuOp;
        script.shopItem = shopItem;

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
        BtnPurchase.interactable = GameData.User.Money >= price;

        // カメラ
        str = new Structure(StructureNo);
        menuOp.SetForStructPanelPreview(str);
        defPos = menuOp.StrPanelCam.transform.position;
        defRot = menuOp.StrPanelCam.transform.rotation;
    }

    public async void BtnPurchaseClicked()
    {
        BtnPurchase.interactable = false;
        GameData.User.Money -= StructureItem.Price;
        GameData.MyStructure[StructureNo] = true;

        NowLoading.Show(parent, "Connecting...");
        try
        {
            // ローカルのUserを更新
            await FirebaseIO.SyncUser().WaitWithTimeOut();
            MenuOperator.LastUserInfoUpdatedTime = DateTime.Now;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        NowLoading.Close();
        
        GameData.Save();
        menuOp.header.UpdateValue();
        shopItem.SetSold();
        // TODO: Add effect

    }

    public void BtnCloseClicked()
    {
        popup.CloseAndDestroy();
    }
}
