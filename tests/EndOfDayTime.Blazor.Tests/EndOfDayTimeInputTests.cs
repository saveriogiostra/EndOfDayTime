using Bunit;
using Xunit;
using EodtCore = EndOfDayTime.Core;
using EndOfDayTime.Blazor;

namespace EndOfDayTime.Blazor.Tests
{
    public class TestModel
    {
        public EodtCore.EndOfDayTime End { get; set; }
    }

    public class EndOfDayTimeInputTests : TestContext
    {
        private IRenderedComponent<EndOfDayTimeInput> RenderInput(EodtCore.EndOfDayTime value)
        {
            var model = new TestModel { End = value };
            return RenderComponent<EndOfDayTimeInput>(parameters => parameters
                .Add(p => p.Value, model.End)
                .Add(p => p.ValueChanged, v => model.End = v)
                .Add(p => p.ValueExpression, () => model.End));
        }

        // ── Rendering ────────────────────────────────────────────────────

        [Fact]
        public void Renders_InputElement()
        {
            var cut = RenderInput(new EodtCore.EndOfDayTime(9, 0));
            Assert.NotNull(cut.Find("input"));
        }

        [Fact]
        public void Renders_CorrectInitialValue()
        {
            var cut = RenderInput(new EodtCore.EndOfDayTime(9, 30));
            Assert.Equal("09:30", cut.Find("input").GetAttribute("value"));
        }

        [Fact]
        public void Renders_EndOfDay_AsString()
        {
            var cut = RenderInput(EodtCore.EndOfDayTime.EndOfDay);
            Assert.Equal("24:00", cut.Find("input").GetAttribute("value"));
        }

        [Fact]
        public void Renders_DefaultValue_AsEmpty()
        {
            var cut = RenderInput(default);
            Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
        }

        // ── Validation error on blur ─────────────────────────────────────

        [Fact]
        public void Blur_InvalidValue_ShowsError()
        {
            var cut = RenderInput(default);
            cut.Find("input").Input("bad");
            cut.Find("input").Blur();
            var error = cut.Find("span.eodt-validation-error");
            Assert.Contains("00:00–24:00", error.TextContent);
        }

        [Fact]
        public void Blur_ValidValue_NoError()
        {
            var cut = RenderInput(default);
            cut.Find("input").Input("24:00");
            cut.Find("input").Blur();
            Assert.Empty(cut.FindAll("span.eodt-validation-error"));
        }

        [Fact]
        public void Blur_EmptyValue_NoError()
        {
            var cut = RenderInput(default);
            cut.Find("input").Input("");
            cut.Find("input").Blur();
            Assert.Empty(cut.FindAll("span.eodt-validation-error"));
        }

        // ── EditForm integration ─────────────────────────────────────────

        [Fact]
        public void InsideEditForm_ValidValue_NoValidationMessage()
        {
            var model = new TestModel { End = new EodtCore.EndOfDayTime(17, 0) };
            var cut = RenderComponent<TestFormValid>(parameters => parameters
                .Add(p => p.Model, model));
            Assert.Empty(cut.FindAll("span.eodt-validation-error"));
        }
    }
}