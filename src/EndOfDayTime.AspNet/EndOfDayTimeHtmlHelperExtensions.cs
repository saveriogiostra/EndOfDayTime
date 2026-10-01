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
        /// The property may be an EndOfDayTime or a nullable EndOfDayTime; null renders an empty field.
        /// </summary>
        public static IHtmlContent EndOfDayTimeInputFor<TModel, TResult>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TResult>> expression,
            string? cssClass = null)
        {
            var name  = html.NameFor(expression);
            var value = html.ValueFor(expression);
            return BuildEndOfDayTimeInput(
                name,
                EodtCore.EndOfDayTime.TryParse(value, out var parsed) ? parsed : null,
                cssClass);
        }

        /// <summary>
        /// Renders an EndOfDayTime input field by name. A null value renders an empty field.
        /// </summary>
        public static IHtmlContent EndOfDayTimeInput(
            this IHtmlHelper html,
            string name,
            EodtCore.EndOfDayTime? value = null,
            string? cssClass = null)
        {
            return BuildEndOfDayTimeInput(name, value, cssClass);
        }

        /// <summary>
        /// Builds the TagBuilder for an EndOfDayTime input.
        /// </summary>
        internal static IHtmlContent BuildEndOfDayTimeInput(
            string name,
            EodtCore.EndOfDayTime? value = null,
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

            if (value.HasValue)
                input.Attributes["value"] = value.Value.ToString();

            if (!string.IsNullOrEmpty(cssClass))
                input.Attributes["class"] = cssClass;

            input.TagRenderMode = TagRenderMode.SelfClosing;
            return input;
        }
    }
}