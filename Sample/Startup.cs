using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Raven.Identity;
using Sample.Common;
using Sample.Models;
using IdentityRole = Raven.Identity.IdentityRole;
using IdentityUser = Raven.Identity.IdentityUser;

namespace Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.Configure<RavenSettings>(Configuration.GetSection("RavenSettings"));
            services.AddRavenDbDocStore();
            services.AddRavenDbAsyncSession();

            services.AddControllersWithViews();

            services.AddIdentity<AppUser, IdentityRole>()
                .AddRavenDbIdentityStores<AppUser>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer()
                .AddAspNetIdentity<AppUser>()
                .AddConfigurationStore()
                .AddOperationalStore();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();

            app.UseIdentityServer();
            app.UseAuthorization();

            var docStore = app.ApplicationServices.GetRequiredService<IDocumentStore>();
            docStore.EnsureExists();
            docStore.EnsureRolesExist(new List<string> { AppUser.AdminRole, AppUser.ManagerRole });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
