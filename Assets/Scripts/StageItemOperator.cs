using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageItemOperator : MonoBehaviour
{
    // 各種コントロール
    public TextMeshProUGUI TxtName;
    public GameObject ImgCheck;
    public TextMeshProUGUI TxtChallengeCount;
    public TextMeshProUGUI TxtClearCount;
    public TextMeshProUGUI TxtClearRate;
    public TextMeshProUGUI TxtPosEvaCount;
    public TextMeshProUGUI TxtChallengeCountFooter;
    public RawImage ImgPreview;

    public Transform parent;
    public StageViewContentOperator ParentView { get; set; }

    // 対応するステージ
    public Stage Stage { get; set; }
    // 自分で編集できるステージか
    public bool IsMyStage { get; set; } = true;
    // PanelからAuthor情報を取得できるようにするか
    public bool AuthorAccessible { get; set; }

    // 現在のImgPreviewの画像を保存し、再設定
    public void SetPreview()
    {
        var source = ImgPreview.texture;
        var copy = new RenderTexture((RenderTexture)source);
        Graphics.CopyTexture(source, copy);
        ImgPreview.texture = copy;
    }

    public void Initialize()
    {
        UpdateValue();
    }

    // Stageをもとに更新
    public void UpdateValue()
    {
        TxtName.text = Stage.Name;
        if (IsMyStage)
        {
            ImgCheck.SetActive(Stage.LocalData.IsClearChecked);
        }
        else
        {
            ImgCheck.SetActive(GameData.User.LocalData.ClearedIDs.Contains(Stage.ID));
        }
        TxtChallengeCount.text = TxtChallengeCountFooter.text = string.Format("{0:#,0}", Stage.ChallengeCount);
        TxtClearCount.text = string.Format("{0:#,0}", Stage.ClearCount);
        TxtClearRate.text = string.Format("{0:0.00}", Stage.ClearRate * 100);
        TxtPosEvaCount.text = string.Format("{0:#,0}", Stage.PosEvaCount);
    }

    public void ThisClicked()
    {
        StagePanelOperator.ShowDialog(parent, this, ParentView.menuOp, AuthorAccessible);
        // 値の変更をまとめて反映
        UpdateValue();
    }
    
}
