using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.WinForms
{
    /// <summary>
    /// A TextBox that accepts times in the range 00:00–24:00,
    /// including end-of-day midnight (24:00).
    /// Validates on lost focus and exposes an ErrorProvider-compatible interface.
    /// </summary>
    [DefaultEvent(nameof(TimeValueChanged))]
    [DefaultProperty(nameof(TimeValue))]
    public class EndOfDayTimeTextBox : TextBox
    {
        // ── Fields ───────────────────────────────────────────────────────

        private EodtCore.EndOfDayTime _timeValue;
        private bool _isValid = true;
        private string _errorMessage = string.Empty;

        // ── Events ───────────────────────────────────────────────────────

        /// <summary>Raised when TimeValue changes to a valid parsed value.</summary>
        public event EventHandler<EodtCore.EndOfDayTime>? TimeValueChanged;

        /// <summary>Raised when validation state changes.</summary>
        public event EventHandler<bool>? IsValidChanged;

        // ── Properties ───────────────────────────────────────────────────

        /// <summary>
        /// The current EndOfDayTime value. Setting this updates the displayed text.
        /// </summary>
        [Browsable(true)]
        [Category("Data")]
        [Description("The current EndOfDayTime value (00:00–24:00).")]
        public EodtCore.EndOfDayTime TimeValue
        {
            get => _timeValue;
            set
            {
                if (_timeValue == value) return;
                var old = _timeValue;
                _timeValue = value;
                var formatted = value == default ? string.Empty : value.ToString();
                if (Text != formatted) Text = formatted;
                TimeValueChanged?.Invoke(this, value);
            }
        }

        /// <summary>True if the current text is a valid EndOfDayTime.</summary>
        [Browsable(false)]
        public bool IsValid => _isValid;

        /// <summary>The last validation error message, or empty if valid.</summary>
        [Browsable(false)]
        public string ErrorMessage => _errorMessage;

        // ── Constructor ──────────────────────────────────────────────────

        /// <summary>Initialises a new instance of <see cref="EndOfDayTimeTextBox"/>.</summary>
        public EndOfDayTimeTextBox()
        {
            MaxLength = 5;
            PlaceholderText = "HH:mm";
        }

        // ── Validation ───────────────────────────────────────────────────

        /// <summary>
        /// Validates the current text. Updates IsValid and ErrorMessage.
        /// Returns true if valid.
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                SetValidationState(false, "Time is required.");
                return false;
            }

            if (!EodtCore.EndOfDayTime.TryParse(Text, out var parsed))
            {
                SetValidationState(false, "Enter a valid time (00:00–24:00).");
                return false;
            }

            SetValidationState(true, string.Empty);
            _timeValue = parsed;
            TimeValueChanged?.Invoke(this, parsed);
            return true;
        }

        private void SetValidationState(bool isValid, string message)
        {
            var changed = _isValid != isValid;
            _isValid = isValid;
            _errorMessage = message;
            if (changed) IsValidChanged?.Invoke(this, isValid);
        }

        // ── Overrides ────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Validate();
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                Validate();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // ── Designer support ─────────────────────────────────────────────

        /// <inheritdoc/>
        public override string ToString() =>
            $"{nameof(EndOfDayTimeTextBox)}: {(_isValid ? TimeValue.ToString() : "invalid")}";
    }
}