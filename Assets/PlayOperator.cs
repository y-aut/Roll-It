using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayOperator : MonoBehaviour
{
    public static Stage Stage { get; set; }
    public Ball Ball { get; private set; }
    public PhysicMaterial ballMat;

    // Start is called before the first frame update
    void Start()
    {
        Stage.Create();
        Ball = gameObject.AddComponent<Ball>();
        Ball.Sphere.transform.position = Stage.Start.Position + new Vector3(0, 2f, 0);
        Ball.Sphere.GetComponent<SphereCollider>().material = ballMat;
    }

    // Update is called once per frame
    void Update()
    {
        if (Ball.Sphere.transform.position.y < GameConst.GAMEOVER_Y)
        {
            Stage.ForEach(Destroy);
            SceneManager.LoadScene("Select Scene");
        }
    }
}
