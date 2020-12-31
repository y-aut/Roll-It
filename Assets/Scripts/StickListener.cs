using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickListener : MonoBehaviour
{

    public Camera cam;

    public GameObject stick;
    public GameObject stickBack;

    public PlayOperator playOp;

    public GameObject ImgPoint; // 重心位置
    public RectTransform PointRect;
    public RectTransform SensorRect;

    // 原点位置の逆数
    public static Quaternion AttitudeOriginInv = Quaternion.Euler(DEFAULT_ATTITUDE_X, 0, 0);

    // スティックの傾き
    private Vector2 stickVec = Vector2.zero;

    public const int MAX_VELOCITY = 10; // 最大速さ
    public const float ACCE_SCALE = 0.4f;
    public const float MAX_ATTITUDE = 45;  // X方向の傾きの最大角（°）
    public const float MAX_ATTITUDE_DISPLAYED = 35;  // 表示上の傾きの最大角（°）
    public const float DEFAULT_ATTITUDE_X = 20;     // 水平位置
    public const float ATTITUDE_SCALE = 1f / 4;
    public const float ROTATE_SCALE = 1f / 15;

    private void Awake()
    {
        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var flgUpdate = Time.timeScale > 0f; // 中断中でも重心位置だけは更新

        if (flgUpdate)
        {
            var vel = playOp.Ball.GetComponent<Rigidbody>().velocity;
            if (vel.magnitude > MAX_VELOCITY)
                playOp.Ball.GetComponent<Rigidbody>().velocity = vel.normalized * MAX_VELOCITY;

            var buf = playOp.Ball.transform.position;
            buf += playOp.CamAngle * new Vector3(0, GameConst.PLAY_CAMDIST_VER, -GameConst.PLAY_CAMDIST_HOR);

            cam.transform.position = buf;
            cam.transform.forward = playOp.Ball.transform.position - cam.transform.position;

            // スティックの値をもとに加速度を設定
            playOp.Ball.GetComponent<Rigidbody>().AddForce(playOp.CamAngle * new Vector3(stickVec.x * ACCE_SCALE, 0, stickVec.y * ACCE_SCALE));
        }

        // デバイスの傾きをもとに加速度を設定

        // attitudeはY軸が上なので、yとzを反転
        var rot = Input.gyro.attitude;
        rot = new Quaternion(-rot.x, -rot.z, -rot.y, rot.w);

        rot = AttitudeOriginInv * rot;
        var attitude_noScaled = rot.eulerAngles.Clamp(MAX_ATTITUDE);

        if (flgUpdate)
        {
            var attitude = attitude_noScaled * ATTITUDE_SCALE;

            playOp.Ball.GetComponent<Rigidbody>().AddForce(playOp.CamAngle * new Vector3(0, 0, attitude.x * ACCE_SCALE));

            // カメラを回転
            var att_z = Mathf.Abs(attitude.z) / (MAX_ATTITUDE * ATTITUDE_SCALE);    // 0~1
            att_z = Prefabs.AttitudeToRotationCurve.Evaluate(att_z);
            att_z *= (attitude.z < 0 ? -1 : 1) * (MAX_ATTITUDE * ATTITUDE_SCALE);
            playOp.CamAngle *= Quaternion.Euler(0, -att_z * ROTATE_SCALE, 0);
        }

        // 重心位置を更新
        ImgPoint.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            (-attitude_noScaled.z / MAX_ATTITUDE_DISPLAYED).Clamp(-1f, 1f) * (SensorRect.rect.width - PointRect.rect.width) / 2,
            (attitude_noScaled.x / MAX_ATTITUDE_DISPLAYED).Clamp(-1f, 1f) * (SensorRect.rect.height - PointRect.rect.height) / 2, 0);
    }

    private void Update()
    {
        // 中断中でも重心位置は更新する
        if (Time.timeScale == 0f) FixedUpdate();
    }

    public void Touched()
    {
        var p = Input.mousePosition;

        // pの中心からの距離 <= stickBackの半径 - stickの半径
        var dist = (p - stickBack.transform.position).magnitude;
        var max = (stickBack.GetComponent<RectTransform>().sizeDelta.x - stick.GetComponent<RectTransform>().sizeDelta.x)
            * GetPixelScale() / 2;   // 解像度が変わるとlossyScaleも変わる
        if (dist > max)
            p = (p - stickBack.transform.position) * max / dist + stickBack.transform.position;

        stick.transform.position = p;
        stickVec = (p - stickBack.transform.position) / GetPixelScale();
    }

    public void Released()
    {
        stick.transform.position = stickBack.transform.position;
        stickVec = Vector2.zero;
    }

    private float GetPixelScale() => stick.transform.lossyScale.x;

    // 現在のAttitudeを基準に設定
    public static void SetOrigin()
    {
        var rot = Input.gyro.attitude;
        rot = new Quaternion(-rot.x, -rot.z, -rot.y, rot.w);
        AttitudeOriginInv = Quaternion.Inverse(rot);
    }

}
