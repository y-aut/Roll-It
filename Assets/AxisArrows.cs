using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Structure移動用の矢印6つをまとめて扱う
public class AxisArrows : MonoBehaviour
{
    public const float LOCALSCALE = 0.3f;
    const int ARROW_COUNT = 6;

    public List<Vector3> roots;
    public GameObject prefab;

    public List<GameObject> Arrows;
    public Ray ArrowRay(int index)
        => new Ray(roots[index], Arrows[index].transform.rotation * new Vector3(0, 0, 1));
    public GameObject Dragged { get; private set; }

    private bool _beganDragged = false;
    public bool BeganDragged    // Draggedがnullでなくなった最初の一回の取得のみtrue
    {
        get
        {
            var buf = _beganDragged;
            _beganDragged = false;
            return buf;
        }
        private set => _beganDragged = value;
    }

    // ドラッグ開始時のポインタの座標
    private Vector3 dragStartPointer;
    // ドラッグ開始時のオブジェクトの座標
    private Vector3Int dragStartObj;

    // ドラッグ開始時のポインタのRayと移動対象のオブジェクトの座標を設定
    public void SetPointerRay(Ray ray, Vector3Int objpos)
    {
        var i = Arrows.IndexOf(Dragged);
        dragStartPointer = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, ArrowRay(i), ray);
        dragStartObj = objpos;
    }

    // ドラッグ後のポインタのRayにより矢印を動かし、その移動量を返す
    public Vector3Int Drag(Ray ray, Vector3Int objpos)
    {
        var index = Arrows.IndexOf(Dragged);
        var dragDelta = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, ArrowRay(index), ray) - dragStartPointer;
        switch (index % 3)
        {
            case 0: dragDelta = new Vector3(dragDelta.x, 0, 0); break;
            case 1: dragDelta = new Vector3(0, dragDelta.y, 0); break;
            default: dragDelta = new Vector3(0, 0, dragDelta.z); break;
        }
        var dragDeltaInt = dragStartObj + Structure.ToPositionInt(dragDelta) - objpos;
        if (dragDeltaInt != Vector3Int.zero)
            Arrows.ForEach(i => i.transform.position += Structure.ToPositionF(dragDeltaInt));
        return dragDeltaInt;
    }

    public void Create()
    {
        Arrows = new List<GameObject>();
        for (int i = 0; i < ARROW_COUNT; ++i)
            Arrows.Add(Instantiate(prefab));
        UpdateObjects();
        SetDownUpEvent();
    }

    public void UpdateObjects()
    {
        // 矢印はデフォルトではZ軸正の向き
        var eulars = new List<Quaternion>()
        {
            Quaternion.Euler(0, 90, 0), Quaternion.Euler(-90, 0, 0), Quaternion.identity,
            Quaternion.Euler(0, -90, 0), Quaternion.Euler(90, 0, 0), Quaternion.Euler(180, 0, 0),
        };

        for (int i = 0; i < ARROW_COUNT; ++i)
        {
            Arrows[i].transform.localScale = Vector3.one * LOCALSCALE;
            Arrows[i].transform.rotation = eulars[i];
            Arrows[i].transform.position = roots[i];
            Arrows[i].transform.Translate(0, 0, -LOCALSCALE / 3);
        }
    }

    public void OnDestroy()
    {
        Arrows.ForEach(i => Destroy(i));
    }

    // Down/Upイベントを追加
    private void SetDownUpEvent()
    {
        foreach (var arrow in Arrows)
        {
            var trigger = arrow.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Dragged = arrow;
                BeganDragged = true;
            });
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
            };
            entry.callback.AddListener(x => Dragged = null);
            trigger.triggers.Add(entry);
        }
    }

}
