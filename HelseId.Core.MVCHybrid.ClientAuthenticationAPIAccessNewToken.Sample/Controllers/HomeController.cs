﻿using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using HelseId.Core.MVCHybrid.ClientAuthenticationAPIAccessNewToken.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using JWT.Builder;
using IdentityModel.Client;

namespace HelseId.Core.MVCHybrid.ClientAuthenticationAPIAccessNewToken.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Settings _settings;

        public HomeController(ILogger<HomeController> logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            ViewData["Message"] = "Secure page.";
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CallApi()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(token);
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); //Can also be used

            var result = await client.GetStringAsync(_settings.ApiUrl); 
            ViewBag.Json = JArray.Parse(result.ToString());
            ViewBag.AccessToken = new JwtBuilder().Decode(token);
            return View();
        }

        public IActionResult Logout()
        {
            return SignOut(_settings.SignInScheme, _settings.DefaultChallengeScheme);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
    }
}
