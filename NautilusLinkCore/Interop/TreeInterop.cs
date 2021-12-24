using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using System;
using System.Collections.Generic;
using System.Linq;
using NTree = TLS.Nautilus.Api.Shared.DataStructures.Tree;
using ITree = Jpp.Ironstone.Structures.ObjectModel.TreeRings.Tree;

namespace TLS.NautilusLinkCore.Interop
{
    internal static class TreeInterop
    {       
        public static void ConvertTressToIronstone(this IEnumerable<NTree> trees, Document target)
        {
            TreeRingManager ringManager = DataService.Current.GetStore<StructureDocumentStore>(target.Name).GetManager<TreeRingManager>();
        }

        public static List<NTree> ConvertTreesFromIronstone(this Document doc)
        {
            TreeRingManager treeRingManager = DataService.Current.GetStore<StructureDocumentStore>(doc.Name).GetManager<TreeRingManager>();
            NautilusDocumentStore nautilus = DataService.Current.GetStore<NautilusDocumentStore>(doc.Name);

            List<NTree> trees = new List<NTree>();

            foreach (ITree tree in treeRingManager.ManagedObjects)
            {
                var ntree = tree.ConvertToNautilus();

                if (nautilus.TreeMappings.Any(mapping => mapping.Key == tree.BaseObjectPtr))
                {
                    ntree.Id = nautilus.TreeMappings.First(mapping => mapping.Key == tree.BaseObjectPtr).Value;
                }
                else
                {
                    ntree.Id = Guid.NewGuid();
                    nautilus.TreeMappings.Add(new KeyValuePair<long, Guid>(tree.BaseObjectPtr, ntree.Id));
                }

                trees.Add(ntree);
            }

            return trees;
        }

        public static NTree ConvertToNautilus(this ITree tree)
        {
            NTree nt = new NTree();
            nt.TreeReference = tree.ID;
            nt.Species = tree.Species;            
            return nt;
        }

        public static ITree ConvertToIronstone(this NTree tree)
        {
            ITree it = new ITree();
            it.Species = tree.Species;
            it.ID = tree.TreeReference;

            return it;            
        }
    }
}
