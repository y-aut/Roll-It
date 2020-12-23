using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayOperator : MonoBehaviour
{
    public Camera cam;
    public Canvas canvas;

    public static Stage Stage { get; private set; }
    public static bool TestPlay { get; private set; }        // Create中の試験プレイ
    public static bool ClearCheck { get; private set; }     // クリアチェック

    public GameObject StickBack;
    public GameObject Stick;
    public GameObject BtnPause;

    public GameObject Ball;
    public PhysicMaterial ballMat;
    public GameObject ImgClear;
    public Fade FadeComponent;

    private EffectManager effect;
    private bool Cleared;   // クリアしたか（演出中）

    // Updateの処理をストップ
    public bool StopUpdate { get; set; } = false;

    // 自分のステージか
    public static bool IsMyStage { get; set; } = true;

    // 視点
    private RotationEnum _angle = RotationEnum.Y270;   // デフォルトはZ+方向
    public RotationEnum Angle
    {
        get => _angle;
        set
        {
            if (Angle == value) return;
            // 速度を0に
            var rb = Ball.GetComponent<Rigidbody>();
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
            var buf = Ball.transform.position;
            buf += Quaternion.Euler(0, i, 0) * new Vector3(-GameConst.PLAY_CAMDIST_HOR, GameConst.PLAY_CAMDIST_VER, 0);
            cam.transform.position = buf;
            cam.transform.forward = Ball.transform.position - cam.transform.position;
            Canvas.ForceUpdateCanvases();
            yield return new WaitForSecondsRealtime(GameConst.ROTATE_VIEWPOINT_MS / 1000f);
        }
        Time.timeScale = 1f;
        StopUpdate = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        NowLoading.Show(canvas.transform, "Loading the stage...");

        if (!IsMyStage)
        {
            _ = FirebaseIO.IncrementChallengeCount(Stage);
        }

        effect = new EffectManager();
        Cleared = false;

        Stage.Create();
        Ball = Instantiate(Prefabs.BallPrefab);
        Ball.name = Structure.BALL_NAME;
        Ball.GetComponent<SphereCollider>().material = ballMat;
        var rig = Ball.AddComponent<Rigidbody>();
        rig.angularDrag = 1.0f;

        Structure ball;
        if (TestPlay && (ball = Stage.Ball) != null)    // 指定した位置からプレイ
            Ball.transform.position = ball.Position;
        else
            Ball.transform.position = Stage.Start.Position + new Vector3(0, Ball.transform.localScale.y / 2, 0);

        NowLoading.Close();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (StopUpdate) return;
        if (Cleared) return;

        Stage.IncrementGeneration();
        // GameOver判定
        if (Ball.transform.position.y < Stage.GameOverY)
        {
            Restart();
            return;
        }
        // Clear判定
        if (Stage.Goal.Collided)
        {
            Stage.Goal.Collided = false;
            if (ClearCheck || (IsMyStage && !TestPlay && !Stage.LocalData.IsClearChecked)
                || (TestPlay && Stage.Ball == null))
            {   // 普通にプレイしてクリアしたときでもクリアチェックOKとする
                Stage.LocalData.IsClearChecked = true;
            }
            Cleared = true;

            Stick.SetActive(false);
            StickBack.SetActive(false);
            BtnPause.SetActive(false);
            DoClearEvent();
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
            else if (str.Type == StructureType.Jump)
            {
                // ジャンプ
                Ball.GetComponent<Rigidbody>().AddForce(Vector3.up * 7, ForceMode.Impulse);
            }
        }
    }

    private void DoClearEvent()
    {
        StartCoroutine(effect.StageClear(ImgClear, () =>
        {
            if (!IsMyStage)
            {
                FadeComponent.FadeIn(1f, () =>
                {
                    Stage.Destroy();
                    MenuOperator.ReadyForResult(Stage);
                    Scenes.LoadScene(SceneType.Menu);
                });
            }
            else
            {
                Stage.Destroy();
                if (TestPlay)
                    Scenes.LoadScene(SceneType.Create);
                else
                    Scenes.LoadScene(SceneType.Menu);
            }
        }));
    }

    // ロード前に設定すべき変数
    public static void Ready(Stage stage, bool isTestPlay, bool isMyStage, bool isClearCheck)
    {
        Stage = stage;
        TestPlay = isTestPlay;
        IsMyStage = isMyStage;
        ClearCheck = isClearCheck;
    }

    public void Pause()
    {
        PausePanel.ShowDialog(this, canvas.transform);
    }

    public void Restart()
    {
        Stage.Destroy();
        Scenes.LoadScene(SceneType.Play);
    }

    public void Quit()
    {
        Stage.Destroy();
        if (TestPlay)
            Scenes.LoadScene(SceneType.Create);
        else
            Scenes.LoadScene(SceneType.Menu);
        return;
    }

}
