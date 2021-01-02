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
    private bool IsPausing; // ポーズ中か

    // 自分のステージか
    public static bool IsMyStage { get; set; } = true;

    // カメラのアングル(XZ平面内での回転のみ)
    public Quaternion CamAngle = Quaternion.identity;

    public void SetCamAngle(Quaternion after)
    {
        // 現在の角度との差が60°以内の場合は回転しない
        var bef = (CamAngle * Vector3.forward).XZCast();
        var aft = (after * Vector3.forward).XZCast();
        if (bef.CosWith(aft) > 0.5f) return;

        // 速度を0に
        var rb = Ball.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var befY = CamAngle.eulerAngles.y;
        var aftY = after.eulerAngles.y;
        // 回転量(°/f)
        int deltaY = (aftY - befY + 360) % 360 <= 180 ? GameConst.ROTATE_VIEWPOINT_DEGREE : -GameConst.ROTATE_VIEWPOINT_DEGREE;

        StartCoroutine(RotateAngleCoroutine(befY, aftY, deltaY));

        CamAngle = after;
    }

    // 視点回転を行うコルーチン
    private IEnumerator RotateAngleCoroutine(float befY, float aftY, float deltaY)
    {
        Time.timeScale = 0f;
        var dif = Mathf.Abs(aftY - befY);
        var cnt = Mathf.FloorToInt(Mathf.Min(dif, 360 - dif) / Mathf.Abs(deltaY));
        for (int i = 0; i <= cnt; ++i)
        {
            if (i == cnt) befY = aftY;
            else befY += deltaY;
            var buf = Ball.transform.position;
            buf += Quaternion.Euler(0, befY, 0) * new Vector3(0, GameConst.PLAY_CAMDIST_VER, -GameConst.PLAY_CAMDIST_HOR);
            cam.transform.position = buf;
            cam.transform.forward = Ball.transform.position - cam.transform.position;
            Canvas.ForceUpdateCanvases();
            do
            {
                yield return new WaitForSecondsRealtime(GameConst.ROTATE_VIEWPOINT_MS / 1000f);
            } while (IsPausing);    // 回転中にポーズされるとこのメソッドを抜けてからtimeScaleが0になってしまう
        }
        Time.timeScale = 1f;
    }

    // Start is called before the first frame update
    async void Start()
    {
        NowLoading.Show(canvas.transform, "Loading the stage...");

        effect = new EffectManager();
        Cleared = false;

        Stage.Create();
        Ball = Instantiate(Prefabs.BallPrefab[GameData.User.ActiveBallTexture]);
        Ball.name = Structure.BALL_NAME;
        Ball.GetComponent<SphereCollider>().material = ballMat;
        var rig = Ball.AddComponent<Rigidbody>();
        rig.angularDrag = 1.0f;

        Structure ball;
        if (TestPlay && (ball = Stage.Ball) != null)    // 指定した位置からプレイ
            Ball.transform.position = ball.Position;
        else
            Ball.transform.position = Stage.Start.Position + new Vector3(0, Ball.transform.localScale.y / 2, 0);

        if (!IsMyStage)
        {
            try
            {
                await FirebaseIO.IncrementChallengeCount(Stage).WaitWithTimeOut();
            }
            catch (System.Exception e)
            {
                e.Show(canvas.transform);
            }
        }

        NowLoading.Close();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;
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
                SetCamAngle(str.Rotation);
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
        IsPausing = true;
        PausePanel.ShowDialog(this, canvas.transform, () => IsPausing = false);
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
