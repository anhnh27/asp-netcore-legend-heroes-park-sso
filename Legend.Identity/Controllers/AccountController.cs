using System;
using System.Threading.Tasks;
using IdentityModel;
using Legend.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Legend.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Legend.Identity.Helper;
using System.IO;
using IdentityServer4.Quickstart.UI;

namespace Legend.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm]RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                if (model.FromWeb)
                {
                    return View("Error", new ErrorViewModel("Password and Confirmation Password are not matched."));
                }
                return BadRequest(new { OK = false, Message = "Password and Confirmation Password are not matched." });
            }
            else
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
                if (model.Picture != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Picture,
                        ClaimValue = model.Picture
                    });
                }
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
                    return BadRequest(response);
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("RequestResetPasswordView")]
        public IActionResult RequestResetPasswordView()
        {
            var model = new RequestResetPasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [Route("Upload")]
        public IActionResult UploadProfilePhoto(IFormFile file)
        {
            try
            {
                Guid fileId = Guid.NewGuid();
                if (!Directory.Exists(Path.Combine("wwwroot/profile-pictures")))
                {
                    Directory.CreateDirectory(Path.Combine("wwwroot/profile-pictures"));
                }

                if (System.IO.File.Exists(Path.Combine("wwwroot/profile-pictures/", fileId.ToString())))
                {
                    System.IO.File.Delete(Path.Combine("wwwroot/profile-pictures/", fileId.ToString()));
                }

                if (file.Length > 0)
                {
                    using (var fileStream = new FileStream(Path.Combine("wwwroot/profile-pictures/", fileId.ToString()), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                }

                var url = Url.Action("GetPhoto", "Account", new { fileId = fileId.ToString() }, HttpContext.Request.Scheme);

                return Ok(new { Message = "Success", FileUrl = url });
            }
            catch (Exception ex) 
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetPhoto")]
        public IActionResult GetPhoto(string fileId) 
        {
            var fileExist = System.IO.File.Exists(Path.Combine("wwwroot/profile-pictures/", fileId));
            if (fileExist)
            {
                var image = System.IO.File.OpenRead(Path.Combine("wwwroot/profile-pictures/", fileId));
                return File(image, "image/jpeg");
            }
            else
            {
                return NotFound();
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
                    return Ok(new { Ok = true, Message = "Could not find user with email: " + email, AccountExist = false });
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(userEntity);

                var callback = Url.Action("ResetPassword", "Account", new { email, code }, protocol: HttpContext.Request.Scheme);
                var subject = "Legend Heroes Park - Reset Password";
                var body = string.Format("Please reset your password by clicking <a href='{0}'>here</a>.", callback);

                await _emailSender.SendEmailAsync(email, subject, body);

                return Ok(new { Ok = true, Message = "An email has been sent to your email. Please check your email and follow instruction to reset your password. The email might be in your spam folder.", AccountExist = true });
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { Ok = false, ex.Message });
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
                    return Ok(new { Ok = true, Message = "Could not find user with email: " + email });
                }

                var temporaryPassword = Utilities.GenerateRandomPassword();
                var result = await _userManager.ResetPasswordAsync(user, code, temporaryPassword);

                if (!result.Succeeded)
                {
                    return Ok(new { Ok = false, Message = "Reset password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
                }

                var subject = "Legend Heroes Park - Temporary Password";
                var body = string.Format("Your temporary password is: '{0}' <br/> Please login and change your password.", temporaryPassword);

                await _emailSender.SendEmailAsync(email, subject, body);

                return Ok(new { Ok = true, Message = "Your password has been reset. Check your email to get temporary password." });
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { Ok = false, ex.Message });
                return result;
            }
        }

        [HttpPost]
        [Route("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody]ResetPasswordViewModel model)
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
                    return Ok(new { Ok = true, Message = "Could not find user with email: " + model.Email });
                }

                var pwdErrors = new List<string>();
                var validators = _userManager.PasswordValidators;
                foreach (var item in validators)
                {
                    var isValid = await item.ValidateAsync(_userManager, user, model.NewPassword);

                    if (!isValid.Succeeded)
                    {
                        foreach (var error in isValid.Errors)
                        {
                            pwdErrors.Add(error.Description);
                        }
                    }
                }

                if (pwdErrors.Count > 0)
                {
                    return Ok(new { Ok = false, Message = "Password is not valid. Message: " + string.Join(", ", pwdErrors) });
                }

                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                {
                    return Ok(new { Ok = false, Message = "Update password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
                }

                return Ok(new { Ok = true, Message = "Your password has been changes." });
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { Ok = false, ex.Message });
                return result;
            }
        }
    }
}