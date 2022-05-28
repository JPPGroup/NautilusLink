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
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Housing.ObjectModel;
using Jpp.Ironstone.Housing.ObjectModel.Detail;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace TLS.NautilusLinkCore.Workloads
{
    public class GenerateSite : IWorkload
    {
        ILogger _logger;
        Document _target;

        [CommandMethod("NAUT_GENERATESITE")]
        [IronstoneCommand]
        public static void GenerateSiteCommand()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ILogger<IWorkload> logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<IWorkload>>();
#pragma warning restore CS0618 // Type or member is obsolete

            try
            {                   
                GenerateSite generateSite = new GenerateSite(logger);
                generateSite.Run().GetAwaiter().GetResult();                
                
            } catch (System.Exception e)
            {
                logger.LogCritical(e, "General command failure");
                throw;
            }
        }

        public GenerateSite(ILogger<IWorkload> logger)
        {
            _logger = logger;
            _target = Application.DocumentManager.MdiActiveDocument;
        }

        public async Task Run()
        {
            string t = Directory.GetCurrentDirectory();

            _logger.LogDebug($"Current working directory is {Directory.GetCurrentDirectory()}");

            if (CoreExtensionApplication.ForgeDesignAutomation)
                SupportFiles.CopyPlotFiles();

            using (Transaction trans = _target.TransactionManager.StartTransaction())
            {
                Site site = ProcessSite();                

                //Add xrefs
                _logger.LogTrace($"Processing xrefs...");
                ExportXrefs();
                LinkXrefs();

                //Generate Sheets
                _logger.LogTrace($"Generating sheets...");
                var controller = GenerateSheets(site.Name, site.Reference);

                //Plot Sheets            
                _logger.LogTrace($"Removing sheet default layouts...");
                controller.RemoveDefaultLayouts();
                trans.Commit();

                _logger.LogTrace($"Creating plot directory...");
                if (!Directory.Exists("plots"))
                    Directory.CreateDirectory("plots");

                _logger.LogTrace($"Plotting sheets...");
                PlotSheets(controller);
            }

            using (Transaction trans = _target.TransactionManager.StartTransaction())
            {
                _target.Database.SetXrefRelative(t);
                trans.Commit();
            }

            _logger.LogTrace($"Bundling sheets...");
            await BundlePlots().ConfigureAwait(false);

            _logger.LogDebug($"Generate site completed");
        }

        private void ExportXrefs()
        {
            try
            {
                string path1 = "xrefs.zip";
                string path2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "xrefs.zip");
                string path = null;

                if (File.Exists(path2))
                {
                    path = path2;
                }

                if (File.Exists(path1))
                {
                    path = path1;
                }               

                if (path != null)
                {
                    using (var fileStream = new FileStream(path, FileMode.Open))
                    {
                        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                        {
                            archive.ExtractToDirectory("xrefs");
                            _logger.LogTrace($"Xref bundle found and extracted");
                            return;
                        }
                    }
                }
            } catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failure to unbundle xref files");
            }

            _logger.LogTrace($"No xref bundle found.");
        }

        private void LinkXrefs()
        {
            if (!Directory.Exists("xrefs"))
            {
                _logger.LogTrace($"No xref directory found.");
                return;
            }                

            var files = Directory.GetFiles("xrefs");

            if (files.Length == 0)
            {
                _logger.LogTrace($"No xref files found to link.");
                return;
            }

            Database db = _target.Database;

            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (var file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string absPath = Path.Combine(Directory.GetCurrentDirectory(), file);

                    var xId = db.AttachXref(absPath, name);
                    if (xId.IsValid)
                    {
                        BlockTableRecord btr = db.GetModelSpace(true);

                        var br = new BlockReference(Autodesk.AutoCAD.Geometry.Point3d.Origin, xId);
                        btr.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                        _logger.LogTrace($"Linked {name}");
                    }
                }
                tr.Commit();
            }
            _logger.LogTrace($"Linking complete");
        }

        private Site ProcessSite()
        {
            //Import nautilus data
            _logger.LogDebug("Loading site data");
            Site? site = LoadSiteData();

            if (site == null)
            {
                _logger.LogError("Site data not loaded");
                throw new InvalidOperationException("Site data not loaded");
            }

            return site;
        }

        private Site? LoadSiteData()
        {
            if (File.Exists("sitedata.json"))
            {
                return LoadSiteFrom("sitedata.json");
            }

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sitedata.json");
            if (File.Exists(path))
            {
                return LoadSiteFrom(path);
            }

            _logger.LogError("No site export file found.");
            return null;
        }

        private Site LoadSiteFrom(string path)
        {
            string jsonData = File.ReadAllText(path);
            Site? site = JsonSerializer.Deserialize<Site>(jsonData);

            if (site == null)
                throw new InvalidOperationException("Stie data invalid - load failed");

            site.ConvertToIronstone(_target);
            return site;
        }

        private LayoutSheetController GenerateSheets(string ProjectName, string ProjectNumber)
        {
            var coreLogger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<CoreExtensionApplication>>();
            var coreConfig = CoreExtensionApplication._current.Container.GetRequiredService<IConfiguration>();
            LayoutSheetController sheetController = new LayoutSheetController(coreLogger, _target.Database, coreConfig);

            sheetController.CreateTreeSheet(coreConfig, ProjectName, ProjectNumber);

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            DetailPlotManager plotManager = DataService.Current.GetStore<HousingDocumentStore>(acDoc.Name).GetManager<DetailPlotManager>();

            int i = 1;
            foreach (DetailPlot dp in plotManager.ManagedObjects)
            {                
                sheetController.CreatePlotSheet(coreConfig, ProjectName, ProjectNumber, dp, i.ToString());
                i++;
            }

            _logger.LogTrace($"Sheet Controller has {sheetController.Sheets.Count}.");

            return sheetController;
        }

        private void PlotSheets(LayoutSheetController controller)
        {
            _logger.LogTrace($"Preparing to plot {controller.Sheets.Count} sheets");
            try
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
                                string name = Path.Combine(Directory.GetCurrentDirectory(), $"plots\\{sheet.GetPDFName()}");
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
                        _logger.LogTrace($"BACKGROUNDPLOT set to {Convert.ToInt32(Application.GetSystemVariable("BACKGROUNDPLOT"))}");

                    }
                }
            }
            catch (System.Exception e)
            {
                _logger.LogCritical(e, "General plot failure");
                throw;
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

                        _logger.LogTrace($"Saving dwg file");                                                
                        _target.Database.SaveAs("Model.dwg", DwgVersion.Current);
                        archive.CreateEntryFromFile("Model.dwg", "Model.dwg");

                        foreach (string path in Directory.GetFiles("xrefs"))
                        {
                            _logger.LogTrace($"Adding {path} to result archive");
                            archive.CreateEntryFromFile($"{path}", $"xrefs/{Path.GetFileName(path)}");
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
