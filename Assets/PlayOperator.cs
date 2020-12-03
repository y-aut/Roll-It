using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayOperator : MonoBehaviour
{
    public Camera cam;

    public static Stage Stage { get; set; }
    public Ball Ball { get; private set; }
    public PhysicMaterial ballMat;

    // Updateの処理をストップ
    public bool StopUpdate { get; set; } = false;

    // 視点
    private RotationEnum _angle = RotationEnum.Y270;   // デフォルトはZ+方向
    public RotationEnum Angle
    {
        get => _angle;
        set
        {
            if (Angle == value) return;
            // 速度を0に
            var rb = Ball.Sphere.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // X+を0°とした回転前のカメラの回転量
            int first = (int)Angle * 90;
            // 回転後のカメラの回転量
            int last = (int)value * 90;
            // 回転量(°/f)
            int delta = (last - first + 360) % 360 == 270 ? -GameConst.ROTATE_VIEWPOINT_DEGREE : GameConst.ROTATE_VIEWPOINT_DEGREE;

            StartCoroutine(RotateAngleCoroutine(first, last, delta));

            _angle = value;
        }
    }

    // 視点回転を行うコルーチン
    private IEnumerator RotateAngleCoroutine(int first, int last, int delta)
    {
        StopUpdate = true;
        Time.timeScale = 0f;
        for (int i = first; (i - last) % 360 != 0; i += delta)
        {
            var buf = Ball.Sphere.transform.position;
            buf += Quaternion.Euler(0, i, 0) * new Vector3(-GameConst.PLAY_CAMDIST_HOR, GameConst.PLAY_CAMDIST_VER, 0);
            cam.transform.position = buf;
            cam.transform.forward = Ball.Sphere.transform.position - cam.transform.position;
            Canvas.ForceUpdateCanvases();
            yield return new WaitForSecondsRealtime(GameConst.ROTATE_VIEWPOINT_MS / 1000f);
        }
        Time.timeScale = 1f;
        StopUpdate = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        Stage.Create();
        Ball = gameObject.AddComponent<Ball>();
        Ball.Sphere.transform.position = Stage.Start.Position + new Vector3(0, 2f, 0);
        Ball.Sphere.GetComponent<SphereCollider>().material = ballMat;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (StopUpdate) return;
        Stage.IncrementGeneration();
        // GameOver判定
        if (Ball.Sphere.transform.position.y < GameConst.GAMEOVER_Y)
        {
            Stage.Destroy();
            SceneManager.LoadScene("Select Scene");
            return;
        }
        // Collision判定
        foreach (var str in Stage.CollidedStructures())
        {
            str.Collided = false;
            if (str.Type == StructureType.Angle)
            {
                // 視点回転
                Angle = str.RotationInt;
            }
        }
    }


}
