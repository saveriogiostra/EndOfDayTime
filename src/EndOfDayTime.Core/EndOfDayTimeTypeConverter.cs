using System;
using System.ComponentModel;
using System.Globalization;

namespace EndOfDayTime.Core
{
    /// <summary>
    /// TypeConverter for <see cref="EndOfDayTime"/>.
    /// Enables automatic conversion in WPF, WinForms, and ASP.NET model binding.
    /// </summary>
    public class EndOfDayTimeTypeConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
                return EndOfDayTime.Parse(s);
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is EndOfDayTime t)
                return t.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}