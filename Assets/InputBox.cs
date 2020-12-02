using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputBox : MonoBehaviour
{
    private bool AllowEmpty;
    private bool AllowCancel;
    private string defaultString;
    private string description;

    private TMP_InputField Text;    // TextMeshProUGUI.Textはゼロ幅スペースが入る
    private Button BtnOK;
    private Button BtnCancel;
    private Popup popup;
    private Action<string> OKClickedAction;     // OKボタンが押されたときにコールする

    public static void ShowDialog(Action<string> OKClicked, Transform parent, string desc, bool allowEmpty = false, bool allowCancel = true, string defaultString = "")
    {
        var input = Instantiate(Prefabs.InputBoxPrefab, parent, false);
        input.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = input.GetComponent<InputBox>();
        script.AllowEmpty = allowEmpty;
        script.AllowCancel = allowCancel;
        script.defaultString = defaultString;
        script.description = desc;
        script.OKClickedAction = OKClicked;
        script.popup = input.GetComponent<Popup>();
        script.popup.Open();
    }

    private void Start()
    {
        Text = gameObject.GetComponentInChildren<TMP_InputField>();
        Text.text = defaultString;
        var TxtDesc = new List<TextMeshProUGUI>(gameObject.GetComponentsInChildren<TextMeshProUGUI>()).Find(i => i.gameObject.name == "TxtDesc");
        TxtDesc.text = description;
        var buttons = new List<Button>(GetComponentsInChildren<Button>());
        BtnOK = buttons.Find(i => i.gameObject.name == "BtnOK");
        BtnCancel = buttons.Find(i => i.gameObject.name == "BtnCancel");
        if (!AllowCancel)
        {
            BtnOK.gameObject.transform.position = BtnCancel.gameObject.transform.position;
            BtnCancel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        BtnOK.interactable = Text.text != "" || AllowEmpty;
    }

    public void BtnOK_Click()
    {
        if (Text.text == "" && !AllowEmpty) return;
        popup.CloseAndDestroy();
        OKClickedAction(Text.text);
    }

    public void BtnCancel_Click()
    {
        popup.CloseAndDestroy();
    }
}
