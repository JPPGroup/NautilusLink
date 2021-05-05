using Autodesk.AutoCAD.ApplicationServices;
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
    public class SiteDesignerLink
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
            RibbonRowPanel stack = new RibbonRowPanel();

            RibbonButton browserLink = UIHelper.CreateButton(Resources.SiteDesignerLink_BrowserLinkButton_Text, Resources.sitebrowser, RibbonItemSize.Large, "NAUT_OpenSiteDesigner");
            stack.Items.Add(browserLink);
            
            RibbonButton setSite = UIHelper.CreateButton(Resources.SiteDesignerLink_SetSiteButton_Text, Resources.sitebrowser, RibbonItemSize.Standard, "NAUT_SetSite", () => _wrapper.Authenticated);
            stack.Items.Add(setSite);
            
            source.Items.Add(stack);
            panel.Source = source;
            return panel;
        }
        
        [CommandMethod("NAUT_OpenSiteDesigner")]
        public static void OpenSiteDesigner()
        {
            string url = NautilusLinkExtensionApplication._current._provider.GetRequiredService<ISiteClient>().GetUrl();
            System.Diagnostics.Process.Start(url);
        }
        
        [CommandMethod("NAUT_SetSite")]
        public static async void SetSite()
        {
            ISiteClient client = NautilusLinkExtensionApplication._current._provider.GetRequiredService<ISiteClient>();

            SiteSelectViewModel vm = new SiteSelectViewModel();
            vm.Definitions = await client.GetSiteDefinitionsAsync();

            SiteSelectView siteSelect = new SiteSelectView();
            siteSelect.DataContext = vm;
            Application.ShowModalWindow(Application.MainWindow.Handle, siteSelect, false);
        }
    }
}
