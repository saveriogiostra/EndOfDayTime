using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace EndOfDayTime.WebForms.Tests
{
    /// <summary>
    /// Hosts a real page containing EndOfDayTimeTextBox in the ASP.NET pipeline.
    /// </summary>
    public sealed class WebSiteFixture : IDisposable
    {
        private const string Page = @"<%@ Page Language=""C#"" %>
<script runat=""server"">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && Request.QueryString[""init""] != null)
            Box.TimeValue = EndOfDayTime.Core.EndOfDayTime.Parse(Request.QueryString[""init""]);
        if (Request.QueryString[""disabled""] != null)
            Box.Enabled = false;
    }

    protected void Box_TimeValueChanged(object sender, EventArgs e)
    {
        Changed.Text = ""changed"";
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        Result.Text = string.Format(""valid={0};value={1};text={2}"",
            Box.IsValid,
            Box.TimeValue.HasValue ? Box.TimeValue.Value.ToString() : ""null"",
            Box.Text);
    }
</script>
<!DOCTYPE html>
<html>
<head runat=""server""><title>test</title></head>
<body>
  <form id=""form1"" runat=""server"">
    <eodt:EndOfDayTimeTextBox ID=""Box"" runat=""server"" CssClass=""time-box""
        OnTimeValueChanged=""Box_TimeValueChanged"" />
    <asp:Button ID=""Go"" runat=""server"" Text=""Go"" />
    <asp:Label ID=""Result"" runat=""server"" />
    <asp:Label ID=""Changed"" runat=""server"" EnableViewState=""false"" />
  </form>
</body>
</html>";

        private readonly string _directory;

        public AspNetTestHost Host { get; }

        public WebSiteFixture()
        {
            _directory = Path.Combine(Path.GetTempPath(), "eodt-webforms-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            Host = AspNetTestHost.Create(_directory, ("Default.aspx", Page));
        }

        public void Dispose()
        {
            try { Host.Shutdown(); } catch { }

            // The web application's AppDomain unloads asynchronously and holds the
            // bin assemblies until it is gone.
            for (var attempt = 0; attempt < 50; attempt++)
            {
                try { Directory.Delete(_directory, true); return; }
                catch (IOException) { System.Threading.Thread.Sleep(200); }
                catch (UnauthorizedAccessException) { System.Threading.Thread.Sleep(200); }
            }
        }
    }

    public class EndOfDayTimeTextBoxPipelineTests : IClassFixture<WebSiteFixture>
    {
        private readonly AspNetTestHost _host;

        public EndOfDayTimeTextBoxPipelineTests(WebSiteFixture fixture)
        {
            _host = fixture.Host;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private string Get(string query = "")
        {
            var response = _host.Process("Default.aspx", query, "GET", null);
            Assert.True(response.Status == 200, "GET failed (" + response.Status + "):\n" + response.Body);
            return response.Body;
        }

        // Posts the form back the way a browser would: all hidden fields from the
        // previous response, the text box value and the submit button.
        private string Post(string previousHtml, string boxValue)
        {
            var fields = new List<KeyValuePair<string, string>>();
            foreach (Match m in Regex.Matches(previousHtml,
                "<input type=\"hidden\" name=\"([^\"]+)\" id=\"[^\"]+\" value=\"([^\"]*)\""))
                fields.Add(new KeyValuePair<string, string>(m.Groups[1].Value, WebUtility.HtmlDecode(m.Groups[2].Value)));
            Assert.Contains(fields, f => f.Key == "__VIEWSTATE");
            if (boxValue != null)
                fields.Add(new KeyValuePair<string, string>("Box", boxValue));
            fields.Add(new KeyValuePair<string, string>("Go", "Go"));

            var body = string.Join("&", fields.Select(f =>
                Uri.EscapeDataString(f.Key) + "=" + Uri.EscapeDataString(f.Value)));
            var response = _host.Process("Default.aspx", string.Empty, "POST", body);
            Assert.True(response.Status == 200, "POST failed (" + response.Status + "):\n" + response.Body);
            return response.Body;
        }

        private static string InputTag(string html)
        {
            var match = Regex.Match(html, "<input name=\"Box\"[^>]*>");
            Assert.True(match.Success, "Text box input not found in:\n" + html);
            return match.Value;
        }

        private static string Result(string html)
        {
            return Regex.Match(html, "<span id=\"Result\">([^<]*)</span>").Groups[1].Value;
        }

        private static bool Changed(string html)
        {
            return html.Contains("<span id=\"Changed\">changed</span>");
        }

        // ── Rendering ────────────────────────────────────────────────────

        [Fact]
        public void Get_RendersInputWithExpectedAttributes()
        {
            var input = InputTag(Get());
            Assert.Contains("type=\"text\"", input);
            Assert.Contains("id=\"Box\"", input);
            Assert.Contains("maxlength=\"5\"", input);
            Assert.Contains("placeholder=\"HH:mm\"", input);
            Assert.Contains("data-eodt-input=\"true\"", input);
            Assert.Contains("class=\"time-box\"", input);
            Assert.DoesNotContain("value=", input);
        }

        [Fact]
        public void Get_EmptyBox_IsValidWithNullValue()
        {
            Assert.Equal("valid=True;value=null;text=", Result(Get()));
        }

        [Theory]
        [InlineData("17:30")]
        [InlineData("00:00")]
        [InlineData("24:00")]
        public void Get_ValueSetInCode_IsRendered(string time)
        {
            var html = Get("init=" + time);
            Assert.Contains("value=\"" + time + "\"", InputTag(html));
            Assert.Equal("valid=True;value=" + time + ";text=" + time, Result(html));
        }

        [Fact]
        public void Get_Disabled_RendersDisabledInput()
        {
            Assert.Contains("disabled=\"disabled\"", InputTag(Get("disabled=1")));
        }

        // ── Client script ────────────────────────────────────────────────

        [Fact]
        public void Get_IncludesScript_AndWebResourceServesIt()
        {
            var html = Get();
            var match = Regex.Match(html, "<script src=\"/WebResource\\.axd\\?([^\"]+)\"");
            var urls = Regex.Matches(html, "<script src=\"/WebResource\\.axd\\?([^\"]+)\"")
                .Cast<Match>().Select(m => WebUtility.HtmlDecode(m.Groups[1].Value)).ToList();
            Assert.NotEmpty(urls);

            var scripts = urls.Select(q => _host.Process("WebResource.axd", q, "GET", null)).ToList();
            Assert.All(scripts, s => Assert.Equal(200, s.Status));
            Assert.Contains(scripts, s =>
                s.Body.Contains("data-eodt-input") && s.Body.Contains("initEndOfDayTimeInput"));
        }

        // ── Postback ─────────────────────────────────────────────────────

        [Theory]
        [InlineData("09:30")]
        [InlineData("00:00")]
        [InlineData("24:00")]
        public void Post_ValidTime_IsParsedAndRedisplayed(string time)
        {
            var html = Post(Get(), time);
            Assert.Equal("valid=True;value=" + time + ";text=" + time, Result(html));
            Assert.Contains("value=\"" + time + "\"", InputTag(html));
            Assert.True(Changed(html));
        }

        [Theory]
        [InlineData("25:00")]
        [InlineData("24:01")]
        [InlineData("12:60")]
        [InlineData("12:3")]
        [InlineData("abcde")]
        public void Post_InvalidTime_IsNotValid_AndKeepsTheTypedText(string text)
        {
            var html = Post(Get(), text);
            Assert.Equal("valid=False;value=null;text=" + text, Result(html));
            Assert.Contains("value=\"" + text + "\"", InputTag(html));
        }

        [Fact]
        public void Post_SpecialCharactersInText_AreHtmlEncoded()
        {
            // Angle brackets never reach the control: ASP.NET request validation rejects them.
            var html = Post(Get(), "\"x&y");
            Assert.Contains("value=\"&quot;x&amp;y\"", InputTag(html));
            Assert.StartsWith("valid=False;value=null;", Result(html));
        }

        [Fact]
        public void Post_Markup_IsRejectedByRequestValidation()
        {
            var first = Get();
            var fields = Regex.Matches(first, "<input type=\"hidden\" name=\"([^\"]+)\" id=\"[^\"]+\" value=\"([^\"]*)\"")
                .Cast<Match>()
                .Select(m => Uri.EscapeDataString(m.Groups[1].Value) + "=" +
                             Uri.EscapeDataString(WebUtility.HtmlDecode(m.Groups[2].Value)));
            var body = string.Join("&", fields) + "&Box=" + Uri.EscapeDataString("\"><b>") + "&Go=Go";

            var response = _host.Process("Default.aspx", string.Empty, "POST", body);
            Assert.Equal(500, response.Status);
            Assert.Contains("HttpRequestValidationException", response.Body);
        }

        [Fact]
        public void Post_EmptyOverExistingValue_ClearsValue()
        {
            var html = Post(Get("init=17:30"), string.Empty);
            Assert.Equal("valid=True;value=null;text=", Result(html));
            Assert.DoesNotContain("value=", InputTag(html));
            Assert.True(Changed(html));
        }

        [Fact]
        public void Post_UnchangedValue_DoesNotRaiseTimeValueChanged()
        {
            var first = Post(Get(), "09:30");
            Assert.True(Changed(first));

            var second = Post(first, "09:30");
            Assert.Equal("valid=True;value=09:30;text=09:30", Result(second));
            Assert.False(Changed(second));
        }

        [Fact]
        public void Post_ValueSurvivesViewStateRoundTrips()
        {
            var html = Post(Get(), "23:59");
            html = Post(html, "23:59");
            html = Post(html, "24:00");
            Assert.Equal("valid=True;value=24:00;text=24:00", Result(html));
            Assert.True(Changed(html));
        }

        [Fact]
        public void Post_InvalidThenCorrected_BecomesValid()
        {
            var html = Post(Get(), "25:00");
            Assert.StartsWith("valid=False", Result(html));

            html = Post(html, "23:00");
            Assert.Equal("valid=True;value=23:00;text=23:00", Result(html));
        }
    }
}
