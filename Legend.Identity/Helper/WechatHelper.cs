using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Newtonsoft.Json.Linq;


namespace Legend.Identity.Helper
{
    public static class WechatHelper
    {
        public static ClaimsPrincipal GetClaims(JObject payload)
        {
            /** For WeChat user we can not get email, first name, last name **/

            if (payload == null)
                return null;
            var ci = new ClaimsIdentity();
            var email = GetEmail(payload);
            if (!string.IsNullOrEmpty(email))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.Email, email, ClaimValueTypes.String));
            }
            var nickName = payload.Value<string>("nickname");
            if (!string.IsNullOrEmpty(nickName))
            {
                ci.AddClaim(new Claim(JwtClaimTypes.NickName, nickName, ClaimValueTypes.String));
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
            var pic = payload.Value<string>("picture") ?? payload.Value<string>("headimgurl");
            if (!string.IsNullOrEmpty(pic))
            {
                var p = pic.Split('?')[0];
                ci.AddClaim(new Claim(JwtClaimTypes.Picture, p, ClaimValueTypes.String));
            }

            return new ClaimsPrincipal(ci);
        }
        public static async Task<JObject> GetWechatUser(string appId, string appSecret, string code)
        {
            var wxAccessTokenEndpoint = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", appId, appSecret, code);
            var accessTokenRequest = new HttpRequestMessage(HttpMethod.Get, wxAccessTokenEndpoint);
            var accessTokenResponse = await new HttpClient().SendAsync(accessTokenRequest);
            if (accessTokenResponse.IsSuccessStatusCode)
            {
                var accessTokenPayload = JObject.Parse(await accessTokenResponse.Content.ReadAsStringAsync());
                var userInfoEnpoint = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}", accessTokenPayload.Value<string>("access_token"), accessTokenPayload.Value<string>("openid"));
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userInfoEnpoint);
                var userInfoResponse = await new HttpClient().SendAsync(userInfoRequest);
                if (userInfoResponse.IsSuccessStatusCode)
                {
                    var userInfoPayload = JObject.Parse(await userInfoResponse.Content.ReadAsStringAsync());
                    return userInfoPayload;
                }
                return null;
            }

            return null;
        }
        /// <summary>
        /// Gets the Wechat user ID.
        /// </summary>
        public static string GetId(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return user.Value<string>("sub") ?? user.Value<string>("unionid");
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
            return user.Value<string>("email") ?? string.Format("{0}.wechatuser@example.com", user.Value<string>("unionid"));
        }
    }
}
