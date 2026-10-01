using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace EndOfDayTime.WebForms.Tests
{
    /// <summary>
    /// Runs .aspx pages through the real ASP.NET pipeline (System.Web) in a separate
    /// AppDomain, without IIS. Lives inside the web application's AppDomain.
    /// </summary>
    public class AspNetTestHost : MarshalByRefObject
    {
        public override object InitializeLifetimeService() => null;

        public HostResponse Process(string page, string query, string method, string body)
        {
            using (var output = new StringWriter())
            using (var done = new ManualResetEvent(false))
            {
                var request = new TestWorkerRequest(page, query ?? string.Empty, output, method, body, done);
                HttpRuntime.ProcessRequest(request);
                if (!done.WaitOne(TimeSpan.FromSeconds(60)))
                    throw new TimeoutException("ASP.NET did not finish the request.");
                return new HostResponse { Status = request.Status, Body = output.ToString() };
            }
        }

        public void Shutdown() => HttpRuntime.UnloadAppDomain();

        /// <summary>
        /// Creates a web application in <paramref name="siteDirectory"/> containing the
        /// given pages, with the control assemblies copied to its bin folder.
        /// </summary>
        public static AspNetTestHost Create(string siteDirectory, params (string name, string content)[] files)
        {
            var testBin = Path.GetDirectoryName(new Uri(typeof(AspNetTestHost).Assembly.CodeBase).LocalPath);
            var siteBin = Path.Combine(siteDirectory, "bin");
            Directory.CreateDirectory(siteBin);
            Directory.CreateDirectory(Path.Combine(siteDirectory, "aspnet-temp"));

            foreach (var pattern in new[] { "EndOfDayTime*.dll", "System.*.dll", "Microsoft.Bcl*.dll", "xunit*.dll" })
                foreach (var dll in Directory.GetFiles(testBin, pattern))
                    File.Copy(dll, Path.Combine(siteBin, Path.GetFileName(dll)), true);

            File.WriteAllText(Path.Combine(siteDirectory, "Web.config"), BuildWebConfig(testBin, siteDirectory));
            foreach (var file in files)
                File.WriteAllText(Path.Combine(siteDirectory, file.name), file.content, Encoding.UTF8);

            return (AspNetTestHost)ApplicationHost.CreateApplicationHost(
                typeof(AspNetTestHost), "/", siteDirectory);
        }

        private static string BuildWebConfig(string testBin, string siteDirectory)
        {
            // Reuse the binding redirects MSBuild generated for the test assembly:
            // a real application gets the same ones from NuGet.
            var runtime = string.Empty;
            var testConfig = Path.Combine(testBin, "EndOfDayTime.WebForms.Tests.dll.config");
            if (File.Exists(testConfig))
            {
                var element = XDocument.Load(testConfig).Root.Elements("runtime").FirstOrDefault();
                if (element != null) runtime = element.ToString();
            }

            var temp = Path.Combine(siteDirectory, "aspnet-temp");
            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<configuration>\n" +
                "  <system.web>\n" +
                "    <compilation debug=\"true\" targetFramework=\"4.8\" tempDirectory=\"" + temp + "\">\n" +
                "      <assemblies>\n" +
                "        <add assembly=\"netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51\" />\n" +
                "      </assemblies>\n" +
                "    </compilation>\n" +
                "    <httpRuntime targetFramework=\"4.8\" />\n" +
                "    <customErrors mode=\"Off\" />\n" +
                "    <pages controlRenderingCompatibilityVersion=\"4.0\" clientIDMode=\"AutoID\">\n" +
                "      <controls>\n" +
                "        <add tagPrefix=\"eodt\" namespace=\"EndOfDayTime.WebForms\" assembly=\"EndOfDayTime.WebForms\" />\n" +
                "      </controls>\n" +
                "    </pages>\n" +
                "  </system.web>\n" +
                "  " + runtime + "\n" +
                "</configuration>\n";
        }

        private sealed class TestWorkerRequest : SimpleWorkerRequest
        {
            private readonly string _method;
            private readonly byte[] _body;
            private readonly ManualResetEvent _done;

            public int Status { get; private set; } = 200;

            public TestWorkerRequest(string page, string query, TextWriter output,
                string method, string body, ManualResetEvent done)
                : base(page, query, output)
            {
                _method = method;
                _body = body == null ? null : Encoding.UTF8.GetBytes(body);
                _done = done;
            }

            public override string GetHttpVerbName() => _method;

            public override string GetKnownRequestHeader(int index)
            {
                if (_body != null)
                {
                    if (index == HeaderContentLength) return _body.Length.ToString();
                    if (index == HeaderContentType) return "application/x-www-form-urlencoded";
                }
                return base.GetKnownRequestHeader(index);
            }

            public override byte[] GetPreloadedEntityBody() => _body;

            public override bool IsEntireEntityBodyIsPreloaded() => true;

            public override void SendStatus(int statusCode, string statusDescription)
            {
                Status = statusCode;
                base.SendStatus(statusCode, statusDescription);
            }

            public override void EndOfRequest()
            {
                base.EndOfRequest();
                _done.Set();
            }
        }
    }

    [Serializable]
    public class HostResponse
    {
        public int Status { get; set; }
        public string Body { get; set; }
    }
}
