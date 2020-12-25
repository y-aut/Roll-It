using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Structureを128bitに圧縮したクラス
 * ビット配列は先頭から、
    Type: 7bit (0 ~ 127)
    Texture: 4bit (0 ~ 15)
    PositionInt(X,Y,Z): 11bit (-1024 ~ 1023) * 3
    LocalScaleInt(X,Y,Z): 10bit (0 ~ 1023) * 3      このYまでで64bit
    MoveDirInt(X,Y,Z): 10bit (-512 ~ 511) * 3
    RotationInt: 8bit (0 ~ 255)
    X,Y,ZInversed: 1bit * 3
    Tag: 13bit (0 ~ 8191)
 * 計 128bit
 */
[Serializable]
public class StructureZip
{
    [SerializeField]
    private long p, q;

    public StructureZip(long _p, long _q)
    {
        p = _p; q = _q;
    }

    public long GetRawP() => p;
    public long GetRawQ() => q;

    // Pack
    public StructureZip(Structure src)
    {
        p = (long)src.Type; p <<= 4;
        p += src.Texture; p <<= 11;
        p += PackSignedInt(src.PositionInt.x, 11); p <<= 11;
        p += PackSignedInt(src.PositionInt.y, 11); p <<= 11;
        p += PackSignedInt(src.PositionInt.z, 11); p <<= 10;
        p += src.LocalScaleInt.x; p <<= 10;
        p += src.LocalScaleInt.y;
        q = src.LocalScaleInt.z; q <<= 10;
        q += PackSignedInt(src.MoveDirInt.x, 10); q <<= 10;
        q += PackSignedInt(src.MoveDirInt.y, 10); q <<= 10;
        q += PackSignedInt(src.MoveDirInt.z, 10); q <<= 8;
        q += (long)src.RotationInt; q <<= 1;
        q += Convert.ToInt32(src.XInversed); q <<= 1;
        q += Convert.ToInt32(src.YInversed); q <<= 1;
        q += Convert.ToInt32(src.ZInversed); q <<= 13;
        q += src.Tag;
    }

    // Unpack
    public Structure ToStructure(Stage parent)
    {
        long cp = p; long cq = q;

        var Tag = (int)(cq & LowerMask(13)); cq >>= 13;
        var ZInversed = Convert.ToBoolean(cq & LowerMask(1)); cq >>= 1;
        var YInversed = Convert.ToBoolean(cq & LowerMask(1)); cq >>= 1;
        var XInversed = Convert.ToBoolean(cq & LowerMask(1)); cq >>= 1;
        var RotationInt = (RotationEnum)(cq & LowerMask(8)); cq >>= 8;
            
        var MoveDirInt = new Vector3Int();
        MoveDirInt.z = UnpackSignedInt(cq & LowerMask(10), 10); cq >>= 10;
        MoveDirInt.y = UnpackSignedInt(cq & LowerMask(10), 10); cq >>= 10;
        MoveDirInt.x = UnpackSignedInt(cq & LowerMask(10), 10); cq >>= 10;

        var LocalScaleInt = new Vector3Int();
        LocalScaleInt.z = (int)cq;
        LocalScaleInt.y = (int)(cp & LowerMask(10)); cp >>= 10;
        LocalScaleInt.x = (int)(cp & LowerMask(10)); cp >>= 10;

        var PositionInt = new Vector3Int();
        PositionInt.z = UnpackSignedInt(cp & LowerMask(11), 11); cp >>= 11;
        PositionInt.y = UnpackSignedInt(cp & LowerMask(11), 11); cp >>= 11;
        PositionInt.x = UnpackSignedInt(cp & LowerMask(11), 11); cp >>= 11;

        var Texture = (int)(cp & LowerMask(4)); cp >>= 4;
        var Type = (StructureType)cp;

        return new Structure(Type, Texture, PositionInt, LocalScaleInt, MoveDirInt, RotationInt, XInversed, YInversed, ZInversed, Tag, parent);
    }

    // 符号付き整数をbitビットに圧縮する
    private static long PackSignedInt(int x, int bit)
    {
        // 正数の場合はそのままでOK
        if (x >= 0) return x;
        // 負の場合は、下からbitビット目は1であるはずなので、下位bitビットをマスクして返す
        else return x & LowerMask(bit);
    }

    // PackしたintをUnpackする
    private static int UnpackSignedInt(long y, int bit)
    {
        // 符号ビットが0のときはそのままでOK
        if ((y & (1 << (bit - 1))) == 0) return (int)y;
        // 負の場合は、下位bitビットを除くビットを1埋めして返す
        long mask = ~((1 << bit) - 1);
        return (int)(y | mask);
    }

    // 下位nビットのマスク
    private static int LowerMask(int n) => (1 << n) - 1;
}

[Serializable]
public class StructureZipCollection
{
    [SerializeField]
    private List<StructureZip> v;

    public StructureZipCollection(List<StructureZip> src)
    {
        v = src;
    }

    public StructureZipCollection(List<Structure> src)
    {
        v = src.Select(i => new StructureZip(i)).ToList();
    }

    public StructureZipCollection(List<long> rawList)
    {
        v = new List<StructureZip>();
        for (int i = 0; i < rawList.Count; i += 2)
        {
            v.Add(new StructureZip(rawList[i], rawList[i + 1]));
        }
    }

    public static implicit operator List<StructureZip>(StructureZipCollection col) => col.v;

    public List<Structure> ToStructures(Stage parent) => v.Select(i => i.ToStructure(parent)).ToList();

    public List<long> GetRawList()
    {
        var res = new List<long>();
        foreach (var i in v)
        {
            res.Add(i.GetRawP());
            res.Add(i.GetRawQ());
        }
        return res;
    }
}