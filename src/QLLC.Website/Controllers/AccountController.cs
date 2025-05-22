using LinqKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.SecurityLogin;
using Tasin.Website.Common.ConfigModel;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.ViewModels.AccountViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tasin.Website.Common.Services;

namespace Tasin.Website.Controllers
{
    //[Authorize]
    public class AccountController : BaseController<AccountController>
    {
        private readonly IUserService _userService;

        private readonly IConfiguration _configuration;
        //private readonly IEmailSender _emailSender; TODO
        private readonly IHttpContextAccessor _accessor;

        public object ContextType { get; private set; }

        public AccountController(
            IUserService userService,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _userService = userService;
            _configuration = configuration;
            //_emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var notification = TempData.Get<string>("Notification");
            var username = "";
            var password = "";
            var rememberme = false;
            if (Request.Cookies["RememberMe"] != null)
            {
                string value = Request.Cookies["RememberMe"];
                string[] parts = value.Split('|');
                if (parts.Length == 2)
                {
                    username = parts[1];
                    password = Utils.DecodePassword(parts[0] ?? string.Empty, "sha256") ?? "";
                    rememberme = true;
                }
            }
            ViewBag.Username = username;
            ViewBag.Password = password;
            ViewBag.Rememberme = rememberme;

            int loginFailedTimes = GetLoginFailedTimesInCookies();
            SetViewBagDataForLoginView(loginFailedTimes);
            if (returnUrl == "/Account/Logout") returnUrl = "";
            if (returnUrl != null)
                ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Notification = notification ?? string.Empty;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl ="")
        {
            //Logger.LogInformation("logging");
            int loginFailedTimes = GetLoginFailedTimesInCookies();
            #region SET DATA FOR VIEWS
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.UIConfig = UIConfigs;
            #endregion
            #region CHECK LOGIN AND CAPTCHA
            //Check captcha khi số lần đăng nhập thất bại > 0
            if (loginFailedTimes > 0 && !new Captcha().ValidateCaptchaCode(model.Captcha, Request, Response))
            {
                SetViewBagDataForLoginView(loginFailedTimes);
                ModelState.AddModelError("", "Mã bảo mật không đúng!");
                return View(model);
            }

            //Validate loginviewModel
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                {
                    SetViewBagDataForLoginView(loginFailedTimes);
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không được để trống");
                    return View(model);
                }

                var userDBResponse = await UserService.Login(model);
                if (userDBResponse.IsSuccess == false)
                {
                    SetViewBagDataForLoginView(loginFailedTimes);
                    ModelState.AddModelError("", userDBResponse.ErrorMessageList.FirstOrDefault()?.ToString());
                    return View(model);
                }
                var userDB = userDBResponse.Data;

                //Check if user is not exist => Login fail => Cookies ++
                if (userDB == null)
                {
                    loginFailedTimes++;
                    SetLoginFailedTimesInCookies(loginFailedTimes, model.UserName);
                    SetViewBagDataForLoginView(loginFailedTimes, model.UserName);
                    if (loginFailedTimes > DefaultConfig.MaxNumOfLoginFailed)
                    {
                        await LockUser(model.UserName);
                        return RedirectToAction("Login");
                    }

                    ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu. Bạn còn " + (DefaultConfig.MaxNumOfLoginFailed - loginFailedTimes + 1) + " lần đăng nhập");
                    return View(model);
                }

                //if (!userDB.IsActived)
                //{
                //    SetViewBagDataForLoginView(loginFailedTimes);
                //    ModelState.AddModelError("", "Tài khoản chưa kích hoạt, vui lòng kiểm tra lại");
                //    return View(model);
                //}

                //Login success => Remove all times login failed
                RemoveLoginFailedTimesCookies();
                #endregion
                // check if password is correct
                model.Password = Utils.EncodePassword(model.Password, EEncodeType.SHA_256);
                if (!model.Password.Equals(userDB.Password))
                {
                    loginFailedTimes++;
                    SetLoginFailedTimesInCookies(loginFailedTimes, model.UserName);
                    SetViewBagDataForLoginView(loginFailedTimes, model.UserName);
                    if (loginFailedTimes > DefaultConfig.MaxNumOfLoginFailed)
                    {
                        LockUser(model.UserName);
                        return RedirectToAction("Login");
                    }

                    ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu. Bạn còn " + (DefaultConfig.MaxNumOfLoginFailed - loginFailedTimes + 1) + " lần đăng nhập");
                    return View(model);
                }

                #region Claims and set Cookies
                var handlingClaimModel = Helper.GenerateLoginClaim(new Helper.LoginClaim()
                {
                    AccountType = model.AccountType,
                    EnumActionList = userDB.EnumActionList,
                    RoleIdList = userDB.RoleIdList,
                    UserId = userDB.Id,
                    IsMobile = model.IsMobile,
                    UserName = userDB.UserName,
                    Password = model.Password,
                    RememberMe = model.RememberMe
                });
                var account = handlingClaimModel.LoginViewModel;
                var principal = handlingClaimModel.ClaimsPrincipal;
                var authProperties = handlingClaimModel.AuthenticationProperties;

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties).ConfigureAwait(false);
                Response.Cookies.Delete("RememberMe");
                if (model.RememberMe == true)
                {

                    var encodePassword = Utils.EncodePassword(account.Password, EEncodeType.SHA_256);
                    Response.Cookies.Append("RememberMe", encodePassword + "|" + account.UserName, new CookieOptions()
                    {
                        Expires = DateTime.Now.AddDays(30)
                    });
                }
                #endregion
            }
            else
            {
                SetViewBagDataForLoginView(loginFailedTimes);
            }

            return View(model);
        }
        public async Task<IActionResult> Logout(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                TempData.Put("Notification", message);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Logger.LogInformation(HttpContext.User.Identity.Name + " is logout");
            return RedirectToAction("Login");
        }


        public IActionResult CheckLogin()
        {
            var claims = HttpContext.User.Claims;
            return null;
        }

        private int GetLoginFailedTimesInCookies()
        {
            if (Request.Cookies[DefaultConfig.LoginFailedCookieName] != null)
            {
                //format: loginFailedTimes|username
                string value = Request.Cookies[DefaultConfig.LoginFailedCookieName];
                string[] parts = value.Split('|');
                if (parts.Length != 2) //chống hack
                    return DefaultConfig.MaxNumOfLoginFailed - 1;

                int retVal = 0;
                string loginFailedTimes = parts[0];
                if (int.TryParse(loginFailedTimes, out retVal))
                    return retVal;

                return DefaultConfig.MaxNumOfLoginFailed - 1;
            }

            return 0;
        }


        private string GetUserNameLoginFailed()
        {
            if (Request.Cookies[DefaultConfig.LoginFailedCookieName] != null)
            {
                //format: loginFailedTimes|username
                string value = Request.Cookies[DefaultConfig.LoginFailedCookieName];
                string[] parts = value.Split('|');
                if (parts.Length != 2) //chống hack
                    return string.Empty;

                return parts[1];
            }

            return string.Empty;
        }

        private void SetLoginFailedTimesInCookies(int value, string userName)
        {
            RemoveLoginFailedTimesCookies();
            Response.Cookies.Append(DefaultConfig.LoginFailedCookieName, value.ToString() + "|" + userName, new CookieOptions()
            {
                Expires = DateTime.Now.AddMinutes(DefaultConfig.LockTimeMinutes)
            });
        }

        private void RemoveLoginFailedTimesCookies()
        {
            Response.Cookies.Delete(DefaultConfig.LoginFailedCookieName);
        }

        private void SetViewBagDataForLoginView(int loginFailedTimes, string userNameLoginFailed = null)
        {
            if (loginFailedTimes > DefaultConfig.MaxNumOfLoginFailed)
            {
                if (userNameLoginFailed == null)
                    userNameLoginFailed = GetUserNameLoginFailed();
                ViewBag.IsLock = true;
                ViewBag.UserNameLoginFailed = userNameLoginFailed;
                ViewBag.MaxNumOfLoginFailed = DefaultConfig.MaxNumOfLoginFailed;
            }
            else
            {
                ViewBag.IsLock = false;
            }
            ViewBag.NumOfLoginFailed = loginFailedTimes;
        }
        [AllowAnonymous]
        public async Task LockUser(string userName)
        {
            await UserService.LockUser(userName);
        }

        [AllowAnonymous]
        public async Task<IActionResult> LoginByToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            // Validate the token
            var handler = new JwtSecurityTokenHandler();
            var tokenSecretKey = Utils.DecodePassword(_configuration.GetSection("JWT:SecretKey").Value, EEncodeType.SHA_256);
            var key = Encoding.UTF8.GetBytes(tokenSecretKey); // Use the same key as in your token generation

            try
            {
                var claimsPrincipal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.FromSeconds(5)
                }, out SecurityToken validatedToken);

                // Token is valid, proceed with sign-in
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal);

                var userId = claimsPrincipal.Claims.FirstOrDefault(c=>c.Type == "UserID").Value;

                return Redirect("/"); // Redirect to home or another page
            }
            catch (SecurityTokenException)
            {
                return RedirectToAction("Login");
                //return Unauthorized("Đường link đã hết hạn.");
            }
        }
    }
}
