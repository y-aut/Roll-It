using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StagePanelOperator : MonoBehaviour
{
    // 各種コントロール
    public TextMeshProUGUI TxtName;
    public GameObject BtnPlay;
    public GameObject BtnEdit;
    public GameObject BtnDelete;
    public GameObject BtnPublish;
    public GameObject BtnRename;
    public GameObject ImgRename;
    public GameObject BtnDownload;
    public GameObject ImgCheck;
    public TextMeshProUGUI TxtAuthorName;
    public Button BtnAuthorName;
    public TextMeshProUGUI TxtChallengeCount;
    public TextMeshProUGUI TxtClearCount;
    public TextMeshProUGUI TxtClearRate;
    public TextMeshProUGUI TxtPosEvaCount;
    public TextMeshProUGUI TxtNegEvaCount;
    public GameObject ImgGraphPos;
    public RawImage ImgPreview;

    public MenuOperator menuOp;
    private Transform parent;
    public StageItemOperator StageItem; // このステージに対応するStageItem
    public Popup popup;

    // 対応するステージ
    public Stage Stage { get; set; }

    // 自分のステージか、オンラインのステージか
    private bool _isMyStage = true;
    private bool IsMyStage
    {
        get => _isMyStage;
        set
        {
            _isMyStage = value;
            BtnEdit.SetActive(value);
            BtnDelete.SetActive(value);
            BtnPublish.SetActive(value);
            BtnRename.SetActive(value);
            BtnDownload.SetActive(!value);
        }
    }

    // Author情報を取得できるようにするか
    private bool AuthorAccessible;

    public static void ShowDialog(Transform parent, StageItemOperator stageItem, MenuOperator menuOp, bool authorAccessible)
    {
        var panel = Instantiate(Prefabs.StagePanelPrefab, parent, false);
        panel.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = panel.GetComponent<StagePanelOperator>();

        script.Stage = stageItem.Stage;
        script.IsMyStage = stageItem.IsMyStage;
        script.parent = parent;
        script.StageItem = stageItem;
        script.menuOp = menuOp;
        script.AuthorAccessible = authorAccessible;
        script.ImgPreview.texture = stageItem.ImgPreview.texture;

        script.Initialize();
        script.popup.Open();
    }

    public void Initialize()
    {
        // 初期値
        InitializeControls();
        UpdateControls();
        UpdateAuthor();
    }

    // 開いた直後のみ呼び出す処理
    private void InitializeControls()
    {
        if (!AuthorAccessible)
        {
            Destroy(BtnAuthorName);
            TxtAuthorName.fontStyle = FontStyles.Normal;    // 下線を削除
        }
        if (IsMyStage)
        {
            // ダウンロードしたステージ、またはユーザーIDを未取得ならば公開させない
            BtnPublish.GetComponent<Button>().interactable
                = Stage.AuthorID == GameData.User.ID && GameData.User.ID != IDType.Empty;
        }
        else
        {
            // 自分のステージはダウンロードさせない
            BtnDownload.GetComponent<Button>().interactable = Stage.AuthorID != GameData.User.ID;
        }
        ImgRename.SetActive(IsMyStage);
    }

    // StageをもとにAuthor以外の値を更新
    private void UpdateControls()
    {
        TxtName.text = Stage.Name;
        ImgRename.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            Mathf.Min(TxtName.preferredWidth, TxtName.gameObject.GetComponent<RectTransform>().rect.width - 4) + 10, 0, 0);
        if (IsMyStage)
        {
            BtnPublish.GetComponentInChildren<TextMeshProUGUI>().text
                = Stage.LocalData.IsPublished ? "Make Private" : "Make Public";
            ImgCheck.SetActive(Stage.LocalData.IsClearChecked);
        }
        else
        {
            ImgCheck.SetActive(GameData.User.LocalData.ClearedIDs.Contains(Stage.ID));
        }
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
        if (author.IsNotFound && AuthorAccessible)
        {
            Destroy(BtnAuthorName);
            TxtAuthorName.fontStyle = FontStyles.Normal;    // 下線を削除
        }
    }

    // Click Events
    public void BtnEditClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            MessageBox.ShowDialog(parent, "Stages in public cannot be edited. You need to make this stage private first.", MessageBoxType.OKOnly, () => { });
            return;
        }
        StageItem.ParentView.menuOp.SaveHistory();
        CreateOperator.Ready(Stage, false);
        Scenes.LoadScene(SceneType.Create);
    }

    public void BtnRenameClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            MessageBox.ShowDialog(parent, "Stages in public cannot be renamed. You need to make this stage private first.", MessageBoxType.OKOnly, () => { });
            return;
        }
        InputBox.ShowDialog(parent, "New name", result =>
        {
            Stage.Name = result;
            UpdateControls();
            StageItem.UpdateValue();
            GameData.Save();
        }, defaultString: Stage.Name);
    }

    public void BtnPlayClicked()
    {
        StageItem.ParentView.menuOp.SaveHistory();
        PlayOperator.Ready(Stage, false, IsMyStage, false);
        Scenes.LoadScene(SceneType.Play);
    }

    public void BtnDeleteClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            MessageBox.ShowDialog(parent, "Stages in public cannot be deleted. You need to make this stage private first.", MessageBoxType.OKOnly, () => { });
            return;
        }

        MessageBox.ShowDialog(parent, "Are you sure you want to delete this stage?",
            MessageBoxType.YesNo, () =>
            {
                // Stageの消去, StageItemのDestroy, StageViewからの消去をした後にこのオブジェクトをDestroy
                GameData.Stages.Remove(Stage);
                GameData.Save();
                StageItem.ParentView.RemoveStage(Stage);

                Destroy(StageItem.gameObject);
                popup.CloseAndDestroy();
            });
    }

    public async void BtnPublishClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            // Unpublish
            NowLoading.Show(parent, "Unpublishing the stage...");

            try
            {
                await FirebaseIO.UnpublishStage(Stage).WaitWithTimeOut();
                GameData.Save();
                UpdateControls();
            }
            catch (System.Exception e)
            {
                e.Show(parent);
            }

            NowLoading.Close();
        }
        else
        {
            // クリアチェック
            if (!Stage.LocalData.IsClearChecked)
            {
                MessageBox.ShowDialog(parent, "A clear check needs to be done to publish this stage. Do you try it now?",
                    MessageBoxType.YesNo, () =>
                    {
                        StageItem.ParentView.menuOp.SaveHistory();
                        PlayOperator.Ready(Stage, false, true, true);
                        Scenes.LoadScene(SceneType.Play);
                    });
            }
            else
            {
                // Publish
                NowLoading.Show(parent, "Publishing the stage...");

                try
                {
                    await FirebaseIO.PublishStage(Stage).WaitWithTimeOut();
                    GameData.Save();
                    UpdateControls();
                }
                catch (System.Exception e)
                {
                    e.Show(parent);
                }

                NowLoading.Close();
            }
        }
    }

    public void BtnDownloadClicked()
    {
        var stage = new Stage(Stage)
        {
            LocalData = new StageLocal()
        };
        GameData.Stages.Add(stage);
        MessageBox.ShowDialog(parent, "Downloaded the stage.", MessageBoxType.OKOnly, () => { });
        BtnDownload.GetComponent<Button>().interactable = false;
    }

    public async void BtnAuthorClicked()
    {
        UserPanelOperator.ShowDialog(parent, await Cache.GetUser(Stage.AuthorID), menuOp);
    }

    public void BtnCloseClicked()
    {
        popup.CloseAndDestroy();
    }

    private void Update()
    {
        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BtnCloseClicked();
        }
    }


}
