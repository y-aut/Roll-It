using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Structureを128bitに圧縮したクラス
 * ビット配列は先頭から、
    Version: 7bit (0 ~ 127) --- 0
    No: 14bit (0 ~ 16383)
    PositionInt(X,Y,Z): 11bit (-1024 ~ 1023) * 3
    LocalScaleInt(X,Y,Z): 10bit (0 ~ 1023) * 3      このXまでで64bit
    MoveDirInt(X,Y,Z): 7bit (-64 ~ 63) * 3
    RotationInt: 8bit (0 ~ 255)
    X,Y,ZInversed: 1bit * 3
    Tag: 12bit (0 ~ 4095)
 * 計 128bit
 */
[Serializable]
public class StructureZip
{
    public const int VERSION = 0;

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
        p = VERSION; p <<= 14;
        p |= (uint)src.No; p <<= 11;
        p |= PackSignedInt(src.PositionInt.x, 11); p <<= 11;
        p |= PackSignedInt(src.PositionInt.y, 11); p <<= 11;
        p |= PackSignedInt(src.PositionInt.z, 11); p <<= 10;
        p |= (uint)src.LocalScaleInt.x;
        q = src.LocalScaleInt.y; q <<= 10;
        q |= (uint)src.LocalScaleInt.z; q <<= 7;
        q |= PackSignedInt(src.MoveDirInt.x, 7); q <<= 7;
        q |= PackSignedInt(src.MoveDirInt.y, 7); q <<= 7;
        q |= PackSignedInt(src.MoveDirInt.z, 7); q <<= 8;
        q |= (long)src.RotationInt; q <<= 1;
        q |= Convert.ToUInt32(src.XInversed); q <<= 1;
        q |= Convert.ToUInt32(src.YInversed); q <<= 1;
        q |= Convert.ToUInt32(src.ZInversed); q <<= 12;
        q |= (uint)src.Tag;
    }

    // Unpack
    public Structure ToStructure(Stage parent)
    {
        long cp = p; long cq = q;

        var Tag = (int)(cq & LowerMask(12)); cq >>= 12;
        var ZInversed = Convert.ToBoolean(cq & LowerMask(1)); cq >>= 1;
        var YInversed = Convert.ToBoolean(cq & LowerMask(1)); cq >>= 1;
        var XInversed = Convert.ToBoolean(cq & LowerMask(1)); cq >>= 1;
        var RotationInt = (RotationEnum)(cq & LowerMask(8)); cq >>= 8;
            
        var MoveDirInt = new Vector3Int();
        MoveDirInt.z = UnpackSignedInt(cq & LowerMask(7), 7); cq >>= 7;
        MoveDirInt.y = UnpackSignedInt(cq & LowerMask(7), 7); cq >>= 7;
        MoveDirInt.x = UnpackSignedInt(cq & LowerMask(7), 7); cq >>= 7;

        var LocalScaleInt = new Vector3Int();
        LocalScaleInt.z = (int)(cq & LowerMask(10)); cq >>= 10;
        LocalScaleInt.y = (int)cq;
        LocalScaleInt.x = (int)(cp & LowerMask(10)); cp >>= 10;

        var PositionInt = new Vector3Int();
        PositionInt.z = UnpackSignedInt(cp & LowerMask(11), 11); cp >>= 11;
        PositionInt.y = UnpackSignedInt(cp & LowerMask(11), 11); cp >>= 11;
        PositionInt.x = UnpackSignedInt(cp & LowerMask(11), 11); cp >>= 11;

        var No = (int)(cp & LowerMask(14));

        return new Structure(No, PositionInt, LocalScaleInt, MoveDirInt, RotationInt, XInversed, YInversed, ZInversed, Tag, parent);
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