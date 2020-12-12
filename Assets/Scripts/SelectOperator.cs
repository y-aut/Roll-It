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

    // 自分のステージか
    public static bool IsMyStages { get; set; } = true;

    // Start is called before the first frame update
    void Start()
    {
        BtnNew.gameObject.SetActive(IsMyStages);
        if (IsMyStages)
        {
            // StageをContentに追加
            foreach (var stage in GameData.Stages)
            {
                var item = Instantiate(Prefabs.StageItemPrefab, content.transform, false);
                var script = item.GetComponent<StageItemOperator>();
                script.Stage = stage;
                script.canvas = canvas;
            }
        }
        else
        {
            // オンラインのステージを追加
            FirebaseIO.GetAllStages();
            StartCoroutine(LoadItemList());
        }
    }

    // 非同期で読み込んだステージを反映
    IEnumerator LoadItemList()
    {
        NowLoading.Show(canvas.transform, "Loading stages...");

        while (!FirebaseIO.LoadFinished)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        foreach (var stage in FirebaseIO.AnswerStages)
        {
            var item = Instantiate(Prefabs.StageItemPrefab, content.transform, false);
            var script = item.GetComponent<StageItemOperator>();
            script.Stage = stage;
            script.canvas = canvas;
            script.IsMyStage = false;
        }

        NowLoading.Close();
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
        SceneManager.LoadScene("Menu Scene");
    }

    public void BtnNewClicked()
    {
        var stage = new Stage();
        stage.Initialize();
        GameData.Stages.Add(stage);
        CreateOperator.Stage = stage;
        SceneManager.LoadScene("Create Scene");
    }


}
