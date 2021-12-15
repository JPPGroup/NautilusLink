using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLS.Nautilus.Api.Shared.DataStructures;

namespace TLS.NautilusLinkCore.Interop
{
    internal static class TreeInterop
    {       
        public static void ConvertTressToIronstone(this IEnumerable<Tree> trees, Database target)
        {

        }

        public static List<Tree> ConvertTreesFromIronstone(this Database database)
        {
            throw new NotImplementedException();
        }
    }
}
