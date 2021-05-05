using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;
using Microsoft.Identity.Client;

namespace TLS.NautilusLink
{
    class AuthWrapper 
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

        public AuthWrapper()
        {
             _application = PublicClientApplicationBuilder.Create(ClientID).WithB2CAuthority(Authority).WithRedirectUri("http://localhost").Build();
             //_application = PublicClientApplicationBuilder.Create(ClientID).WithB2CAuthority(Authority).Build();
        }

        public async Task<bool> SilentAuthAsync()
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await _application.GetAccountsAsync(PolicySignUpSignIn);
            IAccount account = accounts.FirstOrDefault();
            try
            {
                authResult = await _application.AcquireTokenSilent(scopes, account).ExecuteAsync();
                AccessToken = authResult.AccessToken;
                Authenticated = true;
                AuthenticationStateChanged?.Invoke(this, null);
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
                AccessToken = authResult.AccessToken;
                Authenticated = true;
                AuthenticationStateChanged?.Invoke(this, null);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
