using Jpp.Ironstone.DocumentManagement.ObjectModel;
using System;

namespace TLS.NautilusLinkCore.Workloads.SiteSheets
{
    internal static  class TreeSheet
    {
        internal static LayoutSheet CreateTreeSheet(this LayoutSheetController controller, string ProjectName, string ProjectNumber)
        {
            var sheet = controller.AddLayout("002 - Trees", PaperSize.A1Landscape);
            sheet.TitleBlock.Title = "Tree Layout";
            sheet.TitleBlock.DrawingNumber = "002";
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

            //Set scale
            sheet.TitleBlock.Scale = "1:???";

            return sheet;
        }
    }
}
