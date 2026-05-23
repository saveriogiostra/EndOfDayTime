using System;
using System.Windows.Forms;
using EndOfDayTime.WinForms;
using Xunit;
using EodtCore = EndOfDayTime.Core;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace EndOfDayTime.WinForms.Tests
{
    public class EndOfDayTimeTextBoxTests
    {
        // ── Construction ─────────────────────────────────────────────────

        [Fact]
        public void Constructor_DefaultTimeValue_IsDefault()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.Equal(default(EodtCore.EndOfDayTime), control.TimeValue);
        }

        [Fact]
        public void Constructor_MaxLength_IsFive()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.Equal(5, control.MaxLength);
        }

        [Fact]
        public void Constructor_IsValid_IsTrue()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.True(control.IsValid);
        }

        // ── TimeValue property ───────────────────────────────────────────

        [Fact]
        public void SetTimeValue_UpdatesText()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
            Assert.Equal("09:30", control.Text);
        }

        [Fact]
        public void SetTimeValue_EndOfDay_UpdatesText()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = EodtCore.EndOfDayTime.EndOfDay;
            Assert.Equal("24:00", control.Text);
        }

        [Fact]
        public void SetTimeValue_Default_TextIsEmpty()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
            control.TimeValue = default;
            Assert.Equal(string.Empty, control.Text);
        }

        [Fact]
        public void SetTimeValue_SameValue_DoesNotFireEvent()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
            int count = 0;
            control.TimeValueChanged += (s, e) => count++;
            control.TimeValue = new EodtCore.EndOfDayTime(9, 0); // same value
            Assert.Equal(0, count);
        }

        // ── Validate() ───────────────────────────────────────────────────

        [Fact]
        public void Validate_ValidText_ReturnsTrue()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "09:30";
            Assert.True(control.Validate());
            Assert.True(control.IsValid);
        }

        [Fact]
        public void Validate_ValidText_UpdatesTimeValue()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "09:30";
            control.Validate();
            Assert.Equal(new EodtCore.EndOfDayTime(9, 30), control.TimeValue);
        }

        [Fact]
        public void Validate_EndOfDay_ReturnsTrue()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "24:00";
            Assert.True(control.Validate());
            Assert.Equal(EodtCore.EndOfDayTime.EndOfDay, control.TimeValue);
        }

        [Fact]
        public void Validate_InvalidText_ReturnsFalse()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "bad";
            Assert.False(control.Validate());
            Assert.False(control.IsValid);
        }

        [Fact]
        public void Validate_InvalidText_SetsErrorMessage()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "25:00";
            control.Validate();
            Assert.Contains("00:00–24:00", control.ErrorMessage);
        }

        [Fact]
        public void Validate_EmptyText_ReturnsFalse()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = string.Empty;
            Assert.False(control.Validate());
            Assert.Equal("Time is required.", control.ErrorMessage);
        }

        [Fact]
        public void Validate_AfterInvalid_ThenValid_ClearsError()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "bad";
            control.Validate();
            Assert.False(control.IsValid);

            control.Text = "09:00";
            control.Validate();
            Assert.True(control.IsValid);
            Assert.Equal(string.Empty, control.ErrorMessage);
        }

        // ── TimeValueChanged event ───────────────────────────────────────

        [Fact]
        public void TimeValueChanged_Fires_WhenTimeValueSet()
        {
            var control = new EndOfDayTimeTextBox();
            EodtCore.EndOfDayTime? received = null;
            control.TimeValueChanged += (s, e) => received = e;
            control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
            Assert.NotNull(received);
            Assert.Equal(new EodtCore.EndOfDayTime(9, 30), received!.Value);
        }

        [Fact]
        public void TimeValueChanged_Fires_WhenValidateSucceeds()
        {
            var control = new EndOfDayTimeTextBox();
            EodtCore.EndOfDayTime? received = null;
            control.TimeValueChanged += (s, e) => received = e;
            control.Text = "17:00";
            control.Validate();
            Assert.NotNull(received);
            Assert.Equal(new EodtCore.EndOfDayTime(17, 0), received!.Value);
        }

        // ── IsValidChanged event ─────────────────────────────────────────

        [Fact]
        public void IsValidChanged_Fires_WhenValidationStateChanges()
        {
            var control = new EndOfDayTimeTextBox();
            int count = 0;
            control.IsValidChanged += (s, e) => count++;

            control.Text = "bad";
            control.Validate(); // valid → invalid: fires
            Assert.Equal(1, count);

            control.Text = "09:00";
            control.Validate(); // invalid → valid: fires
            Assert.Equal(2, count);
        }

        [Fact]
        public void IsValidChanged_DoesNotFire_WhenStateUnchanged()
        {
            var control = new EndOfDayTimeTextBox();
            int count = 0;
            control.IsValidChanged += (s, e) => count++;

            control.Text = "bad";
            control.Validate(); // valid → invalid: fires
            Assert.Equal(1, count);

            control.Text = "nope"; // 4 chars — no auto-validate
            control.Validate(); // invalid → invalid: does not fire
            Assert.Equal(1, count);
        }

        // ── ErrorProvider integration ────────────────────────────────────

        [Fact]
        public void ErrorProvider_Attach_ShowsErrorOnInvalid()
        {
            var control = new EndOfDayTimeTextBox();
            var errorProvider = new ErrorProvider();
            errorProvider.Attach(control);

            // Set text directly via TimeValue to bypass key filtering
            // then corrupt it to simulate an invalid state
            control.Text = "99:99"; // numerically invalid time
            control.Validate();

            Assert.Equal("Enter a valid time (00:00–24:00).", errorProvider.GetError(control));
        }

        [Fact]
        public void ErrorProvider_Attach_ClearsErrorOnValid()
        {
            var control = new EndOfDayTimeTextBox();
            var errorProvider = new ErrorProvider();
            errorProvider.Attach(control);

            control.Text = "bad";
            control.Validate();
            control.Text = "09:00";
            control.Validate();

            Assert.Equal(string.Empty, errorProvider.GetError(control));
        }
    }
}