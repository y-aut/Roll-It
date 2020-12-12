using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ダイアログボックスを表示
public class Popup : MonoBehaviour
{
    const float DURATION_TIME = 0.2f;  // Open/Closeにかかる秒数

    private enum StateEnum { Opening, Closing, Other }
    private StateEnum State { get; set; } = StateEnum.Other;

    GameObject PnlBlack;
    float generation_time = 0f;     // Open/Close処理を開始してから経過した秒数
    float timeScaleDef;     // timeScaleを0に変更する前のtimeScale

    // Closeするまで待機してもらう
    public bool IsClosing => State == StateEnum.Closing;

    // Update is called once per frame
    void Update()
    {
        if (State == StateEnum.Opening)
        {
            generation_time += Time.unscaledDeltaTime;
            gameObject.transform.localScale = Prefabs.OpenCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME)) * Vector3.one;
            if (generation_time >= DURATION_TIME) State = StateEnum.Other;
        }
        else if (State == StateEnum.Closing)
        {
            generation_time += Time.unscaledDeltaTime;
            gameObject.transform.localScale = Prefabs.CloseCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME)) * Vector3.one;
            if (generation_time >= DURATION_TIME)
            {
                PnlBlack.SetActive(false);
                Time.timeScale = timeScaleDef;
                State = StateEnum.Other;
            }
        }
    }

    public void Open()
    {
        // 時間を停止し、操作を無効にするためPnlBlackを表示
        timeScaleDef = Time.timeScale;
        Time.timeScale = 0f;

        try
        {
            PnlBlack = new List<Image>(
                gameObject.transform.parent.gameObject.GetComponentsInChildren<Image>(true))
                .Find(i => i.gameObject.name == "PnlBlack").gameObject;
            PnlBlack.SetActive(true);
        }
        catch (System.Exception)
        {
            throw new System.Exception("PnlBlack does not exist in the current scene.");
        }

        generation_time = 0f;
        State = StateEnum.Opening;
    }

    // Destroyは各自で
    public void Close()
    {
        PnlBlack.SetActive(false);
        generation_time = 0f;
        State = StateEnum.Closing;
    }
}
