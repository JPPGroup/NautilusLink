using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.DocumentManagement.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using Microsoft.Extensions.Configuration;
using System;

namespace TLS.NautilusLinkCore.Workloads.SiteSheets
{
    internal static  class TreeSheet
    {
        internal static LayoutSheet? CreateTreeSheet(this LayoutSheetController controller, IConfiguration settings, string ProjectName, string ProjectNumber)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            TreeRingManager ringManager = DataService.Current.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();
            if (ringManager.ManagedObjects.Count < 1)
                return null;

            var sheet = controller.AddLayout("002 - Trees", PaperSize.A1Landscape);
            sheet.TitleBlock.Title = settings["nl:treesheet:name"];
            sheet.TitleBlock.DrawingNumber = settings["nl:treesheet:number"];
            sheet.TitleBlock.Revision = "C1";
            sheet.TitleBlock.DrawnBy = "NTL";
            sheet.TitleBlock.CheckedBy = "-";
            sheet.TitleBlock.Date = DateTime.Now.ToString("MMM yy");

            sheet.TitleBlock.ProjectNumber = ProjectNumber;
            sheet.TitleBlock.Project = ProjectName;

            sheet.StatusBlock.Status = StatusBlock.StatusOptions.Construction;
            
            sheet.RevisionBlocks[0].Revision = "C1";
            sheet.RevisionBlocks[0].Description = "Initial CONSTRUCTION issue";
            sheet.RevisionBlocks[0].DrawnBy = "NTL";
            sheet.RevisionBlocks[0].CheckedBy = "-";
            sheet.RevisionBlocks[0].Date = "TBC";

            //Set up page
            sheet.NoteArea.Notes = settings["nl:treesheet:notes"];

            var viewport = sheet.DrawingArea.AddFullViewport();
            viewport.SetLayer(Constants.VIEWPORT_LAYER);
                         
            viewport.FocusOn(settings, ringManager.GetBoundingBox());

            //Set scale
            sheet.TitleBlock.Scale = "1:???";

            return sheet;
        }
    }
}
