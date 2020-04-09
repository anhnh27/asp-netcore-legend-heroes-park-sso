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
        public static async Task<JObject> GetWechatUser(string token, string openId)
        {
            var userInfoEnpoint = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}", token, openId);
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userInfoEnpoint);
            var userInfoResponse = await new HttpClient().SendAsync(userInfoRequest);
            if (userInfoResponse.IsSuccessStatusCode)
            {
                var userInfoPayload = JObject.Parse(await userInfoResponse.Content.ReadAsStringAsync());
                return userInfoPayload;
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
    }
}
