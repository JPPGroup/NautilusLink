using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using System;
using System.Collections.Generic;
using System.Linq;
using NTree = TLS.Nautilus.Api.Shared.DataStructures.Tree;
using ITree = Jpp.Ironstone.Structures.ObjectModel.TreeRings.Tree;
using TLS.Nautilus.Api.Shared.DataStructures;
using Microsoft.Extensions.Logging;

namespace TLS.NautilusLinkCore.Interop
{
    internal static class TreeInterop
    {       
        public static void ConvertTreesToIronstone(this IEnumerable<NTree> trees, Document target, ILogger logger)
        {
            TreeRingManager ringManager = DataService.Current.GetStore<StructureDocumentStore>(target.Name).GetManager<TreeRingManager>();
            NautilusDocumentStore nautilus = DataService.Current.GetStore<NautilusDocumentStore>(target.Name);

            //List<NTree> trees = new List<NTree>();

            logger.LogDebug($"Converting trees, {trees.Count()} found ");
            foreach (NTree tree in trees)
            {
                ITree itree = tree.ConvertToIronstone();
                itree.Generate();
                itree.AddLabel();

                ringManager.Add(itree);

                //TODO: Add mapping code for n to i trees
                /*
                if (nautilus.TreeMappings.Any(mapping => mapping.Key == tree.BaseObjectPtr))
                {
                    itree.Id = nautilus.TreeMappings.First(mapping => mapping.Key == tree.BaseObjectPtr).Value;
                }
                else
                {
                    itree.Id = Guid.NewGuid();
                    nautilus.TreeMappings.Add(new KeyValuePair<long, Guid>(tree.BaseObjectPtr, ntree.Id));
                }*/                
            }

            logger.LogDebug($"Conversion finished, {ringManager.ManagedObjects.Count} trees added");
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
            //TODO:FInish conversion
            throw new NotImplementedException();
            NTree nt = new NTree();
            nt.TreeReference = tree.ID;
            nt.Species = ToNautilusSpecies(tree.Species);            
            return nt;
        }

        public static ITree ConvertToIronstone(this NTree tree)
        {
            ITree it = new ITree();
            ToIronstoneSpecies(tree.Species, it);
            it.ID = tree.TreeReference;
            it.ActualHeight = (float)tree.Height;
            it.Location = new Autodesk.AutoCAD.Geometry.Point3d(tree.Location.X, tree.Location.Y, 0);
            
            switch (tree.Phase)
            {
                case Nautilus.Api.Shared.DataStructures.Phase.Proposed:
                    it.Phase = Jpp.Ironstone.Structures.ObjectModel.TreeRings.Phase.Proposed;
                    break;

                case Nautilus.Api.Shared.DataStructures.Phase.Existing:
                    it.Phase = Jpp.Ironstone.Structures.ObjectModel.TreeRings.Phase.Existing;
                    break;
            }


            return it;            
        }


        //TODO: Add remaining species conversions
        private static TreeSpecies ToNautilusSpecies(string treeSpecies)
        {
            return TreeSpecies.CrackWillow;
        }

        //TODO: Add remaining species conversions
        private static void ToIronstoneSpecies(TreeSpecies treeSpecies, ITree tree)
        {
            switch (treeSpecies)
            {
                case TreeSpecies.EnglishElm:
                    tree.Species = "EnglishElm";
                    tree.WaterDemand = Jpp.Ironstone.Structures.ObjectModel.TreeRings.WaterDemand.High;
                    tree.TreeType = Jpp.Ironstone.Structures.ObjectModel.TreeRings.TreeType.Deciduous;
                    break;

                case TreeSpecies.WheatleyElm:
                    tree.Species = "WheatleyElm";
                    tree.WaterDemand = Jpp.Ironstone.Structures.ObjectModel.TreeRings.WaterDemand.High;
                    tree.TreeType = Jpp.Ironstone.Structures.ObjectModel.TreeRings.TreeType.Deciduous;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Tree species not recognized");
            }

           /* public static Dictionary<string, int> DeciduousHigh = new Dictionary<string, int>()
        {
            { "EnglishElm",24 },
            { "WheatleyElm",22 },
            { "WHychElm",18 },
            { "EUcalyptus",18 },
            { "Hawthorn",10 },
            { "ENglishOak",20 },
            { "HOlmOak",16 },
            { "RedOak",24 },
            { "TurkeyOak",24 },
            { "HYbridBlackPoplar",28 },
            { "LombardyPoplar",25 },
            { "WHItePoplar",15 },
            { "CrackWillow",24 },
            { "WEepingWillow",16 },
            { "WHITeWillow",24 },
        }*/
        }
    }
}
