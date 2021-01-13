using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampObject : MonoBehaviour
{
    public Renderer Off;
    public Renderer On;
    public Light OnLight;

    private bool _lamp = false;
    public bool Lamp
    {
        get => _lamp;
        set
        {
            _lamp = value;
            if (Off != null) Off.enabled = !value;
            if (On != null) On.enabled = value;
            if (OnLight != null) OnLight.enabled = value;
        }
    }

    private void Awake()
    {
        Lamp = false;
    }
}
