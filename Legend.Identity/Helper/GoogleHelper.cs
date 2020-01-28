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
    public static class GoogleHelper
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
            var firstName = payload.Value<string>("given_name");
            if (!string.IsNullOrEmpty(firstName))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.GivenName, firstName, ClaimValueTypes.String));
            }
            var lastName = payload.Value<string>("family_name");
            if (!string.IsNullOrEmpty(lastName))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.FamilyName, lastName, ClaimValueTypes.String));
            }
            var pic = payload.Value<string>("picture");
            if (!string.IsNullOrEmpty(pic))
            {
                var p = pic.Split('?')[0];
                ci.AddClaim(new Claim(JwtClaimTypes.Picture, p, ClaimValueTypes.String));
            }

            return new ClaimsPrincipal(ci);
        }
        public static async Task<JObject> GetGoogleUser(string idToken)
        {
            var userInformationEndpoint = string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}", idToken);
            var request = new HttpRequestMessage(HttpMethod.Get, userInformationEndpoint);
            var response = await new HttpClient().SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                return payload;
            }
            return null;
        }
        /// <summary>
        /// Gets the Google user ID.
        /// </summary>
        public static string GetId(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return user.Value<string>("sub");
        }
        /// <summary>
        /// Gets the user's email.
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
