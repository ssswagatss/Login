using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebSite.Models;

namespace WebSite.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            SuccessLoginResponse loginData = new SuccessLoginResponse();
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "UserName", model.UserName },
                    { "Password", model.Password },
                    { "grant_type", "password"}
                };

                var content = new FormUrlEncodedContent(values);
                var response =await client.PostAsync ("http://localhost:54486/Token", content);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    //var jsonObject = serializer.DeserializeObject(responseString);

                    loginData = JsonConvert.DeserializeObject<SuccessLoginResponse>(responseString);

                    FormsAuthentication.Initialize();
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                        loginData.UserName,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30), // value of time out property
                        false, // Value of IsPersistent property
                        loginData.AccessToken,
                        FormsAuthentication.FormsCookiePath);

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    if (ticket.IsPersistent)
                    {
                        cookie.Expires = ticket.Expiration;
                    }
                    Response.Cookies.Add(cookie);
                    Session["UserInfo"] = loginData;
                }
            }
                    return Json(loginData);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}