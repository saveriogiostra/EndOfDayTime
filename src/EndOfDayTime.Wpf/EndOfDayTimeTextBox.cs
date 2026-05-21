using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Wpf
{
    /// <summary>
    /// A TextBox that exposes an EndOfDayTime dependency property,
    /// accepts 00:00–24:00, and validates on lost focus.
    /// </summary>
    public class EndOfDayTimeTextBox : TextBox
    {

/// <summary>Dependency property for <see cref="TimeValue"/>.</summary>        // ── Dependency Property ──────────────────────────────────────────
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

        private bool _isValid = true;
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
            LostFocus += OnLostFocus;
            MaxLength = 5;
        }

        // ── Callbacks ────────────────────────────────────────────────────

        private static void OnTimeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not EndOfDayTimeTextBox control) return;

            var newValue = (EodtCore.EndOfDayTime)e.NewValue;
            var oldValue = (EodtCore.EndOfDayTime)e.OldValue;

            // Update text only if it differs, to avoid cursor jumping
            var formatted = newValue == default ? string.Empty : newValue.ToString();
            if (control.Text != formatted)
                control.Text = formatted;

            control.RaiseEvent(new RoutedPropertyChangedEventArgs<EodtCore.EndOfDayTime>(
                oldValue, newValue, TimeValueChangedEvent));
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Validate();
        }

        // ── Public methods ───────────────────────────────────────────────

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
            TimeValue = parsed;
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private void SetValidationState(bool isValid, string? message)
        {
            var bindingExpression =
                GetBindingExpression(TimeValueProperty) ??
                (System.Windows.Data.BindingExpressionBase?)GetBindingExpression(TextProperty);

            if (bindingExpression == null) return;

            if (isValid)
            {
                System.Windows.Controls.Validation.ClearInvalid(bindingExpression);
            }
            else
            {
                var error = new System.Windows.Controls.ValidationError(
                    new EndOfDayTimeValidationRule(),
                    bindingExpression)
                {
                    ErrorContent = message
                };
                System.Windows.Controls.Validation.MarkInvalid(bindingExpression, error);
            }
        }

        // ── Keyboard handling ────────────────────────────────────────────

  /// <inheritdoc/>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Accept Enter as confirmation
            if (e.Key == Key.Enter)
            {
                Validate();
                e.Handled = true;
            }
        }
    }
}