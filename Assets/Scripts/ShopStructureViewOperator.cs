using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopStructureViewOperator : MonoBehaviour
{
    public GameObject Content;
    public MenuOperator menuOp;

    // Start is called before the first frame update
    void Start()
    {
        // 未所持のアイテムを表示
        for (int i = 0; i < Prefabs.StructureItemList.Count; ++i)
        {
            if (!GameData.MyStructure[i] && Prefabs.StructureItemList[i].Price.IsForSale)
            {
                var item = Instantiate(Prefabs.ShopStructureItemPrefab, Content.transform, false);
                var script = item.GetComponent<ShopStructureItemOperator>();
                script.Initialize(i, menuOp);
            }
        }
    }

}
