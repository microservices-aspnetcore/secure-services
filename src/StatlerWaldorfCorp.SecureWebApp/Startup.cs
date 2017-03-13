using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;


namespace StatlerWaldorfCorp.SecureWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)                
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

            Console.WriteLine("Using OpenID Auth domain of : " + openIdSettings.Value.Domain);
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

            var options = CreateOpenIdConnectOptions(openIdSettings);
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("name");
            options.Scope.Add("email");
            options.Scope.Add("picture");

            app.UseOpenIdConnectAuthentication(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private OpenIdConnectOptions CreateOpenIdConnectOptions(
            IOptions<OpenIDSettings> openIdSettings)
        {
            return new OpenIdConnectOptions("Auth0")
            {
                Authority = $"https://{openIdSettings.Value.Domain}",
                ClientId = openIdSettings.Value.ClientId,
                ClientSecret = openIdSettings.Value.ClientSecret,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false,

                ResponseType = "code",
                CallbackPath = new PathString("/signin-auth0"),

                ClaimsIssuer = "Auth0",
                SaveTokens = true,
                Events = CreateOpenIdConnectEvents()
            };
        }

        private OpenIdConnectEvents CreateOpenIdConnectEvents()
        {
            return new OpenIdConnectEvents()
            {
                OnTicketReceived = context =>
                {
                    var identity = 
                        context.Principal.Identity as ClaimsIdentity;
                    if (identity != null) {
                        if (!context.Principal.HasClaim( c => c.Type == ClaimTypes.Name) &&
                        identity.HasClaim( c => c.Type == "name"))
                        identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
                    }
                    return Task.FromResult(0);
                }
            };
        }
    }
}
