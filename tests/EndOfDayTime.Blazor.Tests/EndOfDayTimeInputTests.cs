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

    public class NullableTestModel
    {
        public EodtCore.EndOfDayTime? End { get; set; }
    }

    public class EndOfDayTimeInputTests : TestContext
    {
        private void SetupJSInterop()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        private IRenderedComponent<EndOfDayTimeInput<EodtCore.EndOfDayTime>> RenderInput(EodtCore.EndOfDayTime value)
        {
            SetupJSInterop();
            var model = new TestModel { End = value };
            return RenderComponent<EndOfDayTimeInput<EodtCore.EndOfDayTime>>(parameters => parameters
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
            var input = cut.Find("input");
            Assert.NotNull(input);
        }

        [Fact]
        public void Renders_EndOfDay_AsString()
        {
            var cut = RenderInput(EodtCore.EndOfDayTime.EndOfDay);
            Assert.NotNull(cut.Find("input"));
        }

        [Fact]
        public void Renders_Midnight_AsString()
        {
            RenderInput(new EodtCore.EndOfDayTime(0, 0));
            Assert.Contains(JSInterop.Invocations, i =>
                i.Identifier == "setValue" && Equals(i.Arguments[0], "00:00"));
        }

        [Fact]
        public void Renders_NullValue_AsEmpty()
        {
            SetupJSInterop();
            var model = new NullableTestModel();
            RenderComponent<EndOfDayTimeInput<EodtCore.EndOfDayTime?>>(parameters => parameters
                .Add(p => p.Value, model.End)
                .Add(p => p.ValueChanged, v => model.End = v)
                .Add(p => p.ValueExpression, () => model.End));
            Assert.DoesNotContain(JSInterop.Invocations, i => i.Identifier == "setValue");
        }

        [Fact]
        public void Nullable_ClearedInput_SetsNull()
        {
            SetupJSInterop();
            var model = new NullableTestModel { End = new EodtCore.EndOfDayTime(9, 0) };
            var cut = RenderComponent<EndOfDayTimeInput<EodtCore.EndOfDayTime?>>(parameters => parameters
                .Add(p => p.Value, model.End)
                .Add(p => p.ValueChanged, v => model.End = v)
                .Add(p => p.ValueExpression, () => model.End));
            cut.InvokeAsync(() => cut.Instance.OnDigitsChanged(string.Empty));
            Assert.Null(model.End);
        }

        [Fact]
        public void Nullable_Midnight_SetsMidnight()
        {
            SetupJSInterop();
            var model = new NullableTestModel();
            var cut = RenderComponent<EndOfDayTimeInput<EodtCore.EndOfDayTime?>>(parameters => parameters
                .Add(p => p.Value, model.End)
                .Add(p => p.ValueChanged, v => model.End = v)
                .Add(p => p.ValueExpression, () => model.End));
            cut.InvokeAsync(() => cut.Instance.OnDigitsChanged("00:00"));
            Assert.Equal(new EodtCore.EndOfDayTime(0, 0), model.End);
        }

        // ── Validation error on blur ─────────────────────────────────────

        [Fact]
        public void Blur_InvalidValue_ShowsError()
        {
            var cut = RenderInput(default);
            cut.InvokeAsync(() => cut.Instance.OnDigitsChanged("25:00"));
            cut.InvokeAsync(() => cut.Instance.OnBlur());
            cut.WaitForAssertion(() =>
                Assert.NotEmpty(cut.FindAll("span.eodt-validation-error")));
        }

        [Fact]
        public void Blur_ValidValue_NoError()
        {
            var cut = RenderInput(default);
            cut.InvokeAsync(() => cut.Instance.OnDigitsChanged("09:00"));
            cut.InvokeAsync(() => cut.Instance.OnBlur());
            cut.WaitForAssertion(() =>
                Assert.Empty(cut.FindAll("span.eodt-validation-error")));
        }

        [Fact]
        public void Blur_EmptyValue_NoError()
        {
            var cut = RenderInput(default);
            cut.InvokeAsync(() => cut.Instance.OnBlur());
            cut.WaitForAssertion(() =>
                Assert.Empty(cut.FindAll("span.eodt-validation-error")));
        }

        // ── EditForm integration ─────────────────────────────────────────

        [Fact]
        public void InsideEditForm_ValidValue_NoValidationMessage()
        {
            SetupJSInterop();
            var model = new TestModel { End = new EodtCore.EndOfDayTime(17, 0) };
            var cut = RenderComponent<TestFormValid>(parameters => parameters
                .Add(p => p.Model, model));
            Assert.Empty(cut.FindAll("span.eodt-validation-error"));
        }
    }
}