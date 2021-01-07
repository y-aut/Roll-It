using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderPanelOperator : MonoBehaviour
{
    public TextMeshProUGUI TxtName;
    public RawImage ImgIcon;
    public TextMeshProUGUI TxtJewelCount;
    public TextMeshProUGUI TxtPosEvaCount;
    public TextMeshProUGUI TxtChallengeCount;
    public TextMeshProUGUI TxtFavoredCount;

    private void SetValue(User user)
    {
        TxtName.text = user.Name;
        TxtJewelCount.text = string.Format("{0:#,0}", user.Money.Jewel);
        TxtPosEvaCount.text = string.Format("{0:#,0}", user.PosEvaCount);
        TxtChallengeCount.text = string.Format("{0:#,0}", user.ChallengedCount);
        TxtFavoredCount.text = string.Format("{0:#,0}", user.FavoredCount);

        if (Prefabs.StructureItemList[GameData.User.ActiveBallNo].Preview != null)
            ImgIcon.texture = Prefabs.StructureItemList[GameData.User.ActiveBallNo].Preview;
    }

    public void UpdateValue() => SetValue(GameData.User);

    public void UpdateIcon()
    {
        ImgIcon.texture = Prefabs.StructureItemList[GameData.User.ActiveBallNo].Preview;
    }
}
