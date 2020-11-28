using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Structure移動用の矢印6つ、サイズ変更用のキューブ8つをまとめて扱う
public class TransformTools : MonoBehaviour
{
    public const float ARROW_LOCALSCALE = 0.3f;
    const int ARROW_COUNT = 6;
    const int XZCUBE_COUNT = 8;
    const int YCUBE_COUNT = 8;

    public Structure Focused { get; set; }
    private List<Vector3> arrowRoots, XZpos, Ypos;
    public GameObject ArrowPrefab, XZCubePrefab, YCubePrefab, RotateArrowPrefab;

    private List<GameObject> Arrows, XZCubes, YCubes;
    private GameObject RotateArrow;

    // Arrowの向いている方向
    public Ray ArrowRay(int index)
        => new Ray(arrowRoots[index], Arrows[index].transform.rotation * new Vector3(0, 0, 1));
    // Cubeを引っ張る方向
    public Ray XZCubeRay(int index)
        => new Ray(XZpos[index], index % 2 == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1));
    public Ray YCubeRay(int index) => new Ray(Ypos[index], new Vector3(0, 1, 0));

    public TransformToolType DraggedType { get; private set; }
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
    private Vector3Int dragStartObjPos;
    // ドラッグ開始時のオブジェクトのサイズ
    private Vector3Int dragStartObjScale;

    // ドラッグ開始時のポインタのRayと移動対象のオブジェクトの座標、サイズを設定
    public void SetPointerRay(Ray ray, Vector3Int objPos, Vector3Int objScale)
    {
        if (DraggedType == TransformToolType.Arrow)
        {
            var i = Arrows.IndexOf(Dragged);
            dragStartPointer = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, ArrowRay(i), ray);
        }
        else if (DraggedType == TransformToolType.XZCube)
        {
            var i = XZCubes.IndexOf(Dragged);
            dragStartPointer = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, XZCubeRay(i), ray);
        }
        else
        {
            var i = YCubes.IndexOf(Dragged);
            dragStartPointer = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, YCubeRay(i), ray);
        }
        dragStartObjPos = objPos;
        dragStartObjScale = objScale;
    }

    // ドラッグ後のポインタのRayにより、移動量、サイズ変更量を返す
    public void Drag(Ray ray, Vector3Int objPos, Vector3Int objScale, out Vector3Int deltaPos, out Vector3Int deltaScale)
    {
        if (DraggedType == TransformToolType.Arrow)
        {
            var index = Arrows.IndexOf(Dragged);
            var dragDelta = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, ArrowRay(index), ray) - dragStartPointer;
            switch (index % 3)
            {
                case 0: dragDelta = new Vector3(dragDelta.x, 0, 0); break;
                case 1: dragDelta = new Vector3(0, dragDelta.y, 0); break;
                default: dragDelta = new Vector3(0, 0, dragDelta.z); break;
            }
            deltaPos = dragStartObjPos + Structure.ToPositionInt(dragDelta) - objPos;
            deltaScale = Vector3Int.zero;
        }
        else if (DraggedType == TransformToolType.XZCube)
        {
            var index = XZCubes.IndexOf(Dragged);
            var dragDelta = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, XZCubeRay(index), ray) - dragStartPointer;
            switch (index % 2)
            {
                case 0: dragDelta = new Vector3(dragDelta.x, 0, 0); break;
                default: dragDelta = new Vector3(0, 0, dragDelta.z); break;
            }
            // サイズがdragDeltaだけ変わるので、Posはその1/2だけ変わる
            // POSITION_SCALE = LOCALSCALE_SCALE * 2 より、deltaScaleをPositionIntとして扱えば1/2になる
            deltaScale = Structure.ToLocalScaleInt(dragDelta);
            deltaPos = dragStartObjPos + deltaScale - objPos;
            // 負の方向へ伸ばす場合は-1倍
            if (index % 4 >= 2) deltaScale *= -1;
            deltaScale = dragStartObjScale + deltaScale - objScale;
        }
        else
        {
            var index = YCubes.IndexOf(Dragged);
            var dragDelta = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, YCubeRay(index), ray) - dragStartPointer;
            dragDelta = new Vector3(0, dragDelta.y, 0);
            // サイズがdragDeltaだけ変わるので、Posはその1/2だけ変わる
            // POSITION_SCALE = LOCALSCALE_SCALE * 2 より、deltaScaleをPositionIntとして扱えば1/2になる
            deltaScale = Structure.ToLocalScaleInt(dragDelta);
            deltaPos = dragStartObjPos + deltaScale - objPos;
            // 負の方向へ伸ばす場合は-1倍
            if (index >= 4) deltaScale *= -1;
            deltaScale = dragStartObjScale + deltaScale - objScale;
        }
    }

    public void Create()
    {
        Arrows = new List<GameObject>();
        for (int i = 0; i < ARROW_COUNT; ++i)
            Arrows.Add(Instantiate(ArrowPrefab));
        XZCubes = new List<GameObject>();
        for (int i = 0; i < XZCUBE_COUNT; ++i)
            XZCubes.Add(Instantiate(XZCubePrefab));
        YCubes = new List<GameObject>();
        for (int i = 0; i < YCUBE_COUNT; ++i)
            YCubes.Add(Instantiate(YCubePrefab));

        if (Focused.IsRotatable)
            RotateArrow = Instantiate(RotateArrowPrefab);

        UpdateObjects();
        SetDownUpEvent();
    }

    public void UpdateObjects()
    {
        Focused.GetArrowRoots(out var roots);
        Focused.GetXZResizeEdges(out var xzpos);
        Focused.GetYResizeVertexes(out var ypos);
        arrowRoots = roots;
        XZpos = xzpos;
        Ypos = ypos;

        // 矢印はデフォルトではZ軸正の向き
        var eulars = new List<Quaternion>()
        {
            Quaternion.Euler(0, 90, 0), Quaternion.Euler(-90, 0, 0), Quaternion.identity,
            Quaternion.Euler(0, -90, 0), Quaternion.Euler(90, 0, 0), Quaternion.Euler(180, 0, 0),
        };

        for (int i = 0; i < ARROW_COUNT; ++i)
        {
            Arrows[i].transform.localScale = Vector3.one * ARROW_LOCALSCALE;
            Arrows[i].transform.rotation = eulars[i];
            Arrows[i].transform.position = arrowRoots[i];
            Arrows[i].transform.Translate(0, 0, -ARROW_LOCALSCALE / 3);
        }

        for (int i = 0; i < XZCUBE_COUNT; ++i)
            XZCubes[i].transform.position = XZpos[i];
        for (int i = 0; i < XZCUBE_COUNT; i += 2)
            XZCubes[i].transform.rotation = Quaternion.Euler(0, 90, 0);
        
        for (int i = 0; i < YCUBE_COUNT; ++i)
            YCubes[i].transform.position = Ypos[i];

        if (Focused.IsRotatable)
            RotateArrow.transform.position = Focused.GetRotateArrowPos();
    }

    public void OnDestroy()
    {
        Arrows.ForEach(i => Destroy(i));
        XZCubes.ForEach(i => Destroy(i));
        YCubes.ForEach(i => Destroy(i));
        if (Focused.IsRotatable) Destroy(RotateArrow);
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
                DraggedType = TransformToolType.Arrow;
            });
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
            };
            entry.callback.AddListener(x => Dragged = null);
            trigger.triggers.Add(entry);
        }

        foreach (var cube in XZCubes)
        {
            var trigger = cube.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Dragged = cube;
                BeganDragged = true;
                DraggedType = TransformToolType.XZCube;
            });
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
            };
            entry.callback.AddListener(x => Dragged = null);
            trigger.triggers.Add(entry);
        }

        foreach (var cube in YCubes)
        {
            var trigger = cube.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Dragged = cube;
                BeganDragged = true;
                DraggedType = TransformToolType.YCube;
            });
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
            };
            entry.callback.AddListener(x => Dragged = null);
            trigger.triggers.Add(entry);
        }

        if (Focused.IsRotatable) {
            var trigger = RotateArrow.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Focused.RotationInt = Focused.RotationInt.Increment();
            });
            trigger.triggers.Add(entry);
        }
    }

}

public enum TransformToolType
{
    Arrow, XZCube, YCube,
}