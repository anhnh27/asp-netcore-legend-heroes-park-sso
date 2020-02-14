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
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                context.Database.Migrate();

                context.Clients.RemoveRange(context.Clients.ToList());

                if (!context.Clients.Any())
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
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var apiResourceSections = config.GetSection("IdentityServer:ApiResources").GetChildren();
                    var apiResources = new List<ApiResource>();

                    foreach (var apiResourceSection in apiResourceSections)
                    {
                        apiResources.Add(new ApiResource(apiResourceSection.GetValue<string>("Name"), apiResourceSection.GetValue<string>("DisplayName")));
                    }

                    foreach (var resource in apiResources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                #region Add Default User
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var admin = userManager.FindByEmailAsync("anhnh27@hotmail.com").Result;
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "anhnh27",
                        Email = "anhnh27@hotmail.com",
                    };

                    var result = userManager.CreateAsync(admin, "P@ssword1").Result;
                   
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    admin = userManager.FindByEmailAsync("anhnh27@hotmail.com").Result;

                    result = userManager.AddClaimsAsync(admin, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Email, "anhnh27@hotmail.com"),
                        new Claim(JwtClaimTypes.GivenName, "Alex"),
                        new Claim(JwtClaimTypes.FamilyName, "Nguyen"),
                        new Claim(JwtClaimTypes.Picture, "https://i.pinimg.com/originals/cf/68/73/cf68732c4a8b2b8d62a272d161fb58f4.jpg"),
                    }).Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("admin created");
                }
                else
                {
                    Console.WriteLine("admin already exists");
                }
                #endregion
            }
        }
    }
}