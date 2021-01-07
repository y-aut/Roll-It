using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelOperator : MonoBehaviour
{
    // 各種コントロール
    public TextMeshProUGUI TxtStageName;
    public TextMeshProUGUI TxtAuthorName;
    public Button BtnAuthorName;
    public TextMeshProUGUI TxtChallengeCount;
    public TextMeshProUGUI TxtClearCount;
    public TextMeshProUGUI TxtClearRate;
    public TextMeshProUGUI TxtPosEvaCount;
    public TextMeshProUGUI TxtNegEvaCount;
    public GameObject ImgGraphPos;
    public Button BtnPosEva;
    public Button BtnNegEva;

    public Transform parent;
    public MenuOperator menuOp;
    // 対応するステージ
    private Stage Stage;

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public async Task SetStage(Stage stage)
    {
        Stage = stage;
        // 既に評価したステージは評価できなくする
        SetEvaluationInteractable(!(GameData.User.LocalData.PosEvaIDs.Contains(stage.ID)
            || GameData.User.LocalData.NegEvaIDs.Contains(stage.ID)));
        try
        {
            await FirebaseIO.IncrementClearCount(Stage).WaitWithTimeOut(); ;
        }
        catch (System.Exception e)
        {
            // TODO: ステージが削除されている場合
            e.Show(parent);
        }
        // ClearCountをインクリメントしたあとでコントロールを更新
        UpdateControls();
        UpdateAuthor();
    }

    // StageをもとにAuthor以外の値を更新
    private void UpdateControls()
    {
        TxtStageName.text = Stage.Name;
        TxtChallengeCount.text = string.Format("{0:#,0}", Stage.ChallengeCount);
        TxtClearCount.text = string.Format("{0:#,0}", Stage.ClearCount);
        TxtClearRate.text = string.Format("{0:0.00}", Stage.ClearRate * 100);
        TxtPosEvaCount.text = string.Format("{0:#,0}", Stage.PosEvaCount);
        TxtNegEvaCount.text = string.Format("{0:#,0}", Stage.NegEvaCount);
        if (Stage.PosEvaCount + Stage.NegEvaCount != 0)
            ImgGraphPos.transform.localScale = new Vector3((float)Stage.PosEvaCount / (Stage.PosEvaCount + Stage.NegEvaCount), 1, 1);
    }

    // Authorを更新
    private async void UpdateAuthor()
    {
        var author = await Cache.GetUser(Stage.AuthorID);
        TxtAuthorName.text = author.Name;
        if (author.IsNotFound)
        {
            BtnAuthorName.interactable = false;
            TxtAuthorName.fontStyle = FontStyles.Normal;    // 下線を削除
        }
        else
        {
            BtnAuthorName.interactable = true;
            TxtAuthorName.fontStyle = FontStyles.Underline;
        }
    }

    // Click Events
    public async void BtnPosEvaClicked()
    {
        NowLoading.Show(parent, "Connecting...");
        try
        {
            await FirebaseIO.IncrementPosEvaCount(Stage).WaitWithTimeOut();
            UpdateControls();
            SetEvaluationInteractable(false);
        }
        catch (System.Exception e)
        {
            e.Show(parent);
        }
        NowLoading.Close();
    }

    public async void BtnNegEvaClicked()
    {
        NowLoading.Show(parent, "Connecting...");
        try
        {
            await FirebaseIO.IncrementNegEvaCount(Stage).WaitWithTimeOut();
            UpdateControls();
            SetEvaluationInteractable(false);
        }
        catch (System.Exception e)
        {
            e.Show(parent);
        }
        NowLoading.Close();
    }

    public async void BtnAuthorClicked()
    {
        NowLoading.Show(parent, "Connecting...");
        var user = await Cache.GetUser(Stage.AuthorID);
        NowLoading.Close();
        UserPanelOperator.ShowDialog(parent, user, menuOp);
    }

    private void SetEvaluationInteractable(bool value)
    {
        BtnPosEva.interactable = BtnNegEva.interactable = value;
    }

    public void BtnNextClicked()
    {
        SetActive(false);
        menuOp.LoadHistory();
    }

}
