using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EndOfDayTime.WinForms;
using Xunit;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.WinForms.Tests
{
    /// <summary>
    /// Drives the control with real window messages (WM_CHAR, WM_KEYDOWN, WM_KILLFOCUS),
    /// so input goes through the same path as a physical keyboard instead of
    /// assigning <see cref="Control.Text"/>.
    /// </summary>
    public class EndOfDayTimeTextBoxKeyboardTests
    {
        private const int WM_KILLFOCUS = 0x0008;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_CHAR = 0x0102;
        private const int VK_BACK = 0x08;
        private const int VK_RETURN = 0x0D;
        private const int VK_DELETE = 0x2E;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static EndOfDayTimeTextBox CreateControl(bool required = false)
        {
            var control = new EndOfDayTimeTextBox { IsRequired = required };
            Assert.NotEqual(IntPtr.Zero, control.Handle); // forces the native edit control to exist
            return control;
        }

        private static void Type(Control control, string keys)
        {
            foreach (var c in keys)
                SendMessage(control.Handle, WM_CHAR, (IntPtr)c, IntPtr.Zero);
        }

        private static void Press(Control control, int virtualKey, int times = 1)
        {
            for (var i = 0; i < times; i++)
                SendMessage(control.Handle, WM_KEYDOWN, (IntPtr)virtualKey, IntPtr.Zero);
        }

        private static void LoseFocus(Control control) =>
            SendMessage(control.Handle, WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);

        // ── Valid input ──────────────────────────────────────────────────

        [Theory]
        [InlineData("0930", "09:30", 9, 30)]
        [InlineData("0000", "00:00", 0, 0)]
        [InlineData("2359", "23:59", 23, 59)]
        [InlineData("2400", "24:00", 24, 0)]
        public void TypingDigits_ProducesValidTime(string keys, string text, int hour, int minute)
        {
            using var control = CreateControl();
            Type(control, keys);

            Assert.Equal(text, control.Text);
            Assert.True(control.IsValid);
            Assert.Equal(new EodtCore.EndOfDayTime(hour, minute), control.TimeValue);
            Assert.Equal(string.Empty, control.ErrorMessage);
        }

        [Fact]
        public void TypingDigits_InsertsColonAfterSecondDigit()
        {
            using var control = CreateControl();
            Type(control, "1");
            Assert.Equal("1", control.Text);
            Type(control, "2");
            Assert.Equal("12:", control.Text);
            Type(control, "3");
            Assert.Equal("12:3", control.Text);
        }

        [Fact]
        public void TypingFifthDigit_IsIgnored()
        {
            using var control = CreateControl();
            Type(control, "12345");
            Assert.Equal("12:34", control.Text);
            Assert.Equal(new EodtCore.EndOfDayTime(12, 34), control.TimeValue);
        }

        // ── Invalid input ────────────────────────────────────────────────

        [Theory]
        [InlineData("2500", "25:00")]
        [InlineData("2401", "24:01")]
        [InlineData("1260", "12:60")]
        [InlineData("9999", "99:99")]
        public void TypingOutOfRangeTime_IsInvalid(string keys, string text)
        {
            using var control = CreateControl();
            Type(control, keys);

            Assert.Equal(text, control.Text);
            Assert.False(control.IsValid);
            Assert.Null(control.TimeValue);
            Assert.Equal("Enter a valid time (00:00–24:00).", control.ErrorMessage);
        }

        [Theory]
        [InlineData("bad")]
        [InlineData("nope")]
        [InlineData(":-+. ")]
        public void TypingNonDigits_IsRejected(string keys)
        {
            using var control = CreateControl();
            Type(control, keys);

            Assert.Equal(string.Empty, control.Text);
            Assert.Null(control.TimeValue);
            Assert.True(control.IsValid); // nothing was entered
        }

        [Fact]
        public void TypingMixedCharacters_KeepsOnlyDigits()
        {
            using var control = CreateControl();
            Type(control, "1a2:3b0");
            Assert.Equal("12:30", control.Text);
            Assert.Equal(new EodtCore.EndOfDayTime(12, 30), control.TimeValue);
        }

        [Fact]
        public void TypingValidTimeAfterInvalid_ClearsError()
        {
            using var control = CreateControl();
            var errorProvider = new ErrorProvider();
            errorProvider.Attach(control);

            Type(control, "2500");
            Assert.False(control.IsValid);
            Assert.Equal("Enter a valid time (00:00–24:00).", errorProvider.GetError(control));

            Press(control, VK_BACK, 4);
            Type(control, "2300");
            Assert.True(control.IsValid);
            Assert.Equal(new EodtCore.EndOfDayTime(23, 0), control.TimeValue);
            Assert.Equal(string.Empty, errorProvider.GetError(control));
        }

        // ── Editing ──────────────────────────────────────────────────────

        [Fact]
        public void Backspace_RemovesLastDigit_AndClearsValue()
        {
            using var control = CreateControl();
            Type(control, "1230");
            Press(control, VK_BACK);

            Assert.Equal("12:3", control.Text);
            Assert.Null(control.TimeValue);
            Assert.True(control.IsValid); // still typing

            Type(control, "5");
            Assert.Equal("12:35", control.Text);
            Assert.Equal(new EodtCore.EndOfDayTime(12, 35), control.TimeValue);
        }

        [Fact]
        public void Backspace_UntilEmpty_LeavesNullValue()
        {
            using var control = CreateControl();
            Type(control, "1230");
            Press(control, VK_BACK, 4);

            Assert.Equal(string.Empty, control.Text);
            Assert.Null(control.TimeValue);
        }

        [Fact]
        public void Delete_WithSelection_RemovesSelectedDigits()
        {
            using var control = CreateControl();
            Type(control, "1230");
            control.SelectAll();
            Press(control, VK_DELETE);

            Assert.Equal(string.Empty, control.Text);
            Assert.Null(control.TimeValue);
        }

        [Fact]
        public void TypingOverSelection_ReplacesIt()
        {
            using var control = CreateControl();
            Type(control, "1230");
            control.SelectAll();
            Type(control, "0845");

            Assert.Equal("08:45", control.Text);
            Assert.Equal(new EodtCore.EndOfDayTime(8, 45), control.TimeValue);
        }

        // ── Commit (Enter / lost focus) ──────────────────────────────────

        [Fact]
        public void Enter_OnIncompleteTime_IsInvalid()
        {
            using var control = CreateControl();
            Type(control, "12");
            Assert.True(control.IsValid); // not judged while typing

            Press(control, VK_RETURN);
            Assert.False(control.IsValid);
            Assert.Null(control.TimeValue);
        }

        [Fact]
        public void LostFocus_OnIncompleteTime_IsInvalid()
        {
            using var control = CreateControl();
            Type(control, "123");
            LoseFocus(control);

            Assert.False(control.IsValid);
            Assert.Equal("Enter a valid time (00:00–24:00).", control.ErrorMessage);
        }

        [Fact]
        public void LostFocus_OnEmptyField_IsValidWhenNotRequired()
        {
            using var control = CreateControl();
            LoseFocus(control);

            Assert.True(control.IsValid);
            Assert.Null(control.TimeValue);
        }

        [Fact]
        public void LostFocus_OnEmptyField_IsInvalidWhenRequired()
        {
            using var control = CreateControl(required: true);
            LoseFocus(control);

            Assert.False(control.IsValid);
            Assert.Equal("Time is required.", control.ErrorMessage);
        }

        // ── Events ───────────────────────────────────────────────────────

        [Fact]
        public void Typing_RaisesTimeValueChangedOncePerChange()
        {
            using var control = CreateControl();
            var received = new List<EodtCore.EndOfDayTime?>();
            control.TimeValueChanged += (s, e) => received.Add(e);

            Type(control, "0930");
            Press(control, VK_BACK);
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
        }
    }
}
