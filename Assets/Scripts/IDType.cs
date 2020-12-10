using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IDの型
[Serializable]
public struct IDType
{
    public static IDType Empty => new IDType();

    [SerializeField]
    long p1, p2;

    public IDType(string base64)
    {
        // url-safeな20文字のbase64文字列をバイト列に変換
        base64 = base64.Replace('-', '+').Replace('_', '/');
        // 末尾に1byte加えて16byteにする
        byte[] bytes = new byte[16];
        Array.Copy(Convert.FromBase64String(base64), bytes, 15);

        p1 = BitConverter.ToInt64(bytes, 0);
        p2 = BitConverter.ToInt64(bytes, 8);
    }

    public static bool operator ==(IDType left, IDType right) => left.p1 == right.p1 && left.p2 == right.p2;
    public static bool operator !=(IDType left, IDType right) => !(left == right);

    public override string ToString()
    {
        var bytes = new List<byte>(BitConverter.GetBytes(p1));
        bytes.AddRange(BitConverter.GetBytes(p2));
        bytes.RemoveAt(bytes.Count - 1);
        return Convert.ToBase64String(bytes.ToArray()).Replace('+', '-').Replace('/', '_');
    }

    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => (int)p1;
}
