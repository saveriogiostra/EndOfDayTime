using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.AspNet
{
    /// <summary>
    /// Tag helper that renders an EndOfDayTime input field with smart digit input.
    /// Usage: &lt;eodt-input asp-for="MyProperty" /&gt;
    /// </summary>
    [HtmlTargetElement("eodt-input")]
    public class EndOfDayTimeInputTagHelper : TagHelper
    {
        private readonly IHtmlGenerator _generator;

        /// <summary>Model expression for the EndOfDayTime property.</summary>
        [HtmlAttributeName("asp-for")]
        public ModelExpression? For { get; set; }

        /// <summary>Optional CSS class.</summary>
        public string? Class { get; set; }

        /// <summary>Optional placeholder text. Defaults to HH:mm.</summary>
        public string Placeholder { get; set; } = "HH:mm";

        /// <inheritdoc/>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        /// <summary>Initialises a new instance of <see cref="EndOfDayTimeInputTagHelper"/>.</summary>
        public EndOfDayTimeInputTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.TagMode = TagMode.SelfClosing;

            output.Attributes.SetAttribute("type", "text");
            output.Attributes.SetAttribute("maxlength", "5");
            output.Attributes.SetAttribute("placeholder", Placeholder);
            output.Attributes.SetAttribute("data-eodt-input", "true");
            output.Attributes.SetAttribute("autocomplete", "off");
            output.Attributes.SetAttribute("style", "width:70px; text-align:center; font-family:Consolas;");

            if (!string.IsNullOrEmpty(Class))
                output.Attributes.SetAttribute("class", Class);

            if (For != null)
            {
                output.Attributes.SetAttribute("name", For.Name);
                output.Attributes.SetAttribute("id", For.Name.Replace(".", "_"));

                var value = For.Model;
                if (value is EodtCore.EndOfDayTime t && t != default)
                    output.Attributes.SetAttribute("value", t.ToString());
            }
        }
    }
}