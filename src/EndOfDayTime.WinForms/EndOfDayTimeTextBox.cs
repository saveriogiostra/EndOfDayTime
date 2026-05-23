using System;
using System.ComponentModel;
using System.Windows.Forms;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.WinForms
{
    /// <summary>
    /// A TextBox that accepts times in the range 00:00–24:00,
    /// including end-of-day midnight (24:00).
    /// Automatically inserts the colon after the second digit.
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
        private bool _updating = false;
        private string _digits = string.Empty;

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
                _timeValue = value;
                if (value == default)
                    SetDigits(string.Empty);
                else
                    SetDigits($"{value.Hour:D2}{value.Minute:D2}");
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
            TextAlign = HorizontalAlignment.Center;
            UpdateWidth();
        }

        /// <inheritdoc/>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateWidth();
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateWidth();
        }

        private void UpdateWidth()
        {
            var size = TextRenderer.MeasureText("00:00", Font);
            Width = size.Width + 20;
        }

        // ── Core digit management ────────────────────────────────────────

        private void SetDigits(string digits, int? cursorDigitPos = null)
        {
            _digits = digits.Length > 4 ? digits.Substring(0, 4) : digits;
            RenderDigits(cursorDigitPos);
        }

        private void RenderDigits(int? cursorDigitPos = null)
        {
            _updating = true;
            var rendered = _digits.Length >= 2
                ? _digits.Substring(0, 2) + ":" + _digits.Substring(2)
                : _digits;
            Text = rendered;

            // Position cursor
            if (cursorDigitPos.HasValue)
            {
                var cp = Math.Clamp(cursorDigitPos.Value, 0, _digits.Length);
                var textPos = cp >= 2 ? cp + 1 : cp; // skip colon offset
                SelectionStart = Math.Clamp(textPos, 0, Text.Length);
            }
            else
            {
                SelectionStart = Text.Length;
            }

            _updating = false;

            if (_digits.Length == 4)
                Validate();
            else if (_digits.Length > 0)
                SetValidationState(true, string.Empty);
        }

        // ── Position mapping ─────────────────────────────────────────────

        // Maps text position → digit position (skips colon at text pos 2)
        private int TextPosToDigitPos(int textPos)
        {
            if (textPos <= 2) return textPos;
            return textPos - 1;
        }

        // ── Key handling ─────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (char.IsControl(e.KeyChar)) return;

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            e.Handled = true;

            int insertAt;

            if (SelectionLength > 0)
            {
                var dStart = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
                var dEnd = Math.Clamp(TextPosToDigitPos(SelectionStart + SelectionLength), 0, _digits.Length);
                if (dEnd > dStart)
                    _digits = _digits.Remove(dStart, dEnd - dStart);
                insertAt = dStart;
            }
            else
            {
                insertAt = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
            }

            if (_digits.Length < 4)
            {
                _digits = _digits.Insert(insertAt, e.KeyChar.ToString());
                if (_digits.Length > 4) _digits = _digits.Substring(0, 4);
                SetDigits(_digits, insertAt + 1);
            }
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
                return;
            }

            if (e.KeyCode == Keys.Back)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (SelectionLength > 0)
                {
                    var dStart = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
                    var dEnd = Math.Clamp(TextPosToDigitPos(SelectionStart + SelectionLength), 0, _digits.Length);
                    if (dEnd > dStart)
                        SetDigits(_digits.Remove(dStart, dEnd - dStart), dStart);
                }
                else if (_digits.Length > 0)
                {
                    var dPos = Math.Clamp(TextPosToDigitPos(SelectionStart) - 1, 0, _digits.Length - 1);
                    SetDigits(_digits.Remove(dPos, 1), dPos);
                }
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (SelectionLength > 0)
                {
                    var dStart = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
                    var dEnd = Math.Clamp(TextPosToDigitPos(SelectionStart + SelectionLength), 0, _digits.Length);
                    if (dEnd > dStart)
                        SetDigits(_digits.Remove(dStart, dEnd - dStart), dStart);
                }
                else if (_digits.Length > 0)
                {
                    var dPos = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length - 1);
                    SetDigits(_digits.Remove(dPos, 1), dPos);
                }
                return;
            }
        }

        /// <inheritdoc/>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (_updating) return;

            // Handle paste — extract digits only
            var digits = string.Empty;
            foreach (var c in Text)
                if (char.IsDigit(c) && digits.Length < 4)
                    digits += c;

            SetDigits(digits);
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
            if (_digits.Length > 0)
                Validate();
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"{nameof(EndOfDayTimeTextBox)}: {(_isValid ? TimeValue.ToString() : "invalid")}";
    }
}