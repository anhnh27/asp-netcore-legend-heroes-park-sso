using System;
using System.Linq;
using System.Text;
using Legend.API.Data;
using Legend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Legend.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class IdentityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(ApplicationDbContext context, ILogger<IdentityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var claims = _context.Users
                .Where(u => u.IsActive)
                .Include("Claims")
                .Select(u => new
                {
                    UserId = u.Id,
                    Email = u.Claims.Where(x => x.ClaimType == "email").FirstOrDefault().ClaimValue,
                    Photo = u.Claims.Where(x => x.ClaimType == "picture").FirstOrDefault().ClaimValue,
                });

                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]UserRequestModel request)
        {
            try
            {
                var user = _context.Users.Where(x => x.Email == request.Email).FirstOrDefault();
                if (user == null)
                {
                    _context.Users.Add(user);
                    _context.SaveChanges();

                    return Ok();
                }
                else
                {
                    throw new Exception("Email is already existed");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody]UserRequestModel request)
        {
            try
            {
                var user = _context.Users.Where(x => x.Id == request.UserId).FirstOrDefault();
                user.IsActive = false;
                _context.Users.Update(user);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
