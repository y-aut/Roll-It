using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectOperator : MonoBehaviour
{
    public GameObject content;
    public GameObject stageItemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // StageをContentに追加
        var stages = GameData.MyStages;
        for (int i = 0; i < stages.Count; ++i)
        {
            var item = Instantiate(stageItemPrefab);
            // イベントを追加
            var btns = item.GetComponentsInChildren<Button>();
            int id = stages[i].ID;  // 遅延評価を防ぐ
            foreach (var btn in btns)
            {
                if (btn.name == "BtnEdit") btn.onClick.AddListener(() => BtnEditClicked(id));
                else if (btn.name == "BtnRename") btn.onClick.AddListener(() => BtnRenameClicked(id));
                else if (btn.name == "BtnPlay") btn.onClick.AddListener(() => BtnPlayClicked(id));
                else if (btn.name == "BtnDelete") btn.onClick.AddListener(() => BtnDeleteClicked(id));
            }

            item.transform.SetParent(content.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu Scene");
    }

    public void BtnNewClicked()
    {
        var stage = new Stage();
        stage.Initialize();
        GameData.MyStages.Add(stage);
        CreateOperator.Stage = stage;
        SceneManager.LoadScene("Create Scene");
    }

    public void BtnEditClicked(int stageId)
    {
        CreateOperator.Stage = GameData.MyStages.Find(i => i.ID == stageId);
        SceneManager.LoadScene("Create Scene");
    }

    public void BtnRenameClicked(int stageId)
    {

    }

    public void BtnPlayClicked(int stageId)
    {
        PlayOperator.Stage = GameData.MyStages.Find(i => i.ID == stageId);
        SceneManager.LoadScene("Play Scene");
    }

    public void BtnDeleteClicked(int stageId)
    {
        GameData.MyStages.Remove(GameData.MyStages.Find(i => i.ID == stageId));
        SceneManager.LoadScene("Select Scene");
    }

}
