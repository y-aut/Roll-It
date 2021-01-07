using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureIndexerOperator : MonoBehaviour
{
    // インデクサボタンの幅
    const int SELECTED_WIDTH = 50;
    const int UNSELECTED_WIDTH = 40;

    const float DURATION_TIME = 0.1f;  // Selectにかかる秒数

    float generation_time = 0f;     // Select処理を開始してから経過した秒数

    public GameObject StructurePanel;
    // 初めに選択されているボタン
    public GameObject FirstSelected;

    enum StateEnum { Selecting, Other }
    StateEnum State { get; set; } = StateEnum.Other;

    GameObject BtnUnselected;   // 選択を解除されているボタン
    GameObject BtnSelected;     // 選択されているボタン

    GameObject Selected
    {
        set
        {
            if (value == BtnSelected) return;
            // まだUnselectしきれていなければUnselectする
            if (BtnUnselected != null) {
                var size = BtnUnselected.GetComponent<RectTransform>().sizeDelta;
                size.x = UNSELECTED_WIDTH;
                BtnUnselected.GetComponent<RectTransform>().sizeDelta = size;
            }
            BtnUnselected = BtnSelected;
            BtnSelected = value;
            generation_time = 0f;
            SetActive();
            State = StateEnum.Selecting;
        }
    }

    // 選択されているカテゴリーのみを表示
    private void SetActive()
    {
        var cate = BtnSelected.GetComponent<StructureIndexerButtonOperator>().Category;
        foreach (var item in StructurePanel.GetComponentsInChildren<StructureItemOperator>(true))
        {
            item.gameObject.SetActive(item.StructureItem.Type.GetCategory() == cate);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        BtnSelected = FirstSelected;
        {   // BtnSelectのサイズをSELECTED_WIDTHに
            var size = BtnSelected.GetComponent<RectTransform>().sizeDelta;
            size.x = SELECTED_WIDTH;
            BtnSelected.GetComponent<RectTransform>().sizeDelta = size;
        }
        SetActive();
    }

    // Update is called once per frame
    void Update()
    {
        if (State == StateEnum.Selecting)
        {
            generation_time += Time.deltaTime;
            var t = Prefabs.StructureIndexerSelectCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME));

            var size = BtnSelected.GetComponent<RectTransform>().sizeDelta;
            size.x = UNSELECTED_WIDTH * (1 - t) + SELECTED_WIDTH * t;
            BtnSelected.GetComponent<RectTransform>().sizeDelta = size;

            size = BtnUnselected.GetComponent<RectTransform>().sizeDelta;
            size.x = UNSELECTED_WIDTH * t + SELECTED_WIDTH * (1 - t);
            BtnUnselected.GetComponent<RectTransform>().sizeDelta = size;

            if (generation_time >= DURATION_TIME)
            {
                State = StateEnum.Other;
            }
        }
    }

    public void BtnClicked(GameObject button)
    {
        Selected = button;
    }
}