using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TLS.NautilusLinkCore.Interop;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices.Core;

using Site = TLS.Nautilus.Api.Shared.DataStructures.Site;

namespace TLS.NautilusLinkCore.Workloads
{
    public class GenerateSite : IWorkload
    {
        ILogger _logger;

        public void Run(ILogger logger)
        {
            _logger = logger;

            Database target = Application.DocumentManager.MdiActiveDocument.Database;

            //Import nautilus data
            logger.LogDebug("Loading site data");
            Site site = LoadSiteData();

            if(site == null)
            {
                _logger.LogError("Site data not loaded");
                return;
            }

            site.Trees.ConvertTressToIronstone(target);
            
            //Add xrefs
            
            //Generate Sheets
            
            //Plot Sheets            
        }

        [CommandMethod("NAUT_GENERATESITE")]
        [IronstoneCommand]
        public static void GenerateSiteCommand()
        {
            ILogger logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger>();

            GenerateSite generateSite = new GenerateSite();
            generateSite.Run(logger);
        }

        private Site? LoadSiteData()
        {
            if (File.Exists("nautilusexport.json"))
            {
                _logger.LogError("No site export file found.");
                return null;
            }

            string jsonData = File.ReadAllText("nautilusexport.json");
            Site site = JsonSerializer.Deserialize<Site>(jsonData);

            return site;
        }
    }
}
