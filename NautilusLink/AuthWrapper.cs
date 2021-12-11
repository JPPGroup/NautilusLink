using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using TLS.Nautilus.Api;

namespace TLS.NautilusLink
{
    public class AuthWrapper 
    {
        public static string Tenant = "nautilustl.onmicrosoft.com";
        public static string AzureADB2CHostname = "nautilustl.b2clogin.com";
        public static string ClientID = "84a1236c-9109-4f29-882b-1711abf9ce0a";
        public static string PolicySignUpSignIn = "B2C_1_signupsignin1";
        /*public static string PolicyEditProfile = "b2c_1_edit_profile";
        public static string PolicyResetPassword = "b2c_1_reset";*/

        public static string AuthorityBase = $"https://{AzureADB2CHostname}/tfp/{Tenant}/";
        public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
        /*public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
        public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";*/
        public static string[] scopes = {"https://nautilustl.onmicrosoft.com/70134f93-251d-46fa-8424-c84041a93c1b/Api.Access"};

        public bool Authenticated { get; set; }
        public string AccessToken { get; private set; }
        
        public event EventHandler AuthenticationStateChanged;
        
        private IPublicClientApplication _application;
        private MsalCacheHelper _cache;

        public AuthWrapper()
        {
             _application = PublicClientApplicationBuilder.Create(ClientID).WithB2CAuthority(Authority).WithRedirectUri("http://localhost").Build();
             //_application = PublicClientApplicationBuilder.Create(ClientID).WithB2CAuthority(Authority).Build();
        }

        public async Task<bool> SilentAuthAsync()
        {
            await BuildCache();
            
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await _application.GetAccountsAsync(PolicySignUpSignIn);
            IAccount account = accounts.FirstOrDefault();
            try
            {
                authResult = await _application.AcquireTokenSilent(scopes, account).ExecuteAsync();
                SetAccessToken(authResult.AccessToken);
            }
            catch (MsalUiRequiredException ex)
            {
                Authenticated = false;
            }

            return Authenticated;
        }

        public async Task InteractiveAuthAsync()
        {
            /*IEnumerable<IAccount> accounts = await _application.GetAccountsAsync(PolicySignUpSignIn);
            IAccount account = accounts.FirstOrDefault();*/

            try
            {
                var authResult = await _application.AcquireTokenInteractive(scopes).WithUseEmbeddedWebView(false).ExecuteAsync();
                SetAccessToken(authResult.AccessToken);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task BuildCache()
        {
            if (_cache == null)
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                StorageCreationPropertiesBuilder builder = new StorageCreationPropertiesBuilder("nautiluscredentials.dat", path);
                _cache = await MsalCacheHelper.CreateAsync(builder.Build());
                ITokenCache userCache =_application.UserTokenCache;
                _cache.RegisterCache(userCache);
            }
        }
        
        private void SetAccessToken(string token)
        {
            AccessToken = token;
            NautilusApi.SetToken(AccessToken);
            
            Authenticated = true;
            AuthenticationStateChanged?.Invoke(this, null);
        }
    }
}
