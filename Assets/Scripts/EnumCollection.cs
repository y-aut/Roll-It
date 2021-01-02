using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enumを引数とするコレクション
public class EnumCollection<TEnum, T> where TEnum : Enum
{
    private List<T> v;
    private int NB;

    public EnumCollection(Func<TEnum, T> defVal)
    {
        GetNB();
        v = new List<T>(NB);
        for (int i = 0; i < NB; ++i)
            v.Add(defVal((TEnum)(object)i));
    }

    public EnumCollection(EnumCollection<TEnum, T> src)
    {
        NB = src.NB;
        v = new List<T>(src.v);
    }

    private void GetNB()
    {
        NB = (int)Enum.Parse(typeof(TEnum), "NB");
    }

    public T this[TEnum i]
    {
        get => v[(int)(object)i];
        set => v[(int)(object)i] = value;
    }

}
