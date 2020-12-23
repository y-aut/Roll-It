using System.Collections;
using System.Collections.Generic;
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

    public void SetStage(Stage stage)
    {
        Stage = stage;
        // 既に評価したステージは評価できなくする
        SetEvaluationInteractable(!(GameData.User.LocalData.PosEvaIDs.Contains(stage.ID)
            || GameData.User.LocalData.NegEvaIDs.Contains(stage.ID)));
        _ = FirebaseIO.IncrementClearCount(Stage);
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
        if (Stage.Author == null) await Stage.GetAuthor();
        TxtAuthorName.text = Stage.Author.Name;
        if (Stage.Author.ID == User.NotFound.ID)
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
        await FirebaseIO.IncrementPosEvaCount(Stage);
        NowLoading.Close();
        UpdateControls();
        SetEvaluationInteractable(false);
    }

    public async void BtnNegEvaClicked()
    {
        NowLoading.Show(parent, "Connecting...");
        await FirebaseIO.IncrementNegEvaCount(Stage);
        NowLoading.Close();
        UpdateControls();
        SetEvaluationInteractable(false);
    }

    public async void BtnAuthorClicked()
    {
        NowLoading.Show(parent, "Connecting...");
        var user = await FirebaseIO.GetUser(Stage.AuthorID);
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
