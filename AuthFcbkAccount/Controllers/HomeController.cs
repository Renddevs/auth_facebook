using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Facebook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.IO;
using AuthFcbkAccount.Models;

namespace CoreFacebookAuth.Controllers
{
    public class HomeController : Controller
    {
        string appid = string.Empty;
        string appsecret = string.Empty;

        public HomeController()
        {
            var configuration = GetConfiguration();
            appid = configuration.GetSection("AppID").Value;
            appsecret = configuration.GetSection("AppSecret").Value;
        }
        public IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Headers["Referer"].ToString());
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }

        public IActionResult Facebook()
        {
            var fb = new FacebookClient();
            var loginurl = fb.GetLoginUrl(new
            {
                client_id = appid,
                client_secret = appsecret,
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });
            return Redirect(loginurl.AbsoluteUri);
        }

        public IActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = appid,
                client_secret = appsecret,
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });
            var accesstoken = result.access_token;
            fb.AccessToken = accesstoken;
            dynamic data = fb.Get("me?fields=link,first_name,currency,last_name,email,gender,locale,timezone,verified,picture,age_range");
            TempData["email"] = data.email;
            TempData["name"] = data.first_name + " " + data.last_name;
            TempData["picture"] = data.picture.data.url;
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
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