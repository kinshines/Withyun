using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Withyun.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Withyun.Web.Models;
using Withyun.Infrastructure.Utility;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Withyun.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        readonly AccountService _accountService;
        readonly IHostingEnvironment _env;
        public ManageController(AccountService accountService, IHostingEnvironment env)
        {
            _accountService = accountService;
            _env = env;
        }

        //
        // GET: /Account/Index
        public ActionResult Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "密码修改成功."
                : message == ManageMessageId.ChangeEmailSuccess ? "邮箱修改成功."
                : message == ManageMessageId.Error ? "出错啦."
                : "";
            var userId = GetUserId();
            var user = _accountService.Find(userId);
            var model = new IndexViewModel
            {
                Email = user.Email,
                HasAlipay = user.HasAlipay,
                HasWechat = user.HasWechat,
            };
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = GetUserId();
            var user = _accountService.Find(userId);
            if (user.PasswordHash != Security.Sha256(model.OldPassword))
            {
                ModelState.AddModelError("","原密码输入错误");
                return View(model);
            }
            user.PasswordHash = Security.Sha256(model.NewPassword);
            _accountService.Update(user);
            var identity = _accountService.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);
            return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
        }

        public ActionResult ChangeEmail()
        {
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = GetUserId();
            var user = _accountService.ValidateCode(userId, model.Code, model.NewEmail);
            var identity = _accountService.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);
            return RedirectToAction("Index", new { Message = ManageMessageId.ChangeEmailSuccess });
        }

        public ActionResult UploadAlipay()
        {
            return View();
        }
        public ActionResult UploadWeChat()
        {
            return View();
        }



        public ActionResult UploadAvatar()
        {
            string imgUrl="/images/user/"+ GetUserId()+".jpg";
            
            if (_env.ContentRootFileProvider.GetFileInfo(imgUrl).Exists)
            {
                ViewBag.ImgUrl = ".." + imgUrl;
            }
            else
            {
                ViewBag.ImgUrl = "../images/bg-4.png";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAvatar(IFormFile fileField)
        {
            try
            {

                string uploadFolder = "/images/user/";
                string imgName = GetUserId() +".jpg";
                string uploadPath = string.Empty;
                uploadPath = _env.ContentRootFileProvider.GetFileInfo(uploadFolder).PhysicalPath;
                using(var stream=new FileStream(uploadPath+ imgName, FileMode.Create))
                {
                    fileField.CopyTo(stream);
                }

                try
                {
                    //等比例缩放图片
                    uploadFolder = "/images/avatar/";

                    string zoomedPicFullPath = _env.ContentRootFileProvider.GetFileInfo(uploadFolder + imgName).PhysicalPath;

                    // 获取等比例缩放 UploadedImgUrl 后的图片路径
                    Image newImg = ImgHandler.ZoomPicture(Image.FromStream(fileField.OpenReadStream()) , 50, 50);
                    newImg.Save(zoomedPicFullPath);
                    newImg.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "等比例缩放图片错误");
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "头像保存错误");
            }
            return RedirectToAction("UploadAvatar");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAlipay(IFormFile fileField)
        {
            try
            {
                //等比例缩放图片
                string uploadFolder = "/images/alipay/";
                int userId = GetUserId();
                string imgName = userId + ".jpg";
                string zoomedPicFullPath = _env.ContentRootFileProvider.GetFileInfo(uploadFolder+ imgName).PhysicalPath;

                // 获取等比例缩放 UploadedImgUrl 后的图片路径
                Image newImg = ImgHandler.ZoomPictureProportionately(Image.FromStream(fileField.OpenReadStream()), 200, 200);
                newImg.Save(zoomedPicFullPath);
                newImg.Dispose();
                var user = _accountService.Find(userId);
                user.HasAlipay = true;
                _accountService.Update(user);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "支付宝二维码保存错误");
            }
            return RedirectToAction("UploadAlipay");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadWechat(IFormFile fileField)
        {
            try
            {
                //等比例缩放图片
                string uploadFolder = "/images/wechat/";
                int userId = GetUserId();
                string imgName = userId + ".jpg";
                string zoomedPicFullPath = _env.ContentRootFileProvider.GetFileInfo(uploadFolder + imgName).PhysicalPath;

                // 获取等比例缩放 UploadedImgUrl 后的图片路径
                Image newImg = ImgHandler.ZoomPictureProportionately(Image.FromStream(fileField.OpenReadStream()), 200, 200);
                newImg.Save(zoomedPicFullPath);
                newImg.Dispose();
                var user = _accountService.Find(userId);
                user.HasWechat = true;
                _accountService.Update(user);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "微信二维码保存错误");
            }
            return RedirectToAction("UploadWechat");
        }

        [AllowAnonymous]
        public string GetAvatar(int? id)
        {
            return GetImg("avatar", id);
        }

        [AllowAnonymous]
        public string GetAlipayImg(int? id)
        {
            return GetImg("alipay", id);
        }

        [AllowAnonymous]
        public string GetWechatImg(int? id)
        {
            return GetImg("wechat", id);
        }
        [AllowAnonymous]
        public string GetUserImg(int? id)
        {
            return GetImg("user", id);
        }

        private string GetImg(string category, int? id)
        {
            if (!id.HasValue)
                id = GetUserId();
            string path = "~/images/" + category + "/" + id + ".jpg";
            string diskPath = _env.ContentRootFileProvider.GetFileInfo(path).PhysicalPath;
            if (System.IO.File.Exists(diskPath))
            {
                //return File(diskPath, "image/jpeg");
                return "http://www.withyun.com/images/" + category + "/" + id + ".jpg";
            }

            //return File(Server.MapPath("~/images/" + category + "/noimage.png"), "image/png");
            return "http://www.withyun.com/images/" + category + "/noimage.png";
        }


        public ActionResult GenerateCode(string newEmail)
        {
            int useId = GetUserId();
            return Json(_accountService.GenerateCode(useId, newEmail));
        }

        [AllowAnonymous]
        public ActionResult AlipayTutorial()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult WechatTutorial()
        {
            return View();
        }

        public ActionResult DeleteAlipay()
        {
            var userId = GetUserId();
            string path = "~/images/alipay/" + userId + ".jpg";
            string diskPath = _env.ContentRootFileProvider.GetFileInfo(path).PhysicalPath;
            if (System.IO.File.Exists(diskPath))
            {
                System.IO.File.Delete(diskPath);
            }
            var user = _accountService.Find(userId);
            user.HasAlipay = false;
            _accountService.Update(user);
            return View("UploadAlipay");
        }

        public ActionResult DeleteWechat()
        {
            var userId = GetUserId();
            string path = "~/images/wechat/" + userId + ".jpg";
            string diskPath = _env.ContentRootFileProvider.GetFileInfo(path).PhysicalPath;
            if (System.IO.File.Exists(diskPath))
            {
                System.IO.File.Delete(diskPath);
            }
            var user = _accountService.Find(userId);
            user.HasWechat = false;
            _accountService.Update(user);
            return View("UploadWechat");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error,
            ChangeEmailSuccess,
            AddAlipaySuccess
        }

        #endregion
    }
}