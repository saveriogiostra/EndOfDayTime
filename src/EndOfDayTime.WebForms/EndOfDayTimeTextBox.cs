#if NET48
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.WebForms
{
    /// <summary>
    /// A WebForms TextBox control that accepts times in the range 00:00–24:00,
    /// including end-of-day midnight (24:00).
    /// </summary>
    [ToolboxData("<{0}:EndOfDayTimeTextBox runat=\"server\" />")]
    public class EndOfDayTimeTextBox : WebControl, IPostBackDataHandler
    {
        // ── Properties ───────────────────────────────────────────────────

        /// <summary>The current EndOfDayTime value.</summary>
        public EodtCore.EndOfDayTime TimeValue
        {
            get
            {
                var s = ViewState["TimeValue"] as string;
                if (s != null && EodtCore.EndOfDayTime.TryParse(s, out var t))
                    return t;
                return default;
            }
            set
            {
                ViewState["TimeValue"] = value == default ? string.Empty : value.ToString();
            }
        }

        /// <summary>Returns true if the current value is valid.</summary>
        public bool IsValid
        {
            get
            {
                var s = ViewState["TimeValue"] as string;
                return string.IsNullOrEmpty(s) || EodtCore.EndOfDayTime.TryParse(s, out _);
            }
        }

        /// <summary>Optional CSS class for the input element.</summary>
        public string InputCssClass { get; set; } = string.Empty;

        // ── Events ───────────────────────────────────────────────────────

        /// <summary>Raised when TimeValue changes.</summary>
        public event EventHandler TimeValueChanged = delegate { };

        // ── Rendering ────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            var value = TimeValue == default ? string.Empty : TimeValue.ToString();
            var cssClass = string.IsNullOrEmpty(InputCssClass) ? string.Empty : $" class=\"{InputCssClass}\"";

            writer.Write(
                $"<input type=\"text\" " +
                $"id=\"{ClientID}\" " +
                $"name=\"{UniqueID}\" " +
                $"value=\"{value}\" " +
                $"maxlength=\"5\" " +
                $"placeholder=\"HH:mm\" " +
                $"data-eodt-input=\"true\" " +
                $"autocomplete=\"off\" " +
                $"style=\"width:70px;text-align:center;font-family:Consolas;\"{cssClass} />");
        }

        // ── IPostBackDataHandler ─────────────────────────────────────────

        /// <inheritdoc/>
        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            var posted = postCollection[postDataKey] ?? string.Empty;
            var current = TimeValue == default ? string.Empty : TimeValue.ToString();

            if (posted == current) return false;

            if (EodtCore.EndOfDayTime.TryParse(posted, out var parsed))
                TimeValue = parsed;
            else
                ViewState["TimeValue"] = posted;

            return true;
        }

        /// <inheritdoc/>
        public void RaisePostDataChangedEvent()
        {
            TimeValueChanged(this, EventArgs.Empty);
        }

        // ── Script ───────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Page != null)
            {
                var scriptKey = "EndOfDayTimeInputScript";
                if (!Page.ClientScript.IsClientScriptBlockRegistered(GetType(), scriptKey))
                {
                    var scriptUrl = Page.ClientScript.GetWebResourceUrl(
                        GetType(), "EndOfDayTime.WebForms.endofdaytime-input.js");
                    Page.ClientScript.RegisterClientScriptInclude(
                        GetType(), scriptKey, scriptUrl);
                }
            }
        }
    }
}
#endif