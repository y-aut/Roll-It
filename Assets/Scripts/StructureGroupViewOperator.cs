using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureGroupViewOperator : MonoBehaviour
{
    public StructureType Type { get; private set; }
    private MenuOperator menuOp;
    private StructureViewOperator parent;
    public Transform Content;
    public ChildScrollRect ScrollRect;

    public void Initialize(StructureType type, MenuOperator _menuOp, StructureViewOperator _parent)
    {
        Type = type;
        menuOp = _menuOp;
        parent = _parent;
        ScrollRect.parentScrollRect = parent.ScrollRect;

        foreach (var i in Type.GetStructureNos())
        {
            var item = Instantiate(Prefabs.StructureItemPrefab, Content, false);
            var script = item.GetComponent<StructureItemOperator>();
            script.Initialize(i, menuOp, this);
            script.InUse = (i == GameData.User.ActiveBallNo);
        }
    }

    public void UpdateActiveBall()
    {
        foreach (var script in GetComponentsInChildren<StructureItemOperator>())
            script.InUse = script.StructureNo == GameData.User.ActiveBallNo;
    }
}
