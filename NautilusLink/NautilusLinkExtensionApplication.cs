using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TLS.Nautilus.Api;
using TLS.NautilusLink;
using TLS.NautilusLink.Properties;

[assembly: ExtensionApplication(typeof(NautilusLinkExtensionApplication))]

namespace TLS.NautilusLink
{
    public class NautilusLinkExtensionApplication : IIronstoneExtensionApplication
    {
        private RibbonButton _loginState;
        private AuthWrapper _auth;
        private System.Threading.SynchronizationContext _syncContext;

        public static NautilusLinkExtensionApplication _current;
        internal IServiceProvider _provider;
        private ISiteClient _siteClient;
        private IConfiguration _settings;

        private SiteDesignerLink _siteDesigner;
        
        public void Initialize()
        {
            CoreExtensionApplication._current.RegisterExtension(this);
            _auth = new AuthWrapper();
            _syncContext = SynchronizationContext.Current;
            _current = this;

            Task.Run(async () =>
            {
                try
                {
                    await _auth.SilentAuthAsync();
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            });
            _auth.AuthenticationStateChanged += AuthOnAuthenticationStateChanged;

            foreach (Document doc in Application.DocumentManager)
            {
                doc.Database.SaveComplete += async (sender, args) => await SiteDesignerLink.SyncToRemote(_siteClient, doc);
            }
            
            Application.DocumentManager.DocumentCreated += (sender, args) => args.Document.Database.SaveComplete += async (o, eventArgs) => await SiteDesignerLink.SyncToRemote(_siteClient, args.Document);
        }

        private void AuthOnAuthenticationStateChanged(object sender, EventArgs e)
        {
            _syncContext.Post(state =>
            {
                if (_auth.Authenticated)
                {
                    _loginState.Text = Resources.ExtensionApplication_LoginStateButton_LogoutText;
                    _loginState.Image = UIHelper.LoadImage(Resources.logout);
                    _loginState.LargeImage = UIHelper.LoadImage(Resources.logout);
                }
                else
                {
                    _loginState.Text = Resources.ExtensionApplication_LoginStateButton_LoginText;
                    _loginState.Image = UIHelper.LoadImage(Resources.loginstate);
                    _loginState.LargeImage = UIHelper.LoadImage(Resources.loginstate);
                    
                }
            }, null);
        }

        public void Terminate()
        {
            
        }

        public void RegisterServices(IServiceCollection container)
        {
            container.AddHttpClient();
            //TODO: Change this to production
            container.AddNautilusApi(options => options.UseStaging().UseHttp());
        }

        public void InjectContainer(IServiceProvider container)
        {
            _provider = container;
            _siteDesigner = new SiteDesignerLink(_auth);
            _siteClient = container.GetRequiredService<ISiteClient>();
            _settings = container.GetRequiredService<IConfiguration>();
        }

        public void CreateUI()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            /*RibbonTab _nautilusTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_Tab_Name,
                Title = Resources.ExtensionApplication_Tab_Name,
                Id = "NAUT"
            };
            
            rc.Tabs.Add(_nautilusTab);*/
            RibbonTab _nautilusTab = rc.FindTab(Jpp.Ironstone.Core.Constants.IronstoneGeneralTabId);
            bool enabled = bool.Parse(_settings["nl:uienable"]);

            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = "Nautilus Account" };
            RibbonRowPanel stack = new RibbonRowPanel();

            _loginState = UIHelper.CreateButton(Resources.ExtensionApplication_LoginStateButton_LoginText, Resources.loginstate, RibbonItemSize.Large, "NAUT_Login", () => enabled);
            stack.Items.Add(_loginState);
            
            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            _nautilusTab.Panels.Add(panel);
            
            _nautilusTab.Panels.Add(_siteDesigner.BuildUi());
        }

        [CommandMethod("NAUT_Login")]
        public static void Login()
        {
            Task.Run(async () => await _current._auth.InteractiveAuthAsync());
        }
    }
}
