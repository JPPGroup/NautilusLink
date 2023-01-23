using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Jpp.Ironstone.Core.UI;
using Microsoft.Extensions.DependencyInjection;
using TLS.Nautilus.Api;
using TLS.NautilusLink.Properties;
using TLS.NautilusLink.ViewModels;
using TLS.NautilusLink.Views;

namespace TLS.NautilusLink
{
    public partial class SiteDesignerLink
    {
        private AuthWrapper _wrapper;

        public SiteDesignerLink(AuthWrapper wrapper)
        {
            _wrapper = wrapper;
        }
        
        public RibbonPanel BuildUi()
        {
            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = Resources.SiteDesignerLink_TabPanel_Text };
            RibbonRowPanel row = new RibbonRowPanel();
            RibbonRowPanel stack = new RibbonRowPanel { IsTopJustified = true };

            RibbonButton browserLink = UIHelper.CreateButton(Resources.SiteDesignerLink_BrowserLinkButton_Text, Resources.sitebrowser, RibbonItemSize.Large, "NAUT_OpenSiteDesigner", () => _wrapper.Authenticated);
            row.Items.Add(browserLink);

            RibbonButton setSite = UIHelper.CreateButton(Resources.SiteDesignerLink_SetSiteButton_Text, Resources.pin, RibbonItemSize.Standard, "NAUT_SetSite", () => _wrapper.Authenticated);
            stack.Items.Add(setSite);
            stack.Items.Add(new RibbonRowBreak());

            RibbonButton forceSync = UIHelper.CreateButton(Resources.SiteDesignerLink_ForceSyncButton_Text, Resources.sync, RibbonItemSize.Standard, "NAUT_ForceSync", () => _wrapper.Authenticated);
            stack.Items.Add(forceSync);
            stack.Items.Add(new RibbonRowBreak());

            row.Items.Add(stack);
            source.Items.Add(row);
            panel.Source = source;
            return panel;
        }
        
        [CommandMethod("NAUT_OpenSiteDesigner")]
        public static void OpenSiteDesigner()
        {
            string url;
            
            Guid? site = ReadSite();
            if (site.HasValue)
            {
                url = NautilusLinkExtensionApplication._current._provider.GetRequiredService<ISiteClient>().GetSiteUrl(site.Value);
            }
            url = NautilusLinkExtensionApplication._current._provider.GetRequiredService<ISiteClient>().GetUrl();
            System.Diagnostics.Process.Start(url);
        }
        
        [CommandMethod("NAUT_SetSite")]
        public static async void SetSite()
        {
            ISiteClient client = NautilusLinkExtensionApplication._current._provider.GetRequiredService<ISiteClient>();

            if (ReadSite().HasValue)
            {
                Application.DocumentManager.CurrentDocument.Editor.WriteMessage("Document is already bound to a site\n");
                return;
            }

            SiteSelectViewModel vm = new SiteSelectViewModel();
            vm.Definitions = await client.GetSiteDefinitionsAsync();

            SiteSelectView siteSelect = new SiteSelectView();
            siteSelect.DataContext = vm;
            Application.ShowModalWindow(Application.MainWindow.Handle, siteSelect, false);
            
            WriteSite(vm.Selected.Id);
        }

        private static void WriteSite(Guid site)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Transaction tr = doc.TransactionManager.TopTransaction;

                // Find the NOD in the database
                DBDictionary nod = (DBDictionary) tr.GetObject(doc.Database.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // We use Xrecord class to store data in Dictionaries
                Xrecord plotXRecord = new Xrecord();
                ResultBuffer rb = new ResultBuffer();

                TypedValue tv = new TypedValue((int)DxfCode.Text, site.ToString());
                rb.Add(tv);
                plotXRecord.Data = rb;

                // Create the entry in the Named Object Dictionary
                string id = typeof(SiteDesignerLink).FullName + "SiteId";
                nod.SetAt(id, plotXRecord);
                tr.AddNewlyCreatedDBObject(plotXRecord, true);
                trans.Commit();
            }
        }
        
        private static Guid? ReadSite()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                // Find the NOD in the database
                DBDictionary nod = (DBDictionary)trans.GetObject(doc.Database.NamedObjectsDictionaryId, OpenMode.ForRead);
                string id = typeof(SiteDesignerLink).FullName + "SiteId";

                if (nod.Contains(id))
                {
                    ObjectId objId = nod.GetAt(id);
                    Xrecord XRecord = (Xrecord)trans.GetObject(objId, OpenMode.ForRead);
                    foreach (TypedValue value in XRecord.Data)
                    {
                        if (value.TypeCode == (short) DxfCode.Text)
                        {
                            string castValue = (string) (value.Value);
                            return Guid.Parse(castValue);
                        }
                    }
                }
            }

            return null;
        }
        
        [CommandMethod("NAUT_ForceSync")]
        public static async void ForceSync()
        {
            ISiteClient client = NautilusLinkExtensionApplication._current._provider.GetRequiredService<ISiteClient>();
            Sync(client, Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument);
        }
    }
}
