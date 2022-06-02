using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TLS.NautilusLinkCore.Workloads;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    public class GenerateSiteTests : IronstoneTestFixture
    {
        private string workingDir;

        public GenerateSiteTests() : base(Assembly.GetExecutingAssembly(), typeof(GenerateSiteTests)) { }

        [Test]
        public void RunGeneration()
        {
            Assert.IsTrue(RunTest<bool>(nameof(RunGenerationResident)), "Test run did not complete.");
            string log = LogHelper.GetLogReader().ReadToEnd();
            StringAssert.DoesNotContain("No site export file found.", log);
        }

        public bool RunGenerationResident()
        {
            Debugger.Launch();
            try
            {
                SetWorkingDirectory("rungeneration");

                ILogger<IWorkload> logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<IWorkload>>();
                GenerateSite generateSite = new GenerateSite(logger);
                generateSite.Run().GetAwaiter().GetResult();

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                RestoreWorkingDirectory();
            }
        }

        //TODO: COmplete this test        
        public void RunGenerationNoXrefs()
        {
            Assert.IsTrue(RunTest<bool>(nameof(RunGenerationResident)), "Test run did not complete.");
            string log = LogHelper.GetLogReader().ReadToEnd();
            StringAssert.DoesNotContain("No site export file found.", log);
        }

        private void SetWorkingDirectory(string test)
        {
            workingDir = Directory.GetCurrentDirectory();
            string location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"testoutput\\{test}");
            if (Directory.Exists(location))
            {
                foreach (string dir in Directory.GetDirectories(location))
                {
                    Directory.Delete(dir, true);
                }
                foreach(string file in Directory.GetFiles(location))
                {
                    File.Delete(file);
                }

            }
            else
            {
                Directory.CreateDirectory(location);
            }

            Directory.SetCurrentDirectory(location);
        }

        private void RestoreWorkingDirectory()
        {
            Directory.SetCurrentDirectory(workingDir);
        }
    }
}
