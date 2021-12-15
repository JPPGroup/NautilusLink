using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using TLS.Nautilus.Api.Shared.DataStructures;

namespace TLS.NautilusLinkCore.Interop
{
    internal static class SiteInterop
    {
        public static void ConvertToIronstone(this Site site, Database target)
        {
            site.Trees.ConvertTressToIronstone(target);
        }

        public static Site ConvertFromIronstone(this Database database)
        {
            throw new NotImplementedException();

            Site site = new Site();
            site.Trees = database.ConvertTreesFromIronstone();
        }
    }
}
