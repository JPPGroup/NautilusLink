using System;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.UI;
using TLS.NautilusLink;
using TLS.NautilusLink.Properties;
using Unity;

[assembly: ExtensionApplication(typeof(NautilusLinkExtensionApplication))]

namespace TLS.NautilusLink
{
    public class NautilusLinkExtensionApplication : IIronstoneExtensionApplication
    {
        private RibbonButton _loginState;
        private AuthWrapper _auth;
        private System.Threading.SynchronizationContext _syncContext;

        private static NautilusLinkExtensionApplication _current;
        
        public void Initialize()
        {
            CoreExtensionApplication._current.RegisterExtension(this);
            _auth = new AuthWrapper();
            _syncContext = SynchronizationContext.Current;
            _current = this;
            
            Task.Run(async () =>
            {
                await _auth.SilentAuthAsync();
            });
            _auth.AuthenticationStateChanged += AuthOnAuthenticationStateChanged;
        }

        private void AuthOnAuthenticationStateChanged(object sender, EventArgs e)
        {
            _syncContext.Post(state =>
            {
                if (_auth.Authenticated)
                {
                    _loginState.Text = Resources.ExtensionApplication_LoginStateButton_LogoutText;
                }
                else
                {
                    _loginState.Text = Resources.ExtensionApplication_LoginStateButton_LoginText;
                }
            }, null);
        }

        public void Terminate()
        {
            
        }

        public void InjectContainer(IUnityContainer container)
        {
            
        }

        public void CreateUI()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            RibbonTab _nautilusTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_Tab_Name,
                Title = Resources.ExtensionApplication_Tab_Name,
                Id = "NAUT"
            };
            
            rc.Tabs.Add(_nautilusTab);
            
            
            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = "Site Designer" };
            RibbonRowPanel stack = new RibbonRowPanel();

            /*RibbonToggleButton aboutButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_AboutWindow_Name, Resources.About, RibbonItemSize.Standard, _container.Resolve<About>(), "10992236-c8f6-4732-b5e0-2d9194f07068");
            RibbonButton feedbackButton = UIHelper.CreateButton(Resources.ExtensionApplication_UI_BtnFeedback, Resources.Feedback, RibbonItemSize.Standard, "Core_Feedback");
            RibbonToggleButton reviewButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_ReviewWindow_Name, Resources.Review, RibbonItemSize.Large, _container.Resolve<Review>(), "18cd4414-8fc8-4978-9e97-ae3915e29e07");
            RibbonToggleButton libraryButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_LibraryWindow_Name, Resources.Library_Small, RibbonItemSize.Standard, _container.Resolve<Libraries>(), "08ccb73d-6e6b-4ea0-8d99-61bbeb7c20af");

            RibbonRowPanel column = new RibbonRowPanel { IsTopJustified = true };
            column.Items.Add(aboutButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(feedbackButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(libraryButton);
            
            stack.Items.Add(column);
            stack.Items.Add(reviewButton);*/

            _loginState = UIHelper.CreateButton(Resources.ExtensionApplication_LoginStateButton_LoginText, Resources.loginstate, RibbonItemSize.Large, "NAUT_Login");
            stack.Items.Add(_loginState);
            
            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            _nautilusTab.Panels.Add(panel);
        }

        [CommandMethod("NAUT_Login")]
        public static void Login()
        {
            Task.Run(async () => await _current._auth.InteractiveAuthAsync());
        }
    }
}
