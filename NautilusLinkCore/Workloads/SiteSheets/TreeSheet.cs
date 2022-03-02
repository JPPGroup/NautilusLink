using Jpp.Ironstone.DocumentManagement.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore.Workloads.SiteSheets
{
    public static  class TreeSheet
    {
        public static LayoutSheet CreateTreeSheet(this LayoutSheetController controller)
        {
            var sheet = controller.AddLayout("002", PaperSize.A1Landscape);
            sheet.TitleBlock.Title = "Tree Layout";
            sheet.TitleBlock.DrawingNumber = "002";            

            //Set up page

            return sheet;
        }
    }
}
