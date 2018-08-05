using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Withyun.Web.Models;
using Withyun.Infrastructure.Services;
using Withyun.Infrastructure.Utility;
using Microsoft.AspNetCore.Authentication;
using Withyun.Core.Dtos;
using Microsoft.AspNetCore.Identity;
using Withyun.Core.Entities;
using System.Threading.Tasks;

namespace Withyun.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        readonly AccountService _accountService;
        readonly EmailService _emailService;
        readonly SignInManager<User> _signInManager;
        public AccountController(AccountService accountService,EmailService emailService, SignInManager<User> signInManager)
        {
            _accountService = accountService;
            _emailService = emailService;
            _signInManager = signInManager;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _accountService.Find(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email","该邮箱尚未注册Withyun");
            }
            else if(!user.EmailConfirmed)
            {
                var code = Security.GenerateUserToken("Confirmation", user.UserId, user.SecurityStamp);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.UserId, code = code }, protocol: Request.Scheme);
                ViewBag.Link = callbackUrl;
                return View("DisplayEmail");
            }
            else if(user.PasswordHash==Security.Sha256(model.Password))
            {
                user.LoginTime = DateTime.Now;
                _accountService.Update(user);
                var identity = _accountService.CreateIdentity(user);
                var principal = _accountService.CreatePrincipal(identity);
                _signInManager.SignOutAsync();
                _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties { IsPersistent = true });
                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "邮箱或密码输入错误");
            }
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult PartialLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _accountService.Find(model.Email);
            if (user == null)
            {
                return Json(new OperationResult<string>(false, "该邮箱尚未注册Withyun"));
            }
            else if (!user.EmailConfirmed)
            {
                return Json(new OperationResult<string>(false, "当前账号尚未激活"));
            }
            else if (user.PasswordHash == Security.Sha256(model.Password))
            {
                user.LoginTime = DateTime.Now;
                _accountService.Update(user);
                var identity = _accountService.CreateIdentity(user);
                var principal = _accountService.CreatePrincipal(identity);
                _signInManager.SignOutAsync();
                _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties { IsPersistent = true });
                return Json(new OperationResult<string>(true));
            }
            else
            {
                return Json(new OperationResult<string>(false, "邮箱或密码输入错误"));
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_accountService.Exist(model.Email))
                {
                    ModelState.AddModelError("Email", "邮箱已注册");
                }
                else
                {
                    var user = new User()
                    {
                        UserName = model.UserName,
                        PasswordHash = Security.Sha256(model.Password),
                        Email = model.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        CreateTime = DateTime.Now
                    };
                    user = _accountService.Add(user);
                    if (user.UserId > 0)
                    {
                        var code = Security.GenerateUserToken("Confirmation", user.UserId, user.SecurityStamp);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.UserId, code = code }, protocol: Request.Scheme);
                        _emailService.SendEmail(EmailType.Confirmation, user.Email, user.UserName, callbackUrl, "");
                        return View("DisplayEmail");
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "注册失败");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public ActionResult ConfirmEmail(int userId, string code)
        {
            if (code == null)
            {
                return View("Error");
            }
            var user = _accountService.Find(userId);
            if (user == null)
            {
                return View("Error");
            }

            var result = Security.ValidateUserToken("Confirmation", code, userId, user.SecurityStamp);
            if (result)
            {
                user.EmailConfirmed = true;
                user.SecurityStamp = Guid.NewGuid().ToString();
                _accountService.Update(user);
            }
            return View(result ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _accountService.Find(model.Email);
                if (user == null || !user.EmailConfirmed)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = Security.GenerateUserToken("ResetPassword", user.UserId, user.SecurityStamp);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.UserId, code = code }, protocol: Request.Scheme);
                //await UserManager.SendEmailAsync(user.Id, "重置密码", "点击以下链接重置密码: <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>");
                _emailService.SendEmail(EmailType.ResetPassword, user.Email, user.UserName, callbackUrl, "");
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code,string userId)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _accountService.Find(model.UserId);
            if (user == null)
            {
                return View(model);
            }
            var result = Security.ValidateUserToken("ResetPassword", model.Code, user.UserId, user.SecurityStamp);
            if (!result)
            {
                ModelState.AddModelError("", "链接已失效");
            }
            else
            {
                user.PasswordHash = Security.Sha256(model.Password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.LoginTime = DateTime.Now;
                _accountService.Update(user);
                var identity = _accountService.CreateIdentity(user);
                var principal = _accountService.CreatePrincipal(identity);
                _signInManager.SignOutAsync();
                _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties { IsPersistent = true });
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        public async Task<ActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult CheckLogin()
        {
            return Json(new OperationResult(User.Identity.IsAuthenticated));
        }

        [AllowAnonymous]
        public ActionResult PartialLogin()
        {
            return PartialView();
        }

        [AllowAnonymous]
        public ActionResult LoginPartial()
        {
            return PartialView("_LoginPartial");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}