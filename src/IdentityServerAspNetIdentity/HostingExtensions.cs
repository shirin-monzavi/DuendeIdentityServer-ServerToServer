using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServerAspNetIdentity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                         .AddEntityFrameworkStores<ApplicationDbContext>()
                         .AddDefaultTokenProviders();

        const string connectionString1 = @"Data Source=172.16.16.140;Initial Catalog=ConfigurationDbContext;Persist Security Info=True;User ID=DotNetTeam;Password=Salam@321;TrustServerCertificate=True";
        const string connectionString2 = @"Data Source=172.16.16.140;Initial Catalog=PersistedGrantDbContext;Persist Security Info=True;User ID=DotNetTeam;Password=Salam@321;TrustServerCertificate=True";

        builder.Services
                        .AddIdentityServer(options =>
                        {
                            options.Events.RaiseErrorEvents = true;
                            options.Events.RaiseInformationEvents = true;
                            options.Events.RaiseFailureEvents = true;
                            options.Events.RaiseSuccessEvents = true;

                            // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                            options.EmitStaticAudienceClaim = true;
                        })
                         .AddConfigurationStore(options =>
                         {
                             options.ConfigureDbContext = b => b.UseSqlServer(connectionString1);
                         })
                        .AddOperationalStore(options =>
                        {
                            options.ConfigureDbContext = b => b.UseSqlServer(connectionString2);
                        })
                        .AddAspNetIdentity<ApplicationUser>()
                        .AddProfileService<CustomProfileService>();

        builder.Services.AddAuthentication();



        //builder.Services
        //    .AddIdentityServer(options =>
        //    {
        //        options.Events.RaiseErrorEvents = true;
        //        options.Events.RaiseInformationEvents = true;
        //        options.Events.RaiseFailureEvents = true;
        //        options.Events.RaiseSuccessEvents = true;

        //        // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
        //        options.EmitStaticAudienceClaim = true;
        //    })
        //    .add(Config.IdentityResources)
        //    .AddInMemoryApiScopes(Config.ApiScopes)
        //    .AddInMemoryClients(Config.Clients)
        //    .AddAspNetIdentity<ApplicationUser>()
        //    .AddProfileService<CustomProfileService>();

        builder.Services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        //InitializeDatabase(app);

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context.Database.Migrate();

            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.ApiResources)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }


            if (!context.Clients.Any())
            {
                foreach (var resource in Config.ApiResources)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityProviders.Any())
            {
                foreach (var resource in Config.IdentityProviders)
                {
                    context.IdentityProviders.Add(resource);
                }
                context.SaveChanges();
            }
        }
    }
}