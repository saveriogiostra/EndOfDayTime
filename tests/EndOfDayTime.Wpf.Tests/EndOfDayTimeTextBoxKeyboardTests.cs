using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EndOfDayTime.Wpf;
using Xunit;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Wpf.Tests
{
    /// <summary>
    /// Drives the control with the routed input events WPF raises for a physical
    /// keyboard (PreviewTextInput, PreviewKeyDown, LostFocus) instead of assigning Text.
    /// </summary>
    public class EndOfDayTimeTextBoxKeyboardTests
    {
        private static void RunOnSta(Action action)
        {
            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception e) { ex = e; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw new Xunit.Sdk.XunitException(ex.ToString());
        }

        private static void Type(EndOfDayTimeTextBox control, string keys)
        {
            foreach (var c in keys)
            {
                control.RaiseEvent(new TextCompositionEventArgs(
                    Keyboard.PrimaryDevice,
                    new TextComposition(InputManager.Current, control, c.ToString()))
                {
                    RoutedEvent = TextCompositionManager.PreviewTextInputEvent
                });
            }
        }

        private static void Press(EndOfDayTimeTextBox control, Key key, int times = 1)
        {
            using var source = new HwndSource(new HwndSourceParameters("eodt-test"));
            for (var i = 0; i < times; i++)
            {
                control.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                });
            }
        }

        private static void LoseFocus(EndOfDayTimeTextBox control) =>
            control.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent));

        // ── Valid input ──────────────────────────────────────────────────

        [Theory]
        [InlineData("0930", "09:30", 9, 30)]
        [InlineData("0000", "00:00", 0, 0)]
        [InlineData("2359", "23:59", 23, 59)]
        [InlineData("2400", "24:00", 24, 0)]
        public void TypingDigits_ProducesValidTime(string keys, string text, int hour, int minute)
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, keys);

                Assert.Equal(text, control.Text);
                Assert.True(control.IsValid);
                Assert.Equal(new EodtCore.EndOfDayTime(hour, minute), control.TimeValue);
            });
        }

        [Fact]
        public void TypingDigits_InsertsColonAfterSecondDigit()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "1");
                Assert.Equal("1", control.Text);
                Type(control, "2");
                Assert.Equal("12:", control.Text);
                Type(control, "3");
                Assert.Equal("12:3", control.Text);
            });
        }

        [Fact]
        public void TypingFifthDigit_IsIgnored()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "12345");
                Assert.Equal("12:34", control.Text);
                Assert.Equal(new EodtCore.EndOfDayTime(12, 34), control.TimeValue);
            });
        }

        // ── Invalid input ────────────────────────────────────────────────

        [Theory]
        [InlineData("2500", "25:00")]
        [InlineData("2401", "24:01")]
        [InlineData("1260", "12:60")]
        [InlineData("9999", "99:99")]
        public void TypingOutOfRangeTime_IsInvalid(string keys, string text)
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, keys);

                Assert.Equal(text, control.Text);
                Assert.False(control.IsValid);
                Assert.Null(control.TimeValue);
            });
        }

        [Fact]
        public void TypingOutOfRangeTime_MarksBindingInvalid_AndLeavesSourceNull()
        {
            RunOnSta(() =>
            {
                var source = new Source { Time = new EodtCore.EndOfDayTime(9, 0) };
                var control = new EndOfDayTimeTextBox();
                control.SetBinding(EndOfDayTimeTextBox.TimeValueProperty,
                    new System.Windows.Data.Binding(nameof(Source.Time)) { Source = source });
                Assert.Equal("09:00", control.Text);

                Press(control, Key.Back, 4);
                Type(control, "2500");

                Assert.Null(source.Time);
                Assert.True(Validation.GetHasError(control));
                Assert.Equal("Enter a valid time (00:00–24:00).",
                    Validation.GetErrors(control)[0].ErrorContent);

                Press(control, Key.Back, 4);
                Type(control, "2300");

                Assert.Equal(new EodtCore.EndOfDayTime(23, 0), source.Time);
                Assert.False(Validation.GetHasError(control));
            });
        }

        [Theory]
        [InlineData("bad")]
        [InlineData("nope")]
        [InlineData(":-+. ")]
        public void TypingNonDigits_IsRejected(string keys)
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, keys);

                Assert.Equal(string.Empty, control.Text);
                Assert.Null(control.TimeValue);
                Assert.True(control.IsValid); // nothing was entered
            });
        }

        [Fact]
        public void TypingMixedCharacters_KeepsOnlyDigits()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "1a2:3b0");
                Assert.Equal("12:30", control.Text);
                Assert.Equal(new EodtCore.EndOfDayTime(12, 30), control.TimeValue);
            });
        }

        // ── Editing ──────────────────────────────────────────────────────

        [Fact]
        public void Backspace_RemovesLastDigit_AndClearsValue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "1230");
                Press(control, Key.Back);

                Assert.Equal("12:3", control.Text);
                Assert.Null(control.TimeValue);
                Assert.True(control.IsValid); // still typing

                Type(control, "5");
                Assert.Equal("12:35", control.Text);
                Assert.Equal(new EodtCore.EndOfDayTime(12, 35), control.TimeValue);
            });
        }

        [Fact]
        public void Backspace_UntilEmpty_LeavesNullValue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "1230");
                Press(control, Key.Back, 4);

                Assert.Equal(string.Empty, control.Text);
                Assert.Null(control.TimeValue);
            });
        }

        [Fact]
        public void TypingOverSelection_ReplacesIt()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "1230");
                control.SelectAll();
                Type(control, "0845");

                Assert.Equal("08:45", control.Text);
                Assert.Equal(new EodtCore.EndOfDayTime(8, 45), control.TimeValue);
            });
        }

        // ── Commit (Enter / lost focus) ──────────────────────────────────

        [Fact]
        public void Enter_OnIncompleteTime_IsInvalid()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "12");
                Assert.True(control.IsValid); // not judged while typing

                Press(control, Key.Enter);
                Assert.False(control.IsValid);
                Assert.Null(control.TimeValue);
            });
        }

        [Fact]
        public void LostFocus_OnIncompleteTime_IsInvalid()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Type(control, "123");
                LoseFocus(control);
                Assert.False(control.IsValid);
            });
        }

        [Fact]
        public void LostFocus_OnEmptyField_DependsOnIsRequired()
        {
            RunOnSta(() =>
            {
                var optional = new EndOfDayTimeTextBox();
                LoseFocus(optional);
                Assert.True(optional.IsValid);

                var required = new EndOfDayTimeTextBox { IsRequired = true };
                LoseFocus(required);
                Assert.False(required.IsValid);
            });
        }

        // ── Events ───────────────────────────────────────────────────────

        [Fact]
        public void Typing_RaisesTimeValueChangedOncePerChange()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                var received = new List<EodtCore.EndOfDayTime?>();
                control.TimeValueChanged += (s, e) => received.Add(e.NewValue);

                Type(control, "0930");
                Press(control, Key.Back);
                Type(control, "5");
                LoseFocus(control);

                Assert.Equal(
                    new EodtCore.EndOfDayTime?[]
                    {
                        new EodtCore.EndOfDayTime(9, 30),
                        null,
                        new EodtCore.EndOfDayTime(9, 35),
                    },
                    received);
            });
        }

        private class Source
        {
            public EodtCore.EndOfDayTime? Time { get; set; }
        }
    }
}
