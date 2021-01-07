using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StructureItem))]
public class StructureItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("_type");
        var type = (StructureType)typeProp.enumValueIndex;

        var prefabsProp = property.FindPropertyRelative("_prefabs");
        var obj = prefabsProp.arraySize == 0 ? null : prefabsProp.GetArrayElementAtIndex(0).objectReferenceValue;

        // label.text ... "Element ##"
        label.text = $"{label.text.Substring("Element ".Length)}: {type}, {(obj != null ? obj.name : "None")}";
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PrefixLabel(position, label);

        var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // IsUnfolded
        var unfoldedProp = property.FindPropertyRelative("_isUnfolded");
        unfoldedProp.boolValue = EditorGUI.Foldout(rect, unfoldedProp.boolValue, label);
        // 折りたたまれている場合はGUIの更新をせずに終了
        if (!unfoldedProp.boolValue) return;
        ++EditorGUI.indentLevel;

        // Type
        rect.y += EditorGUIUtility.singleLineHeight + EditorConst.Y_MARGIN;
        EditorGUI.PropertyField(rect, typeProp);
        type = (StructureType)typeProp.enumValueIndex;

        // Prefabs
        List<string> primitiveNames;
        if (type.HasMultiPrimitives()) primitiveNames = type.GetPrimitiveNames();
        else primitiveNames = new List<string>() { "Prefab" };
        prefabsProp.arraySize = primitiveNames.Count;
        for (int i = 0; i < primitiveNames.Count; ++i)
        {
            rect.y += EditorGUIUtility.singleLineHeight + EditorConst.Y_MARGIN;
            prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue
                = EditorGUI.ObjectField(rect, primitiveNames[i],
                prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), false);
        }

        // Price
        var priceProp = property.FindPropertyRelative("_price");
        rect.y += EditorGUIUtility.singleLineHeight + EditorConst.Y_MARGIN;
        EditorGUI.PropertyField(rect, priceProp);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var unfoldedProp = property.FindPropertyRelative("_isUnfolded");
        if (!unfoldedProp.boolValue) return EditorGUIUtility.singleLineHeight;

        var typeProp = property.FindPropertyRelative("_type");
        var type = (StructureType)typeProp.enumValueIndex;
        var priceProp = property.FindPropertyRelative("_price");
        return (EditorGUIUtility.singleLineHeight + EditorConst.Y_MARGIN) * (2 + type.GetPrimitivesCount())
            + EditorGUI.GetPropertyHeight(priceProp);
    }
}

public static partial class AddMethod
{
    // Primitiveを2つ以上使用するStructureについて、各Primitiveの名前を記述
    private static readonly Dictionary<StructureType, List<string>> PrimitiveNames = new Dictionary<StructureType, List<string>>()
    {
        { StructureType.Goal, new List<string>() { "Goal", "Flag" } },
        { StructureType.Lift, new List<string>() { "Lift", "Goal" } },
    };

    public static bool HasMultiPrimitives(this StructureType type) => PrimitiveNames.ContainsKey(type);
    public static int GetPrimitivesCount(this StructureType type) => type.HasMultiPrimitives() ? type.GetPrimitiveNames().Count : 1;
    public static List<string> GetPrimitiveNames(this StructureType type) => PrimitiveNames[type];
}