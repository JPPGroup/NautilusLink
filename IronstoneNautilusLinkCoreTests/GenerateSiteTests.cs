using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using TLS.NautilusLinkCore.Workloads;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    public class GenerateSiteTests : IronstoneTestFixture
    {
        public GenerateSiteTests() : base(Assembly.GetExecutingAssembly(), typeof(GenerateSiteTests)) { }

        [Test]
        public void RunGeneration()
        {
            RunTest<bool>(nameof(RunGenerationResident));
            string log = LogHelper.GetLogReader().ReadToEnd();
            StringAssert.Contains("No site export file found.", log);
        }

        public bool RunGenerationResident()
        {
            GenerateSite.GenerateSiteCommand();
            return true;
        }
    }
}
