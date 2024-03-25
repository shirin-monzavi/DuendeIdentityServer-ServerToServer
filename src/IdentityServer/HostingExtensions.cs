using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
        const string connectionString1 = @"Data Source=172.16.16.140;Initial Catalog=ConfigurationDbContext;Persist Security Info=True;User ID=DotNetTeam;Password=Salam@321;TrustServerCertificate=True";
        const string connectionString2 = @"Data Source=172.16.16.140;Initial Catalog=PersistedGrantDbContext;Persist Security Info=True;User ID=DotNetTeam;Password=Salam@321;TrustServerCertificate=True";

        builder.Services
                        .AddIdentityServer()
                         .AddConfigurationStore(options =>
                         {
                             options.ConfigureDbContext = b => b.UseSqlServer(connectionString1);
                         })
                        .AddOperationalStore(options =>
                        {
                            options.ConfigureDbContext = b => b.UseSqlServer(connectionString2);
                        })
                        .AddTestUsers(TestUsers.Users);

        builder.Services.AddAuthentication();
        //.AddGoogle("Google", options =>
        //{
        //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

        //    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        //    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        //});

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        InitializeDatabase(app);

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

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
        }
    }

}
