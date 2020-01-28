// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Legend.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };


        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                 new ApiResource("legend_api", "legend api")
            };


        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // client credentials & rop flow client
                new Client
                {
                    ClientId = "e44254fe-01e7-42c4-a42f-cffcb74c50ab",
                    ClientName = "Legend Mobile",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = { new Secret("1cb5160c-93b2-4aad-ab55-28130c96208f".Sha256()) },
                    AllowedScopes = { "openid", "profile", "email", "legend_api" }
                },

                // MVC client using code flow + pkce
                new Client
                {
                    ClientId = "31b7732e-733b-4081-91ff-290879dd0d65b",
                    ClientName = "Legend Admin Portal",
                    ClientSecrets = { new Secret("3d73bc46-d313-4155-8074-8cb1c13ada03".Sha256()) },

                    //https://github.com/openiddict/openiddict-core/issues/35
                    RedirectUris = { "http://localhost:5002/signin-oidc" }, //DOT NOT change this url
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" }, //DOT NOT change this url


                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "legend_api"
                    }
                },
            };
    }
}