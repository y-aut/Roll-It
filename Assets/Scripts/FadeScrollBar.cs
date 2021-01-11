using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 移動中のみ表示するスクロールバー
public class FadeScrollBar : MonoBehaviour
{
    public GameObject ScrollHandle;
    private Image ImgScrollHandle;
    private RectTransform RectScrollHandle;

    private float lastRectX;
    private float sinceValueChanged;
    private bool visible = false;

    private void Awake()
    {
        ImgScrollHandle = ScrollHandle.GetComponent<Image>();
        RectScrollHandle = ScrollHandle.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ImgScrollHandle.CrossFadeAlpha(0, 0, true);
        lastRectX = RectScrollHandle.localPosition.x;
    }

    private void Update()
    {
        if (Mathf.Abs(lastRectX - RectScrollHandle.localPosition.x) > 0.1f)
        {
            // スクロールされた
            lastRectX = RectScrollHandle.localPosition.x;
            sinceValueChanged = 0f;
            ImgScrollHandle.CrossFadeAlpha(1, 0, true);  // フェードイン
            visible = true;
        }
        else
        {
            if (!visible) return;
            sinceValueChanged += Time.deltaTime;
            if (sinceValueChanged > 1f)
            {
                ImgScrollHandle.CrossFadeAlpha(0, 0.25f, true); // フェードアウト
                visible = false;
            }
        }
    }

}
