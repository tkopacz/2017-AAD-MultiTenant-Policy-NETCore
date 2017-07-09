using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Graph;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Authentication.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://msdn.microsoft.com/Library/Azure/Ad/Graph/api/entity-and-complex-type-reference#application-entity
    /// groupMembershipClaims to 7
    /// </remarks>
    public class AzureAdOpenIdConnectOptionsSetup : IConfigureOptions<OpenIdConnectOptions>
    {
        private readonly AzureAdOptions _aadOptions;

        public AzureAdOpenIdConnectOptionsSetup(IOptions<AzureAdOptions> aadOptions)
        {
            _aadOptions = aadOptions.Value;
        }

        public void Configure(OpenIdConnectOptions oidcOptions)
        {
            oidcOptions.ClientId = _aadOptions.ClientId;
            oidcOptions.Authority = _aadOptions.Authority;
            oidcOptions.UseTokenLifetime = true;
            oidcOptions.CallbackPath = _aadOptions.CallbackPath;
            oidcOptions.ResponseType = "code id_token";
            oidcOptions.ClientSecret = _aadOptions.ClientSecret; //Required - for client assertion!
            //Could also request groups from AAD during login - but - we will get only guid's not names

            oidcOptions.TokenValidationParameters = new TokenValidationParameters
            {
                // Instead of using the default validation (validating against a single issuer value, as we do in line of business apps),
                // we inject our own multitenant validation logic
                //ValidateIssuer = false,

                // If the app is meant to be accessed by entire organizations, add your issuer validation logic here.
                IssuerValidator = (issuer, securityToken, validationParameters) =>
                {
                    if (myIssuerValidationLogic(issuer)) return issuer;
                    return "ERROR";
                }
            };

            oidcOptions.Events = new OpenIdConnectEvents
            {
                OnTicketReceived = (context) =>
                {
                    // If your authentication logic is based on users then add your logic here
                    return Task.FromResult(0);
                },
                OnAuthenticationFailed = (context) =>
                {
                    context.Response.Redirect("/Home/Error");
                    context.HandleResponse(); // Suppress the exception
                    return Task.FromResult(0);
                },
                // If your application needs to do authenticate single users, add your user validation below.
                OnTokenValidated = (context) =>
                {
                    return myUserValidationLogic(context.Ticket.Principal);
                },
                OnAuthorizationCodeReceived = (context) =>
                {
                    Task.Run(async () =>
                    {
                        Debug.WriteLine(context.TokenEndpointRequest.Code);
                        try
                        {
                            List<Claim> claims;
                            var gsc = new GraphServiceClient(new AzureAuthenticationProvider(_aadOptions, context.Ticket.Principal, context.TokenEndpointRequest.Code));
                            
                            //To read groups - we need admin consen
                            var me = await gsc.Me.Request().GetAsync();
                            var myGroup = await gsc.Me.MemberOf.Request().GetAsync();
                            claims = generateClaims(myGroup);
                            
                            //Including Transitive, DirectoryObjects require admin roles!
                            //var objects = await gsc.DirectoryObjects.GetByIds(me.ToList(), new string[] { "group", "directoryRole" }).Request().PostAsync();
                            //claims = generateClaims(objects);

                            //Update principal
                            var principal = context.Ticket.Principal;
                            (principal.Identity as ClaimsIdentity).AddClaims(claims);
                            //Replace ticket
                            context.Ticket = new AuthenticationTicket(
                                                 principal,
                                                 context.Ticket.Properties,
                                                 context.Ticket.AuthenticationScheme);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }).Wait();

                    return Task.FromResult(0);
                }
            };
        }

        private static List<Claim> generateClaims(IEnumerable<object> data)
        {
            var claims = new List<Claim>();
            foreach (var item in data)
            {
                switch (item)
                {
                    case Microsoft.Graph.Group group:
                        claims.Add(new Claim("tkgroups", group.DisplayName));
                        break;
                    case Microsoft.Graph.DirectoryRole role:
                        claims.Add(new Claim("tkgroups", role.DisplayName));
                        break;
                }
            }
            return claims;
        }

        //TK:
        private bool myIssuerValidationLogic(string issuer)
        {
            return true; //Any Tenant
        }

        private async Task myUserValidationLogic(ClaimsPrincipal principal)
        {
            return;
        }
    }
}

