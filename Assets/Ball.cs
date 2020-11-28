using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public GameObject Sphere { get; private set; }

    private void Awake()
    {
        Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var rig = Sphere.AddComponent<Rigidbody>();
        rig.angularDrag = 1.0f;
    }

    public void OnDestroy()
    {
        Destroy(Sphere);
    }
}
