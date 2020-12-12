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

    public TMP_InputField InputField;    // TextMeshProUGUI.Textはゼロ幅スペースが入る
    public Button BtnOK;
    public Button BtnCancel;
    public TextMeshProUGUI TxtDesc;
    public Popup popup;
    private Action<string> OKClickedAction;     // OKボタンが押されたときにコールする
    private Action CancelClickedAction;         // Cancelボタンが押されたときにコールする

    public static void ShowDialog(Transform parent, string desc, Action<string> OKClicked, Action CancelClicked = null, bool allowEmpty = false, bool allowCancel = true, string defaultString = "")
    {
        var input = Instantiate(Prefabs.InputBoxPrefab, parent, false);
        input.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = input.GetComponent<InputBox>();
        script.AllowEmpty = allowEmpty;
        script.AllowCancel = allowCancel;
        script.defaultString = defaultString;
        script.description = desc;
        script.OKClickedAction = OKClicked;
        script.CancelClickedAction = CancelClicked ?? (() => { });
        script.popup.Open();
    }

    private void Start()
    {
        InputField.text = defaultString;
        TxtDesc.text = description;
        if (!AllowCancel)
        {
            BtnOK.gameObject.transform.localPosition = BtnCancel.gameObject.transform.localPosition;
            BtnCancel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        BtnOK.interactable = InputField.text != "" || AllowEmpty;

        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BtnCancel_Click();
        }
    }

    public void BtnOK_Click()
    {
        if (InputField.text == "" && !AllowEmpty) return;
        popup.Close();
        var txt = InputField.text;
        StartCoroutine(WaitClose(() => OKClickedAction(txt)));
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
