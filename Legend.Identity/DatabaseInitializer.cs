using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Legend.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Legend.Identity
{
    public class DatabaseInitializer
    {
        public static void Init(IServiceProvider provider, bool useInMemoryStores, IConfiguration config)
        {
            if (!useInMemoryStores)
            {
                provider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                provider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                provider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();

                InitializeIdentityServer(provider);
            }


            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = userManager.FindByEmailAsync(config.GetSection("Admin:Email").Value).Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = config.GetSection("Admin:Email").Value,
                    Email = config.GetSection("Admin:Email").Value,
                };
                var result = userManager.CreateAsync(admin, config.GetSection("Admin:Pwd").Value).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                admin = userManager.FindByEmailAsync(config.GetSection("Admin:Email").Value).Result;

                result = userManager.AddClaimsAsync(admin, new Claim[]{
                    new Claim(JwtClaimTypes.Role, "admin"),
                    new Claim(JwtClaimTypes.Email, config.GetSection("Admin:Email").Value),
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
        }

        private static void InitializeIdentityServer(IServiceProvider provider)
        {
            var context = provider.GetRequiredService<ConfigurationDbContext>();
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
                foreach (var resource in Config.Ids)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.Apis)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}