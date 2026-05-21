using System.Threading;
using System.Windows;
using EndOfDayTime.Wpf;
using Xunit;
using EodtCore = EndOfDayTime.Core;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace EndOfDayTime.Wpf.Tests
{
    public class EndOfDayTimeTextBoxTests
    {
        private static T RunOnSta<T>(Func<T> func)
        {
            T result = default!;
            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try { result = func(); }
                catch (Exception e) { ex = e; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            return result;
        }

        private static void RunOnSta(Action action) =>
            RunOnSta<bool>(() => { action(); return true; });

        // ── Construction ─────────────────────────────────────────────────

        [Fact]
        public void Constructor_DefaultTimeValue_IsDefault()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Assert.Equal(default(EodtCore.EndOfDayTime), control.TimeValue);
            });
        }

        [Fact]
        public void Constructor_MaxLength_IsFive()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Assert.Equal(5, control.MaxLength);
            });
        }

        // ── TimeValue property ───────────────────────────────────────────

        [Fact]
        public void SetTimeValue_UpdatesText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
                Assert.Equal("09:30", control.Text);
            });
        }

        [Fact]
        public void SetTimeValue_EndOfDay_UpdatesText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = EodtCore.EndOfDayTime.EndOfDay;
                Assert.Equal("24:00", control.Text);
            });
        }

        [Fact]
        public void SetTimeValue_Default_TextIsEmpty()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
                control.TimeValue = default;
                Assert.Equal(string.Empty, control.Text);
            });
        }

        // ── Validate() ───────────────────────────────────────────────────

        [Fact]
        public void Validate_ValidText_ReturnsTrue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "09:30";
                Assert.True(control.Validate());
                Assert.True(control.IsValid);
            });
        }

        [Fact]
        public void Validate_ValidText_UpdatesTimeValue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "24:00";
                control.Validate();
                Assert.Equal(EodtCore.EndOfDayTime.EndOfDay, control.TimeValue);
            });
        }

        [Fact]
        public void Validate_InvalidText_ReturnsFalse()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "bad";
                Assert.False(control.Validate());
                Assert.False(control.IsValid);
            });
        }

        [Fact]
        public void Validate_EmptyText_ReturnsFalse()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = string.Empty;
                Assert.False(control.Validate());
                Assert.False(control.IsValid);
            });
        }

        [Fact]
        public void Validate_EndOfDay_ReturnsTrue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "24:00";
                Assert.True(control.Validate());
                Assert.Equal(EodtCore.EndOfDayTime.EndOfDay, control.TimeValue);
            });
        }

        // ── TimeValueChanged event ───────────────────────────────────────

        [Fact]
        public void TimeValueChanged_Fires_WhenTimeValueSet()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                EodtCore.EndOfDayTime? received = null;
                control.TimeValueChanged += (s, e) => received = e.NewValue;
                control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
                Assert.NotNull(received);
                Assert.Equal(new EodtCore.EndOfDayTime(9, 30), received!.Value);
            });
        }
    }
}