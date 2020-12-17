using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializableList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<string> s;

    public SerializableList() : base() { }
    public SerializableList(IEnumerable<T> list) : base(list) { }

    public void OnBeforeSerialize()
    {
        s = new List<string>();
        ForEach(i => s.Add(JsonUtility.ToJson(i)));
    }

    public void OnAfterDeserialize()
    {
        s.ForEach(i => Add(JsonUtility.FromJson<T>(i)));
    }

    public SerializableList<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return new SerializableList<TResult>(((IEnumerable<T>)this).Select(selector));
    }

}

public static partial class AddMethod
{
    public static SerializableList<T> ToSerializableList<T>(this IEnumerable<T> src)
        => new SerializableList<T>(src);
}