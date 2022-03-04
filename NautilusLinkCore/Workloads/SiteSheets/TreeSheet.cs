using Jpp.Ironstone.DocumentManagement.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore.Workloads.SiteSheets
{
    internal static  class TreeSheet
    {
        internal static LayoutSheet CreateTreeSheet(this LayoutSheetController controller, string ProjectName, string ProjectNumber)
        {
            var sheet = controller.AddLayout("002 - Trees", PaperSize.A1Landscape);
            sheet.TitleBlock.Title = "Tree Layout";
            sheet.TitleBlock.DrawingNumber = "002";            

            sheet.TitleBlock.ProjectNumber = ProjectNumber;
            sheet.TitleBlock.Project = ProjectName;

            //Set up page

            return sheet;
        }
    }
}
