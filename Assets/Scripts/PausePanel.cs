using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
    public Popup popup;
    private PlayOperator playOperator;
    private Action afterClosed;

    // ポーズ画面を表示
    public static void ShowDialog(PlayOperator playOp, Transform parent, Action after)
    {
        var pause = Instantiate(Prefabs.PausePanelPrefab, parent, false);
        pause.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = pause.GetComponent<PausePanel>();
        script.playOperator = playOp;
        script.afterClosed = after;
        script.popup.Open();
    }

    private void Update()
    {
        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BtnClose_Click();
        }
    }

    public void BtnSetOrigin_Click()
    {
        StickListener.SetOrigin();
    }

    public void BtnRestart_Click()
    {
        popup.Close();
        StartCoroutine(WaitClose(playOperator.Restart));
    }

    public void BtnQuit_Click()
    {
        popup.Close();
        StartCoroutine(WaitClose(playOperator.Quit));
    }

    public void BtnClose_Click()
    {
        popup.Close();
        StartCoroutine(WaitClose(afterClosed));
    }

    private IEnumerator WaitClose(Action after)
    {
        while (popup.IsClosing) yield return new WaitForEndOfFrame();
        after();
        Destroy(popup.gameObject);
    }
}
