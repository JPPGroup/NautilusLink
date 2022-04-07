using Jpp.Ironstone.DocumentManagement.ObjectModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore.Workloads.SiteSheets
{
    internal static class BaseSheet
    {
        public static void SetTitleBlock(LayoutSheet sheet, IConfiguration settings, string sheetname, string ProjectName, string ProjectNumber)
        {
            string title = settings[$"nl:{sheetname}:name"];
            string number = settings[$"nl:{sheetname}:number"];
            
            sheet.TitleBlock.Title = title;
            sheet.TitleBlock.DrawingNumber = number;
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

            sheet.NoteArea.Notes = settings[$"nl:{sheetname}:notes"];
        }
    }
}
