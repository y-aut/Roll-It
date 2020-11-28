using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreateOperator : MonoBehaviour
{
    public Camera cam;
    public static Stage Stage { get; set; }
    public GameObject panel;
    public GameObject arrowPrefab;  // 移動用の矢印のプレハブ

    private Vector2? leftDowned;
    private Vector2? rightDowned;
    private Vector2 old1, old2; // ピンチイン/アウト

    private Structure dragging;     // ドラッグ中のオブジェクト（半透明）
    private Structure focused;      // フォーカスされているオブジェクト
    private AxisArrows arrows;     // 移動用矢印

    const int CAM_RADIUS = 10;  // カメラの回転時、中心となる点のカメラからの距離
    const int CAM_RADIUS_MAX = 20;  // Focus中のオブジェクトからこれ以上離れていたらCAM_RADIUSを半径とする

    // Start is called before the first frame update
    void Start()
    {
        Stage.Create();
    }

    // Update is called once per frame
    void Update()
    {
        // Structureがクリックされたか
        var clicked = Stage.ClickedStructure();
        if (clicked != null)
        {
            clicked.Clicked = false;
            focused = clicked;
            focused.GetArrowRoot(out var roots);
            if (arrows != null) Destroy(arrows);
            arrows = gameObject.AddComponent<AxisArrows>();
            arrows.roots = roots;
            arrows.prefab = arrowPrefab;
            arrows.Create();
        }

        // 矢印がドラッグされたか
        if (arrows != null && arrows.Dragged != null)
        {
            // このフレームでドラッグされたか
            if (arrows.BeganDragged)
            {
                arrows.SetPointerRay(cam.ScreenPointToRay(Input.mousePosition), focused.PositionInt);
            }
            else
            {
                var delta = arrows.Drag(cam.ScreenPointToRay(Input.mousePosition), focused.PositionInt);
                if (delta != Vector3Int.zero)
                    focused.PositionInt += delta;
            }
        }
        else
        {  // 矢印がドラッグされていないときのみ視点移動を行う
            if (Input.touchCount <= 1)
            {
                Vector2 p = Input.mousePosition;
                // 上下左右
                if (Input.GetMouseButtonDown(0) && !PointerOnPanel() && dragging == null)
                {
                    leftDowned = p;
                }
                else if (Input.GetMouseButton(0) && leftDowned != null && dragging == null)
                {
                    cam.transform.position += cam.transform.rotation * ((Vector3)(p - leftDowned)).XYInversed() / 30;
                    leftDowned = p;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    leftDowned = null;
                }
                // 拡大
                else if (Input.mouseScrollDelta.y != 0 && !PointerOnPanel())
                {
                    var scroll = Input.mouseScrollDelta.y;
                    cam.transform.position += (cam.transform.rotation * new Vector3(0, 0, scroll));
                }
                // 回転
                else if (Input.GetMouseButtonDown(1) && !PointerOnPanel())
                {
                    rightDowned = p;
                }
                else if (Input.GetMouseButton(1) && rightDowned != null)
                {
                    var forw = cam.transform.forward;
                    var center = RotateCenter();
                    forw = Quaternion.AngleAxis(p.x - rightDowned.Value.x, cam.transform.rotation * Vector3.up) * Quaternion.AngleAxis(rightDowned.Value.y - p.y, cam.transform.rotation * Vector3.right) * forw;
                    cam.transform.position = center - forw * (cam.transform.position - center).magnitude;
                    cam.transform.forward = forw;
                    rightDowned = p;
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    rightDowned = null;
                }
            }
            else if (Input.touchCount == 2)
            {
                var t1 = Input.GetTouch(0);
                var t2 = Input.GetTouch(1);

                if (t2.phase == TouchPhase.Began)
                {
                    old1 = t1.position; old2 = t2.position;
                }
                else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
                {
                    // 2点の移動量の平均をとって回転
                    var avg = (t1.position - old1 + t2.position - old2) / 2;
                    var forw = cam.transform.forward;
                    var center = RotateCenter();
                    forw = Quaternion.AngleAxis(avg.x, cam.transform.rotation * Vector3.up) * Quaternion.AngleAxis(-avg.y, cam.transform.rotation * Vector3.right) * forw;
                    cam.transform.position = center - forw * (cam.transform.position - center).magnitude;
                    cam.transform.forward = forw;

                    // 2点の距離の増加量をもとに拡大
                    var scroll = (t2.position - t1.position).magnitude - (old2 - old1).magnitude;
                    cam.transform.position += (cam.transform.rotation * new Vector3(0, 0, scroll));

                    old1 = t1.position; old2 = t2.position;
                }
            }
        }

    }

    // アイテムをドラッグ
    public void ItemDragged(GameObject sender)
    {
        // フィールド上にあるか
        if (PointerOnPanel())
        {
            if (dragging != null)
            {
                dragging.ForEach(Destroy);
                dragging = null;
            }
            return;
        }

        // 場所を決定する
        var pos = Structure.ToPositionInt(cam.ScreenToWorldPoint(Input.mousePosition.NewZ(10f)));

        if (dragging == null)
        {
            if (sender.name == "ImgFloor")
            {
                dragging = new Structure(StructureType.Floor, pos, new Vector3Int(4, 1, 4));
                dragging.Create();
                dragging.Fade();
            }
            else
                return;
        }

        if (dragging.PositionInt != pos)
            dragging.PositionInt = pos;

    }

    // ドラッグ中のアイテムを離す
    public void ItemReleased()
    {
        if (dragging != null)
        {
            // 置ける場所なら置く
            dragging.Opaque();
            Stage.Add(dragging);
            dragging = null;
        }
    }

    // ポインタがpanelの上にあるか
    private bool PointerOnPanel()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panel.GetComponent<RectTransform>(),
            Input.mousePosition, null, out var localPoint);
        return panel.GetComponent<RectTransform>().rect.Contains(localPoint);
    }

    // 視点回転時の中心座標を取得
    private Vector3 RotateCenter()
    {
        // focus中の物体が近ければ、それまでの距離を半径とする
        float r;
        if (focused != null && (r = (cam.transform.position - focused.Position).magnitude) < CAM_RADIUS_MAX)
            return cam.transform.position + cam.transform.forward * r;
        else
            return cam.transform.position + cam.transform.forward * CAM_RADIUS;
    }

    // Finish
    public void BtnFinishClicked()
    {
        GameData.Save();
        SceneManager.LoadScene("Select Scene");
    }
}
