using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Legend.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Text;
using Legend.Admin.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Legend.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var users = new List<UserViewModel>();
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}/{1}", _configuration.GetSection("ResourceAPIUrl").Value, "api/identity"));
            request.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jResponse = await response.Content.ReadAsStringAsync();
                var jUsers = JArray.Parse(jResponse);
                foreach (var item in jUsers)
                {
                    users.Add(new UserViewModel
                    {
                        UserId = item.Value<string>("userId"),
                        Email = item.Value<string>("email"),
                        Picture = item.Value<string>("photo") ?? @Url.Content("~/images/avatar-01.png"),
                    });
                }
            }
            else
            {
                _logger.LogError("get user error");
                return StatusCode(500);
            }

            return View(users);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AddUser()
        {
            var roleLst = new[] {
                new SelectListItem() { Text = "Staff", Value = "staff" },
                new SelectListItem() { Text = "Manager", Value = "manager" },
            };

            var operationLst = new [] {
                new SelectListItem() { Text = "Scan QR", Value = OperationTypes.ScanQR.ToString() },
                new SelectListItem() { Text = "Scan Other QR", Value = OperationTypes.ScanOtherQR.ToString() },
                new SelectListItem() { Text = "Transfer Points", Value = OperationTypes.TransferPoints.ToString() },
                new SelectListItem() { Text = "Deduct Points", Value = OperationTypes.DeductPoints.ToString() },
            };

            ViewBag.Operations = operationLst;

            var model = new CreateUserViewModel()
            {
                SelectedOperations = new int[] { },
                OperationList = operationLst,

                RoleList = roleLst,
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddUser(UserViewModel user)
        {
            var client = new HttpClient();
            var response = await client.PostAsync(
                string.Format("{0}/{1}", _configuration.GetSection("ResourceAPIUrl").Value, "api/identity"),
                new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                _logger.LogError("get user error");
                return StatusCode(500);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Deactive(string UserId)
        {
            var client = new HttpClient();
            var response = await client.PutAsync(
                string.Format("{0}/{1}", _configuration.GetSection("ResourceAPIUrl").Value, "api/identity"),
                new StringContent(JsonConvert.SerializeObject(new { UserId }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                _logger.LogError("get user error");
                return StatusCode(500);
            }
        }

        public IActionResult LogOut()
        {
            return SignOut("Cookies", "oidc");
        }

        [AllowAnonymous]
        public IActionResult LoggedOut()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
