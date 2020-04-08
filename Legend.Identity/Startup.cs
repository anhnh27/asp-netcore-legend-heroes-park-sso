// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Legend.Identity.Custom;
using Legend.Identity.Data;
using Legend.Identity.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using IdentityModel;

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

            services.AddTransient<IEmailSender, EmailSender>(i =>
                new EmailSender(
                    Configuration["EmailSender:Host"],
                    Configuration.GetValue<int>("EmailSender:Port"),
                    Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    Configuration["EmailSender:UserName"],
                    Configuration["EmailSender:Password"]
                )
            );

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

            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(2));

            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.User.RequireUniqueEmail = true;

                #region Password Require Options
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                #endregion
            });

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    options.SaveTokens = true;

                    options.Events.OnCreatingTicket = context =>
                    {
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.Email, context.User.GetProperty("email").ToString()));
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.GivenName, context.User.GetProperty("given_name").ToString()));
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.FamilyName, context.User.GetProperty("family_name").ToString()));
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.Picture, context.User.GetProperty("picture").ToString()));
                        List<AuthenticationToken> tokens = context.Properties.GetTokens().ToList();
                        tokens.Add(new AuthenticationToken()
                        {
                            Name = "TicketCreated",
                            Value = DateTime.UtcNow.ToString()
                        });
                        context.Properties.StoreTokens(tokens);
                        return Task.CompletedTask;
                    };
                })
                .AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    options.Events.OnCreatingTicket = context =>
                    {
                        var profileImg = context.User.GetProperty("picture").GetProperty("data").GetProperty("url").ToString();
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.Email, context.User.GetProperty("email").ToString()));
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.GivenName, context.User.GetProperty("given_name").ToString()));
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.FamilyName, context.User.GetProperty("family_name").ToString()));
                        context.Identity.AddClaim(new Claim(JwtClaimTypes.Picture, profileImg));

                        List<AuthenticationToken> tokens = context.Properties.GetTokens().ToList();
                        tokens.Add(new AuthenticationToken()
                        {
                            Name = "TicketCreated",
                            Value = DateTime.UtcNow.ToString()
                        });
                        context.Properties.StoreTokens(tokens);
                        return Task.CompletedTask;
                    };
                });
            //.AddWeixinAuthentication(options =>
            //{
            //    options.ClientId = Configuration["Authentication:WeChat:AppId"];
            //    options.ClientSecret = Configuration["Authentication:WeChat:AppSecret"];
            //});

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

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
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

            app.UseCors("CorsPolicy");

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