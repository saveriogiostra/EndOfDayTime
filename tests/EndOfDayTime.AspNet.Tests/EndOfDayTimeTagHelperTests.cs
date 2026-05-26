using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using EndOfDayTime.AspNet;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.AspNet.Tests
{
    public class EndOfDayTimeTagHelperTests
    {
        private TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "eodt-input",
                new TagHelperAttributeList(),
                (_, __) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        private TagHelperContext CreateContext()
        {
            return new TagHelperContext(
                "eodt-input",
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-id");
        }

        // ── Rendering ────────────────────────────────────────────────────

        [Fact]
        public void Process_RendersInputElement()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal("input", output.TagName);
        }

        [Fact]
        public void Process_SetsDataAttribute()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.True(output.Attributes.ContainsName("data-eodt-input"));
        }

        [Fact]
        public void Process_SetsMaxLength5()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal("5", output.Attributes["maxlength"].Value.ToString());
        }

        [Fact]
        public void Process_SetsPlaceholder()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal("HH:mm", output.Attributes["placeholder"].Value.ToString());
        }

        [Fact]
        public void Process_CustomPlaceholder()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>())
            {
                Placeholder = "Enter time"
            };
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal("Enter time", output.Attributes["placeholder"].Value.ToString());
        }

        [Fact]
        public void Process_SetsTypeText()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal("text", output.Attributes["type"].Value.ToString());
        }

        [Fact]
        public void Process_SelfClosingTag()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal(TagMode.SelfClosing, output.TagMode);
        }

        [Fact]
        public void Process_WithCssClass_SetsClass()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>())
            {
                Class = "my-input"
            };
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.Equal("my-input", output.Attributes["class"].Value.ToString());
        }

        [Fact]
        public void Process_WithoutCssClass_NoClassAttribute()
        {
            var helper = new EndOfDayTimeInputTagHelper(Mock.Of<IHtmlGenerator>());
            var output = CreateOutput();
            helper.Process(CreateContext(), output);
            Assert.False(output.Attributes.ContainsName("class"));
        }
    }
}