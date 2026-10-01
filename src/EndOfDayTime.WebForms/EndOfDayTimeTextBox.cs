using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.WebForms
{
    /// <summary>
    /// A WebForms TextBox that accepts times in the range 00:00–24:00,
    /// including end-of-day midnight (24:00).
    /// All standard TextBox members (CssClass, Enabled, Width, AutoPostBack, …) apply.
    /// </summary>
    [ToolboxData("<{0}:EndOfDayTimeTextBox runat=\"server\" />")]
    public class EndOfDayTimeTextBox : TextBox
    {
        private const string ScriptResource = "EndOfDayTime.WebForms.endofdaytime-input.js";

        /// <summary>Initialises a new instance of <see cref="EndOfDayTimeTextBox"/>.</summary>
        public EndOfDayTimeTextBox()
        {
            MaxLength = 5;
        }

        // ── Properties ───────────────────────────────────────────────────

        /// <summary>
        /// The current EndOfDayTime value, or null when <see cref="TextBox.Text"/> is empty or invalid.
        /// </summary>
        public EodtCore.EndOfDayTime? TimeValue
        {
            get
            {
                if (EodtCore.EndOfDayTime.TryParse(Text, out var t))
                    return t;
                return null;
            }
            set
            {
                Text = value.HasValue ? value.Value.ToString() : string.Empty;
            }
        }

        /// <summary>Returns true if the text is empty or a valid time.</summary>
        public bool IsValid =>
            string.IsNullOrWhiteSpace(Text) || EodtCore.EndOfDayTime.TryParse(Text, out _);

        // ── Events ───────────────────────────────────────────────────────

        /// <summary>Raised on postback when the posted text differs from the previous one.</summary>
        public event EventHandler TimeValueChanged = delegate { };

        /// <inheritdoc/>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            TimeValueChanged(this, EventArgs.Empty);
        }

        // ── Rendering ────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);

            writer.AddAttribute("placeholder", "HH:mm");
            writer.AddAttribute("data-eodt-input", "true");
            writer.AddAttribute("autocomplete", "off");

            // Defaults only — anything set through Width, Font or CssClass wins.
            if (Width.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "70px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
        }

        // ── Script ───────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Page != null)
            {
                var type = typeof(EndOfDayTimeTextBox);
                Page.ClientScript.RegisterClientScriptInclude(
                    type,
                    "EndOfDayTimeInputScript",
                    Page.ClientScript.GetWebResourceUrl(type, ScriptResource));
            }
        }
    }
}
