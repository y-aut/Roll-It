using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ダイアログボックスを表示
public class Popup : MonoBehaviour
{
    const int POPUP_DURATION = 10;  // Open/Closeにかかるフレーム数

    public enum StateEnum { Opening, Closing, Other }
    public StateEnum State { get; private set; } = StateEnum.Other;

    GameObject PnlBlack;
    int generation = 0;     // Open/Close処理を開始してから経過したフレーム数
    float timeScaleDef;     // timeScaleを0に変更する前のtimeScale

    // Update is called once per frame
    void Update()
    {
        if (State == StateEnum.Opening)
        {
            gameObject.transform.localScale = Prefabs.OpenCurve.Evaluate(++generation / (float)POPUP_DURATION) * Vector3.one;
            if (generation == POPUP_DURATION) State = StateEnum.Other;
        }
        else if (State == StateEnum.Closing)
        {
            gameObject.transform.localScale = Prefabs.CloseCurve.Evaluate(++generation / (float)POPUP_DURATION) * Vector3.one;
            if (generation == POPUP_DURATION)
            {
                PnlBlack.SetActive(false);
                Time.timeScale = timeScaleDef;
                Destroy(gameObject);
            }
        }
    }

    public void Open()
    {
        // 時間を停止し、操作を無効にするためPnlBlackを表示
        timeScaleDef = Time.timeScale;
        Time.timeScale = 0f;

        PnlBlack = new List<Image>(
            gameObject.transform.parent.gameObject.GetComponentsInChildren<Image>(true))
            .Find(i => i.gameObject.name == "PnlBlack").gameObject;
        PnlBlack.SetActive(true);

        generation = 0;
        State = StateEnum.Opening;
    }

    public void CloseAndDestroy()
    {
        PnlBlack.SetActive(false);
        generation = 0;
        State = StateEnum.Closing;
    }
}
