using System.Globalization;
using System.Windows.Controls;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Wpf
{
    /// <summary>
    /// WPF ValidationRule that accepts times in the range 00:00–24:00.
    /// Add this to a Binding's ValidationRules collection.
    /// </summary>
    public class EndOfDayTimeValidationRule : ValidationRule
    {
   /// <inheritdoc/>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "Time is required.");

            if (!EodtCore.EndOfDayTime.TryParse(input, out _))
                return new ValidationResult(false, "Enter a valid time (00:00–24:00).");

            return ValidationResult.ValidResult;
        }
    }
}