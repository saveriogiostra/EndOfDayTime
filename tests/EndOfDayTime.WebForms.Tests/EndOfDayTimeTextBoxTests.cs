using System;
using EndOfDayTime.WebForms;
using Xunit;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.WebForms.Tests
{
    public class EndOfDayTimeTextBoxTests
    {
        // ── TimeValue property ───────────────────────────────────────────

        [Fact]
        public void TimeValue_Default_IsDefault()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.Equal(default(EodtCore.EndOfDayTime), control.TimeValue);
        }

        [Fact]
        public void TimeValue_SetValidTime_ReturnsCorrectValue()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
            Assert.Equal(new EodtCore.EndOfDayTime(9, 30), control.TimeValue);
        }

        [Fact]
        public void TimeValue_SetEndOfDay_ReturnsEndOfDay()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = EodtCore.EndOfDayTime.EndOfDay;
            Assert.True(control.TimeValue.IsEndOfDay);
        }

        [Fact]
        public void TimeValue_SetDefault_ReturnsDefault()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
            control.TimeValue = default;
            Assert.Equal(default(EodtCore.EndOfDayTime), control.TimeValue);
        }

        // ── IsValid ──────────────────────────────────────────────────────

        [Fact]
        public void IsValid_Default_IsTrue()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.True(control.IsValid);
        }

        [Fact]
        public void IsValid_ValidTime_IsTrue()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
            Assert.True(control.IsValid);
        }

        [Fact]
        public void IsValid_EndOfDay_IsTrue()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = EodtCore.EndOfDayTime.EndOfDay;
            Assert.True(control.IsValid);
        }

        // ── InputCssClass ────────────────────────────────────────────────

        [Fact]
        public void InputCssClass_Default_IsEmpty()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.Equal(string.Empty, control.InputCssClass);
        }

        [Fact]
        public void InputCssClass_Set_ReturnsValue()
        {
            var control = new EndOfDayTimeTextBox();
            control.InputCssClass = "my-input";
            Assert.Equal("my-input", control.InputCssClass);
        }

        // ── TimeValueChanged event ────────────────────────────────────────

        [Fact]
        public void TimeValueChanged_CanSubscribe()
        {
            var control = new EndOfDayTimeTextBox();
            var fired = false;
            control.TimeValueChanged += (s, e) => fired = true;
            Assert.False(fired); // just verifying subscription works
        }
    }
}