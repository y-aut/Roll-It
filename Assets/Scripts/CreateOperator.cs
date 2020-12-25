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
    public Canvas canvas;
    public Button BtnDelete;

    private Vector2? leftDowned;
    private Vector2? rightDowned;
    private Vector2 old1, old2;     // ピンチイン/アウト

    private Structure dragged;      // ドラッグ中のオブジェクト（半透明）
    private Structure focused;      // フォーカスされているオブジェクト
    private TransformTools tools;   // 移動用矢印

    private static bool newStage;   // 新しく作ったステージか
    private bool IsConfirming = false;  // クリアチェックの確認ダイアログを表示しているか
    private bool IsEditted = false; // 一度でも編集したか

    private StructureZipCollection before;  // 編集する前のステージ

    const int CAM_RADIUS = 10;      // カメラの回転時、中心となる点のカメラからの距離
    const int CAM_RADIUS_MAX = 20;  // Focus中のオブジェクトからこれ以上離れていたらCAM_RADIUSを半径とする

    // ロード前に設定すべき変数
    public static void Ready(Stage stage, bool isNewStage)
    {
        Stage = stage;
        newStage = isNewStage;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!newStage)
            before = new StructureZipCollection(Stage.Structs);
        Stage.Create();
    }

    // Update is called once per frame
    void Update()
    {
        // Structureがクリックされたか
        var clicked = Stage.ClickedStructure();
        if (clicked != null)
        {
            // 確認
            if (Stage.LocalData.IsClearChecked && Stage.AuthorID == GameData.User.ID
                && clicked.IsSaved && !IsEditted)
            {
                if (IsConfirming) return;   // Modal表示中

                ConfirmIfClearChecked(() =>
                {
                    IsEditted = true;
                }, () =>
                {
                    clicked.Clicked = false;
                });
                return;
            }
            else if (!IsEditted)
                IsEditted = true;

            clicked.Clicked = false;
            focused = clicked;
            BtnDelete.interactable = focused.IsDeletable;

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
            // このフレームでドラッグが開始されたか
            if (tools.BeganDragged)
            {
                tools.SetPointerRay(cam.ScreenPointToRay(Input.mousePosition));
            }
            else
            {
                tools.Drag(cam.ScreenPointToRay(Input.mousePosition), out var deltaPos, out var deltaPos2, out var deltaScale);
                // 0 < Scale < GameConst.LOCALSCALE_LIMIT
                if ((focused.LocalScaleInt + deltaScale).IsAllBetween(1, Structure.LOCALSCALE_LIMIT))
                {
                    focused.PositionInt += deltaPos;
                    if (focused.HasPosition2) focused.PositionInt2 += deltaPos2;
                    focused.LocalScaleInt += deltaScale;

                    // 変形後のStructureにあわせて更新
                    tools.UpdateObjects();
                }

                tools.IsLegal = Stage.CheckSpace(focused.PositionInt, focused.LocalScaleInt);
            }
        }
        // ドラッグが終了した直後
        else if (tools != null && tools.FinishDragged)
        {
            if (!Stage.CheckSpace(focused.PositionInt, focused.LocalScaleInt))
                tools.ReturnToFormer();
        }
        else
        {  // 矢印やキューブがドラッグされていないときのみ視点移動を行う
            if (Input.touchCount <= 1)
            {
                Vector2 p = Input.mousePosition;

                // 上下左右
                if (Input.GetMouseButtonDown(0) && dragged == null)
                {
                    if (!PointerOnPanel())
                        leftDowned = p;
                    else
                        leftDowned = null;
                }
                else if (Input.GetMouseButton(0) && leftDowned != null && dragged == null)
                {
                    cam.transform.position += cam.transform.rotation * ((Vector3)(p - leftDowned)).XYMinus() / (GetPixelScale() * 30);
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
            else if (Input.touchCount >= 2)
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

                    // 2つのベクトルのなす角θがcosθ<0なら拡大縮小
                    if (d1 == Vector2.zero || d2 == Vector2.zero
                        || (Vector2.Dot(d1, d2) / (d1.magnitude * d2.magnitude)) < 0f)
                    {
                        // 2点の距離の増加量をもとに拡大
                        var scroll = ((t2.position - t1.position).magnitude - (old2 - old1).magnitude) / (GetPixelScale() * 20);
                        cam.transform.position += (cam.transform.rotation * new Vector3(0, 0, scroll));
                    }
                    else
                    {
                        // 2点の移動量の平均をとって回転
                        var avg = (t1.position - old1 + t2.position - old2) / 2 / GetPixelScale();
                        var forw = cam.transform.forward;
                        var center = RotateCenter();
                        forw = Quaternion.AngleAxis(avg.x, cam.transform.rotation * Vector3.up) * Quaternion.AngleAxis(-avg.y, cam.transform.rotation * Vector3.right) * forw;
                        cam.transform.position = center - forw * (cam.transform.position - center).magnitude;
                        cam.transform.forward = forw;
                    }

                    old1 = t1.position; old2 = t2.position;
                    leftDowned = null;
                }
            }

            // カメラが範囲外に出ていたら戻す
            cam.transform.position = AddMethod.Fix(Structure.ToPositionF(new Vector3Int(-GameConst.STAGE_LIMIT, -GameConst.STAGE_LIMIT, -GameConst.STAGE_LIMIT)),
                cam.transform.position, Structure.ToPositionF(new Vector3Int(GameConst.STAGE_LIMIT - 1, GameConst.STAGE_LIMIT - 1, GameConst.STAGE_LIMIT - 1)));
        }

    }

    private void FixedUpdate()
    {
        Stage.IncrementGeneration();
    }

    // アイテムをドラッグ
    public void ItemDragged(GameObject sender)
    {
        // 二本指ならピンチアウト時なのでスルー
        if (Input.touchCount >= 2) return;

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
            switch (sender.name.GetStructureTypeFromImageName())
            {
                case StructureType.Floor:
                    dragged = new Structure(StructureType.Floor, pos, new Vector3Int(4, 1, 4), Stage);
                    break;
                case StructureType.Board:
                    dragged = new Structure(StructureType.Board, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case StructureType.Plate:
                    dragged = new Structure(StructureType.Plate, pos, new Vector3Int(4, 1, 4), Stage);
                    break;
                case StructureType.Slope:
                    dragged = new Structure(StructureType.Slope, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case StructureType.Arc:
                    dragged = new Structure(StructureType.Arc, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case StructureType.Angle:
                    dragged = new Structure(StructureType.Angle, pos, new Vector3Int(1, 1, 1), Stage);
                    break;
                case StructureType.Lift:
                    dragged = new Structure(StructureType.Lift, pos, new Vector3Int(0, 4, 0), new Vector3Int(4, 1, 4), Stage);
                    break;
                case StructureType.Ball:
                    dragged = new Structure(StructureType.Ball, pos, new Vector3Int(1, 1, 1), Stage);
                    break;
                case StructureType.Chopsticks:
                    dragged = new Structure(StructureType.Chopsticks, pos, new Vector3Int(4, 4, 4), Stage);
                    break;
                case StructureType.Jump:
                    dragged = new Structure(StructureType.Jump, pos, new Vector3Int(4, 1, 4), Stage);
                    break;
                case StructureType.Box:
                    dragged = new Structure(StructureType.Box, pos, new Vector3Int(1, 1, 1), Stage);
                    break;
                default:
                    return;
            }
            dragged.Create();
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
            if (Stage.LocalData.IsClearChecked && Stage.AuthorID == GameData.User.ID
                && dragged.IsSaved && !IsEditted)
            {
                if (IsConfirming) return;

                ConfirmIfClearChecked(() =>
                {
                    IsEditted = true;
                    ItemReleased();
                }, () =>
                {
                    // 削除
                    Stage.Delete(dragged);
                    dragged.Destroy();
                    dragged = null;
                });
                return;
            }
            if (Stage.CheckSpace(dragged.PositionInt, dragged.LocalScaleInt))
            {
                // 置ける場所なら置く
                dragged = null;
                if (!IsEditted) IsEditted = true;
            }
            else
            {
                // 置けないなら削除
                Stage.Delete(dragged);
                dragged.Destroy();
                dragged = null;
            }
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
            if (tools != null) tools.Destroy();
        }
    }

    // Back
    public void BtnBackClicked()
    {
        if (IsEditted)
        {
            MessageBox.ShowDialog(canvas.transform, "Are you sure you want to discard the current changes?",
                MessageBoxType.YesNo, () =>
                {
                    if (!newStage)
                        Stage.Structs = before.ToStructures(Stage);
                    Scenes.LoadScene(SceneType.Menu);
                });
        }
        else
            Scenes.LoadScene(SceneType.Menu);
    }

    // Finish
    public void BtnFinishClicked()
    {
        if (newStage)
        {
            // 名前を尋ねる
            InputBox.ShowDialog(canvas.transform, "Name the new stage.", result =>
            {
                Stage.Name = result;
                GameData.Stages.Add(Stage);
                GameData.Save();
                Scenes.LoadScene(SceneType.Menu);
            });
        }
        else
        {
            if (IsEditted)
            {
                Stage.LocalData.IsClearChecked = false;
                Stage.ResetCount();
            }
            GameData.Save();
            Scenes.LoadScene(SceneType.Menu);
        }
    }

    // Test
    public void BtnTestClicked()
    {
        GameData.Save();
        PlayOperator.Ready(Stage, true, true, false);
        Scenes.LoadScene(SceneType.Play);
    }

    // クリアチェックしている場合の確認
    private void ConfirmIfClearChecked(System.Action okClicked, System.Action cancelClicked)
    {
        IsConfirming = true;
        MessageBox.ShowDialog(canvas.transform,
            Stage.CountNonZero() ?
            "If you edit this stage, the statistics will be reset and a clear check needs to be done again. Are you sure you want to edit?" :
            "If you edit this stage, a clear check needs to be done again. Are you sure you want to edit?",
            MessageBoxType.YesNo,
            () => { okClicked(); IsConfirming = false; },
            () => { cancelClicked(); IsConfirming = false; });
    }

    // 画面のスケールを取得
    private float GetPixelScale() => panel.transform.lossyScale.x;
}
