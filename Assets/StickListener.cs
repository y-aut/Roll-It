using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickListener : MonoBehaviour
{
    public Camera cam;

    public GameObject stick;
    public GameObject stickBack;

    public PlayOperator playOp;
    public GameObject ball;
    private Vector3 acceleration;

    public const int MAX_VELOCITY = 10; // 最大速さ
    public const float ACCE_SCALE = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        ball = playOp.Ball.Sphere;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playOp.StopUpdate) return;
        // スティックの値をもとに加速度を設定
        var vel = ball.GetComponent<Rigidbody>().velocity;
        if (vel.magnitude > MAX_VELOCITY)
            ball.GetComponent<Rigidbody>().velocity = vel.normalized * MAX_VELOCITY;
        ball.GetComponent<Rigidbody>().AddForce(acceleration * ACCE_SCALE);

        var buf = ball.transform.position;
        // IDENTITYはX+方向
        buf += playOp.Viewpoint.ToQuaternion() * new Vector3(-GameConst.PLAY_CAMDIST_HOR, GameConst.PLAY_CAMDIST_VER, 0);
        cam.transform.position = buf;
        cam.transform.forward = ball.transform.position - cam.transform.position;
    }

    public void Touched()
    {
        var p = Input.mousePosition;

        // pの中心からの距離 <= stickBackの半径 - stickの半径
        var dist = (p - stickBack.transform.position).magnitude;
        var max = (stickBack.GetComponent<RectTransform>().sizeDelta.x - stick.GetComponent<RectTransform>().sizeDelta.x)
            * stick.transform.lossyScale.x / 2;   // 解像度が変わるとlossyScaleも変わる
        if (dist > max)
            p = (p - stickBack.transform.position) * max / dist + stickBack.transform.position;

        stick.transform.position = p;

        acceleration = playOp.Viewpoint.ToQuaternion() * Quaternion.Euler(90, 90, 0) * (p - stickBack.transform.position) / stick.transform.lossyScale.x;
    }

    public void Released()
    {
        stick.transform.position = stickBack.transform.position;
        acceleration = new Vector3(0, 0, 0);
    }

}
