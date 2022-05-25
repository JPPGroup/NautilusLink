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

            ringManager.UpdateAll();

            var sheet = controller.AddLayout("002 - Trees", PaperSize.A1Landscape);
            
            BaseSheet.SetTitleBlock(sheet, settings, "treesheet", ProjectName, ProjectNumber);

            //Set up page
            var viewport = sheet.DrawingArea.AddFullViewport();
            viewport.SetLayer(Constants.VIEWPORT_LAYER);
                         
            viewport.FocusOn(settings, ringManager.GetBoundingBox());

            //Set scale
            sheet.TitleBlock.Scale = "1:???";

            return sheet;
        }
    }
}
