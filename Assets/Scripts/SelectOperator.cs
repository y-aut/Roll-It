using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectOperator : MonoBehaviour
{
    public Canvas canvas;
    public GameObject content;
    public Button BtnNew;
    public Camera PrevCam;

    public Stage PrevStage { get; set; }    // Previewで表示しているStage

    // 自分のステージか
    public static bool IsMyStages { get; set; } = true;

    // Start is called before the first frame update
    async void Start()
    {
        BtnNew.gameObject.SetActive(IsMyStages);
        if (IsMyStages)
        {
            // ローカルのStagesを更新
            NowLoading.Show(canvas.transform, "Updating stages...");
            await FirebaseIO.UpdateMyStages();
            NowLoading.Close();

            // StageをContentに追加
            foreach (var stage in GameData.Stages)
            {
                var item = Instantiate(Prefabs.StageItemPrefab, content.transform, false);
                var script = item.GetComponent<StageItemOperator>();
                script.Stage = stage;
                script.canvas = canvas;
                script.selectOp = this;
            }
        }
        else
        {
            // オンラインのステージを追加
            NowLoading.Show(canvas.transform, "Loading stages...");

            var stages = await FirebaseIO.GetAllStages();
            foreach (var stage in stages)
            {
                var item = Instantiate(Prefabs.StageItemPrefab, content.transform, false);
                var script = item.GetComponent<StageItemOperator>();
                script.Stage = stage;
                script.canvas = canvas;
                script.IsMyStage = false;
                script.selectOp = this;
            }

            NowLoading.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;

        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMenu();
        }
    }

    public void BackToMenu()
    {
        Scenes.LoadScene(SceneType.Menu);
    }

    public void BtnNewClicked()
    {
        var stage = new Stage();
        GameData.Stages.Add(stage);
        CreateOperator.Ready(stage, true);
        Scenes.LoadScene(SceneType.Create);
    }

    // オブジェクトを配置してPrevCamで表示する
    public void CreatePreview(Stage stage)
    {
        if (PrevStage != null) PrevStage.Destroy();
        PrevStage = stage;
        stage.Create();
        PrevCam.transform.position = stage.Start.Position
            + new Vector3(0, GameConst.PLAY_CAMDIST_VER + Prefabs.BallPrefab.transform.localScale.y, -GameConst.PLAY_CAMDIST_HOR);
    }


}
