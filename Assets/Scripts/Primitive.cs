using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 球、直方体などの基礎的な図形
// Stuructureの構成要素; 初期化時に生成されないようにする
public class Primitive
{
    public GameObject Obj { get; private set; }
    public GameObject Prefab { get; set; } = null;

    // Create後に行うアクション
    public Action CreateAction { get; set; } = null;

    // 以下のプロパティはCreate前に設定する
    // 衝突を検出
    public bool DetectsCollision { get; set; } = false;
    // Create Sceneでのみ表示
    public bool CreateOnly { get; set; } = false;
    // PlayモードではKinematicをfalseに
    public bool NonKinematic { get; set; } = false;

    public Structure Parent { get; set; }

    // Transform
    private Vector3 _position = Vector3.zero;
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            if (Obj != null) UpdateObject();
        }
    }

    private Vector3 _localScale = Vector3.one;
    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            _localScale = value;
            if (Obj != null) UpdateObject();
        }
    }

    private Quaternion _rotation = Quaternion.identity;
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            if (Obj != null) UpdateObject();
        }
    }

    public Primitive(GameObject prefab)
    {
        Prefab = prefab;
    }

    public Primitive(GameObject prefab, Structure parent)
    {
        Prefab = prefab;
        Parent = parent;
    }

    // ワールドに生成
    public void Create()
    {
        Obj = UnityEngine.Object.Instantiate(Prefab);

        CreateAction?.Invoke();
        UpdateObject();
        SetKinematic();
        SetClickEvent();
    }

    // ワールドから削除
    public void Destroy() => UnityEngine.Object.Destroy(Obj);

    private void UpdateObject()
    {
        Obj.transform.position = Position;
        Obj.transform.localScale = LocalScale;
        Obj.transform.rotation = Rotation;
    }

    private void SetKinematic()
    {
        if (NonKinematic)
            Obj.GetComponent<Rigidbody>().isKinematic = Scenes.GetActiveScene() != SceneType.Play;
    }

    // Clickイベントを追加
    private void SetClickEvent()
    {
        if (!Obj.TryGetComponent(out EventTrigger trigger))
            trigger = Obj.AddComponent<EventTrigger>();
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
        if (!DetectsCollision) return;
        CollisionEvent script;
        // "Collider"という名前の子オブジェクトをもつ場合はそちらにつける
        var child = Obj.transform.Find("Collider");
        if (child != null) script = child.gameObject.AddComponent<CollisionEvent>();
        else script = Obj.AddComponent<CollisionEvent>();
        script.Primitive = this;
    }

    // Kinematicな物体の位置を変更する
    public void MovePosition(Vector3 position)
    {
        _position = position;
        var rb = Obj.GetComponent<Rigidbody>();
        rb.MovePosition(position);
    }

    // Kinematicでない物体に力を加える
    public void AddForce(Vector3 force)
    {
        var rb = Obj.GetComponent<Rigidbody>();
        rb.AddForce(force);
    }
}
