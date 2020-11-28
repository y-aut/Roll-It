using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AddMethod
{
    public static Vector3 NewX(this Vector3 vec, float x) => new Vector3(x, vec.y, vec.z);
    public static Vector3 NewY(this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);
    public static Vector3 NewZ(this Vector3 vec, float z) => new Vector3(vec.x, vec.y, z);
    public static Vector3 YZSwapped(this Vector3 vec) => new Vector3(vec.x, vec.z, vec.y);
    public static Vector3 XInversed(this Vector3 vec) => new Vector3(-vec.x, vec.y, vec.z);
    public static Vector3 YInversed(this Vector3 vec) => new Vector3(vec.x, -vec.y, vec.z);
    public static Vector3 ZInversed(this Vector3 vec) => new Vector3(vec.x, vec.y, -vec.z);
    public static Vector3 XYInversed(this Vector3 vec) => new Vector3(-vec.x, -vec.y, vec.z);
    public static Vector3 XZCast(this Vector3 vec) => new Vector3(vec.x, 0, vec.z);
    public static Vector3Int Abs(this Vector3Int vec) => new Vector3Int(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    public static bool IsPositive(this Vector3Int vec) => vec.x > 0 && vec.y > 0 && vec.z > 0;

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

}

