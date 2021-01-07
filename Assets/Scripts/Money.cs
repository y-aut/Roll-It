using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲーム内通貨
[Serializable]
public struct Money
{
    public static readonly Money Free = new Money(0, 0);
    public static readonly Money Default = new Money(-1, 0);
    public static readonly Money NotForSale = new Money(-2, 0);

    [SerializeField]
    private int _jewel;
    public int Jewel { get => _jewel; set => _jewel = value; }

    [SerializeField]
    private int _coin;
    public int Coin { get => _coin; set => _coin = value; }

    public Money(int jewel, int coin)
    {
        _jewel = jewel;
        _coin = coin;
    }

    public static Money FromJewel(int jewel) => new Money(jewel, 0);
    public static Money FromCoin(int coin) => new Money(0, coin);

    // 特殊な値でないか
    public bool IsForSale => Jewel >= 0;

    public long Pack() => AddMethod.Pack(Jewel, Coin);
    public static Money Unpack(long packed) => new Money(AddMethod.GetUpper(packed), AddMethod.GetLower(packed));

    public static bool operator ==(Money left, Money right) => left.Jewel == right.Jewel && left.Coin == right.Coin;
    public static bool operator !=(Money left, Money right) => !(left == right);
    public static bool operator <(Money left, Money right) => left.Jewel < right.Jewel && left.Coin < right.Coin;
    public static bool operator >(Money left, Money right) => right < left;
    public static bool operator <=(Money left, Money right) => left.Jewel <= right.Jewel && left.Coin <= right.Coin;
    public static bool operator >=(Money left, Money right) => right <= left;
    public static Money operator -(Money left) => new Money(-left.Jewel, -left.Coin);
    public static Money operator +(Money left, Money right) => new Money(left.Jewel + right.Jewel, left.Coin + right.Coin);
    public static Money operator -(Money left, Money right) => left + (-right);

    public override bool Equals(object obj)
    {
        if (obj is Money money) return this == money;
        else return false;
    }
    public override int GetHashCode() => (Jewel << 16) | Coin;
}
