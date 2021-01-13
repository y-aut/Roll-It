using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ストラクチャのアイテムを管理するクラス
[Serializable]
public class StructureItem
{
    [SerializeField]
    private StructureType _type;
    public StructureType Type { get => _type; private set => _type = value; }

    [SerializeField]
    private List<GameObject> _prefabs;
    public List<GameObject> Prefabs { get => _prefabs; private set => _prefabs = value; }

    // 特定のタイミングで差し替えるマテリアル
    [SerializeField]
    private List<Material> _materials;
    public List<Material> Materials { get => _materials; private set => _materials = value; }

    [SerializeField]
    private bool _isDefault;
    public bool IsDefault { get => _isDefault; private set => _isDefault = value; }

    // Preview画像
    public RenderTexture Preview { get; set; }

#if UNITY_EDITOR
    [SerializeField]
    private bool _isUnfolded = false;
#endif
}
