using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Legend.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Legend.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class IdentityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IdentityController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var users = _context.Users
                .Include("Claims")
                .Select(u => new
                {
                    u.UserName,
                    u.Claims
                }).ToList();

                return Ok(users);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
