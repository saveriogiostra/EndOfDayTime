using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Wpf
{
    /// <summary>
    /// WPF IValueConverter for EndOfDayTime — enables binding in XAML.
    /// </summary>
    public class EndOfDayTimeConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EodtCore.EndOfDayTime t)
                return t.ToString();
            return string.Empty;
        }

        /// <summary>
        /// Converts text back to an EndOfDayTime. Empty text becomes null when the target
        /// is nullable. Invalid text (or empty text for a non-nullable target) returns
        /// <see cref="DependencyProperty.UnsetValue"/>, so the binding reports a
        /// validation error and leaves the source unchanged.
        /// </summary>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;

            if (string.IsNullOrWhiteSpace(s))
                return targetType == typeof(EodtCore.EndOfDayTime)
                    ? DependencyProperty.UnsetValue
                    : null;

            return EodtCore.EndOfDayTime.TryParse(s, out var result)
                ? result
                : DependencyProperty.UnsetValue;
        }
    }
}