using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.AspNet
{
    /// <summary>
    /// HtmlHelper extensions for EndOfDayTime input fields.
    /// </summary>
    public static class EndOfDayTimeHtmlHelperExtensions
    {
        /// <summary>
        /// Renders an EndOfDayTime input field for the specified model expression.
        /// </summary>
        public static IHtmlContent EndOfDayTimeInputFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, EodtCore.EndOfDayTime>> expression,
            string? cssClass = null)
        {
            var name  = html.NameFor(expression);
            var value = html.ValueFor(expression);
            EodtCore.EndOfDayTime.TryParse(value, out var parsed);
            return BuildEndOfDayTimeInput(name, parsed, cssClass);
        }

        /// <summary>
        /// Renders an EndOfDayTime input field by name.
        /// </summary>
        public static IHtmlContent EndOfDayTimeInput(
            this IHtmlHelper html,
            string name,
            EodtCore.EndOfDayTime value = default,
            string? cssClass = null)
        {
            return BuildEndOfDayTimeInput(name, value, cssClass);
        }

        /// <summary>
        /// Builds the TagBuilder for an EndOfDayTime input — used internally and by tests.
        /// </summary>
        public static IHtmlContent BuildEndOfDayTimeInput(
            string name,
            EodtCore.EndOfDayTime value = default,
            string? cssClass = null)
        {
            var input = new TagBuilder("input");
            input.Attributes["type"]            = "text";
            input.Attributes["id"]              = name.Replace(".", "_");
            input.Attributes["name"]            = name;
            input.Attributes["maxlength"]       = "5";
            input.Attributes["placeholder"]     = "HH:mm";
            input.Attributes["data-eodt-input"] = "true";
            input.Attributes["autocomplete"]    = "off";
            input.Attributes["style"]           = "width:70px; text-align:center; font-family:Consolas;";

            if (value != default)
                input.Attributes["value"] = value.ToString();

            if (!string.IsNullOrEmpty(cssClass))
                input.Attributes["class"] = cssClass;

            input.TagRenderMode = TagRenderMode.SelfClosing;
            return input;
        }
    }
}