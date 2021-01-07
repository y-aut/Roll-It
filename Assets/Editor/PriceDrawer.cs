using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PriceAttribute))]
public class PriceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        var jewelProp = property.FindPropertyRelative("_jewel");
        var coinProp = property.FindPropertyRelative("_coin");
        var price = new Money(jewelProp.intValue, coinProp.intValue);

        int selected = price == Money.Default ? 0 : (price == Money.NotForSale ? 2 : 1);
        
        var style = EditorGUI.IntPopup(rect, "Sale Style", selected, new string[] { "Default", "For sale", "Not for sale" }, new int[] { 0, 1, 2 });

        if (style == 0)
            price = Money.Default;
        else if (style == 2)
            price = Money.NotForSale;
        else
        {
            if (selected != 1) price = Money.Free;
            rect.y += EditorGUIUtility.singleLineHeight + EditorConst.Y_MARGIN;
            price.Jewel = EditorGUI.IntField(rect, "Jewel", price.Jewel);
            rect.y += EditorGUIUtility.singleLineHeight + EditorConst.Y_MARGIN;
            price.Coin = EditorGUI.IntField(rect, "Coin", price.Coin);
        }

        jewelProp.intValue = price.Jewel;
        coinProp.intValue = price.Coin;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var jewelProp = property.FindPropertyRelative("_jewel");
        var coinProp = property.FindPropertyRelative("_coin");
        var price = new Money(jewelProp.intValue, coinProp.intValue);

        if (price.IsForSale)
            return EditorGUIUtility.singleLineHeight * 3 + EditorConst.Y_MARGIN * 2;
        else
            return EditorGUIUtility.singleLineHeight;
    }
}

public static class EditorConst
{
    public const float Y_MARGIN = 2f;
}