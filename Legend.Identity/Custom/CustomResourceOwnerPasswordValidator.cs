using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Legend.Identity.Data;
using Legend.Identity.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Legend.Identity.Custom
{
    public class CustomResourceOwnerPasswordValidator : ResourceOwnerPasswordValidator<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CustomResourceOwnerPasswordValidator(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEventService events,
            ILogger<ResourceOwnerPasswordValidator<ApplicationUser>> logger) : base(userManager, signInManager, events, logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public override async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (IsExternal(context.UserName))
            {
                var loginInfo = await CreateExternalLogin(context.UserName, context.Password);

                if (loginInfo == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                    return;
                }

                var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

                if (user == null)
                {
                    var extenalEmail = loginInfo.Principal.FindFirst(JwtClaimTypes.Email)?.Value;
                    var userNameSuffix = _userManager.Users.Where(u => u.Id != null).Count().ToString().PadLeft(7, '0');
                    var userName = string.Format("{0}{1}", loginInfo.LoginProvider.ToLower(), userNameSuffix);
                    var userId = Guid.NewGuid().ToString();
                    if (!string.IsNullOrEmpty(extenalEmail))
                    {
                        user = new ApplicationUser()
                        {
                            Id = userId,
                            Email = extenalEmail,
                            UserName = userName,
                        };

                        user.Claims.Add(new IdentityUserClaim<string>()
                        {
                            ClaimType = JwtClaimTypes.Name,
                            ClaimValue = userName
                        });
                        user.Claims.Add(new IdentityUserClaim<string>()
                        {
                            ClaimType = JwtClaimTypes.Email,
                            ClaimValue = extenalEmail
                        });
                    }
                    else
                    {
                        user = new ApplicationUser()
                        {
                            Id = userId,
                            UserName = userName,
                        };
                        user.Claims.Add(new IdentityUserClaim<string>()
                        {
                            ClaimType = JwtClaimTypes.Name,
                            ClaimValue = userName
                        });
                    }
                    var createResult = await _userManager.CreateAsync(user, context.Password);
                    var addClaimsResult = await _userManager.AddClaimsAsync(user, loginInfo.Principal.Claims);
                    var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                }

                if (user != null)
                {
                    var cp = await _signInManager.CreateUserPrincipalAsync(user);
                    var sub = user.Id;
                    context.Result = new GrantValidationResult(sub, OidcConstants.AuthenticationMethods.Password, cp.Claims);
                    return;
                }
                else
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                    return;
                }

            }

            //TO ALLOW LOGIN BOTH USERNAME & EMAIL
            if (RegexUtilities.IsValidEmail(context.UserName))
            {
                var user = await _userManager.FindByEmailAsync(context.UserName);
                context.UserName = user.UserName;
            }

            await base.ValidateAsync(context);
        }

        public async Task<ExternalLoginInfo> CreateExternalLogin(string provider, string token)
        {
            switch (provider)
            {
                case ExternalProvider.Facebook:
                    {
                        var payload = await FacebookHelper.GetFacebookUser(token);
                        var cp = FacebookHelper.GetClaims(payload);
                        if (cp == null)
                            return null;
                        return new ExternalLoginInfo(cp, ExternalProvider.Facebook, FacebookHelper.GetId(payload), ExternalProvider.Facebook);
                    }
                case ExternalProvider.Google:
                    {
                        var payload = await GoogleHelper.GetGoogleUser(token);
                        var cp = GoogleHelper.GetClaims(payload);
                        if (cp == null)
                            return null;
                        return new ExternalLoginInfo(cp, ExternalProvider.Google, GoogleHelper.GetId(payload), ExternalProvider.Google);
                    }
                case ExternalProvider.Wechat:
                    {
                        string[] variables = token.Split(":");
                        var payload = await WechatHelper.GetWechatUser(variables[0], variables[1]);
                        var cp = WechatHelper.GetClaims(payload);
                        if (cp == null)
                            return null;
                        return new ExternalLoginInfo(cp, ExternalProvider.Wechat, WechatHelper.GetId(payload), ExternalProvider.Wechat);
                    }
                default:
                    return null;
            }
        }

        private bool IsExternal(string userName)
        {
            switch (userName)
            {
                case ExternalProvider.Facebook:
                case ExternalProvider.Google:
                case ExternalProvider.Wechat:
                    return true;
                default:
                    return false;
            }
        }

        public class ExternalProvider
        {
            public const string Facebook = "Facebook";
            public const string Google = "Google";
            public const string Wechat = "WeChat";
        }
    }
}
