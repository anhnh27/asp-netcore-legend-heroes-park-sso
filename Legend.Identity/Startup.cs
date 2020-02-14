// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Legend.Identity.Custom;
using Legend.Identity.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace Legend.Identity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var connectionString = Configuration.GetConnectionString("IdentityDataContextConnection");
            var useInMemory = bool.Parse(Configuration.GetSection("UseInMemoryStores").Value);

            services.AddControllersWithViews();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(connectionString, mySqlOpt => mySqlOpt.MigrationsAssembly(migrationsAssembly));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.User.RequireUniqueEmail = true;

                #region Password Require Options
                //options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequiredLength = 6;
                //options.Password.RequiredUniqueChars = 1;
                #endregion
            });

            #region setup id4
            var builder = services.AddIdentityServer(options =>
            {
                options.InputLengthRestrictions.Password = int.MaxValue;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            });
            if (useInMemory)
            {

                builder.AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Configuration.GetSection("IdentityServer:ApiResources"))
                .AddInMemoryClients(Configuration.GetSection("IdentityServer:Clients"))
                .AddAspNetIdentity<ApplicationUser>();
            }
            else
            {
                // this adds the config data from DB (clients, resources)
                builder.AddConfigurationStore(options => options.ConfigureDbContext = options => options.UseMySql(connectionString, ctx => ctx.MigrationsAssembly(migrationsAssembly)))
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options => options.ConfigureDbContext = options => options.UseMySql(connectionString, ctx => ctx.MigrationsAssembly(migrationsAssembly)))
                .AddAspNetIdentity<ApplicationUser>();
            }

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                builder.AddSigningCredential(GetEmbeddedCertificate(Environment.ContentRootPath));
            }

            services.AddTransient<IProfileService, CustomProfileService>();
            services.AddTransient<IResourceOwnerPasswordValidator, CustomResourceOwnerPasswordValidator>();
            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            var useInMemory = bool.Parse(Configuration.GetSection("UseInMemoryStores").Value);

            if (!useInMemory)
            {
                DatabaseInitializer.Init(app, Configuration);
            }

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private X509Certificate2 GetEmbeddedCertificate(string rootPath)
        {
            try
            {
                string[] paths = { @"C:\sso-self-sign-cert", "sso-ssc.pfx" };

                var fileName = Path.Combine(paths);

                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException("Signing Certificate is missing!");
                }

                return new X509Certificate2(fileName, "abcde12345-");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}