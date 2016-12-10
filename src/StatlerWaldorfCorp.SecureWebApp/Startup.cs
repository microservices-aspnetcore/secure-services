using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;


namespace StatlerWaldorfCorp.SecureWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)                
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(
                options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Add framework services.
            services.AddMvc();

            services.AddOptions();

            services.Configure<OpenIDSettings>(Configuration.GetSection("OpenID"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
                    ILoggerFactory loggerFactory,
                    IOptions<OpenIDSettings> openIdSettings)
        {

            Debug.WriteLine("Using OpenID Auth domain of : " + openIdSettings.Value.Domain);
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication( new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            var options = new OpenIdConnectOptions("Auth0")
            {
                Authority = $"https://{openIdSettings.Value.Domain}",
                ClientId = openIdSettings.Value.ClientId,
                ClientSecret = openIdSettings.Value.ClientSecret,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false,

                ResponseType = "code",
                CallbackPath = new PathString("/signin-auth0"),

                ClaimsIssuer = "Auth0"
            };
            options.Scope.Clear();
            options.Scope.Add("openid");

            app.UseOpenIdConnectAuthentication(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
