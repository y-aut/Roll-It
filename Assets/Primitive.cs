using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 球、直方体などの基礎的な図形
// Stuructureの構成要素; 初期化時に生成されないようにする
public class Primitive
{
    private GameObject obj;

    // TypeかPrefabの一方を設定する
    public PrimitiveType Type { get; set; }
    public GameObject Prefab { get; set; } = null;

    // Create Sceneでのみ表示
    public bool CreateOnly { get; private set; } = false;

    public bool IsPrimitive => Prefab == null;

    public Structure Parent { get; set; }

    // Transform
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

    public Primitive(GameObject prefab)
    {
        Prefab = prefab;
    }

    public Primitive(GameObject prefab, bool createOnly)
    {
        Prefab = prefab;
        CreateOnly = createOnly;
    }

    // ワールドに生成
    public void Create()
    {
        if (IsPrimitive)
        {
            obj = GameObject.CreatePrimitive(Type);

            // 専用ColliderがないCylinderはMeshに
            if (Type == PrimitiveType.Cylinder)
            {
                obj.GetComponent<Collider>().enabled = false;
                obj.AddComponent<MeshCollider>();
            }
        }
        else
        {
            obj = UnityEngine.Object.Instantiate(Prefab);
        }

        UpdateObject();
        SetClickEvent();
    }

    // ワールドから削除
    public void Destroy() => UnityEngine.Object.Destroy(obj);

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
        if (!obj.TryGetComponent(out EventTrigger trigger))
            trigger = obj.AddComponent<EventTrigger>();
        trigger.triggers = new List<EventTrigger.Entry>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick,
        };
        entry.callback.AddListener(x => Parent.Clicked = true);
        trigger.triggers.Add(entry);
    }

    // Collisionイベントを追加
    public void SetCollisionEvent()
    {
        var script = obj.AddComponent<CollisionEvent>();
        script.Primitive = this;
    }

    // Kinematicな物体の位置を変更する
    public void MovePosition(Vector3 position)
    {
        _position = position;
        var rb = obj.GetComponent<Rigidbody>();
        rb.MovePosition(position);
    }

    // Kinematicでない物体に力を加える
    public void AddForce(Vector3 force)
    {
        var rb = obj.GetComponent<Rigidbody>();
        rb.AddForce(force);
    }
}
