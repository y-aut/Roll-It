using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TがPrimitiveのときはSerialize不可（WrappedListを使う）
[Serializable]
public class SerializableList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<string> s;

    public SerializableList() : base() { }
    public SerializableList(IEnumerable<T> list) : base(list) { }
    public SerializableList(int capacity) : base(capacity) { }

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

// ToJson(List<primitive>) -> failure.
// ToJson(WrappedList<Primitive>) -> success.
[Serializable]
public class WrappedList<T> where T : struct
{
    [SerializeField]
    private List<T> v;

    public WrappedList() { v = new List<T>(); }
    public WrappedList(IEnumerable<T> list) { v = new List<T>(list); }
    public WrappedList(int capacity) { v = new List<T>(capacity); }
}

public static partial class AddMethod
{
    public static WrappedList<T> ToWrappedList<T>(this IEnumerable<T> src) where T : struct
        => new WrappedList<T>(src);
}
