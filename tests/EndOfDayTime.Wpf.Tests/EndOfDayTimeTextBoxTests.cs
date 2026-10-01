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
        public void Constructor_DefaultTimeValue_IsNull()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Assert.Null(control.TimeValue);
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
        public void SetTimeValue_Null_TextIsEmpty()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
                control.TimeValue = null;
                Assert.Equal(string.Empty, control.Text);
            });
        }

        [Fact]
        public void SetTimeValue_Midnight_UpdatesText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(0, 0);
                Assert.Equal("00:00", control.Text);
                Assert.Equal(new EodtCore.EndOfDayTime(0, 0), control.TimeValue);
            });
        }

        [Fact]
        public void ClearText_SetsTimeValueToNull()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
                control.Text = string.Empty;
                Assert.Null(control.TimeValue);
            });
        }

        [Fact]
        public void PartialText_SetsTimeValueToNull()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "12:30";
                control.Text = "12:3";
                Assert.Null(control.TimeValue);
                Assert.True(control.IsValid); // still typing — not an error yet
            });
        }

        [Fact]
        public void InvalidText_SetsTimeValueToNull()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "12:30";
                control.Text = "25:00";
                Assert.Null(control.TimeValue);
                Assert.False(control.IsValid);
            });
        }

        [Fact]
        public void IsValid_RecoversAfterInvalidInput()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "25:00";
                Assert.False(control.IsValid);
                control.Text = "12:3";
                Assert.True(control.IsValid);
            });
        }

        [Fact]
        public void UserInput_UpdatesTwoWayBoundSource_AndKeepsBinding()
        {
            RunOnSta(() =>
            {
                var source = new BoundSource();
                var control = new EndOfDayTimeTextBox();
                control.SetBinding(EndOfDayTimeTextBox.TimeValueProperty,
                    new System.Windows.Data.Binding(nameof(BoundSource.Time)) { Source = source });

                control.Text = "09:30";
                Assert.Equal(new EodtCore.EndOfDayTime(9, 30), source.Time);

                control.Text = "09:3";
                Assert.Null(source.Time);
                Assert.NotNull(control.GetBindingExpression(EndOfDayTimeTextBox.TimeValueProperty));
            });
        }

        [Fact]
        public void Converter_ConvertBack_ValidText_ReturnsValue()
        {
            var converter = new EndOfDayTimeConverter();
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            Assert.Equal(new EodtCore.EndOfDayTime(0, 0),
                converter.ConvertBack("00:00", typeof(EodtCore.EndOfDayTime), null!, culture));
            Assert.Equal(EodtCore.EndOfDayTime.EndOfDay,
                converter.ConvertBack("24:00", typeof(EodtCore.EndOfDayTime?), null!, culture));
        }

        [Theory]
        [InlineData("25:00")]
        [InlineData("12:3")]
        [InlineData("abc")]
        public void Converter_ConvertBack_InvalidText_ReturnsUnsetValue(string text)
        {
            var converter = new EndOfDayTimeConverter();
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            Assert.Same(DependencyProperty.UnsetValue,
                converter.ConvertBack(text, typeof(EodtCore.EndOfDayTime), null!, culture));
            Assert.Same(DependencyProperty.UnsetValue,
                converter.ConvertBack(text, typeof(EodtCore.EndOfDayTime?), null!, culture));
        }

        [Fact]
        public void Converter_ConvertBack_EmptyText_DependsOnTargetNullability()
        {
            var converter = new EndOfDayTimeConverter();
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            Assert.Null(converter.ConvertBack("", typeof(EodtCore.EndOfDayTime?), null!, culture));
            Assert.Same(DependencyProperty.UnsetValue,
                converter.ConvertBack("", typeof(EodtCore.EndOfDayTime), null!, culture));
        }

        [Fact]
        public void Converter_InvalidText_LeavesBoundSourceUnchanged()
        {
            RunOnSta(() =>
            {
                var source = new BoundSource { Time = new EodtCore.EndOfDayTime(9, 30) };
                var textBox = new System.Windows.Controls.TextBox();
                textBox.SetBinding(System.Windows.Controls.TextBox.TextProperty,
                    new System.Windows.Data.Binding(nameof(BoundSource.Time))
                    {
                        Source = source,
                        Mode = System.Windows.Data.BindingMode.TwoWay,
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
                        Converter = new EndOfDayTimeConverter()
                    });
                Assert.Equal("09:30", textBox.Text);

                textBox.Text = "25:00";
                Assert.Equal(new EodtCore.EndOfDayTime(9, 30), source.Time);
                Assert.True(System.Windows.Controls.Validation.GetHasError(textBox));

                textBox.Text = "17:00";
                Assert.Equal(new EodtCore.EndOfDayTime(17, 0), source.Time);
                Assert.False(System.Windows.Controls.Validation.GetHasError(textBox));
            });
        }

        private class BoundSource
        {
            public EodtCore.EndOfDayTime? Time { get; set; }
        }

        [Fact]
        public void Converter_Midnight_ConvertsToText()
        {
            var converter = new EndOfDayTimeConverter();
            var text = converter.Convert(
                new EodtCore.EndOfDayTime(0, 0), typeof(string), null!,
                System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal("00:00", text);
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

        [Fact]
        public void TimeValueChanged_Fires_WhenUserEntersText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                var received = new List<EodtCore.EndOfDayTime?>();
                control.TimeValueChanged += (s, e) => received.Add(e.NewValue);
                control.Text = "09:30";
                Assert.Equal(new EodtCore.EndOfDayTime?[] { new EodtCore.EndOfDayTime(9, 30) }, received);
                Assert.Equal("09:30", control.Text);
            });
        }

        [Fact]
        public void TimeValueChanged_Fires_WhenUserClearsText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "09:30";
                var received = new List<EodtCore.EndOfDayTime?>();
                control.TimeValueChanged += (s, e) => received.Add(e.NewValue);
                control.Text = string.Empty;
                Assert.Equal(new EodtCore.EndOfDayTime?[] { null }, received);
            });
        }

        [Fact]
        public void TimeValueChanged_DoesNotFire_WhenValueUnchanged()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "09:30";
                int count = 0;
                control.TimeValueChanged += (s, e) => count++;
                control.Validate();
                Assert.Equal(0, count);
            });
        }
    }
}