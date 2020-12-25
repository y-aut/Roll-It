using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickListener : MonoBehaviour
{
    public Camera cam;

    public GameObject stick;
    public GameObject stickBack;

    public PlayOperator playOp;
    private Vector2 stickVec;

    public const int MAX_VELOCITY = 10; // 最大速さ
    public const float ACCE_SCALE = 0.4f;

    private void Awake()
    {
        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (playOp.StopUpdate) return;
        //// スティックの値をもとに加速度を設定
        //var vel = playOp.Ball.GetComponent<Rigidbody>().velocity;
        //if (vel.magnitude > MAX_VELOCITY)
        //    playOp.Ball.GetComponent<Rigidbody>().velocity = vel.normalized * MAX_VELOCITY;
        //playOp.Ball.GetComponent<Rigidbody>().AddForce(acceleration * ACCE_SCALE);

        //var buf = playOp.Ball.transform.position;
        //// IDENTITYはX+方向
        //buf += playOp.Angle * new Vector3(0, GameConst.PLAY_CAMDIST_VER, -GameConst.PLAY_CAMDIST_HOR);

        //cam.transform.position = buf;
        //cam.transform.forward = playOp.Ball.transform.position - cam.transform.position;

        if (playOp.StopUpdate) return;
        var vel = playOp.Ball.GetComponent<Rigidbody>().velocity;
        if (vel.magnitude > MAX_VELOCITY)
            playOp.Ball.GetComponent<Rigidbody>().velocity = vel.normalized * MAX_VELOCITY;
        
        var buf = playOp.Ball.transform.position;
        buf += playOp.Angle * new Vector3(0, GameConst.PLAY_CAMDIST_VER, -GameConst.PLAY_CAMDIST_HOR);

        cam.transform.position = buf;
        cam.transform.forward = playOp.Ball.transform.position - cam.transform.position;

        //playOp.Ball.GetComponent<Rigidbody>().AddForce(playOp.Angle * new Vector3(stickVec.x * 0.05f, 0, stickVec.y * ACCE_SCALE));
        //playOp.Angle *= Quaternion.Euler(0, stickVec.x * 0.05f, 0);

        // スティックの値をもとに加速度を設定
        var rot = Quaternion.Euler(30, 0, 0) * Input.gyro.attitude;
        stickVec = rot.eulerAngles / 70f;

        playOp.Ball.GetComponent<Rigidbody>().AddForce(playOp.Angle * new Vector3(stickVec.x * 0.05f, 0, stickVec.y * ACCE_SCALE));
        playOp.Angle *= Quaternion.Euler(0, stickVec.x * 0.05f, 0);

    }

    public void Touched()
    {
        //var p = Input.mousePosition;

        //// pの中心からの距離 <= stickBackの半径 - stickの半径
        //var dist = (p - stickBack.transform.position).magnitude;
        //var max = (stickBack.GetComponent<RectTransform>().sizeDelta.x - stick.GetComponent<RectTransform>().sizeDelta.x)
        //    * GetPixelScale() / 2;   // 解像度が変わるとlossyScaleも変わる
        //if (dist > max)
        //    p = (p - stickBack.transform.position) * max / dist + stickBack.transform.position;

        //stick.transform.position = p;

        //stickVec = (p - stickBack.transform.position) / GetPixelScale();
    }

    public void Released()
    {
        //stick.transform.position = stickBack.transform.position;
        //stickVec = Vector2.zero;
    }

    private float GetPixelScale() => stick.transform.lossyScale.x;

}
