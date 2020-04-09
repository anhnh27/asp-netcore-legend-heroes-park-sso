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
using System.Net;

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
                if (model.Email != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Email,
                        ClaimValue = model.Email
                    });
                }
                if (model.FirstName != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.GivenName,
                        ClaimValue = model.FirstName
                    });
                }
                if (model.LastName != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.FamilyName,
                        ClaimValue = model.LastName
                    });
                }
                if (model.Name != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Name,
                        ClaimValue = model.Name
                    });
                }
                else
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Name,
                        ClaimValue = string.Format("{0} {1}", model.FirstName, model.LastName)
                    });
                }
                if (model.PhoneNumber != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.PhoneNumber,
                        ClaimValue = model.PhoneNumber
                    });
                }
                if (model.Nationality != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Locale,
                        ClaimValue = model.Nationality
                    });
                }
                if (model.Gender != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Gender,
                        ClaimValue = model.Gender
                    });
                }
                if (model.Picture != null)
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Picture,
                        ClaimValue = model.Picture
                    });
                }
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

        [HttpGet]
        [AllowAnonymous]
        [Route("ResetPassword")]
        public IActionResult ResetPassword(string email, string code)
        {
            var model = new ResetPasswordRequestModel();
            model.Email = email;
            model.Code = code;
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

        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(RegisterViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Email);
                if (model.Email != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.Email))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.Email));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Email,
                        ClaimValue = model.Email
                    });
                }
                if (model.FirstName != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.GivenName))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.GivenName));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.GivenName,
                        ClaimValue = model.FirstName
                    });
                }
                if (model.LastName != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.FamilyName))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.FamilyName));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.FamilyName,
                        ClaimValue = model.LastName
                    });
                }
                if (model.Name != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.Name))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.Name));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Name,
                        ClaimValue = model.Name
                    });
                }
                else
                {
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Name,
                        ClaimValue = string.Format("{0} {1}", model.FirstName, model.LastName)
                    });
                }
                if (model.PhoneNumber != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.PhoneNumber))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.PhoneNumber));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.PhoneNumber,
                        ClaimValue = model.PhoneNumber
                    });
                }
                if (model.Nationality != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.Locale))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.Locale));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Locale,
                        ClaimValue = model.Nationality
                    });
                }
                if (model.Gender != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.Gender))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.Gender));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Gender,
                        ClaimValue = model.Gender
                    });
                }
                if (model.Picture != null)
                {
                    if (user.Claims.Any(x => x.ClaimType == JwtClaimTypes.Picture))
                    {
                        user.Claims.Remove(user.Claims.FirstOrDefault(x => x.ClaimType == JwtClaimTypes.Picture));
                    }
                    user.Claims.Add(new IdentityUserClaim<string>()
                    {
                        ClaimType = JwtClaimTypes.Picture,
                        ClaimValue = model.Picture
                    });
                }

                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("SignOut")]
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
        [Route("RequestPasswordResetEmail")]
        public async Task<IActionResult> SendPasswordResetEmailAsync(string email, bool fromWeb = true)
        {
            try
            {
                var userEntity = await _userManager.FindByEmailAsync(email);

                if (userEntity == null)
                {
                    return Ok(new { Ok = true, Message = "Could not find user with email: " + email, AccountExist = false });
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(userEntity);

                var callBack = string.Empty;

                if (fromWeb)
                {
                    callBack = Url.Action("ResetPassword", "Account", new { email, code }, protocol: HttpContext.Request.Scheme);
                }
                else
                {
                    callBack = Url.Action("UniversalLink", "Account", new { email, code }, protocol: HttpContext.Request.Scheme);
                }

                var subject = "Legend Heroes Park - Reset Password";
                var body = $"Please click on <a href='{callBack}'>this link</a> to reset your password.";
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
        [Route("UniversalLink")]
        public IActionResult UniversalLink(string email, string code)
        {
            return Redirect(string.Format("legendheroespark://resetpassword/{0}:{1}", email, WebUtility.UrlEncode(code)));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPasswordFromEmail")]
        public async Task<IActionResult> ResetPasswordFromEmail(ResetPasswordRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Ok(new { Ok = false, Message = "Could not find user with email: " + model.Email });
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                var subject = "Legend Heroes Park - Reset Password";

                if (result.Succeeded)
                {
                    var body = string.Format("Your password has been reset.");
                    await _emailSender.SendEmailAsync(model.Email, subject, body);

                    return Ok(new { Ok = true });
                }
                else
                {
                    var body = string.Format("Your password failed to reset.");
                    await _emailSender.SendEmailAsync(model.Email, subject, body);

                    return Ok(new { Ok = false, Message = "Reset password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
                }
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { Ok = false, ex.Message });
                return result;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPasswordFromEmailFromForm")]
        public async Task<IActionResult> ResetPasswordFromEmailFromForm([FromForm]ResetPasswordRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Ok(new { Ok = false, Message = "Could not find user with email: " + model.Email });
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                var subject = "Legend Heroes Park - Reset Password";

                if (result.Succeeded)
                {
                    var body = string.Format("Your password has been reset.");
                    await _emailSender.SendEmailAsync(model.Email, subject, body);

                    return Ok(new { Ok = true, Messasge = "Password has been reset" });
                }
                else
                {
                    var body = string.Format("Your password failed to reset.");
                    await _emailSender.SendEmailAsync(model.Email, subject, body);

                    return Ok(new { Ok = false, Message = "Reset password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
                }
            }
            catch (Exception ex)
            {
                var result = StatusCode(StatusCodes.Status500InternalServerError, new { Ok = false, ex.Message });
                return result;
            }
        }

        //[HttpPost]
        //[Route("ChangePassword")]
        //public async Task<IActionResult> ChangePassword([FromBody]ResetPasswordViewModel model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return NoContent();
        //        }

        //        var user = await _userManager.FindByEmailAsync(model.Email);
        //        if (user == null)
        //        {
        //            return Ok(new { Ok = true, Message = "Could not find user with email: " + model.Email });
        //        }

        //        var pwdErrors = new List<string>();
        //        var validators = _userManager.PasswordValidators;
        //        foreach (var item in validators)
        //        {
        //            var isValid = await item.ValidateAsync(_userManager, user, model.NewPassword);

        //            if (!isValid.Succeeded)
        //            {
        //                foreach (var error in isValid.Errors)
        //                {
        //                    pwdErrors.Add(error.Description);
        //                }
        //            }
        //        }

        //        if (pwdErrors.Count > 0)
        //        {
        //            return Ok(new { Ok = false, Message = "Password is not valid. Message: " + string.Join(", ", pwdErrors) });
        //        }

        //        await _userManager.RemovePasswordAsync(user);
        //        var result = await _userManager.AddPasswordAsync(user, model.NewPassword);

        //        if (!result.Succeeded)
        //        {
        //            return Ok(new { Ok = false, Message = "Update password failed. Error: " + string.Join(", ", result.Errors.Select(e => e.Description).ToArray()) });
        //        }

        //        return Ok(new { Ok = true, Message = "Your password has been changes." });
        //    }
        //    catch (Exception ex)
        //    {
        //        var result = StatusCode(StatusCodes.Status500InternalServerError, new { Ok = false, ex.Message });
        //        return result;
        //    }
        //}
    }
}