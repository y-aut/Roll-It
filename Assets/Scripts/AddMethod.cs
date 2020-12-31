using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class AddMethod
{
    public static T Last<T>(this List<T> list) => list[list.Count - 1];
    public static T LastOrDefault<T>(this List<T> list, T def) => list.Count == 0 ? def : list.Last();

    // XYZ各方向について個別に実装された関数をまとめて扱う
    private static TResult XYZCustom<TResult>(XYZEnum xyz, TResult x, TResult y, TResult z)
        => xyz == XYZEnum.X ? x : (xyz == XYZEnum.Y ? y : z);
    private static TResult XYZCustom<TResult>(XYZEnum xyz, Func<TResult> x, Func<TResult> y, Func<TResult> z)
        => xyz == XYZEnum.X ? x() : (xyz == XYZEnum.Y ? y() : z());
    private static TResult XYZCustom<T1, TResult>(XYZEnum xyz, Func<T1, TResult> x, Func<T1, TResult> y, Func<T1, TResult> z, T1 p1)
        => xyz == XYZEnum.X ? x(p1) : (xyz == XYZEnum.Y ? y(p1) : z(p1));
    private static TResult XYZCustom<T1, T2, TResult>(XYZEnum xyz, Func<T1, T2, TResult> x, Func<T1, T2, TResult> y, Func<T1, T2, TResult> z, T1 p1, T2 p2)
        => xyz == XYZEnum.X ? x(p1, p2) : (xyz == XYZEnum.Y ? y(p1, p2) : z(p1, p2));
    private static TResult XYZCustom<T1, T2, T3, TResult>(XYZEnum xyz, Func<T1, T2, T3, TResult> x, Func<T1, T2, T3, TResult> y, Func<T1, T2, T3, TResult> z, T1 p1, T2 p2, T3 p3)
        => xyz == XYZEnum.X ? x(p1, p2, p3) : (xyz == XYZEnum.Y ? y(p1, p2, p3) : z(p1, p2, p3));

    public static float XYZ(this Vector3 vec, XYZEnum xyz) => XYZCustom(xyz, vec.x, vec.y, vec.z);
    public static int XYZ(this Vector3Int vec, XYZEnum xyz) => XYZCustom(xyz, vec.x, vec.y, vec.z);
    public static Vector3 NewX(this Vector3 vec, float x) => new Vector3(x, vec.y, vec.z);
    public static Vector3 NewY(this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);
    public static Vector3 NewZ(this Vector3 vec, float z) => new Vector3(vec.x, vec.y, z);
    public static Vector3 NewXYZ(this Vector3 vec, XYZEnum xyz, float v) => XYZCustom(xyz, NewX, NewY, NewZ, vec, v);
    public static Vector3Int NewX(this Vector3Int vec, int x) => new Vector3Int(x, vec.y, vec.z);
    public static Vector3Int NewY(this Vector3Int vec, int y) => new Vector3Int(vec.x, y, vec.z);
    public static Vector3Int NewZ(this Vector3Int vec, int z) => new Vector3Int(vec.x, vec.y, z);
    public static Vector3Int NewXYZ(this Vector3Int vec, XYZEnum xyz, int v) => XYZCustom(xyz, NewX, NewY, NewZ, vec, v);
    public static Vector3 XYSwapped(this Vector3 vec) => new Vector3(vec.y, vec.x, vec.z);
    public static Vector3 YZSwapped(this Vector3 vec) => new Vector3(vec.x, vec.z, vec.y);
    public static Vector3 XMinus(this Vector3 vec) => new Vector3(-vec.x, vec.y, vec.z);
    public static Vector3 YMinus(this Vector3 vec) => new Vector3(vec.x, -vec.y, vec.z);
    public static Vector3 ZMinus(this Vector3 vec) => new Vector3(vec.x, vec.y, -vec.z);
    public static Vector3 XYMinus(this Vector3 vec) => new Vector3(-vec.x, -vec.y, vec.z);
    public static Vector3 XCast(this Vector3 vec) => new Vector3(vec.x, 0, 0);
    public static Vector3 YCast(this Vector3 vec) => new Vector3(0, vec.y, 0);
    public static Vector3 ZCast(this Vector3 vec) => new Vector3(0, 0, vec.z);
    public static Vector3 XYZCast(this Vector3 vec, XYZEnum xyz) => XYZCustom(xyz, XCast, YCast, ZCast, vec);
    public static Vector3 XZCast(this Vector3 vec) => new Vector3(vec.x, 0, vec.z);
    public static Vector3 Abs(this Vector3 vec) => vec.Select(i => Mathf.Abs(i));
    public static Vector3Int Abs(this Vector3Int vec) => vec.Select(i => Math.Abs(i));

    public static bool All(this Vector3 vec, Predicate<float> pred) => pred(vec.x) && pred(vec.y) && pred(vec.z);
    public static bool All(this Vector3Int vec, Predicate<int> pred) => pred(vec.x) && pred(vec.y) && pred(vec.z);
    public static bool All(this Vector3 v1, Vector3 v2, Func<float, float, bool> pred)
        => pred(v1.x, v2.x) && pred(v1.y, v2.y) && pred(v1.z, v2.z);
    public static bool All(this Vector3Int v1, Vector3Int v2, Func<int, int, bool> pred)
        => pred(v1.x, v2.x) && pred(v1.y, v2.y) && pred(v1.z, v2.z);
    public static bool All(this Vector3 v1, Vector3 v2, Vector3 v3, Func<float, float, float, bool> pred)
    => pred(v1.x, v2.x, v3.x) && pred(v1.y, v2.y, v3.y) && pred(v1.z, v2.z, v3.z);
    public static bool All(this Vector3Int v1, Vector3Int v2, Vector3Int v3, Func<int, int, int, bool> pred)
    => pred(v1.x, v2.x, v3.x) && pred(v1.y, v2.y, v3.y) && pred(v1.z, v2.z, v3.z);
    public static bool All(this Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Func<float, float, float, float, bool> pred)
    => pred(v1.x, v2.x, v3.x, v4.x) && pred(v1.y, v2.y, v3.y, v4.y) && pred(v1.z, v2.z, v3.z, v4.z);
    public static bool All(this Vector3Int v1, Vector3Int v2, Vector3Int v3, Vector3Int v4, Func<int, int, int, int, bool> pred)
    => pred(v1.x, v2.x, v3.x, v4.x) && pred(v1.y, v2.y, v3.y, v4.y) && pred(v1.z, v2.z, v3.z, v4.z);
    
    public static bool Exist(this Vector3 vec, Predicate<float> pred) => pred(vec.x) || pred(vec.y) || pred(vec.z);
    public static bool Exist(this Vector3Int vec, Predicate<int> pred) => pred(vec.x) || pred(vec.y) || pred(vec.z);
    public static bool Exist(this Vector3 v1, Vector3 v2, Func<float, float, bool> pred)
    => pred(v1.x, v2.x) || pred(v1.y, v2.y) || pred(v1.z, v2.z);
    public static bool Exist(this Vector3Int v1, Vector3Int v2, Func<int, int, bool> pred)
    => pred(v1.x, v2.x) || pred(v1.y, v2.y) || pred(v1.z, v2.z);

    public static bool IsAllPositive(this Vector3Int vec) => vec.All(i => i > 0);
    public static bool NegativeExists(this Vector3 vec) => vec.Exist(i => i < 0);
    public static bool NegativeExists(this Vector3Int vec) => vec.Exist(i => i < 0);
    public static bool IsAllMoreThan(this Vector3Int vec, int min) => vec.All(i => i > min);
    public static bool IsAllMoreThanOrEqual(this Vector3Int vec, int min) => vec.All(i => i >= min);
    public static bool IsAllLessThan(this Vector3Int vec, int max) => vec.All(i => i < max);
    public static bool IsAllBetween(this Vector3Int vec, int min, int max)
        => IsAllMoreThanOrEqual(vec, min) && IsAllLessThan(vec, max);

    public static float CosWith(this Vector2 vec, Vector2 tar) => Vector2.Dot(vec, tar) / (vec.magnitude * tar.magnitude);
    public static float CosWith(this Vector3 vec, Vector3 tar) => Vector3.Dot(vec, tar) / (vec.magnitude * tar.magnitude);

    // 各成分に対して関数を実行
    public static Vector3 Select(this Vector3 vec, Func<float, float> func)
        => new Vector3(func(vec.x), func(vec.y), func(vec.z));
    public static Vector3Int Select(this Vector3Int vec, Func<int, int> func)
        => new Vector3Int(func(vec.x), func(vec.y), func(vec.z));

    public static Vector3 Select(this Vector3 v1, Vector3 v2, Func<float, float, float> func)
        => new Vector3(func(v1.x, v2.x), func(v1.y, v2.y), func(v1.z, v2.z));
    public static Vector3Int Select(this Vector3Int v1, Vector3Int v2, Func<int, int, int> func)
        => new Vector3Int(func(v1.x, v2.x), func(v1.y, v2.y), func(v1.z, v2.z));

    public static Vector3 Select(this Vector3 v1, Vector3 v2, Vector3 v3, Func<float, float, float, float> func)
        => new Vector3(func(v1.x, v2.x, v3.x), func(v1.y, v2.y, v3.y), func(v1.z, v2.z, v3.z));
    public static Vector3Int Select(this Vector3Int v1, Vector3Int v2, Vector3Int v3, Func<int, int, int, int> func)
        => new Vector3Int(func(v1.x, v2.x, v3.x), func(v1.y, v2.y, v3.y), func(v1.z, v2.z, v3.z));


    public static Quaternion ToQuaternion(this RotationEnum rotation)
    {
        switch (rotation)
        {
            case RotationEnum.IDENTITY: return Quaternion.identity;
            case RotationEnum.Y90:      return Quaternion.Euler(0, 90, 0);
            case RotationEnum.Y180:     return Quaternion.Euler(0, 180, 0);
            case RotationEnum.Y270:     return Quaternion.Euler(0, 270, 0);
            default: return Quaternion.identity;
        }
    }

    public static RotationEnum Increment(this RotationEnum rotation)
    {
        switch (rotation)
        {
            case RotationEnum.IDENTITY: return RotationEnum.Y90;
            case RotationEnum.Y90: return RotationEnum.Y180;
            case RotationEnum.Y180: return RotationEnum.Y270;
            case RotationEnum.Y270: return RotationEnum.IDENTITY;
            default: return RotationEnum.IDENTITY;
        }
    }

    // ray1を通る平面で、posからの距離が最大となるものとray2との交点を返す
    public static Vector3 GetIntersectionOfRayAndFathestPlane(Vector3 pos, Ray ray1, Ray ray2)
    {
        var t = -Vector3.Dot(ray1.origin - pos, ray1.direction);
        var a = ray1.origin + t * ray1.direction - pos;   // posから、(posからray1に下ろした垂線の足)までのベクトル
        var test = Vector3.Dot(ray2.direction, a);
        if (test == 0f) return ray2.origin; // ray2が平面上にある
        var s = Vector3.Dot(pos + a - ray2.origin, a) / test;
        return ray2.origin + s * ray2.direction;
    }

    // min <= x <= max となるようにxを調整
    public static float Clamp(this float x, float min, float max)
    {
        if (min > x) return min;
        else if (x > max) return max;
        else return x;
    }

    // nlim <= p <= plim となるようにpを調整
    public static Vector3 Clamp(this Vector3 p, Vector3 nlim, Vector3 plim)
        => p.Select(nlim, plim, (i, j, k) => i.Clamp(j, k));

    // Quarternion.EularAnglesで取得される値を、-abs~absの間にクランプ
    public static Vector3 Clamp(this Vector3 eularAngles, float abs)
        => eularAngles.Select(i => i > 180 ? Mathf.Max(i - 360, -abs) : Mathf.Min(i, abs));

    // 32bit整数2つを64bit整数に
    public static long Pack(int upper, int lower) => (long)upper << 32 | (uint)lower;

    // 64bit整数を32bit整数2つに
    public static int GetUpper(this long packed) => (int)(packed >> 32);
    public static int GetLower(this long packed) => (int)(packed & 0xffffffffL);
}

public enum XYZEnum
{
    X, Y, Z,
}
