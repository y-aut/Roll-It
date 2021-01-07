using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// bool型のリストをlong型のリストで保持
[Serializable]
public class BoolList
{
    [SerializeField]
    private List<long> v;
    [SerializeField]
    private int cnt;

    public BoolList(List<bool> src)
    {
        cnt = src.Count;
        v = Enumerable.Repeat(0L, src.Count / 64 + 1).ToList();
        for (int i = 0; i < src.Count; ++i)
            if (src[i]) v[i / 64] |= 1L << (i % 64);
    }

    public BoolList(BoolList src)
    {
        v = new List<long>(src.v);
        cnt = src.cnt;
    }

    public BoolList(int count)
    {
        v = Enumerable.Repeat(0L, count / 64 + 1).ToList();
        cnt = count;
    }

    public BoolList(int count, Func<int, bool> defVal)
    {
        v = Enumerable.Repeat(0L, count / 64 + 1).ToList();
        cnt = count;
        for (int i = 0; i < cnt; ++i) this[i] = defVal(i);
    }

    private BoolList(int count, Func<int, long> vFunc)
    {
        v = Enumerable.Repeat(0L, count / 64 + 1).ToList();
        cnt = count;
        for (int i = 0; i < v.Count; ++i) v[i] = vFunc(i);
    }

    public int Count
    {
        get => cnt;
        set
        {
            if (cnt == value) return;
            if (value < 0) throw new ArgumentOutOfRangeException();
            var length = value / 64 + 1;
            if (length < v.Count)   // Shrink
            {
                v.RemoveRange(length, v.Count - length);
                // 下位 value % 64 ビットを使用
                v[v.Count - 1] &= (1 << (value % 64)) - 1;
            }
            else if (length > v.Count)  // Expand
            {
                v.AddRange(Enumerable.Repeat(0L, length - v.Count));
            }
            else if (value < cnt)
            {
                v[v.Count - 1] &= (1 << (value % 64)) - 1;
            }
            cnt = value;
        }
    }

    public bool this[int index]
    {
        get
        {
            if (!(0 <= index && index < cnt)) throw new IndexOutOfRangeException();
            return (v[index / 64] & (1L << (index % 64))) != 0;
        }
        set
        {
            if (!(0 <= index && index < cnt)) throw new IndexOutOfRangeException();
            if (value)
                v[index / 64] |= 1L << (index % 64);
            else
                v[index / 64] &= ~(1L << (index % 64));
        }
    }

    public bool IsAllFalse => v.All(i => i == 0L);

    // trueの数を数える
    public int TrueCount() => v.Sum(i => i.PopCount());

    // trueの中で最小のインデックスを取得
    public int GetFirstTrue()
    {
        for (int i = 0; i < Count; ++i)
            if (this[i]) return i;
        return Count;
    }

    public static bool operator ==(BoolList left, BoolList right)
    {
        if (left.Count != right.Count) return false;
        for (int i = 0; i < left.v.Count; ++i)
            if (left.v[i] != right.v[i]) return false;
        return true;
    }

    public static bool operator !=(BoolList left, BoolList right) => !(left == right);

    public static BoolList operator &(BoolList left, BoolList right)
    {
        if (left.Count != right.Count) throw new ArgumentException();
        return new BoolList(left.Count, i => left.v[i] & right.v[i]);
    }

    public static BoolList operator |(BoolList left, BoolList right)
    {
        if (left.Count != right.Count) throw new ArgumentException();
        return new BoolList(left.Count, i => left.v[i] | right.v[i]);
    }

    public static BoolList operator ~(BoolList left) => new BoolList(left.Count, i => ~left.v[i]);

    public override bool Equals(object obj)
    {
        if (obj is BoolList list) return this == list;
        else return false;
    }

    public override int GetHashCode() => AddMethod.GetLower(v[0]);
}
