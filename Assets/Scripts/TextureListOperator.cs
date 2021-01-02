using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureListOperator : MonoBehaviour
{
    public CreateOperator createOp;
    public StructureType Type { get; private set; }

    public void UpdateList(StructureItemOperator itemOp)
    {
        Type = itemOp.Type;
        foreach (var item in GetComponentsInChildren<StructureItemOperator>())
            Destroy(item.gameObject);

        for (int i = 0, cnt = Prefabs.GetTextureCount(Type); i < cnt; ++i)
        {
            var item = Instantiate(Prefabs.StructureItemPrefab, gameObject.transform, false);
            var script = item.GetComponent<StructureItemOperator>();
            script.Initialize(createOp, Type, i, false);
        }
    }

    
}
