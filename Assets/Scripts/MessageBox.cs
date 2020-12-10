using System;
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
    private Action OKClickedAction;     // OKボタンが押されたときにコールする

    public static void ShowDialog(Action OKClicked, Transform parent, string desc, MessageBoxType type)
    {
        var msgbox = Instantiate(Prefabs.MessageBoxPrefab, parent, false);
        msgbox.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = msgbox.GetComponent<MessageBox>();
        script.AnswerType = type;
        script.description = desc;
        script.OKClickedAction = OKClicked;
        script.popup.Open();
    }

    private void Start()
    {
        TxtDesc.text = description;
        if (AnswerType == MessageBoxType.OKOnly || AnswerType == MessageBoxType.YesOnly)
        {
            BtnOK.gameObject.transform.position = BtnCancel.gameObject.transform.position;
            BtnCancel.gameObject.SetActive(false);
        }
        if (AnswerType == MessageBoxType.YesNo || AnswerType == MessageBoxType.YesOnly)
        {
            BtnOK.gameObject.GetComponent<TextMeshProUGUI>().text = "Yes";
            BtnCancel.gameObject.GetComponent<TextMeshProUGUI>().text = "No";
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
        popup.CloseAndDestroy();
        OKClickedAction();
    }

    public void BtnCancel_Click()
    {
        popup.CloseAndDestroy();
    }
}

public enum MessageBoxType
{
    OKCancel, YesNo, OKOnly, YesOnly,
}