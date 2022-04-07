using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.DocumentManagement.ObjectModel;
using Jpp.Ironstone.Housing.ObjectModel.Detail;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using Microsoft.Extensions.Configuration;
using System;

namespace TLS.NautilusLinkCore.Workloads.SiteSheets
{
    internal static  class PlotSheet
    {
        internal static LayoutSheet? CreatePlotSheet(this LayoutSheetController controller, IConfiguration settings, string ProjectName, string ProjectNumber, DetailPlot plot, string LayoutNumber)
        {
            var sheet = controller.AddLayout($"{LayoutNumber} - {plot.PlotId}", PaperSize.A1Landscape);
            
            BaseSheet.SetTitleBlock(sheet, settings, "plotsheet", ProjectName, ProjectNumber);
            sheet.TitleBlock.DrawingNumber = LayoutNumber;
            sheet.TitleBlock.Title = plot.PlotId;

            //Set up page
            var viewport = sheet.DrawingArea.AddFullViewport();
            viewport.SetLayer(Constants.VIEWPORT_LAYER);
                         
            viewport.FocusOn(settings, plot.GetBoundingBox());

            //Set scale
            sheet.TitleBlock.Scale = "As Noted";

            return sheet;
        }
    }
}
