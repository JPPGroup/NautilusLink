using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Housing.ObjectModel;
using Jpp.Ironstone.Housing.ObjectModel.Detail;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using TLS.Nautilus.Api;
using TLS.Nautilus.Api.Shared.DataStructures;
using TLS.NautilusLink.Converters;
using Tree = Jpp.Ironstone.Structures.ObjectModel.TreeRings.Tree;

namespace TLS.NautilusLink
{
    public partial class SiteDesignerLink
    {
        public static async Task Sync(ISiteClient client, Document doc)
        {
            Guid? siteid = ReadSite();
            //TODO: Check not null
            
            await SyncToRemote(client, doc, siteid.Value);
            await SyncFromRemote(client, doc, siteid.Value);
        }

        internal static async Task SyncToRemote(ISiteClient client, Document doc, Guid site)
        {
            await SyncSoilPropertiesToRemote(client, doc, site);
            await SyncTreesToRemote(client, doc, site);
            await SyncPlotsToRemote(client, doc, site);
        }
        
        internal static async Task SyncToRemote(ISiteClient client, Document doc)
        {
            Guid? siteid = ReadSite();

            await SyncToRemote(client, doc, siteid.Value);
        }

        internal static async Task SyncFromRemote(ISiteClient client, Document doc, Guid site)
        {
            await SyncSoilPropertiesFromRemote(client, doc, site);
            await SyncTreesFromRemote(client, doc, site);
            await SyncPlotsFromRemote(client, doc, site);
        }
        
        internal static async Task SyncFromRemote(ISiteClient client, Document doc)
        {
            Guid? siteid = ReadSite();

            await SyncFromRemote(client, doc, siteid.Value);
        }

        private static async Task SyncSoilPropertiesToRemote(ISiteClient client, Document doc, Guid site)
        {
            SoilProperties sp = DataService.Current.GetStore<StructureDocumentStore>(doc.Name).SoilProperties;
            Site siteData = await client.GetSiteAsync(site);

            //TODO: Implement checks for existing properties
            
            switch (sp.SoilShrinkability)
            {
                case Shrinkage.High:
                    siteData.Geo.ModifiedPlasticityIndex = 60;
                    break;
                
                case Shrinkage.Medium:
                    siteData.Geo.ModifiedPlasticityIndex = 40;
                    break;
                
                case Shrinkage.Low:
                    siteData.Geo.ModifiedPlasticityIndex = 20;
                    break;
            }

            siteData.Geo.SafeGroundBearingPressure = sp.GroundBearingPressure;

            await client.SaveSiteAsync(siteData);
        }
        
        private static async Task SyncTreesToRemote(ISiteClient client, Document doc, Guid site)
        {
            TreeRingManager treeRingManager = DataService.Current.GetStore<StructureDocumentStore>(doc.Name).GetManager<TreeRingManager>();
            NautilusDocumentStore nautilus = DataService.Current.GetStore<NautilusDocumentStore>(doc.Name);
            Site siteData = await client.GetSiteAsync(site);
            siteData.Trees.Clear();
            
            foreach (Tree tree in treeRingManager.ManagedObjects)
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
                
                siteData.Trees.Add(ntree);
            }
        }

        private static async Task SyncPlotsToRemote(ISiteClient client, Document doc, Guid site)
        {
            //Sync masters
            DetailPlotMasterManager masterManager = DataService.Current.GetStore<HousingDocumentStore>(doc.Name).GetManager<DetailPlotMasterManager>();
            NautilusDocumentStore nautilus = DataService.Current.GetStore<NautilusDocumentStore>(doc.Name);
            
            Site siteData = await client.GetSiteAsync(site);

            foreach (DetailPlotMaster master in masterManager.ManagedObjects)  
            {
            }
        }

        private static async Task SyncSoilPropertiesFromRemote(ISiteClient client, Document doc, Guid site)
        {

        }
        
        private static async Task SyncTreesFromRemote(ISiteClient client, Document doc, Guid site)
        {

        }
        
        private static async Task SyncPlotsFromRemote(ISiteClient client, Document doc, Guid site)
        {
        }
    }
}
