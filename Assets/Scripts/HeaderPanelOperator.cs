using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeaderPanelOperator : MonoBehaviour
{
    public TextMeshProUGUI TxtName;
    public TextMeshProUGUI TxtCoinCount;
    public TextMeshProUGUI TxtPosEvaCount;
    public TextMeshProUGUI TxtChallengeCount;
    public TextMeshProUGUI TxtFavoredCount;

    private void SetValue(string name, int coin, int posEva, int challenge, int favored)
    {
        TxtName.text = name;
        TxtCoinCount.text = string.Format("{0:#,0}", coin);
        TxtPosEvaCount.text = string.Format("{0:#,0}", posEva);
        TxtChallengeCount.text = string.Format("{0:#,0}", challenge);
        TxtFavoredCount.text = string.Format("{0:#,0}", favored);
    }

    public void UpdateValue()
    {
        var user = GameData.User;
        SetValue(user.Name, user.Coin, user.PosEvaCount, user.ChallengedCount, user.FavoredCount);
    }

}
