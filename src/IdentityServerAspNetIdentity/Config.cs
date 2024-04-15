using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified
                }
            },
             new IdentityResource("color", new [] { "favorite_color" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(name: "api1", displayName: "My API")
            {

                UserClaims={ "favorite_color" },
                Properties={ new KeyValuePair<string, string>("name", "shirin")}
            },

        };

    public static IEnumerable<ApiResource> ApiResources =>
    new ApiResource[]
    {
        new ApiResource("WeatherForeCast","WeatherForecast Api")
        {
            Scopes = { "api1" },

            // additional claims to put into access token
            UserClaims =
            {
                "color",
            },
            Properties = { new KeyValuePair<string, string>("name", "shirin") },
            ShowInDiscoveryDocument=true,
            AllowedAccessTokenSigningAlgorithms={ SecurityAlgorithms.RsaSsaPssSha256 },
            ApiSecrets={ new Secret("secret".Sha256())},

        }
    };


    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "client",

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // scopes that client has access to
                AllowedScopes = { "api1" },
                Claims=new [] { new ClientClaim() {

                } },
            },
            // interactive ASP.NET Core Web App
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                
                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "verification",
                    "api1",
                    //"color"
                },
                AccessTokenLifetime=20
            }
        };

    public static IEnumerable<Duende.IdentityServer.EntityFramework.Entities.IdentityProvider> IdentityProviders =>
    new Duende.IdentityServer.EntityFramework.Entities.IdentityProvider[]
    {
            new Duende.IdentityServer.EntityFramework.Entities.IdentityProvider()
            {
                DisplayName="Facebook",
            },
    };
}

