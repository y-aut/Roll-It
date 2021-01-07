using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enumを引数とするコレクション
[Serializable]
public class EnumCollection<TEnum, T> : ISerializationCallbackReceiver where TEnum : Enum
{
    [SerializeField]
    private SerializableList<T> v;
    private int NB;

    public EnumCollection(Func<TEnum, T> defVal)
    {
        GetNB();
        v = new SerializableList<T>(NB);
        for (int i = 0; i < NB; ++i)
            v.Add(defVal((TEnum)(object)i));
    }

    public EnumCollection(EnumCollection<TEnum, T> src)
    {
        NB = src.NB;
        v = new SerializableList<T>(src.v);
    }

    private void GetNB()
    {
        NB = (int)Enum.Parse(typeof(TEnum), "NB");
    }

    public void ForEach(Action<TEnum, T> action)
    {
        for (int i = 0; i < NB; ++i)
            action((TEnum)(object)i, v[i]);
    }

    public void OnBeforeSerialize()
    {
        v.OnBeforeSerialize();
    }

    public void OnAfterDeserialize()
    {
        v.OnAfterDeserialize();
        GetNB();
    }

    //// Enumの数が変化した時用。Deserialize後に呼び出す
    //public void Adjust(Func<TEnum, T> defVal)
    //{
    //    if (v.Count < NB)
    //    {
    //        for (int i = v.Count; i < NB; ++i)
    //            v.Add(defVal((TEnum)(object)i));
    //    }
    //    else if (v.Count > NB)
    //    {
    //        v.RemoveRange(NB, v.Count - NB);
    //    }
    //}

    public T this[TEnum i]
    {
        get => v[(int)(object)i];
        set => v[(int)(object)i] = value;
    }
}
