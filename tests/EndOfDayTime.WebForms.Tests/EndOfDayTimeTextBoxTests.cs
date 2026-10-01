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
        public void TimeValue_Default_IsNull()
        {
            var control = new EndOfDayTimeTextBox();
            Assert.Null(control.TimeValue);
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
            Assert.True(control.TimeValue.Value.IsEndOfDay);
        }

        [Fact]
        public void TimeValue_SetNull_ReturnsNull()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
            control.TimeValue = null;
            Assert.Null(control.TimeValue);
        }

        [Fact]
        public void TimeValue_SetMidnight_ReturnsMidnight()
        {
            var control = new EndOfDayTimeTextBox();
            control.TimeValue = new EodtCore.EndOfDayTime(0, 0);
            Assert.Equal(new EodtCore.EndOfDayTime(0, 0), control.TimeValue);
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

        [Fact]
        public void InvalidText_IsNotValid_AndHasNoTimeValue()
        {
            var control = new EndOfDayTimeTextBox();
            control.Text = "25:00";
            Assert.False(control.IsValid);
            Assert.Null(control.TimeValue);
            Assert.Equal("25:00", control.Text); // kept so the user can correct it
        }

        // ── Rendering ────────────────────────────────────────────────────

        private static string Render(EndOfDayTimeTextBox control)
        {
            using (var sw = new System.IO.StringWriter())
            using (var writer = new System.Web.UI.HtmlTextWriter(sw))
            {
                control.RenderControl(writer);
                return sw.ToString();
            }
        }

        [Fact]
        public void Render_EmitsInputWithEodtAttributes()
        {
            var html = Render(new EndOfDayTimeTextBox { TimeValue = new EodtCore.EndOfDayTime(0, 0) });
            Assert.StartsWith("<input", html);
            Assert.Contains("data-eodt-input=\"true\"", html);
            Assert.Contains("placeholder=\"HH:mm\"", html);
            Assert.Contains("maxlength=\"5\"", html);
            Assert.Contains("value=\"00:00\"", html);
        }

        [Fact]
        public void Render_CssClass_IsEncoded()
        {
            var html = Render(new EndOfDayTimeTextBox { CssClass = "a\"><script>" });
            Assert.DoesNotContain("<script>", html);
        }

        [Fact]
        public void Render_Disabled_EmitsDisabledAttribute()
        {
            var html = Render(new EndOfDayTimeTextBox { Enabled = false });
            Assert.Contains("disabled=\"disabled\"", html);
        }

        [Fact]
        public void Render_ExplicitWidth_ReplacesDefault()
        {
            var html = Render(new EndOfDayTimeTextBox { Width = System.Web.UI.WebControls.Unit.Pixel(120) });
            Assert.Contains("120px", html);
            Assert.DoesNotContain("70px", html);
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