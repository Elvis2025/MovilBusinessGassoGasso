using Microsoft.Identity.Client;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Model.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace MovilBusiness.services
{
    public class MicrosoftAuthServices //: IMicrosoftAuthService
    {
        private readonly string ClientID = "a1c98f46-d140-49ef-8d18-91202a65b4a9";
        private readonly string[] Scopes = { "api://a1c98f46-d140-49ef-8d18-91202a65b4a9/User.read" };


        private IPublicClientApplication IdentityClient;

        //[MsalUiRequiredExceptionFilter(Scopes = new [] { "api://a1c98f46-d140-49ef-8d18-91202a65b4a9/User.read" })]
        public async Task<AuthenticationToken> GetAuthenticationToken()
        {
            if (IdentityClient == null)
            {
                IdentityClient = Arguments.PlatformService.GetIdentityClient(ClientID);
            }

            var accounts = await IdentityClient.GetAccountsAsync();
            AuthenticationResult result = null;
            bool tryInteractiveLogin = false;

            try
            {
                result = await IdentityClient
                    .AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException e)
            {
                Console.Write(e.Message);
                tryInteractiveLogin = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSAL Silent Error: {ex.Message}");
            }

            if (tryInteractiveLogin)
            {
                try
                {
                    result = await IdentityClient
                        .AcquireTokenInteractive(Scopes)
                        .WithUseEmbeddedWebView(true)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MSAL Interactive Error: {ex.Message}");
                }

            }

            if(result == null)
            {
                throw new Exception("Autenticacion cancelada");
            }

            //new PreferenceManager().SaveMicrosoftToken(result?.AccessToken);

            return new AuthenticationToken
            {
                DisplayName = result?.Account?.Username ?? "",
                ExpiresOn = result?.ExpiresOn ?? DateTimeOffset.MinValue,
                Token = result?.AccessToken ?? "",
                UserId = result?.Account?.Username ?? ""
            };
        }
    }
}
