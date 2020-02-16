using System;
using System.Threading.Tasks;
using IdentityModel;
using Legend.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Legend.Identity.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Legend.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            var user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Email
            };

            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = JwtClaimTypes.Email,
                ClaimValue = model.Email
            });
            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = JwtClaimTypes.GivenName,
                ClaimValue = model.Firstname
            });
            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = JwtClaimTypes.FamilyName,
                ClaimValue = model.Lastname
            });
            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = JwtClaimTypes.Picture,
                ClaimValue = model.Picture
            });
            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = JwtClaimTypes.PhoneNumber,
                ClaimValue = model.PhoneNumber
            });

            var response = await _userManager.CreateAsync(user, model.Password);
            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                //TODO: Handle failed request like: Duplicate Username, Email,...
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("signout")]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, ex);
                return result;
            }
        }

        [HttpPost]
        [Route("resetpwd")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPwdViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetLink = Url.Action("ResetPassword", "Account", new { token = code }, protocol: HttpContext.Request.Scheme);

                    try
                    {
                        await _emailSender.SendEmailAsync("nguyenhoanganh10290@gmail.com", "LHP email verification", "This is verification email from Legend Heroes Park");

                        return Ok(new { message = "Password reset link has been sent to your email address!" });
                    }
                    catch (SmtpException ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                    }
                }
                else
                {
                    return Ok(new { message = "Email is not existed" });
                }
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                return result;
            }
        }
    }
}