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
            var sheet = controller.AddLayout("T01", PaperSize.A1Landscape);
            
            //Set up page

            return sheet;
        }
    }
}
