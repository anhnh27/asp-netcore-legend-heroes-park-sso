using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Legend.Identity
{
    public class DatabaseInitializer
    {
        public static void Init(IApplicationBuilder app, IConfiguration config)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var persistedGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                //persistedGrantDbContext.Database.Migrate();

                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                //configurationDbContext.Database.Migrate();

                configurationDbContext.Clients.RemoveRange(configurationDbContext.Clients.ToList());

                if (!configurationDbContext.Clients.Any())
                {
                    var clientSections = config.GetSection("IdentityServer:Clients").GetChildren();
                    var clients = new List<Client>();

                    foreach (var clientSection in clientSections)
                    {
                        var clientId = clientSection.GetSection("ClientId").Value;
                        var clientSecret = clientSection.GetSection("ClientSecrets").GetChildren().First().GetValue<string>("Value");
                        ICollection<string> grantTypes = clientSection.GetSection("AllowedGrantTypes").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();
                        ICollection<string> scopes = clientSection.GetSection("AllowedScopes").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();

                        var client = new Client
                        {
                            Enabled = clientSection.GetValue<bool>("Enabled"),
                            ClientId = clientSection.GetSection("ClientId").Value,
                            ClientName = clientSection.GetSection("ClientName").Value,
                            ClientSecrets = { new Secret(clientSecret.Sha256()) },
                            AllowedGrantTypes = grantTypes,
                            AllowedScopes = scopes,
                        };

                        if (clientId == "7fd33fc2-5ed7-42f6-9015-5952a502d1c3")
                        {
                            ICollection<string> redirectUris = clientSection.GetSection("RedirectUris").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();
                            client.RedirectUris = redirectUris;
                            client.RequirePkce = clientSection.GetValue<bool>("RequirePkce");
                            client.RequireClientSecret = clientSection.GetValue<bool>("RequireClientSecret");
                            client.AllowOfflineAccess = clientSection.GetValue<bool>("AllowOfflineAccess");
                        }
                        else if (clientId == "31b7732e-733b-4081-91ff-290879dd0d65b")
                        {
                            ICollection<string> redirectUris = clientSection.GetSection("RedirectUris").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();
                            ICollection<string> postLogoutRedirectUris = clientSection.GetSection("PostLogoutRedirectUris").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();
                            client.RedirectUris = redirectUris;
                            client.PostLogoutRedirectUris = redirectUris;
                        }

                        clients.Add(client);
                    }

                    foreach (var client in clients)
                    {
                        configurationDbContext.Clients.Add(client.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        configurationDbContext.IdentityResources.Add(resource.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.ApiResources.Any())
                {
                    var apiResourceSections = config.GetSection("IdentityServer:ApiResources").GetChildren();
                    var apiResources = new List<ApiResource>();

                    foreach (var apiResourceSection in apiResourceSections)
                    {
                        apiResources.Add(new ApiResource(apiResourceSection.GetValue<string>("Name"), apiResourceSection.GetValue<string>("DisplayName")));
                    }

                    foreach (var resource in apiResources)
                    {
                        configurationDbContext.ApiResources.Add(resource.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                //var applicationDbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //applicationDbContext.Database.Migrate();
            }
        }
    }
}