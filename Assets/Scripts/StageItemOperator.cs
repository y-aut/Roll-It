using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageItemOperator : MonoBehaviour
{
    const int SHOWING_HEIGHT = 160;
    const int CLOSING_HEIGHT = 60;
    const float DURATION_TIME = 0.6f;  // Open/Closeにかかる秒数

    float generation_time = 0f;     // Open/Close処理を開始してから経過した秒数
    float heightDif;    // PnlDetailのheightとStageItemのheightの差

    // 各種コントロール
    public GameObject PnlDetail;
    public GameObject BtnDetail;
    public GameObject TxtName;
    public GameObject BtnPlay;
    public GameObject BtnEdit;
    public GameObject BtnDelete;
    public GameObject BtnPublish;
    public GameObject BtnRename;
    public GameObject TxtFigures;
    public GameObject ImgCheck;

    public Canvas canvas;

    // 対応するステージ
    public Stage Stage { get; set; }

    private enum StateEnum { Opening, Closing, Other }
    private StateEnum State { get; set; } = StateEnum.Other;

    // 展開中かどうか
    private bool _showDetail = true;
    public bool ShowDetail
    {
        get => _showDetail;
        set
        {
            if (ShowDetail == value) return;
            generation_time = 0f;
            State = value ? StateEnum.Opening : StateEnum.Closing;
            _showDetail = value;
        }
    }

    // 自分のステージか、オンラインのステージか
    private bool _isMyStage = true;
    public bool IsMyStage
    {
        get => _isMyStage;
        set
        {
            _isMyStage = value;
            BtnEdit.SetActive(value);
            BtnDelete.SetActive(value);
            BtnPublish.SetActive(value);
            BtnRename.SetActive(value);
        }
    }

    // ShowDetailの値によって表示を切り替えるオブジェクト
    private bool DetailsVisible
    {
        set
        {
            PnlDetail.SetActive(value);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 初期値
        UpdateControls();

        heightDif = gameObject.GetComponent<RectTransform>().sizeDelta.y - PnlDetail.GetComponent<RectTransform>().sizeDelta.y;

        // アニメーションなしで閉める
        _showDetail = false;
        DetailsVisible = false;
        BtnDetail.transform.rotation = Quaternion.Euler(0, 0, 180);

        var size = gameObject.GetComponent<RectTransform>().sizeDelta;
        size.y = CLOSING_HEIGHT;
        gameObject.GetComponent<RectTransform>().sizeDelta = size;
    }

    // Update is called once per frame
    void Update()
    {
        if (State == StateEnum.Opening)
        {
            if (generation_time == 0f)
            {
                DetailsVisible = true;
            }
            generation_time += Time.deltaTime;
            var size = gameObject.GetComponent<RectTransform>().sizeDelta;
            var t = Prefabs.ShowDetailCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME));
            size.y = CLOSING_HEIGHT * (1 - t) + SHOWING_HEIGHT * t;
            gameObject.GetComponent<RectTransform>().sizeDelta = size;

            BtnDetail.transform.rotation = Quaternion.Euler(0, 0, 180 * (1 - t));

            var pnlDet_height = Mathf.Max(0f, size.y - heightDif);
            PnlDetail.transform.localScale
                = new Vector3(1, pnlDet_height / PnlDetail.GetComponent<RectTransform>().sizeDelta.y, 1);

            if (generation_time >= DURATION_TIME)
            {
                State = StateEnum.Other;
            }
        }
        else if (State == StateEnum.Closing)
        {
            generation_time += Time.deltaTime;

            var size = gameObject.GetComponent<RectTransform>().sizeDelta;
            var t = Prefabs.ShowDetailCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME));
            size.y = CLOSING_HEIGHT * t + SHOWING_HEIGHT * (1 - t);
            gameObject.GetComponent<RectTransform>().sizeDelta = size;

            BtnDetail.transform.rotation = Quaternion.Euler(0, 0, -180 * t);

            var pnlDet_height = Mathf.Max(0f, size.y - heightDif);
            PnlDetail.transform.localScale
                = new Vector3(1, pnlDet_height / PnlDetail.GetComponent<RectTransform>().sizeDelta.y, 1);

            if (generation_time >= DURATION_TIME)
            {
                DetailsVisible = false;
                State = StateEnum.Other;
            }
        }
    }

    public void ToggleShowDetail()
    {
        ShowDetail = !ShowDetail;
    }

    // Stageをもとに更新
    public void UpdateControls()
    {
        TxtName.GetComponent<TextMeshProUGUI>().text = Stage.Name;
        if (IsMyStage)
        {   // LocalDataがnullであってはいけない
            BtnPublish.GetComponentInChildren<TextMeshProUGUI>().text = Stage.LocalData.IsPublished ? "Unpublish" : "Publish";
            ImgCheck.SetActive(Stage.LocalData.IsClearChecked);
        }
        TxtFigures.GetComponent<TextMeshProUGUI>().text = $"{Stage.ChallengeCount}\n" +
            $"{Stage.ClearCount}\n{string.Format("{0:0.00}", Stage.ClearRate * 100)} %";
    }

    // Click Events
    public void BtnEditClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            MessageBox.ShowDialog(canvas.transform, "Stages in public cannot be edited. You need to unpublish this stage first.", MessageBoxType.OKOnly, () => { });
            return;
        }
        CreateOperator.Stage = Stage;
        SceneManager.LoadScene("Create Scene");
    }

    public void BtnRenameClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            MessageBox.ShowDialog(canvas.transform, "Stages in public cannot be renamed. You need to unpublish this stage first.", MessageBoxType.OKOnly, () => { });
            return;
        }
        InputBox.ShowDialog(canvas.transform, "New name", result =>
        {
            Stage.Name = result;
            TxtName.GetComponent<TextMeshProUGUI>().text = result;
            GameData.Save();
        });
    }

    public void BtnPlayClicked()
    {
        PlayOperator.Ready(Stage, false, IsMyStage, false);
        SceneManager.LoadScene("Play Scene");
    }

    public void BtnDeleteClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            MessageBox.ShowDialog(canvas.transform, "Stages in public cannot be deleted. You need to unpublish this stage first.", MessageBoxType.OKOnly, () => { });
            return;
        }

        MessageBox.ShowDialog(canvas.transform, "Are you sure you want to delete this stage?",
            MessageBoxType.YesNo, () =>
        {
            GameData.Stages.Remove(Stage);
            Destroy(gameObject);
            GameData.Save();
        });
    }

    public void BtnPublishClicked()
    {
        if (Stage.LocalData.IsPublished)
        {
            if (!FirebaseIO.Available)
            {
                ErrorMessage.FirebaseUnavailable(canvas.transform);
                return;
            }
            var _ = FirebaseIO.UnpublishStage(Stage);
            StartCoroutine(UnpublishStage());
        }
        else
        {
            // クリアチェック
            if (!Stage.LocalData.IsClearChecked)
            {
                MessageBox.ShowDialog(canvas.transform, "A clear check needs to be done to publish this stage. Do you want to try it?",
                    MessageBoxType.YesNo, () =>
                {
                    PlayOperator.Ready(Stage, false, true, true);
                    SceneManager.LoadScene("Play Scene");
                });
                return;
            }

            if (!FirebaseIO.Available)
            {
                ErrorMessage.FirebaseUnavailable(canvas.transform);
                return;
            }
            var _ = FirebaseIO.PublishStage(Stage);
            StartCoroutine(PublishStage());
        }
    }

    private IEnumerator UnpublishStage()
    {
        NowLoading.Show(canvas.transform, "Unpublishing the stage...");

        while (!FirebaseIO.LoadFinished) yield return new WaitForSecondsRealtime(0.1f);

        GameData.Save();
        UpdateControls();

        NowLoading.Close();
    }

    private IEnumerator PublishStage()
    {
        NowLoading.Show(canvas.transform, "Publishing the stage...");

        while (!FirebaseIO.LoadFinished) yield return new WaitForSecondsRealtime(0.1f);

        GameData.Save();
        UpdateControls();

        NowLoading.Close();
    }

}
