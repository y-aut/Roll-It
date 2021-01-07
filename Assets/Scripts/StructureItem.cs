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

    [SerializeField, Price]
    private Money _price;
    public Money Price { get => _price; private set => _price = value; }

    // Preview画像
    public RenderTexture Preview { get; set; }

#if UNITY_EDITOR
    [SerializeField]
    private bool _isUnfolded = false;
#endif
}

public class PriceAttribute : PropertyAttribute
{
}
