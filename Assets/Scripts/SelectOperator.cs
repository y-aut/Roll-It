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
    public GameObject stageItemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // StageをContentに追加
        var stages = GameData.MyStages;
        for (int i = 0; i < stages.Count; ++i)
        {
            var item = Instantiate(stageItemPrefab, content.transform, false);
            var TxtName = new List<TextMeshProUGUI>(item.GetComponentsInChildren<TextMeshProUGUI>()).Find(j => j.gameObject.name == "TxtName");
            TxtName.text = stages[i].Name;
            TxtName.name = "TxtName_" + stages[i].ID;  // 名前変更時に探す

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
        InputBox.ShowDialog(result =>
        {
            GameData.MyStages.Find(i => i.ID == stageId).Name = result;
            var TxtName = new List<TextMeshProUGUI>(canvas.GetComponentsInChildren<TextMeshProUGUI>()).Find(i => i.gameObject.name == "TxtName_" + stageId);
            TxtName.text = result;
            GameData.Save();
        }, canvas.transform, "New name");
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
        GameData.Save();
    }

}
