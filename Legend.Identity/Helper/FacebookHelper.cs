using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;

namespace Legend.Identity.Helper
{
    public static class FacebookHelper
    {
        public static ClaimsPrincipal GetClaims(JObject payload)
        {
            if (payload == null)
                return null;
            var ci = new ClaimsIdentity();
            var email = GetEmail(payload);
            if (!string.IsNullOrEmpty(email))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.Email, email, ClaimValueTypes.String));
            }
            var identifier = GetId(payload);
            ci.AddClaim(new Claim(JwtClaimTypes.Picture, $"https://graph.facebook.com/{identifier}/picture?type=large", ClaimValueTypes.String));
            var firstName = payload.Value<string>("first_name");
            if (!string.IsNullOrEmpty(firstName))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.GivenName, firstName, ClaimValueTypes.String));
            }
            var lastName = payload.Value<string>("last_name");
            if (!string.IsNullOrEmpty(lastName))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.FamilyName, lastName, ClaimValueTypes.String));
            }

            return new ClaimsPrincipal(ci);
        }

        public static async Task<JObject> GetFacebookUser(string accessToken)
        {
            var endpoint = QueryHelpers.AddQueryString("https://graph.facebook.com/v5.0/me", new Dictionary<string, string>()
            {
                {"access_token", accessToken},
                {"fields", "first_name,last_name,email"}
            });
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }
        /// <summary>
        /// Gets the Facebook user ID.
        /// </summary>
        public static string GetId(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return user.Value<string>("id");
        }
        /// <summary>
        /// Gets the Facebook email.
        /// </summary>
        public static string GetEmail(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return user.Value<string>("email");
        }
    }
}
