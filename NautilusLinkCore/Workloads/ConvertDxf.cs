using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore.Workloads
{
    internal class ConvertDxf
    {
        ILogger _logger;
        Document _target;

        public ConvertDxf(ILogger<IWorkload> logger)
        {
            _logger = logger;
            _target = Application.DocumentManager.MdiActiveDocument;
        }

        [CommandMethod("NAUT_CONVERTDXF")]
        [IronstoneCommand]
        public static void ConvertDxfCommand()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ILogger<IWorkload> logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<IWorkload>>();
#pragma warning restore CS0618 // Type or member is obsolete

            try
            {
                ConvertDxf converter = new ConvertDxf(logger);
                converter.Run().GetAwaiter().GetResult();
            }
            catch (System.Exception e)
            {
                logger.LogCritical(e, "General command failure");
                throw;
            }
        }

        public async Task Run()
        {
            string t = Directory.GetCurrentDirectory();

            _logger.LogDebug($"Current working directory is {Directory.GetCurrentDirectory()}");

            if (CoreExtensionApplication.ForgeDesignAutomation)
                SupportFiles.CopyPlotFiles();

            _logger.LogInformation($"Exporting dxf...");
            _target.Database.DxfOut("out.dxf", 16, DwgVersion.Current);
            _logger.LogInformation($"Dxf exported");
        }
    }
}
