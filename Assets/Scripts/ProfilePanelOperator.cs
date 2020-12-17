using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProfilePanelOperator : MonoBehaviour
{
    public TextMeshProUGUI TxtName;
    public Popup popup;

    public static void ShowDialog(Transform parent)
    {
        var panel = Instantiate(Prefabs.ProfilePanelPrefab, parent, false);
        panel.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = panel.GetComponent<ProfilePanelOperator>();
        script.popup.Open();
    }

    private void Start()
    {
        TxtName.text = GameData.User.Name;
    }

    private void Update()
    {
        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BtnClose_Click();
        }
    }

    public void BtnRename_Click()
    {
        InputBox.ShowDialog(transform.parent, "New nickname", async name =>
        {
            NowLoading.Show(transform.parent, "Changing the nickname...");
            TxtName.text = name;
            await FirebaseIO.ChangeUserName(name);
            NowLoading.Close();
        }, defaultString: GameData.User.Name);
    }

    public void BtnClose_Click()
    {
        popup.Close();
        StartCoroutine(WaitClose());
    }

    private IEnumerator WaitClose()
    {
        while (popup.IsClosing) yield return new WaitForSeconds(0.1f);
        Destroy(popup.gameObject);
    }
}
