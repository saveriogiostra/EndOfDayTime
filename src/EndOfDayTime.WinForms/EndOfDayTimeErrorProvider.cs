using System;
using System.Windows.Forms;

namespace EndOfDayTime.WinForms
{
    /// <summary>
    /// Extension methods that connect EndOfDayTimeTextBox to a WinForms ErrorProvider.
    /// Usage: errorProvider.Attach(myTimeTextBox);
    /// </summary>
    public static class EndOfDayTimeErrorProviderExtensions
    {
        /// <summary>
        /// Attaches an ErrorProvider to an EndOfDayTimeTextBox.
        /// The provider will automatically show/hide errors as the user types.
        /// </summary>
        public static void Attach(this ErrorProvider errorProvider, EndOfDayTimeTextBox textBox)
        {
            textBox.IsValidChanged += (sender, isValid) =>
            {
                errorProvider.SetError(textBox, isValid ? string.Empty : textBox.ErrorMessage);
            };
        }
    }
}