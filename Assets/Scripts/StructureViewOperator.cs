using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureViewOperator : MonoBehaviour
{
    public MenuOperator menuOp;
    public Transform Content;
    public ScrollRect ScrollRect;

    private void Start()
    {
        foreach (var i in Structure.StructureOrder)
        {
            if (i.ShowInGallery() && !(GameData.MyStructure & Prefabs.TypeBoolList[i]).IsAllFalse)
            {
                var item = Instantiate(Prefabs.StructureGroupViewPrefab, Content, false);
                var script = item.GetComponent<StructureGroupViewOperator>();
                script.Initialize(i, menuOp, this);
            }
        }
    }
}
