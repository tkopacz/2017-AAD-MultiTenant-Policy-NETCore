using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Extensions;
using Microsoft.Extensions.Options;
using TK2017MTAADv2.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace TK2017MTAADv2.Controllers
{
    public class MTController : Controller
    {
        public AzureAdOptions m_aadOptions;
        TenantContext m_db;
        public MTController(IOptions<AzureAdOptions> aadOptions, TenantContext db)
        {
            m_aadOptions = aadOptions.Value;
            m_db = db;
        }
        public IActionResult Index()
        {
            m_db.Database.EnsureCreated();
            return View();
        }
        public string GenerateSignUpUri(string stateMarker,bool admin)
        {
            string strRedirectUri = this.Request.Scheme + "://" + Request.Host + "/MT/ProcessCode";
            string authorizationRequest =
                $"https://login.microsoftonline.com/common/oauth2/authorize?" +
                $"response_type=code" +
                $"&client_id={Uri.EscapeDataString(m_aadOptions.ClientId)}" +
                $"&resource={Uri.EscapeDataString("https://graph.microsoft.com")}" +
                $"&redirect_uri={Uri.EscapeDataString(strRedirectUri)}" +
                $"&state={Uri.EscapeDataString(stateMarker)}";
            if (admin) authorizationRequest += $"&prompt=admin_consent";
            return authorizationRequest;
        }
        //public IActionResult ProcessCode(string code, string error, string error_description, string resource, string state)
        public async Task<IActionResult> ProcessCode(OnboardingModel model)
        {
            //Find Tenant based on tenant guid
            var t = m_db.Tenants.FirstOrDefault(p => p.Secret == model.state);
            if (t!=null)
            {
                var authContext = new AuthenticationContext($"{m_aadOptions.AzureAdInstance}"); //MT
                var creds = new ClientCredential(m_aadOptions.ClientId, m_aadOptions.ClientSecret);
                var redirectUri = new Uri($"{m_aadOptions.Domain}/MT/ProcessCode");
                var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                    model.code, redirectUri, creds,
                    "https://graph.microsoft.com/");
                //Do we already registered that tenant?
                if (m_db.Tenants.FirstOrDefault(p => p.TenantGuid == authResult.TenantId.ToLower()) == null)
                {
                    t.TenantGuid = authResult.TenantId.ToLower();
                } else
                {
                    m_db.Tenants.Remove(t);
                }
                m_db.SaveChanges();
            } //Else - wrong secret
            //Clean old 
            var old = m_db.Tenants.Where(p => p.TenantGuid == "" && p.DtCreated < DateTime.UtcNow.AddMinutes(-5));
            if (old.Count() > 0)
            {
                foreach (var item in old)
                {
                    m_db.Tenants.Remove(item);
                }
                m_db.SaveChanges();
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(Tenant model)
        {
            string stateMarker = Guid.NewGuid().ToString();
            model.Secret = stateMarker;
            model.DtCreated = DateTime.UtcNow;
            m_db.Tenants.Add(model);
            await m_db.SaveChangesAsync();
            return new RedirectResult(GenerateSignUpUri(stateMarker,model.IsAdmin));
        }
    }
}