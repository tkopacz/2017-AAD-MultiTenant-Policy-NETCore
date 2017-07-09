using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Diagnostics;
using System;

namespace Microsoft.AspNetCore.Authentication.Extensions
{
    /// <summary>
    /// Authentication Provider for Graph API SDK
    /// </summary>
    /// <remarks>
    /// Steps
    /// 1. Find Signed-in user
    /// 2. Get Auth Context from tenant for THAT user!!! (this is MT app!)
    /// 3. Get Client Credential (ClientId / Password)
    /// 4. Setup Callback path
    /// 5. Get Token by Authorization Token for redirect uri and creds for graph.microsoft.net
    /// 6. Add Bearer token to header
    /// 
    /// Using User permission (delegated, impersonalisation etc)
    /// Could use Application Permission in other scenarios
    /// </remarks>
    public class AzureAuthenticationProvider : IAuthenticationProvider
    {
        private readonly AzureAdOptions m_aadOptions;
        private readonly ClaimsPrincipal m_principal;
        private readonly string m_code;
        public AzureAuthenticationProvider(AzureAdOptions aadOptions, ClaimsPrincipal principal, string code)
        {
            m_aadOptions = aadOptions;
            m_principal = principal;
            m_code = code;
        }

        AzureAuthenticationProvider(IOptions<AzureAdOptions> aadOptions)
        {
            m_aadOptions = aadOptions.Value;
        }
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            foreach (var item in m_principal.Claims)
            {
                Debug.WriteLine($"{item.Type} - {item.Value}");
            }
            //Permission per APPLICATION
            //string signedInUserID = m_principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            //string tenantID = m_principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            //var authContext = new AuthenticationContext($"{m_aadOptions.AzureAdSingleInstance}{tenantID}");
            //var creds = new ClientCredential(m_aadOptions.ClientId, m_aadOptions.ClientSecret);
            //var authResult = await authContext.AcquireTokenAsync("https://graph.microsoft.com/", creds);


            //Permission per USER - delegated
            //tkadmin@tkdxpl.onmicrosoft.com - a757c7b8-69a2-4b92-b277-be767fc38487
            //tkopaczms@tkopaczmse3.onmicrosoft.com - a07319e7-7cb1-41fe-9ebf-250e5deba957
            string signedInUserID = m_principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            string tenantID = m_principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            //string tenantID = m_aadOptions.TenantId;
            var authContext = new AuthenticationContext($"{m_aadOptions.AzureAdSingleInstance}{tenantID}");
            var creds = new ClientCredential(m_aadOptions.ClientId, m_aadOptions.ClientSecret);
            var redirectUri = new Uri($"{m_aadOptions.Domain}{m_aadOptions.CallbackPath}");
            var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                m_code, redirectUri, creds,
                "https://graph.microsoft.com/");

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}