using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureListOperator : MonoBehaviour
{
    const float DURATION_TIME = 0.2f;  // Open/Closeにかかる秒数

    public CreateOperator createOp;
    public StructureType Type { get; private set; }
    public GameObject PnlTrans;
    public RectTransform Rect;
    public GameObject Triangle;

    StateEnum State = StateEnum.Other;
    float generation_time = 0f;     // Open/Close処理を開始してから経過した秒数

    // Show()時のアニメーション
    public AnimationCurve ScaleCurve;

    public void Show(CreateStructureItemOperator itemOp)
    {
        Type = itemOp.StructureItem.Type;

        foreach (var i in Type.GetStructureNos())
        {
            if (GameData.MyStructure[i])
            {
                var item = Instantiate(Prefabs.CreateStructureItemPrefab, gameObject.transform, false);
                var script = item.GetComponent<CreateStructureItemOperator>();
                script.Initialize(createOp, i, false);
            }
        }

        PnlTrans.SetActive(true);
        Triangle.transform.position = Triangle.transform.position.NewX(itemOp.transform.position.x);
        var pivot = new Vector3((itemOp.transform.position.x - Rect.offsetMin.x) / (Rect.rect.width * Rect.lossyScale.x), 0f);

        Rect.SetPivotWithKeepingPosition(pivot);
        Rect.position = Rect.position.NewY(itemOp.transform.position.y
            + itemOp.gameObject.GetComponent<RectTransform>().rect.height / 2 * itemOp.transform.localScale.y * itemOp.transform.lossyScale.y
            + Rect.rect.height * Rect.lossyScale.y / 2 * 0.8f);
        Rect.localScale = Vector3.one * ScaleCurve.Evaluate(0f);
        gameObject.SetActive(true);
        generation_time = 0f;
        State = StateEnum.Opening;
    }

    public void Close()
    {
        State = StateEnum.Closing;
        gameObject.SetActive(false);
        PnlTrans.SetActive(false);
        foreach (var item in GetComponentsInChildren<CreateStructureItemOperator>())
            Destroy(item.gameObject);
    }

    private void Update()
    {
        if (State == StateEnum.Opening)
        {
            generation_time += Time.unscaledDeltaTime;
            Rect.localScale = Prefabs.OpenCurve.Evaluate(Mathf.Min(1f, generation_time / DURATION_TIME)) * Vector3.one;
            if (generation_time >= DURATION_TIME) State = StateEnum.Other;
        }
        else if (State == StateEnum.Closing)
        {
            State = StateEnum.Other;    // 閉じるアニメーションはなし
        }
    }



}
