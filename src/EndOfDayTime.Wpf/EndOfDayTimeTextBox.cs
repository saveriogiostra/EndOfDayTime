using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EodtCore = EndOfDayTime.Core;

[assembly: System.Windows.ThemeInfo(
    System.Windows.ResourceDictionaryLocation.None,
    System.Windows.ResourceDictionaryLocation.SourceAssembly)]
    
namespace EndOfDayTime.Wpf
{
    /// <summary>
    /// A TextBox that exposes an EndOfDayTime dependency property,
    /// accepts 00:00–24:00, automatically inserts the colon after the second digit,
    /// and validates on lost focus.
    /// </summary>
    public class EndOfDayTimeTextBox : TextBox
    {
        // ── Fields ───────────────────────────────────────────────────────

        private string _digits = string.Empty;
        private bool _updating = false;
        private bool _isValid = true;

        // ── Dependency Property ──────────────────────────────────────────

        /// <summary>Dependency property for <see cref="TimeValue"/>.</summary>
        public static readonly DependencyProperty TimeValueProperty =
            DependencyProperty.Register(
                nameof(TimeValue),
                typeof(EodtCore.EndOfDayTime),
                typeof(EndOfDayTimeTextBox),
                new FrameworkPropertyMetadata(
                    default(EodtCore.EndOfDayTime),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTimeValueChanged));

        /// <summary>The current EndOfDayTime value. Setting this updates the displayed text.</summary>
        public EodtCore.EndOfDayTime TimeValue
        {
            get => (EodtCore.EndOfDayTime)GetValue(TimeValueProperty);
            set => SetValue(TimeValueProperty, value);
        }

        // ── Routed Events ────────────────────────────────────────────────

        /// <summary>Routed event raised when <see cref="TimeValue"/> changes.</summary>
        public static readonly RoutedEvent TimeValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(TimeValueChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<EodtCore.EndOfDayTime>),
                typeof(EndOfDayTimeTextBox));

        /// <summary>Raised when <see cref="TimeValue"/> changes.</summary>
        public event RoutedPropertyChangedEventHandler<EodtCore.EndOfDayTime> TimeValueChanged
        {
            add => AddHandler(TimeValueChangedEvent, value);
            remove => RemoveHandler(TimeValueChangedEvent, value);
        }

        // ── Validation state ─────────────────────────────────────────────

        /// <summary>True if the current text is a valid EndOfDayTime.</summary>
        public bool IsValid => _isValid;

        // ── Constructor ──────────────────────────────────────────────────

        static EndOfDayTimeTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(EndOfDayTimeTextBox),
                new FrameworkPropertyMetadata(typeof(EndOfDayTimeTextBox)));
        }

        /// <summary>Initialises a new instance of <see cref="EndOfDayTimeTextBox"/>.</summary>
        public EndOfDayTimeTextBox()
        {
            MaxLength = 5;
            TextAlignment = TextAlignment.Center;
            LostFocus += OnLostFocus;
        }

        // ── Dependency property callback ──────────────────────────────────

        private static void OnTimeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not EndOfDayTimeTextBox control) return;
            if (control._updating) return;

            var newValue = (EodtCore.EndOfDayTime)e.NewValue;
            var oldValue = (EodtCore.EndOfDayTime)e.OldValue;

            if (newValue == default)
                control.SetDigits(string.Empty);
            else
                control.SetDigits($"{newValue.Hour:D2}{newValue.Minute:D2}");

            control.RaiseEvent(new RoutedPropertyChangedEventArgs<EodtCore.EndOfDayTime>(
                oldValue, newValue, TimeValueChangedEvent));
        }

        // ── Core digit management ─────────────────────────────────────────

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

            if (cursorDigitPos.HasValue)
            {
                var cp = Math.Clamp(cursorDigitPos.Value, 0, _digits.Length);
                var textPos = cp >= 2 ? cp + 1 : cp;
                CaretIndex = Math.Clamp(textPos, 0, Text.Length);
            }
            else
            {
                CaretIndex = Text.Length;
            }

            _updating = false;

            if (_digits.Length == 4)
                Validate();
            else if (_digits.Length > 0)
                SetValidationState(true, null);
        }

        // ── Position mapping ──────────────────────────────────────────────

        private int TextPosToDigitPos(int textPos)
        {
            if (textPos <= 2) return textPos;
            return textPos - 1;
        }

        // ── Keyboard handling ─────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            e.Handled = true;

            foreach (var c in e.Text)
            {
                if (!char.IsDigit(c)) continue;

                int insertAt;
                if (SelectionLength > 0)
                {
                    var dStart = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
                    var dEnd   = Math.Clamp(TextPosToDigitPos(SelectionStart + SelectionLength), 0, _digits.Length);
                    if (dEnd > dStart)
                        _digits = _digits.Remove(dStart, dEnd - dStart);
                    insertAt = dStart;
                }
                else
                {
                    insertAt = Math.Clamp(TextPosToDigitPos(CaretIndex), 0, _digits.Length);
                }

                if (_digits.Length < 4)
                {
                    _digits = _digits.Insert(insertAt, c.ToString());
                    if (_digits.Length > 4) _digits = _digits.Substring(0, 4);
                    SetDigits(_digits, insertAt + 1);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Enter)
            {
                Validate();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                e.Handled = true;
                if (SelectionLength > 0)
                {
                    var dStart = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
                    var dEnd   = Math.Clamp(TextPosToDigitPos(SelectionStart + SelectionLength), 0, _digits.Length);
                    if (dEnd > dStart)
                        SetDigits(_digits.Remove(dStart, dEnd - dStart), dStart);
                }
                else if (_digits.Length > 0)
                {
                    var dPos = Math.Clamp(TextPosToDigitPos(CaretIndex) - 1, 0, _digits.Length - 1);
                    SetDigits(_digits.Remove(dPos, 1), dPos);
                }
                return;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                if (SelectionLength > 0)
                {
                    var dStart = Math.Clamp(TextPosToDigitPos(SelectionStart), 0, _digits.Length);
                    var dEnd   = Math.Clamp(TextPosToDigitPos(SelectionStart + SelectionLength), 0, _digits.Length);
                    if (dEnd > dStart)
                        SetDigits(_digits.Remove(dStart, dEnd - dStart), dStart);
                }
                else if (_digits.Length > 0)
                {
                    var dPos = Math.Clamp(TextPosToDigitPos(CaretIndex), 0, _digits.Length - 1);
                    SetDigits(_digits.Remove(dPos, 1), dPos);
                }
                return;
            }
        }

        /// <inheritdoc/>
        protected override void OnTextChanged(TextChangedEventArgs e)
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

        // ── Validation ────────────────────────────────────────────────────

        /// <summary>Validates the current text. Returns true if valid and updates <see cref="TimeValue"/>.</summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                _isValid = false;
                SetValidationState(false, "Time is required.");
                return false;
            }

            if (!EodtCore.EndOfDayTime.TryParse(Text, out var parsed))
            {
                _isValid = false;
                SetValidationState(false, "Enter a valid time (00:00–24:00).");
                return false;
            }

            _isValid = true;
            SetValidationState(true, null);
            _updating = true;
            TimeValue = parsed;
            _updating = false;
            return true;
        }

        private void SetValidationState(bool isValid, string? message)
        {
            var bindingExpression =
                GetBindingExpression(TimeValueProperty) ??
                (System.Windows.Data.BindingExpressionBase?)GetBindingExpression(TextProperty);

            if (bindingExpression == null) return;

            if (isValid)
                System.Windows.Controls.Validation.ClearInvalid(bindingExpression);
            else
            {
                var error = new System.Windows.Controls.ValidationError(
                    new EndOfDayTimeValidationRule(), bindingExpression)
                {
                    ErrorContent = message
                };
                System.Windows.Controls.Validation.MarkInvalid(bindingExpression, error);
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_digits.Length > 0)
                Validate();
        }
    }
}