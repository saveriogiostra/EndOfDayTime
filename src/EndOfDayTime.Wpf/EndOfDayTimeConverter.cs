using System;
using System.Globalization;
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
                return t == default ? string.Empty : t.ToString();
            return string.Empty;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && EodtCore.EndOfDayTime.TryParse(s, out var result))
                return result;
            return default(EodtCore.EndOfDayTime);
        }
    }
}