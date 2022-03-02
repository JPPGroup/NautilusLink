using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore
{
    internal static class SupportFiles
    {
        public static void CopyPlotFiles()
        {
            // Get the current document and database, and start a transaction            
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            //A way to copy files from bundle package to respective Printer Support Path
            object roamablePath = Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("ROAMABLEROOTPREFIX");            
            string ctbFile = HostApplicationServices.Current.FindFile("JPP standard.ctb", acCurDb, FindFileHint.Default);            
                        
            string pmpFolder = "PMP Files";
            string pc3Folder = "Plotters";
            string plotStylesFolder = "Plot Styles";

            try
            {
                File.Copy(ctbFile, Path.Combine(roamablePath.ToString(), pc3Folder, Path.GetFileName(ctbFile)));
            }
            catch (System.Exception ex)
            {
                acDoc.Editor.WriteMessage("\n" + ex.Message);
            }
        }
    }
}
