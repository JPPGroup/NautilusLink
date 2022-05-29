using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using TLS.Nautilus.Api.Shared.DataStructures;

namespace TLS.NautilusLinkCore.Interop
{
    internal static class SiteInterop
    {
        public static void ConvertToIronstone(this Site site, Document target, ILogger logger)
        {
            logger.LogInformation("Converting site to ironstone");
            site.Trees.ConvertTreesToIronstone(target, logger);
        }

        public static Site ConvertFromIronstone(this Document doc)
        {
            throw new NotImplementedException();

            Site site = new Site();
            site.Trees = doc.ConvertTreesFromIronstone();
        }
    }
}
