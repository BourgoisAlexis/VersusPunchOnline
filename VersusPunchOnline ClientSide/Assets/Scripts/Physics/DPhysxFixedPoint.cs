using System;
using UnityEngine;

public struct FixedPoint : IEquatable<FixedPoint>, IComparable<FixedPoint> {
    public const int fractionalBits = 16;
    private const int _fractionalMask = (1 << fractionalBits) - 1;
    private const int _integerMask = ~_fractionalMask;

    private const float _scaleFactor = 1 << fractionalBits;
    public static readonly FixedPoint zero = new FixedPoint(0);

    public readonly int rawValue;

    private FixedPoint(int rawValue) {
        this.rawValue = rawValue;
    }

    public FixedPoint(float value) {
        rawValue = (int)(value * (1 << fractionalBits));
    }

    public float ToFloat() => rawValue / _scaleFactor;
    public static FixedPoint FromRawValue(int value) => new FixedPoint(value);

    public static FixedPoint operator +(FixedPoint a, FixedPoint b) => new FixedPoint(a.rawValue + b.rawValue);
    public static FixedPoint operator -(FixedPoint a, FixedPoint b) => new FixedPoint(a.rawValue - b.rawValue);
    public static FixedPoint operator -(FixedPoint a) => new FixedPoint(-a.rawValue);

    public static FixedPoint operator *(FixedPoint a, FixedPoint b) {
        long result = (long)a.rawValue * b.rawValue >> fractionalBits;
        return new FixedPoint((int)result);
    }

    public static FixedPoint operator /(FixedPoint a, FixedPoint b) {
        long result = ((long)a.rawValue << fractionalBits) / b.rawValue;
        return new FixedPoint((int)result);
    }

    public bool Equals(FixedPoint other) => rawValue == other.rawValue;
    public int CompareTo(FixedPoint other) => rawValue.CompareTo(other.rawValue);

    public override bool Equals(object obj) => obj is FixedPoint other && Equals(other);
    public override int GetHashCode() => rawValue;

    public static bool operator ==(FixedPoint a, FixedPoint b) => a.rawValue == b.rawValue;
    public static bool operator !=(FixedPoint a, FixedPoint b) => a.rawValue != b.rawValue;
    public static bool operator <(FixedPoint a, FixedPoint b) => a.rawValue < b.rawValue;
    public static bool operator >(FixedPoint a, FixedPoint b) => a.rawValue > b.rawValue;
    public static bool operator <=(FixedPoint a, FixedPoint b) => a.rawValue <= b.rawValue;
    public static bool operator >=(FixedPoint a, FixedPoint b) => a.rawValue >= b.rawValue;

    public override string ToString() => ToFloat().ToString();
}

public struct FixedPoint2 {
    public FixedPoint x;
    public FixedPoint y;

    public FixedPoint2 normalized => Normalize();

    public static readonly FixedPoint2 zero = new FixedPoint2(FixedPoint.zero, FixedPoint.zero);

    public FixedPoint2(FixedPoint x, FixedPoint y) {
        this.x = x;
        this.y = y;
    }

    public FixedPoint2(int x, int y) {
        this.x = new FixedPoint(x);
        this.y = new FixedPoint(y);
    }

    public FixedPoint2(float x, float y) {
        this.x = new FixedPoint(x);
        this.y = new FixedPoint(y);
    }

    public static FixedPoint2 operator +(FixedPoint2 a, FixedPoint2 b) => new FixedPoint2(a.x + b.x, a.y + b.y);
    public static FixedPoint2 operator -(FixedPoint2 a, FixedPoint2 b) => new FixedPoint2(a.x - b.x, a.y - b.y);
    public static FixedPoint2 operator *(FixedPoint2 a, FixedPoint b) => new FixedPoint2(a.x * b, a.y * b);
    public static FixedPoint2 operator /(FixedPoint2 a, FixedPoint b) => new FixedPoint2(a.x / b, a.y / b);

    public static FixedPoint Dot(FixedPoint2 a, FixedPoint2 b) {
        return a.x * b.x + a.y * b.y;
    }

    private FixedPoint2 Normalize() {
        long squareMag = (long)x.rawValue * x.rawValue + (long)y.rawValue * y.rawValue;
        FixedPoint magnitude = FP.Sqrt(FixedPoint.FromRawValue((int)squareMag));

        if (magnitude == FixedPoint.zero)
            return zero;

        return new FixedPoint2(x / magnitude, y / magnitude);
    }

    public Vector2 ToVector2() => new Vector2(x.ToFloat(), y.ToFloat());
    public Vector3 ToVector3() => new Vector3(x.ToFloat(), y.ToFloat(), 0);
    public static FixedPoint2 FromVector2(Vector2 v) => new FixedPoint2(v.x, v.y);

    public override string ToString() => $"({x}, {y})";
}

public static class FP {
    public static FixedPoint fp(this int value) => new FixedPoint(value);
    public static FixedPoint fp(this float value) => new FixedPoint(value);
    public static FixedPoint fp(this double value) => new FixedPoint((float)value);

    public static FixedPoint Abs(FixedPoint value) => FixedPoint.FromRawValue(Math.Abs(value.rawValue));
    public static FixedPoint Sign(FixedPoint value) => new FixedPoint(Math.Sign(value.rawValue));

    public static FixedPoint Sqrt(FixedPoint value) {
        int n = value.rawValue;
        int x = n;
        int y = (x + 1) >> 1;
        while (y < x) {
            x = y;
            y = (x + n / x) >> 1;
        }
        return FixedPoint.FromRawValue(x);
    }
}
