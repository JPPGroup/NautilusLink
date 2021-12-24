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
using System.Threading.Tasks;
using System.IO.Compression;
using Jpp.Ironstone.DocumentManagement.ObjectModel;
using Microsoft.Extensions.Configuration;
using System;
using TLS.NautilusLinkCore.Workloads.SiteSheets;
using Autodesk.AutoCAD.PlottingServices;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices;

namespace TLS.NautilusLinkCore.Workloads
{
    public class GenerateSite : IWorkload
    {
        ILogger _logger;
        Document _target;

        public async Task Run(ILogger<IWorkload> logger)
        {
            _logger = logger;
            _target = Application.DocumentManager.MdiActiveDocument;

            using (Transaction trans = _target.TransactionManager.StartTransaction())
            {
                ProcessSite();

            //Add xrefs

            //Generate Sheets            
                var controller = GenerateSheets();

                //Plot Sheets            
                controller.RemoveDefaultLayouts();
                trans.Commit();

                PlotSheets(controller);
            }          
            
            await BundlePlots();
        }        

        [CommandMethod("NAUT_GENERATESITE")]
        [IronstoneCommand]
        public static async void GenerateSiteCommand()
        {
            Debugger.Launch();
            ILogger<IWorkload> logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<IWorkload>>();

            string t = Directory.GetCurrentDirectory();

            logger.LogDebug($"Current working directory is {Directory.GetCurrentDirectory()}");

            GenerateSite generateSite = new GenerateSite();
            await generateSite.Run(logger);
        }
                
        private void ProcessSite()
        {
            //Import nautilus data
            _logger.LogDebug("Loading site data");
            Site site = LoadSiteData();

            if (site == null)
            {
                _logger.LogError("Site data not loaded");
                throw new InvalidOperationException("Site data not loaded");
            }

            site.Trees.ConvertTressToIronstone(_target);
        }
        private Site? LoadSiteData()
        {
            if (!File.Exists("nautilusexport.json"))
            {
                _logger.LogError("No site export file found.");
                return null;
            }

            string jsonData = File.ReadAllText("nautilusexport.json");
            Site site = JsonSerializer.Deserialize<Site>(jsonData);
            site.ConvertToIronstone(_target);

            return site;
        }

        private LayoutSheetController GenerateSheets()
        {
            var coreLogger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<CoreExtensionApplication>>();
            var coreConfig = CoreExtensionApplication._current.Container.GetRequiredService<IConfiguration>();
            LayoutSheetController sheetController = new LayoutSheetController(coreLogger, _target.Database, coreConfig);

            sheetController.CreateTreeSheet();

            return sheetController;
        }

        private void PlotSheets(LayoutSheetController controller)
        {
            using (PlotEngine pe = PlotFactory.CreatePublishEngine())
            {
                using (PlotProgressDialog ppd = new PlotProgressDialog(false, 1, true))
                {
                    int bpValue = Convert.ToInt32(Application.GetSystemVariable("BACKGROUNDPLOT"));
                    Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                    _logger.LogTrace($"BACKGROUNDPLOT set to {Convert.ToInt32(Application.GetSystemVariable("BACKGROUNDPLOT"))}");

                    ppd.OnBeginPlot();
                    ppd.IsVisible = false;
                    pe.BeginPlot(ppd, null);

                    List<string> expectedFiles = new List<string>();

                    using (Application.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        foreach (LayoutSheet sheet in controller.Sheets.Values)
                        {
                            string name = $"plots\\{sheet.GetPDFName()}";
                            try
                            {
                                sheet.Plot(name, pe, ppd);
                                expectedFiles.Add(name);
                            }
                            catch (System.Exception e)
                            {
                                _logger.LogError(e, $"{name} failed to plot.");
                            }
                        }
                    }

                    ppd.PlotProgressPos = 100;
                    ppd.OnEndPlot();
                    pe.EndPlot(null);

                    Application.SetSystemVariable("BACKGROUNDPLOT", bpValue);
                }
            }
        }

        private async Task BundlePlots()
        {
            await Task.Run(() =>
            {
                using (var fileStream = new FileStream("Results.zip", FileMode.CreateNew))
                {
                    using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        foreach (string path in Directory.GetFiles("plots"))
                        {
                            archive.CreateEntryFromFile(path, Path.GetFileName(path));
                        }
                    }
                }
            });            
        }
    }
}
