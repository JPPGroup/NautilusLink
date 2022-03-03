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
using System.Reflection;

namespace TLS.NautilusLinkCore.Workloads
{
    public class GenerateSite : IWorkload
    {
        ILogger _logger;
        Document _target;

        [CommandMethod("NAUT_GENERATESITE")]
        [IronstoneCommand]
        public static async void GenerateSiteCommand()
        {
            Debugger.Launch();
            ILogger<IWorkload> logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<IWorkload>>();

            string t = Directory.GetCurrentDirectory();

            logger.LogDebug($"Current working directory is {Directory.GetCurrentDirectory()}");

            SupportFiles.CopyPlotFiles();

            GenerateSite generateSite = new GenerateSite();
            await generateSite.Run(logger);
            logger.LogDebug($"Generate site completed");
        }

        public async Task Run(ILogger<IWorkload> logger)
        {
            _logger = logger;
            _target = Application.DocumentManager.MdiActiveDocument;

            using (Transaction trans = _target.TransactionManager.StartTransaction())
            {
                ProcessSite();

                //Add xrefs

                //Generate Sheets
                logger.LogTrace($"Generating sheets...");
                var controller = GenerateSheets();

                //Plot Sheets            
                controller.RemoveDefaultLayouts();
                trans.Commit();

                logger.LogTrace($"Creating plot directory...");
                if (!Directory.Exists("plots"))
                    Directory.CreateDirectory("plots");

                logger.LogTrace($"Plotting sheets...");
                PlotSheets(controller);
            }

            logger.LogTrace($"Bundling sheets...");
            await BundlePlots();
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
        }

        private Site? LoadSiteData()
        {
            if (File.Exists("sitedata.json"))
            {
                string jsonData = File.ReadAllText("sitedata.json");
                Site site = JsonSerializer.Deserialize<Site>(jsonData);
                site.ConvertToIronstone(_target);

                return site;                
            }

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sitedata.json");
            if (File.Exists(path))
            {
                string jsonData = File.ReadAllText(path);
                Site site = JsonSerializer.Deserialize<Site>(jsonData);
                site.ConvertToIronstone(_target);

                return site;
            }

            _logger.LogError("No site export file found.");
            return null;
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
                    try
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
                                    _logger.LogTrace($"Plotted {name}");
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
                    } catch (System.Exception e)
                    {
                        _logger.LogCritical(e, "General plot failure");
                        throw;
                    }
                }
            }
        }

        private async Task BundlePlots()
        {
            /*await Task.Run(() =>
            {
                try
                {
                    if (File.Exists("Results.zip"))
                        File.Delete("Results.zip");

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
                } catch (System.Exception e)
                {
                    _logger.LogCritical(e, "General zip failure");
                }
            });    */
            try
            {
                if (File.Exists("Results.zip"))
                    File.Delete("Results.zip");

                using (var fileStream = new FileStream("Results.zip", FileMode.CreateNew))
                {
                    using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        foreach (string path in Directory.GetFiles("plots"))
                        {
                            _logger.LogTrace($"Adding {path} to result archive");
                            archive.CreateEntryFromFile(path, Path.GetFileName(path));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                _logger.LogCritical(e, "General zip failure");
                throw;
            }
        }
    }
}
