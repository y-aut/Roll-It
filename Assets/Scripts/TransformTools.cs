using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Structure移動用の矢印6つ、サイズ変更用のキューブ8つをまとめて扱う
public class TransformTools
{
    public const float ARROW_LOCALSCALE = 0.3f;
    const int ARROW_COUNT = 6;
    const int XZCUBE_COUNT = 8;
    const int YCUBE_COUNT = 8;

    private CreateOperator createOp;
    public Structure Focused { get; private set; }
    private List<Vector3> arrowRoots, XZpos, Ypos;

    private GameObject frameCube, frameCubeIllegal;
    private List<GameObject> Arrows, Arrows2, XZCubes, YCubes;
    private GameObject RotateArrow, XInverseArrow, YInverseArrow, ZInverseArrow;

    // Arrowの向いている方向
    public Ray ArrowRay(int index)
        => new Ray(arrowRoots[index], Arrows[index].transform.rotation * new Vector3(0, 0, 1));
    public Ray Arrow2Ray(int index)
        => new Ray(arrowRoots[index] + Focused.MoveDir, Arrows2[index].transform.rotation * new Vector3(0, 0, 1));
    // Cubeを引っ張る方向
    public Ray XZCubeRay(int index)
        => new Ray(XZpos[index], index % 2 == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1));
    public Ray YCubeRay(int index) => new Ray(Ypos[index], new Vector3(0, 1, 0));

    public TransformToolType DraggedType { get; private set; }
    public GameObject Dragged { get; private set; }

    private bool _beganDragged = false;
    public bool BeganDragged    // Dragged開始後の最初の一回の取得のみtrue
    {
        get
        {
            var buf = _beganDragged;
            _beganDragged = false;
            return buf;
        }
        private set => _beganDragged = value;
    }

    private bool _finishDragged = false;
    public bool FinishDragged    // Dragged終了後の最初の一回の取得のみtrue
    {
        get
        {
            var buf = _finishDragged;
            _finishDragged = false;
            return buf;
        }
        private set => _finishDragged = value;
    }

    public bool IsLegal     // 現在の位置が設置可能な位置か
    {
        set
        {
            frameCube.SetActive(value);
            frameCubeIllegal.SetActive(!value);
        }
    }

    // ドラッグ開始時のポインタの座標
    private Vector3 dragStartPointer;
    // ドラッグ開始時のオブジェクトの座標
    private Vector3Int dragStartObjPos;
    private Vector3Int dragStartObjPos2;
    // ドラッグ開始時のオブジェクトのサイズ
    private Vector3Int dragStartObjScale;

    public TransformTools(CreateOperator _createOp, Structure focused)
    {
        createOp = _createOp;
        Focused = focused;
    }

    // ドラッグ開始時のポインタのRayと移動対象のオブジェクトの座標、サイズを設定
    public void SetPointerRay(Ray ray)
    {
        if (DraggedType == TransformToolType.Arrow)
        {
            var i = Arrows.IndexOf(Dragged);
            dragStartPointer = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, ArrowRay(i), ray);
        }
        else if (DraggedType == TransformToolType.Arrow2)
        {
            var i = Arrows2.IndexOf(Dragged);
            dragStartPointer = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, Arrow2Ray(i), ray);
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
        dragStartObjPos = Focused.PositionInt;
        dragStartObjPos2 = Focused.PositionInt2;
        dragStartObjScale = Focused.LocalScaleInt;
    }

    // ドラッグ後のポインタのRayにより、移動量、サイズ変更量を返す
    public void Drag(Ray ray, out Vector3Int deltaPos, out Vector3Int deltaPos2, out Vector3Int deltaScale)
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
            deltaPos = dragStartObjPos + Structure.ToPositionInt(dragDelta) - Focused.PositionInt;
            deltaPos2 = Vector3Int.zero;
            deltaScale = Vector3Int.zero;
        }
        else if (DraggedType == TransformToolType.Arrow2)
        {
            var index = Arrows2.IndexOf(Dragged);
            var dragDelta = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, Arrow2Ray(index), ray) - dragStartPointer;
            switch (index % 3)
            {
                case 0: dragDelta = new Vector3(dragDelta.x, 0, 0); break;
                case 1: dragDelta = new Vector3(0, dragDelta.y, 0); break;
                default: dragDelta = new Vector3(0, 0, dragDelta.z); break;
            }
            deltaPos = Vector3Int.zero;
            deltaPos2 = dragStartObjPos2 + Structure.ToPositionInt(dragDelta) - Focused.PositionInt2;
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
            deltaPos = dragStartObjPos + deltaScale - Focused.PositionInt;
            deltaPos2 = Vector3Int.zero;
            // 負の方向へ伸ばす場合は-1倍
            if (index % 4 >= 2) deltaScale *= -1;
            deltaScale = dragStartObjScale + deltaScale - Focused.LocalScaleInt;
        }
        else
        {
            var index = YCubes.IndexOf(Dragged);
            var dragDelta = AddMethod.GetIntersectionOfRayAndFathestPlane(ray.origin, YCubeRay(index), ray) - dragStartPointer;
            dragDelta = new Vector3(0, dragDelta.y, 0);
            // サイズがdragDeltaだけ変わるので、Posはその1/2だけ変わる
            // POSITION_SCALE = LOCALSCALE_SCALE * 2 より、deltaScaleをPositionIntとして扱えば1/2になる
            deltaScale = Structure.ToLocalScaleInt(dragDelta);
            deltaPos = dragStartObjPos + deltaScale - Focused.PositionInt;
            deltaPos2 = Vector3Int.zero;
            // 負の方向へ伸ばす場合は-1倍
            if (index >= 4) deltaScale *= -1;
            deltaScale = dragStartObjScale + deltaScale - Focused.LocalScaleInt;
        }
    }

    // ドラッグ開始時の位置に戻す
    public void ReturnToFormer()
    {
        Focused.PositionInt = dragStartObjPos;
        if (Focused.HasPosition2) Focused.PositionInt2 = dragStartObjPos2;
        Focused.LocalScaleInt = dragStartObjScale;
        IsLegal = true;
        UpdateObjects();
    }

    public void Create()
    {
        frameCube = UnityEngine.Object.Instantiate(Prefabs.FrameCubePrefab);
        frameCube.transform.position = Focused.Position;
        frameCube.transform.localScale = Focused.LocalScale;
        frameCubeIllegal = UnityEngine.Object.Instantiate(Prefabs.FrameCubeIllegalPrefab);
        frameCubeIllegal.transform.position = Focused.Position;
        frameCubeIllegal.transform.localScale = Focused.LocalScale;

        Arrows = new List<GameObject>();
        for (int i = 0; i < ARROW_COUNT; ++i)
            Arrows.Add(UnityEngine.Object.Instantiate(Prefabs.ArrowPrefab));
        if (Focused.HasPosition2)
        {
            Arrows2 = new List<GameObject>();
            for (int i = 0; i < ARROW_COUNT; ++i)
                Arrows2.Add(UnityEngine.Object.Instantiate(Prefabs.ArrowPrefab));
        }
        if (Focused.IsResizable)
        {
            XZCubes = new List<GameObject>();
            for (int i = 0; i < XZCUBE_COUNT; ++i)
                XZCubes.Add(UnityEngine.Object.Instantiate(Prefabs.XZCubePrefab));
            if (Focused.IsYResizable)
            {
                YCubes = new List<GameObject>();
                for (int i = 0; i < YCUBE_COUNT; ++i)
                    YCubes.Add(UnityEngine.Object.Instantiate(Prefabs.YCubePrefab));
            }
        }
        if (Focused.IsRotatable)
            RotateArrow = UnityEngine.Object.Instantiate(Prefabs.RotateArrowPrefab);
        if (Focused.IsXInversable)
            XInverseArrow = UnityEngine.Object.Instantiate(Prefabs.InverseArrowPrefab);
        if (Focused.IsYInversable)
            YInverseArrow = UnityEngine.Object.Instantiate(Prefabs.InverseArrowPrefab);
        if (Focused.IsZInversable)
            ZInverseArrow = UnityEngine.Object.Instantiate(Prefabs.InverseArrowPrefab);

        IsLegal = true;
        UpdateObjects();
        SetDownUpEvent();
    }

    public void UpdateObjects()
    {
        frameCube.transform.position = Focused.Position;
        frameCube.transform.localScale = Focused.LocalScale;
        frameCubeIllegal.transform.position = Focused.Position;
        frameCubeIllegal.transform.localScale = Focused.LocalScale;

        Focused.GetArrowRoots(out var roots);
        arrowRoots = roots;
        if (Focused.IsResizable)
        {
            Focused.GetXZResizeEdges(out var xzpos);
            XZpos = xzpos;
            if (Focused.IsYResizable)
            {
                Focused.GetYResizeVertexes(out var ypos);
                Ypos = ypos;
            }
        }

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
        if (Focused.HasPosition2)
        {
            for (int i = 0; i < ARROW_COUNT; ++i)
            {
                Arrows2[i].transform.localScale = Vector3.one * ARROW_LOCALSCALE;
                Arrows2[i].transform.rotation = eulars[i];
                Arrows2[i].transform.position = arrowRoots[i] + Focused.MoveDir;
                Arrows2[i].transform.Translate(0, 0, -ARROW_LOCALSCALE / 3);
            }
        }

        if (Focused.IsResizable)
        {
            for (int i = 0; i < XZCUBE_COUNT; ++i)
                XZCubes[i].transform.position = XZpos[i];
            for (int i = 0; i < XZCUBE_COUNT; i += 2)
                XZCubes[i].transform.rotation = Quaternion.Euler(0, 90, 0);

            if (Focused.IsYResizable)
            {
                for (int i = 0; i < YCUBE_COUNT; ++i)
                    YCubes[i].transform.position = Ypos[i];
            }
        }

        if (Focused.IsRotatable)
            RotateArrow.transform.position = Focused.GetRotateArrowPos();

        // 反転矢印はデフォルトではX軸方向(Z+向き)
        if (Focused.IsXInversable)
        {
            XInverseArrow.transform.position = Focused.GetXInverseArrowPos();
        }
        if (Focused.IsYInversable)
        {
            YInverseArrow.transform.position = Focused.GetYInverseArrowPos();
            YInverseArrow.transform.rotation = Quaternion.Euler(0, -90, 90);
        }
        if (Focused.IsZInversable)
        {
            ZInverseArrow.transform.position = Focused.GetZInverseArrowPos();
            ZInverseArrow.transform.rotation = Quaternion.Euler(-90, 90, 0);
        }

        if (createOp.auxiFaces != null) createOp.auxiFaces.Update();
    }

    public void Destroy()
    {
        UnityEngine.Object.Destroy(frameCube);
        UnityEngine.Object.Destroy(frameCubeIllegal);
        Arrows.ForEach(i => UnityEngine.Object.Destroy(i));
        if (Focused.HasPosition2) Arrows2.ForEach(i => UnityEngine.Object.Destroy(i));
        if (Focused.IsResizable)
        {
            XZCubes.ForEach(i => UnityEngine.Object.Destroy(i));
            if (Focused.IsYResizable) YCubes.ForEach(i => UnityEngine.Object.Destroy(i));
        }
        if (Focused.IsRotatable) UnityEngine.Object.Destroy(RotateArrow);
        if (Focused.IsXInversable) UnityEngine.Object.Destroy(XInverseArrow);
        if (Focused.IsYInversable) UnityEngine.Object.Destroy(YInverseArrow);
        if (Focused.IsZInversable) UnityEngine.Object.Destroy(ZInverseArrow);
    }

    // Down/Upイベントを追加
    private void SetDownUpEvent()
    {
        for (int i = 0; i < Arrows.Count; ++i)
        {
            var arrow = Arrows[i];
            var trigger = arrow.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            var face = Structure.GetArrowFace(i);
            entry.callback.AddListener(x => PointerDown(arrow, TransformToolType.Arrow, face));
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
            };
            entry.callback.AddListener(x => PointerUp());
            trigger.triggers.Add(entry);
        }

        if (Focused.HasPosition2)
        {
            for (int i = 0; i < Arrows2.Count; ++i)
            {
                var arrow = Arrows2[i];
                var trigger = arrow.AddComponent<EventTrigger>();
                trigger.triggers = new List<EventTrigger.Entry>();
                var entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown,
                };
                var face = Structure.GetArrowFace(i);
                entry.callback.AddListener(x => PointerDown(arrow, TransformToolType.Arrow2, face, true));
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerUp,
                };
                entry.callback.AddListener(x => PointerUp());
                trigger.triggers.Add(entry);
            }
        }

        if (Focused.IsResizable)
        {
            for (int i = 0; i < XZCubes.Count; ++i)
            {
                var cube = XZCubes[i];
                var trigger = cube.AddComponent<EventTrigger>();
                trigger.triggers = new List<EventTrigger.Entry>();
                var entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown,
                };
                var face = Structure.GetXZResizeFace(i);
                entry.callback.AddListener(x => PointerDown(cube, TransformToolType.XZCube, face));
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerUp,
                };
                entry.callback.AddListener(x => PointerUp());
                trigger.triggers.Add(entry);
            }

            if (Focused.IsYResizable)
            {
                for (int i = 0; i < YCubes.Count; ++i)
                {
                    var cube = YCubes[i];
                    var trigger = cube.AddComponent<EventTrigger>();
                    trigger.triggers = new List<EventTrigger.Entry>();
                    var entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerDown,
                    };
                    var face = Structure.GetYResizeFace(i);
                    entry.callback.AddListener(x => PointerDown(cube, TransformToolType.YCube, face));
                    trigger.triggers.Add(entry);
                    entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerUp,
                    };
                    entry.callback.AddListener(x => PointerUp());
                    trigger.triggers.Add(entry);
                }
            }
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

        if (Focused.IsXInversable)
        {
            var trigger = XInverseArrow.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Focused.XInversed = !Focused.XInversed;
            });
            trigger.triggers.Add(entry);
        }

        if (Focused.IsYInversable)
        {
            var trigger = YInverseArrow.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Focused.YInversed = !Focused.YInversed;
            });
            trigger.triggers.Add(entry);
        }

        if (Focused.IsZInversable)
        {
            var trigger = ZInverseArrow.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
            };
            entry.callback.AddListener(x => {
                Focused.ZInversed = !Focused.ZInversed;
            });
            trigger.triggers.Add(entry);
        }

    }

    private void PointerDown(GameObject obj, TransformToolType type, CubeFace face, bool isPos2 = false)
    {
        // 二本指ならスルー
        if (Input.touchCount >= 2) return;

        Dragged = obj;
        BeganDragged = true;
        DraggedType = type;

        // 補助面を表示
        createOp.auxiFaces = new AuxiFaces(CreateOperator.Stage, Focused, face, isPos2);
        createOp.auxiFaces.Update();
    }

    private void PointerUp()
    {
        if (Dragged != null)    // 二本指ならFinishDraggedだけtrueになってしまう
        {
            Dragged = null;
            FinishDragged = true;

            // 補助面を削除
            createOp.auxiFaces.Destroy();
            createOp.auxiFaces = null;
        }
    }

}

public enum TransformToolType
{
    Arrow, Arrow2, XZCube, YCube,
}

public enum CubeFace
{
    XP, YP, ZP, XN, YN, ZN, NB,
}
