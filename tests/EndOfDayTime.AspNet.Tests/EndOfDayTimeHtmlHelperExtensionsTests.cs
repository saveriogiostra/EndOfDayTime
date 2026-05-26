using System.IO;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Xunit;
using EndOfDayTime.AspNet;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.AspNet.Tests
{
    public class EndOfDayTimeHtmlHelperExtensionsTests
    {
        private string RenderHtml(IHtmlContent content)
        {
            using var writer = new StringWriter();
            content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return writer.ToString();
        }

        [Fact]
        public void EndOfDayTimeInput_RendersInputElement()
        {
            var result = EndOfDayTimeHtmlHelperExtensions
                .BuildEndOfDayTimeInput("ShiftEnd");
            var rendered = RenderHtml(result);
            Assert.Contains("<input", rendered);
            Assert.Contains("data-eodt-input", rendered);
        }

        [Fact]
        public void EndOfDayTimeInput_SetsName()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput("ShiftEnd"));
            Assert.Contains("name=\"ShiftEnd\"", result);
        }

        [Fact]
        public void EndOfDayTimeInput_SetsId()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput("ShiftEnd"));
            Assert.Contains("id=\"ShiftEnd\"", result);
        }

        [Fact]
        public void EndOfDayTimeInput_WithValue_SetsValue()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput(
                    "ShiftEnd", new EodtCore.EndOfDayTime(17, 30)));
            Assert.Contains("value=\"17:30\"", result);
        }

        [Fact]
        public void EndOfDayTimeInput_WithEndOfDay_SetsValue()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput(
                    "ShiftEnd", EodtCore.EndOfDayTime.EndOfDay));
            Assert.Contains("value=\"24:00\"", result);
        }

        [Fact]
        public void EndOfDayTimeInput_DefaultValue_NoValueAttribute()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput("ShiftEnd"));
            Assert.DoesNotContain("value=", result);
        }

        [Fact]
        public void EndOfDayTimeInput_WithCssClass_SetsClass()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput(
                    "ShiftEnd", cssClass: "my-input"));
            Assert.Contains("class=\"my-input\"", result);
        }

        [Fact]
        public void EndOfDayTimeInput_SetsMaxLength5()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput("ShiftEnd"));
            Assert.Contains("maxlength=\"5\"", result);
        }

        [Fact]
        public void EndOfDayTimeInput_SetsPlaceholder()
        {
            var result = RenderHtml(
                EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput("ShiftEnd"));
            Assert.Contains("placeholder=\"HH:mm\"", result);
        }
    }
}