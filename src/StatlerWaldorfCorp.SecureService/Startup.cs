using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace StatlerWaldorfCorp.Secureservice
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

        public void ConfigureServices(IServiceCollection services)
        {                                   
            services.AddMvc();
            services.AddOptions();

            services.AddAuthorization( options => {
               options.AddPolicy("CheeseburgerPolicy",
                        policy => policy.RequireClaim("icanhazcheeseburger", "true"));
            });
        }

         public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
                                ILoggerFactory loggerFactory)
         {
            string SecretKey = "seriouslyneverleavethissittinginyourcode";
            SymmetricSecurityKey signingKey = 
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
            
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            
            app.UseJwtBearerAuthentication(new JwtBearerOptions  
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = false,
                    ValidIssuer = "https://fake.issuer.com",

                    ValidateAudience = false,
                    ValidAudience = "https://sampleservice.example.com",

                    ValidateLifetime = true,                    
                }
            });

            app.UseMvc();
         }

        public IConfigurationRoot Configuration { get; }

    }
}