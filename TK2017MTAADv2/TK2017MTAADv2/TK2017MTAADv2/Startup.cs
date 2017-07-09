using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TK2017MTAADv2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAzureAdAuthentication();

            services.AddMvc();

            services.AddAuthorization(options =>
            {
                //In general - for single tenant, where we can control "names" of groups
                options.AddPolicy("AdminPolicy", policy => policy.RequireClaim("tkgroups", "Admin"));
                options.AddPolicy("Admin1Policy", policy => policy.RequireClaim("tkgroups", "Admin1"));
                
                //Require groupMembershipClaims in manifest
                //Guid from: https://portal.azure.com/?r=1#blade/Microsoft_AAD_IAM/GroupDetailsMenuBlade/Properties/groupId/8542e184-3375-49de-8401-131a73ed9d9c
                options.AddPolicy("AdminPolicyByGuid", policy => policy.RequireClaim("groups", new string[] { "8542e184-3375-49de-8401-131a73ed9d9c", /*Another tenant: tkopaczmse3 */"da2d4106-4bd5-4068-b2f1-8e47c7b8fe71" }));
            });
            //https://portal.office.com/account/#apps, App Permission, for user
            //As Admin:
            //https://manage.windowsazure.com/@tkopaczmsE3.onmicrosoft.com#Workspaces/ActiveDirectoryExtension/Directory/a07319e7-7cb1-41fe-9ebf-250e5deba957/apps
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRewriter(new RewriteOptions().AddIISUrlRewrite(env.ContentRootFileProvider, "urlRewrite.config"));

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
