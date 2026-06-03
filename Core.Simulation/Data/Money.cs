using System;

namespace Core.Simulation.Data
{
    public readonly struct Money : IEquatable<Money>, IComparable<Money>
    {
        private readonly long _micros;
        private const long SCALAR = 1_000_000;

        public Money(long micros) => _micros = micros;
        public static Money FromUnits(long units) => new Money(units * SCALAR);

        public static Money Zero => new Money(0);
        public static Money MaxValue => new Money(long.MaxValue);

        public long ToMicros() => _micros;
        public long Micros => _micros;
        public static Money FromMicros(long micros) => new Money(micros);
        public decimal ToDecimal() => _micros / (decimal)SCALAR;
        public string ToLeiString() => $"{ToDecimal():N2} lei";
        public override string ToString() => ToLeiString();

        public static Money operator +(Money a, Money b) => new Money(a._micros + b._micros);
        public static Money operator -(Money a, Money b) => new Money(a._micros - b._micros);
        public static Money operator *(Money a, int multiplier) => new Money(a._micros * multiplier);
        public static Money operator /(Money a, int divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            return new Money(a._micros / divisor);
        }

        public static bool operator >(Money a, Money b) => a._micros > b._micros;
        public static bool operator <(Money a, Money b) => a._micros < b._micros;
        public static bool operator >=(Money a, Money b) => a._micros >= b._micros;
        public static bool operator <=(Money a, Money b) => a._micros <= b._micros;
        public static bool operator ==(Money a, Money b) => a._micros == b._micros;
        public static bool operator !=(Money a, Money b) => a._micros != b._micros;
        public bool Equals(Money other) => _micros == other._micros;
        public override bool Equals(object? obj) => obj is Money other && Equals(other);
        public override int GetHashCode() => _micros.GetHashCode();
        public int CompareTo(Money other) => _micros.CompareTo(other._micros);
    }
}
