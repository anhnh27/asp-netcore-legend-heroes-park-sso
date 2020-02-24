using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Legend.Identity.Data;

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
                        var secret = clientSection.GetSection("ClientSecrets").GetChildren().First().GetValue<string>("Value");
                        ICollection<string> grantTypes = clientSection.GetSection("AllowedGrantTypes").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();
                        ICollection<string> scopes = clientSection.GetSection("AllowedScopes").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();

                        var client = new Client
                        {
                            Enabled = clientSection.GetValue<bool>("Enabled"),
                            ClientId = clientSection.GetSection("ClientId").Value,
                            ClientName = clientSection.GetSection("ClientName").Value,
                            ClientSecrets = { new Secret(secret.Sha256()) },
                            AllowedGrantTypes = grantTypes,
                            AllowedScopes = scopes,
                        };

                        if (clientSection.GetSection("ClientName").Value.StartsWith("MVC"))
                        {
                            ICollection<string> redirectUris = clientSection.GetSection("RedirectUris").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();
                            ICollection<string> postLogoutRedirectUris = clientSection.GetSection("PostLogoutRedirectUris").AsEnumerable().Where(x => x.Value != null).Select(x => x.Value).ToList();

                            client.RedirectUris = redirectUris;
                            client.PostLogoutRedirectUris = redirectUris;
                            client.RequireConsent = clientSection.GetValue<bool>("RequireConsent");
                            client.RequirePkce = clientSection.GetValue<bool>("RequirePkce");
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