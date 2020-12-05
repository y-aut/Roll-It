using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // ボールの名前
    public const string BALL_NAME = "ball";

    public GameObject Sphere { get; private set; }

    private void Awake()
    {
        Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Sphere.name = BALL_NAME;
        var rig = Sphere.AddComponent<Rigidbody>();
        rig.angularDrag = 1.0f;
    }

    public void OnDestroy()
    {
        Destroy(Sphere);
    }
}
