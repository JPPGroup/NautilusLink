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
            Assert.IsTrue(RunTest<bool>(nameof(RunGenerationResident)), "Test run did not complete.");
            string log = LogHelper.GetLogReader().ReadToEnd();
            StringAssert.DoesNotContain("No site export file found.", log);
        }

        public bool RunGenerationResident()
        {
            GenerateSite.GenerateSiteCommand();
            return true;
        }
    }
}
