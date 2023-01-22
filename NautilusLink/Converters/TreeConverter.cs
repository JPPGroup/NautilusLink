using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLS.Nautilus.Api.Shared.DataStructures;
using IsTree = Jpp.Ironstone.Structures.ObjectModel.TreeRings.Tree;
using NautTree = TLS.Nautilus.Api.Shared.DataStructures.Tree;

namespace TLS.NautilusLink.Converters
{
    static class TreeConverter
    {
        public static NautTree ConvertToNautilus(this IsTree tree)
        {
            NautTree nt = new Tree();
            nt.TreeReference = tree.ID;
            //TODO: FIx this
            throw new NotImplementedException();
            //nt.Species = tree.Species;
            nt.Id = Guid.NewGuid();
            return nt;
        }
    }
}
