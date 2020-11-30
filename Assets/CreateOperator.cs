using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateOperator : MonoBehaviour
{
    public Camera cam;
    public static Stage Stage { get; set; }
    public GameObject panel;
    public Button BtnDelete;

    private Vector2? leftDowned;
    private Vector2? rightDowned;
    private Vector2 old1, old2;     // ピンチイン/アウト

    private Structure dragged;      // ドラッグ中のオブジェクト（半透明）
    private Structure focused;      // フォーカスされているオブジェクト
    private GameObject framecube;   // 外枠
    private TransformTools tools;   // 移動用矢印

    const int CAM_RADIUS = 10;      // カメラの回転時、中心となる点のカメラからの距離
    const int CAM_RADIUS_MAX = 20;  // Focus中のオブジェクトからこれ以上離れていたらCAM_RADIUSを半径とする

    // Start is called before the first frame update
    void Start()
    {
        Stage.Create();
    }

    // Update is called once per frame
    void Update()
    {
        Stage.IncrementGeneration();
        // Structureがクリックされたか
        var clicked = Stage.ClickedStructure();
        if (clicked != null)
        {
            clicked.Clicked = false;
            focused = clicked;
            BtnDelete.interactable = focused.IsDeletable;

            // 外枠を表示
            if (framecube != null) Destroy(framecube);
            framecube = Instantiate(Prefabs.FrameCubePrefab);
            framecube.transform.position = focused.Position;
            framecube.transform.localScale = focused.LocalScale;

            // 矢印、サイズ変更用キューブを表示
            if (tools != null) tools.Destroy();
            tools = new TransformTools
            {
                Focused = focused
            };
            tools.Create();
        }

        // 矢印やキューブがドラッグされたか
        if (tools != null && tools.Dragged != null)
        {
            // このフレームでドラッグされたか
            if (tools.BeganDragged)
            {
                tools.SetPointerRay(cam.ScreenPointToRay(Input.mousePosition), focused.PositionInt, focused.LocalScaleInt);
            }
            else
            {
                tools.Drag(cam.ScreenPointToRay(Input.mousePosition), focused.PositionInt, focused.LocalScaleInt, out var deltaPos, out var deltaScale);
                // Scaleが0以下になる変形はしない
                if ((focused.LocalScaleInt + deltaScale).IsPositive())
                {
                    focused.PositionInt += deltaPos;
                    focused.LocalScaleInt += deltaScale;

                    // 変形後のStructureにあわせて更新
                    framecube.transform.position = focused.Position;
                    framecube.transform.localScale = focused.LocalScale;
                    tools.UpdateObjects();
                }
            }
        }
        else
        {  // 矢印やキューブがドラッグされていないときのみ視点移動を行う
            if (Input.touchCount <= 1)
            {
                Vector2 p = Input.mousePosition;
                // 上下左右
                if (Input.GetMouseButtonDown(0) && !PointerOnPanel() && dragged == null)
                {
                    leftDowned = p;
                }
                else if (Input.GetMouseButton(0) && leftDowned != null && dragged == null)
                {
                    cam.transform.position += cam.transform.rotation * ((Vector3)(p - leftDowned)).XYMinus() / 30;
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
                    var d1 = t1.position - old1; var d2 = t2.position - old2;

                    // 2つのベクトルのなす角θが|cosθ|>0.8なら拡大縮小
                    if (d1 != Vector2.zero && d2 != Vector2.zero
                        && Mathf.Abs(Vector2.Dot(d1, d2) / (d1.magnitude * d2.magnitude)) > 0.8)
                    {
                        // 2点の距離の増加量をもとに拡大
                        var scroll = ((t2.position - t1.position).magnitude - (old2 - old1).magnitude) / panel.transform.lossyScale.x / 50;
                        cam.transform.position += (cam.transform.rotation * new Vector3(0, 0, scroll));
                    }
                    else
                    {
                        // 2点の移動量の平均をとって回転
                        var avg = (t1.position - old1 + t2.position - old2) / 2 / panel.transform.lossyScale.x;
                        var forw = cam.transform.forward;
                        var center = RotateCenter();
                        forw = Quaternion.AngleAxis(avg.x, cam.transform.rotation * Vector3.up) * Quaternion.AngleAxis(-avg.y, cam.transform.rotation * Vector3.right) * forw;
                        cam.transform.position = center - forw * (cam.transform.position - center).magnitude;
                        cam.transform.forward = forw;
                    }

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
            if (dragged != null)
            {
                dragged.Destroy();
                dragged = null;
            }
            return;
        }

        // 場所を決定する
        var pos = Structure.ToPositionInt(cam.ScreenToWorldPoint(Input.mousePosition.NewZ(6f)));

        if (dragged == null)
        {
            switch (sender.name)
            {
                case "ImgFloor":
                    dragged = new Structure(StructureType.Floor, pos, new Vector3Int(4, 1, 4), Stage);
                    break;
                case "ImgBoard":
                    dragged = new Structure(StructureType.Board, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case "ImgPlate":
                    dragged = new Structure(StructureType.Plate, pos, new Vector3Int(4, 1, 4), Stage);
                    break;
                case "ImgSlope":
                    dragged = new Structure(StructureType.Slope, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case "ImgArc":
                    dragged = new Structure(StructureType.Arc, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case "ImgViewpoint":
                    dragged = new Structure(StructureType.Viewpoint, pos, new Vector3Int(1, 1, 1), Stage);
                    break;
                case "ImgLift":
                    dragged = new Structure(StructureType.Lift, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                default:
                    return;
            }
            dragged.Create();
            dragged.Fade();
            Stage.Add(dragged);
        }

        if (dragged.PositionInt != pos)
            dragged.PositionInt = pos;

    }

    // ドラッグ中のアイテムを離す
    public void ItemReleased()
    {
        if (dragged != null)
        {
            // 置ける場所なら置く
            dragged.Opaque();
            dragged = null;
            // 置けないなら削除
            //Stage.Delete(dragged);
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

    // Delete
    public void BtnDeleteClicked()
    {
        if (focused != null && focused.IsDeletable)
        {
            focused.Destroy();
            Stage.Delete(focused);
            focused = null;
            BtnDelete.interactable = false;
            if (framecube != null) Destroy(framecube);
            if (tools != null) tools.Destroy();
        }
    }

    // Finish
    public void BtnFinishClicked()
    {
        GameData.Save();
        SceneManager.LoadScene("Select Scene");
    }
}
