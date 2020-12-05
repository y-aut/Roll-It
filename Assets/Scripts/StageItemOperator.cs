using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageItemOperator : MonoBehaviour
{
    const int SHOWING_HEIGHT = 160;
    const int CLOSING_HEIGHT = 60;
    const float DURATION_TIME = 0.6f;  // Open/Closeにかかる秒数

    float generation_time = 0f;     // Open/Close処理を開始してから経過した秒数
    float heightDif;    // PnlDetailのheightとStageItemのheightの差

    // 各種コントロール
    public GameObject PnlDetail;
    public GameObject BtnDetail;

    private enum StateEnum { Opening, Closing, Other }
    private StateEnum State { get; set; } = StateEnum.Other;

    // 展開中かどうか
    private bool _showDetail = true;
    public bool ShowDetail
    {
        get => _showDetail;
        set
        {
            if (ShowDetail == value) return;
            generation_time = 0f;
            State = value ? StateEnum.Opening : StateEnum.Closing;
            _showDetail = value;
        }
    }

    // ShowDetailの値によって表示を切り替えるオブジェクト
    private bool DetailsVisible
    {
        set
        {
            PnlDetail.SetActive(value);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        heightDif = gameObject.GetComponent<RectTransform>().sizeDelta.y - PnlDetail.GetComponent<RectTransform>().sizeDelta.y;

        // アニメーションなしで閉める
        _showDetail = false;
        DetailsVisible = false;
        BtnDetail.transform.rotation = Quaternion.Euler(0, 0, 180);

        var size = gameObject.GetComponent<RectTransform>().sizeDelta;
        size.y = CLOSING_HEIGHT;
        gameObject.GetComponent<RectTransform>().sizeDelta = size;
    }

    // Update is called once per frame
    void Update()
    {
        if (State == StateEnum.Opening)
        {
            if (generation_time == 0f)
            {
                DetailsVisible = true;
            }
            generation_time += Time.deltaTime;
            var size = gameObject.GetComponent<RectTransform>().sizeDelta;
            var t = Prefabs.ShowDetailCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME));
            size.y = CLOSING_HEIGHT * (1 - t) + SHOWING_HEIGHT * t;
            gameObject.GetComponent<RectTransform>().sizeDelta = size;

            BtnDetail.transform.rotation = Quaternion.Euler(0, 0, 180 * (1 - t));

            var pnlDet_height = Mathf.Max(0f, size.y - heightDif);
            PnlDetail.transform.localScale
                = new Vector3(1, pnlDet_height / PnlDetail.GetComponent<RectTransform>().sizeDelta.y, 1);

            if (generation_time >= DURATION_TIME)
            {
                State = StateEnum.Other;
            }
        }
        else if (State == StateEnum.Closing)
        {
            generation_time += Time.deltaTime;

            var size = gameObject.GetComponent<RectTransform>().sizeDelta;
            var t = Prefabs.ShowDetailCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME));
            size.y = CLOSING_HEIGHT * t + SHOWING_HEIGHT * (1 - t);
            gameObject.GetComponent<RectTransform>().sizeDelta = size;

            BtnDetail.transform.rotation = Quaternion.Euler(0, 0, -180 * t);

            var pnlDet_height = Mathf.Max(0f, size.y - heightDif);
            PnlDetail.transform.localScale
                = new Vector3(1, pnlDet_height / PnlDetail.GetComponent<RectTransform>().sizeDelta.y, 1);

            if (generation_time >= DURATION_TIME)
            {
                DetailsVisible = false;
                State = StateEnum.Other;
            }
        }
    }

    public void ToggleShowDetail()
    {
        ShowDetail = !ShowDetail;
    }

}
