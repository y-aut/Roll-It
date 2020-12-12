﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    private MessageBoxType AnswerType;
    private string description;

    public Button BtnOK;
    public Button BtnCancel;
    public TextMeshProUGUI TxtDesc;
    public Popup popup;
    private Action OKClickedAction;         // OKボタンが押されたときにコールする
    private Action CancelClickedAction;     // キャンセルボタンが押されたときにコールする

    public static void ShowDialog(Transform parent, string desc, MessageBoxType type, Action OKClicked, Action CancelClicked = null)
    {
        var msgbox = Instantiate(Prefabs.MessageBoxPrefab, parent, false);
        msgbox.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = msgbox.GetComponent<MessageBox>();
        script.AnswerType = type;
        script.description = desc;
        script.OKClickedAction = OKClicked;
        script.CancelClickedAction = CancelClicked ?? (() => { });
        script.popup.Open();
    }

    private void Start()
    {
        TxtDesc.text = description;
        if (AnswerType == MessageBoxType.OKOnly || AnswerType == MessageBoxType.YesOnly)
        {
            BtnOK.gameObject.transform.localPosition = BtnCancel.gameObject.transform.localPosition;
            BtnCancel.gameObject.SetActive(false);
        }
        if (AnswerType == MessageBoxType.YesNo || AnswerType == MessageBoxType.YesOnly)
        {
            BtnOK.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
            BtnCancel.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "No";
        }
    }

    private void Update()
    {
        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BtnCancel_Click();
        }
    }

    public void BtnOK_Click()
    {
        popup.Close();
        StartCoroutine(WaitClose(OKClickedAction));
    }

    public void BtnCancel_Click()
    {
        popup.Close();
        StartCoroutine(WaitClose(CancelClickedAction));
    }

    private IEnumerator WaitClose(Action after)
    {
        while (popup.IsClosing) yield return new WaitForSeconds(0.1f);
        after();
        Destroy(popup.gameObject);
    }
}

public enum MessageBoxType
{
    OKCancel, YesNo, OKOnly, YesOnly,
}