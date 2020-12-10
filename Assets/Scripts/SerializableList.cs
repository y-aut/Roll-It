using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializableList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<string> jsons;

    public SerializableList() : base() { }
    public SerializableList(IEnumerable<T> list) : base(list) { }

    public void OnBeforeSerialize()
    {
        jsons = new List<string>();
        ForEach(i => jsons.Add(JsonUtility.ToJson(i)));
    }

    public void OnAfterDeserialize()
    {
        jsons.ForEach(i => Add(JsonUtility.FromJson<T>(i)));
    }

    public SerializableList<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return new SerializableList<TResult>(((IEnumerable<T>)this).Select(selector));
    }

}
