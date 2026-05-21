using System;

namespace EndOfDayTime.Core
{
    /// <summary>
    /// Represents a time of day in the range 00:00–24:00, where 24:00 denotes end-of-day midnight.
    /// </summary>
    [System.ComponentModel.TypeConverter(typeof(EndOfDayTimeTypeConverter))]
    public readonly struct EndOfDayTime : IEquatable<EndOfDayTime>, IComparable<EndOfDayTime>
    {
        private readonly short _minutes;

        /// <summary>The minimum value, 00:00.</summary>
        public static readonly EndOfDayTime MinValue = new EndOfDayTime(0);

        /// <summary>The maximum value, 24:00 (end of day).</summary>
        public static readonly EndOfDayTime MaxValue = new EndOfDayTime(1440);

        /// <summary>End-of-day midnight, 24:00.</summary>
        public static readonly EndOfDayTime EndOfDay = new EndOfDayTime(1440);

        /// <summary>The hour component (0–24).</summary>
        public int Hour => _minutes / 60;

        /// <summary>The minute component (0–59).</summary>
        public int Minute => _minutes % 60;

        /// <summary>Returns true if this value represents 24:00.</summary>
        public bool IsEndOfDay => _minutes == 1440;

        private EndOfDayTime(short minutes) { _minutes = minutes; }

        /// <summary>
        /// Initialises a new EndOfDayTime with the specified hour and minute.
        /// </summary>
        /// <param name="hour">Hour (0–24).</param>
        /// <param name="minute">Minute (0–59). Must be 0 when hour is 24.</param>
        public EndOfDayTime(int hour, int minute)
        {
            if (hour < 0 || hour > 24)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 24.");
            if (minute < 0 || minute > 59)
                throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59.");
            if (hour == 24 && minute != 0)
                throw new ArgumentOutOfRangeException(nameof(minute), "When hour is 24, minute must be 0.");
            _minutes = (short)(hour * 60 + minute);
        }

        /// <summary>Creates an EndOfDayTime from a total number of minutes (0–1440).</summary>
        /// <param name="minutes">Total minutes from midnight.</param>
        public static EndOfDayTime FromMinutes(int minutes)
        {
            if (minutes < 0 || minutes > 1440)
                throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be between 0 and 1440.");
            return new EndOfDayTime((short)minutes);
        }

        /// <summary>Total minutes from midnight (0–1440).</summary>
        public int TotalMinutes => _minutes;

        /// <summary>Parses a string in HH:mm format (00:00–24:00).</summary>
        /// <exception cref="FormatException">Thrown when the string is not a valid EndOfDayTime.</exception>
        public static EndOfDayTime Parse(string value)
        {
            if (TryParse(value, out var result)) return result;
            throw new FormatException($"'{value}' is not a valid EndOfDayTime. Expected format HH:mm (00:00–24:00).");
        }

        /// <summary>Tries to parse a string in HH:mm format (00:00–24:00).</summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="result">The parsed value, or default on failure.</param>
        /// <returns>True if parsing succeeded.</returns>
        public static bool TryParse(string value, out EndOfDayTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value)) return false;
            var parts = value.Trim().Split(':');
            if (parts.Length != 2) return false;
            if (parts[0].Length != 2 || parts[1].Length != 2) return false;
            if (!int.TryParse(parts[0], out int hour)) return false;
            if (!int.TryParse(parts[1], out int minute)) return false;
            if (hour < 0 || hour > 24) return false;
            if (minute < 0 || minute > 59) return false;
            if (hour == 24 && minute != 0) return false;
            result = new EndOfDayTime((short)(hour * 60 + minute));
            return true;
        }

        /// <summary>Returns the time formatted as HH:mm.</summary>
        public override string ToString() => $"{Hour:D2}:{Minute:D2}";

        /// <inheritdoc/>
        public bool Equals(EndOfDayTime other) => _minutes == other._minutes;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is EndOfDayTime other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => _minutes.GetHashCode();

        /// <inheritdoc/>
        public int CompareTo(EndOfDayTime other) => _minutes.CompareTo(other._minutes);

        /// <summary>Equality operator.</summary>
        public static bool operator ==(EndOfDayTime a, EndOfDayTime b) => a.Equals(b);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(EndOfDayTime a, EndOfDayTime b) => !a.Equals(b);

        /// <summary>Less-than operator.</summary>
        public static bool operator <(EndOfDayTime a, EndOfDayTime b) => a._minutes < b._minutes;

        /// <summary>Greater-than operator.</summary>
        public static bool operator >(EndOfDayTime a, EndOfDayTime b) => a._minutes > b._minutes;

        /// <summary>Less-than-or-equal operator.</summary>
        public static bool operator <=(EndOfDayTime a, EndOfDayTime b) => a._minutes <= b._minutes;

        /// <summary>Greater-than-or-equal operator.</summary>
        public static bool operator >=(EndOfDayTime a, EndOfDayTime b) => a._minutes >= b._minutes;

        /// <summary>Returns the duration between two times as a TimeSpan.</summary>
        public static TimeSpan operator -(EndOfDayTime end, EndOfDayTime start)
            => TimeSpan.FromMinutes(end._minutes - start._minutes);

#if NET6_0_OR_GREATER
        /// <summary>
        /// Converts to TimeOnly. Throws if this value is 24:00 — use the next day's 00:00 instead.
        /// </summary>
        public TimeOnly ToTimeOnly()
        {
            if (IsEndOfDay)
                throw new InvalidOperationException("24:00 cannot be represented as TimeOnly. Use the next day's 00:00.");
            return new TimeOnly(Hour, Minute);
        }

        /// <summary>Explicit conversion to TimeOnly. Throws if value is 24:00.</summary>
        public static explicit operator TimeOnly(EndOfDayTime t) => t.ToTimeOnly();

        /// <summary>Implicit conversion from TimeOnly.</summary>
        public static implicit operator EndOfDayTime(TimeOnly t) => new EndOfDayTime(t.Hour, t.Minute);
#endif

        /// <summary>Converts to TimeSpan.</summary>
        public TimeSpan ToTimeSpan() => TimeSpan.FromMinutes(_minutes);

        /// <summary>Explicit conversion to TimeSpan.</summary>
        public static explicit operator TimeSpan(EndOfDayTime t) => t.ToTimeSpan();
    }
}