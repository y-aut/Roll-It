﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ダイアログボックスを表示
public class Popup : MonoBehaviour
{
    const float DURATION_TIME = 0.2f;  // Open/Closeにかかる秒数

    public bool StopTime = true;
    private StateEnum State { get; set; } = StateEnum.Other;

    // PnlBlackがあるCanvasをさす
    // 初めに開かれるPopupウィンドウはその親がCanvasなので、親を設定する
    // 次以降に開かれるPopupはその親もPopupなので、そのPnlBlackCanvasをコピー
    GameObject PnlBlackCanvas;

    GameObject PnlBlack;
    bool pnlBlackVisible;   // もともとPnlBlackが見えていたか
    int pnlBlackIndex;      // PnlBlackのSiblingIndex
    float generation_time = 0f;     // Open/Close処理を開始してから経過した秒数
    float timeScaleDef;     // timeScaleを0に変更する前のtimeScale
    bool flgDestroy = false;    // Close()した後に自動でDestroyするか

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
                if (StopTime)
                {
                    Time.timeScale = timeScaleDef;
                }
                State = StateEnum.Other;
                if (flgDestroy) Destroy(gameObject);
            }
        }
    }

    public void Open()
    {
        // 時間を停止し、操作を無効にするためPnlBlackを表示
        if (StopTime)
        {
            timeScaleDef = Time.timeScale;
            Time.timeScale = 0f;
        }

        try
        {
            PnlBlackCanvas = gameObject.transform.parent.gameObject;
            var parentPopup = PnlBlackCanvas.GetComponent<Popup>();
            // 親もPopupウィンドウのときは、親のPnlBlackCanvasをコピー
            if (parentPopup != null)
                PnlBlackCanvas = parentPopup.PnlBlackCanvas;
            
            PnlBlack = new List<Image>(PnlBlackCanvas.GetComponentsInChildren<Image>(true))
                .Find(i => i.gameObject.name == "PnlBlack").gameObject;
            pnlBlackVisible = PnlBlack.activeInHierarchy;
            pnlBlackIndex = PnlBlack.transform.GetSiblingIndex();
            PnlBlack.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);  // 上から2番目に設定
            PnlBlack.SetActive(true);
        }
        catch (Exception)
        {
            throw new Exception("PnlBlack does not exist in the current scene.");
        }

        gameObject.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        generation_time = 0f;
        State = StateEnum.Opening;
    }

    // Destroyは各自で
    public void Close()
    {
        if (!pnlBlackVisible) PnlBlack.SetActive(false);
        // PnlBlackがこのウィンドウよりも上にあるとき、上から別のウィンドウが開いているのでそのままにしておく
        if (PnlBlack.transform.GetSiblingIndex() <= transform.GetSiblingIndex())
        {
            // 後ろのpopupウィンドウが先に閉じられているときは、pnlBlackが最前面になってしまうので、上から2番目に設定する
            PnlBlack.transform.SetSiblingIndex(Math.Min(pnlBlackIndex, transform.GetSiblingIndex() - 2));
        }
        generation_time = 0f;
        State = StateEnum.Closing;
    }

    public void CloseAndDestroy()
    {
        flgDestroy = true;
        Close();
    }
}

public enum StateEnum { Opening, Closing, Other }
