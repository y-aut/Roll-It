using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class AddMethod
{
    public static T Last<T>(this List<T> list) => list[list.Count - 1];

    public static Vector3 NewX(this Vector3 vec, float x) => new Vector3(x, vec.y, vec.z);
    public static Vector3 NewY(this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);
    public static Vector3 NewZ(this Vector3 vec, float z) => new Vector3(vec.x, vec.y, z);
    public static Vector3Int NewX(this Vector3Int vec, int x) => new Vector3Int(x, vec.y, vec.z);
    public static Vector3Int NewY(this Vector3Int vec, int y) => new Vector3Int(vec.x, y, vec.z);
    public static Vector3Int NewZ(this Vector3Int vec, int z) => new Vector3Int(vec.x, vec.y, z);
    public static Vector3 XYSwapped(this Vector3 vec) => new Vector3(vec.y, vec.x, vec.z);
    public static Vector3 YZSwapped(this Vector3 vec) => new Vector3(vec.x, vec.z, vec.y);
    public static Vector3 XMinus(this Vector3 vec) => new Vector3(-vec.x, vec.y, vec.z);
    public static Vector3 YMinus(this Vector3 vec) => new Vector3(vec.x, -vec.y, vec.z);
    public static Vector3Int YMinus(this Vector3Int vec) => new Vector3Int(vec.x, -vec.y, vec.z);
    public static Vector3 ZMinus(this Vector3 vec) => new Vector3(vec.x, vec.y, -vec.z);
    public static Vector3 XYMinus(this Vector3 vec) => new Vector3(-vec.x, -vec.y, vec.z);
    public static Vector3 XZCast(this Vector3 vec) => new Vector3(vec.x, 0, vec.z);
    public static Vector3 Abs(this Vector3 vec) => new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    public static Vector3Int Abs(this Vector3Int vec) => new Vector3Int(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    public static bool IsAllPositive(this Vector3Int vec) => vec.x > 0 && vec.y > 0 && vec.z > 0;
    public static bool NegativeExists(this Vector3 vec) => vec.x < 0 || vec.y < 0 || vec.z < 0;
    public static bool NegativeExists(this Vector3Int vec) => vec.x < 0 || vec.y < 0 || vec.z < 0;
    public static bool IsAllMoreThanOrEqual(this Vector3Int vec, int min) => vec.x >= min && vec.y >= min && vec.z >= min;
    public static bool IsAllLessThan(this Vector3Int vec, int max) => vec.x < max && vec.y < max && vec.z < max;
    public static bool IsAllBetween(this Vector3Int vec, int min, int max)
        => IsAllMoreThanOrEqual(vec, min) && IsAllLessThan(vec, max);

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
    public static float Fix(float min, float x, float max)
    {
        if (min > x) return min;
        else if (x > max) return max;
        else return x;
    }

    // nlim <= p <= plim となるようにpを調整
    public static Vector3 Fix(Vector3 nlim, Vector3 p, Vector3 plim)
        => new Vector3(Fix(nlim.x, p.x, plim.x), Fix(nlim.y, p.y, plim.y), Fix(nlim.z, p.z, plim.z));


}

