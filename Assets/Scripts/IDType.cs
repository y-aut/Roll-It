using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IDの型
// 0-9, A-Z の36文字 * 6文字
[Serializable]
public struct IDType
{
    public static IDType Empty => new IDType();
    public static IDType NotFound => new IDType("ZZZZZZ");

    [SerializeField]
    uint p;      // 31bit にすると 36^6 でおさえられる

    public IDType(uint _p) { p = _p; }

    public IDType(string str)
    {
        // 数字+大文字アルファベット36文字 * 6
        p = 0;
        foreach (var c in str)
        {
            p *= 36;
            if ('0' <= c && c <= '9') p += (uint)(c - '0');
            else p += (uint)(c - 'A' + 10);
        }
    }

    public static bool TryParse(string str, out IDType id)
    {
        id = new IDType();
        if (str.Length != 6) return false;

        foreach (var c in str)
        {
            id.p *= 36;
            if ('0' <= c && c <= '9') id.p += (uint)(c - '0');
            else if ('A' <= c && c <= 'Z') id.p += (uint)(c - 'A' + 10);
            else if ('a' <= c && c <= 'z') id.p += (uint)(c - 'a' + 10);
            else return false;
        }

        return true;
    }

    public static IDType Generate()
    {
        // IDを生成する

        // タイムスタンプ: 86400秒 * 12倍精度 < 2^20
        // 乱数: 2^11

        var r = new System.Random();
        return new IDType(((uint)((DateTime.Now - DateTime.Today).TotalSeconds * 12) << 11)
            + (uint)r.Next(0, 1 << 11));
    }

    public static bool operator ==(IDType left, IDType right) => left.p == right.p;
    public static bool operator !=(IDType left, IDType right) => !(left == right);

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("");
        uint id = p;
        for (int i = 0; i < 6; ++i)
        {
            var sur = id % 36;
            if (sur < 10) sb.Insert(0, (char)('0' + sur));
            else sb.Insert(0, (char)('A' + sur - 10));
            id /= 36;
        }
        return sb.ToString();
    }

    public uint GetRawVal() => p;

    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => (int)p;
}

[Serializable]
public class IDTypeCollection
{
    [SerializeField]
    private List<long> v;

    public IDTypeCollection(List<IDType> src)
    {
        v = new List<long>();
        for (int i = 0; i < src.Count / 2; ++i)
            v.Add(AddMethod.Pack((int)src[i * 2].GetRawVal(), (int)src[i * 2 + 1].GetRawVal()));
        if (src.Count % 2 == 1)
            v.Add(AddMethod.Pack((int)src.Last().GetRawVal(), (int)IDType.Empty.GetRawVal()));
    }

    public IDTypeCollection(List<long> rawList)
    {
        v = new List<long>(rawList);
    }

    public List<IDType> ToList()
    {
        var list = new List<IDType>();
        foreach (var i in v)
        {
            list.Add(new IDType((uint)i.GetUpper()));
            list.Add(new IDType((uint)i.GetLower()));
        }
        if (list.Count != 0 && list.Last() == IDType.Empty) list.RemoveAt(list.Count - 1);
        return list;
    }

    public List<long> GetRawList() => v;
}

[Serializable]
public class IDPair
{
    [SerializeField]
    private long v;

    public IDPair(IDType id1, IDType id2)
    {
        v = AddMethod.Pack((int)id1.GetRawVal(), (int)id2.GetRawVal());
    }

    public IDPair(long rawVal)
    {
        v = rawVal;
    }

    public long GetRawVal() => v;

    public IDType ID1 => new IDType((uint)v.GetUpper());
    public IDType ID2 => new IDType((uint)v.GetLower());
}