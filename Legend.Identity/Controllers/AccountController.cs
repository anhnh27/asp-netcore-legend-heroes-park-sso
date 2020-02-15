using System;
using System.Threading.Tasks;
using IdentityModel;
using Legend.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Legend.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Legend.Identity.Custom;
using System.Net.Mail;
using System.Net;

namespace Legend.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

                    //TODO: send reset link to user email
                    try
                    {
                        var senderEmail = new MailAddress("legendheroesparkopenplatform@gmail.com", "legendheroesparkopenplatform@gmail.com");
                        var password = "legend@2019";
                        var subject = "Legend Heroes Park - Reset Password";
                        var body = "An email has been sent to your email. Please check your email and follow instruction to reset your password!";

                        var receiverEmail = new MailAddress(user.Email);

                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                        SmtpServer.Port = 587;
                        SmtpServer.UseDefaultCredentials = false;
                        SmtpServer.EnableSsl = true;
                        SmtpServer.Credentials = new NetworkCredential("legendheroesparkopenplatform@gmail.com", password);

                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(senderEmail.Address);
                        mail.To.Add(senderEmail.Address);
                        mail.Subject = subject;
                        mail.Body = body;

                        SmtpServer.Send(mail);

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