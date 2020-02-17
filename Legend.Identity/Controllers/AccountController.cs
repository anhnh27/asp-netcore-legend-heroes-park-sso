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
using Microsoft.AspNetCore.Authorization;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Legend.Identity.Helper;

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
                ClaimValue = model.FirstName
            });
            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = JwtClaimTypes.FamilyName,
                ClaimValue = model.LastName
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

        [HttpGet]
        [AllowAnonymous]
        [Route("requestpasswordresetemail/{email}")]
        public async Task<IActionResult> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var userEntity = await _userManager.FindByEmailAsync(email);

                if (userEntity == null)
                {
                    return NoContent();
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(userEntity);

                var callback = Url.Action("ResetPassword", "Account", new { email, code }, protocol: HttpContext.Request.Scheme);
                var subject = "Legend Heroes Park - Reset Password";
                var body = string.Format("Please reset your password by clicking <a href='{0}'>here</a>.", callback);

                await _emailSender.SendEmailAsync(email, subject, body);

                return Ok(new { ResetPasswordLink = body });
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                return result;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword(string email, [FromQuery]string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
                {
                    return NoContent();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NoContent();
                }

                var temporaryPassword = Utilities.GenerateRandomPassword();
                var result = await _userManager.ResetPasswordAsync(user, code, "1");

                if (!result.Succeeded)
                {
                    return Ok(new { Message = "Reset password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
                }

                var subject = "Legend Heroes Park - Temporary Password";
                var body = "Your temporary password is: " + temporaryPassword + "<br/> Please login and change your password.";

                await _emailSender.SendEmailAsync(email, subject, body);

                return Ok(new { Message = "Your password has been reset. Check your email to get temporary password." });
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                return result;
            }
        }

        [HttpPost]
        [Route("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody]ResetPasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return NoContent();
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NoContent();
                }

                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                {
                    return Ok(new { Message = "Update password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
                }

                return Ok(new { Message = "Your password has been changes." });
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                return result;
            }
        }
    }
}