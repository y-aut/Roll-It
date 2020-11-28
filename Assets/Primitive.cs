using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 球、直方体などの基礎的な図形
// Stuructureの構成要素; 初期化時に生成されないようにする
[Serializable]
public class Primitive
{
    [NonSerialized]
    private GameObject obj;

    [SerializeField]
    private PrimitiveType _type;
    public PrimitiveType Type { get => _type; set => _type = value; }

    // これをSerializeすると循環参照になるので、StructureをDeserializeしたタイミングで設定
    public Structure Parent { get; set; }

    // Transform
    [SerializeField]
    private Vector3 _position;
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            if (obj != null) UpdateObject();
        }
    }

    [SerializeField]
    private Vector3 _localScale;
    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            _localScale = value;
            if (obj != null) UpdateObject();
        }
    }

    [SerializeField]
    private Quaternion _rotation;
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            if (obj != null) UpdateObject();
        }
    }

    public Primitive(PrimitiveType type)
    {
        Type = type;
    }

    // ワールドに生成
    public void Create()
    {
        obj = GameObject.CreatePrimitive(Type);
        UpdateObject();
        SetClickEvent();
    }

    private void UpdateObject()
    {
        obj.transform.position = Position;
        obj.transform.localScale = LocalScale;
        obj.transform.rotation = Rotation;
    }

    // objにactionを作用させる。Destroy時に必要
    public void Act(Action<GameObject> action)
    {
        action(obj);
    }

    // objのアルファ値を変更
    private void SetAlpha(float a)
    {
        var color = obj.GetComponent<Renderer>().material.color;
        color.a = a;
        obj.GetComponent<Renderer>().material.color = color;
    }

    // objを半透明に
    public void Fade() => SetAlpha(GameConst.FADE_ALPHA);

    // objを不透明に
    public void Opaque() => SetAlpha(1f);

    // Clickイベントを追加
    private void SetClickEvent()
    {
        var trigger = obj.AddComponent<EventTrigger>();
        trigger.triggers = new List<EventTrigger.Entry>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick,
        };
        entry.callback.AddListener(x => Parent.Clicked = true);
        trigger.triggers.Add(entry);
    }
}
