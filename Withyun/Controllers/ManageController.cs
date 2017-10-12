using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Domain.Services;
using Withyun.ViewModels;
using Domain.Helper;
using System.Drawing;
using NLogUtility;

namespace Withyun.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {

        readonly AccountService _accountService=new AccountService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _accountService.Dispose();
            }
            base.Dispose(disposing);
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
            var userId = User.Identity.GetUserId<int>();
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
            var userId = User.Identity.GetUserId<int>();
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
            var userId = User.Identity.GetUserId<int>();
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
            string imgUrl="/images/user/"+User.Identity.GetUserId()+".jpg";

            if (System.IO.File.Exists(Server.MapPath(imgUrl)))
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
        public ActionResult UploadAvatar(FormCollection form)
        {
            try
            {
                HttpPostedFileBase file = Request.Files["fileField"];

                string uploadFolder = "/images/user/";
                string imgName = User.Identity.GetUserId()+".jpg";
                string uploadPath = string.Empty;
                uploadPath = Server.MapPath(uploadFolder);
                file.SaveAs(uploadPath + imgName);

                try
                {
                    //等比例缩放图片
                    uploadFolder = "/images/avatar/";
                    uploadPath = Server.MapPath(uploadFolder);
                    string zoomedPicFullPath = uploadPath + imgName;

                    // 获取等比例缩放 UploadedImgUrl 后的图片路径
                    Image newImg = ImgHandler.ZoomPicture(Image.FromStream(file.InputStream) , 50, 50);
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
        public ActionResult UploadAlipay(FormCollection form)
        {
            try
            {
                HttpPostedFileBase file = Request.Files["fileField"];
                //等比例缩放图片
                string uploadFolder = "/images/alipay/";
                int userId = User.Identity.GetUserId<int>();
                string imgName = userId + ".jpg";
                string uploadPath = Server.MapPath(uploadFolder);
                string zoomedPicFullPath = uploadPath + imgName;

                // 获取等比例缩放 UploadedImgUrl 后的图片路径
                Image newImg = ImgHandler.ZoomPictureProportionately(Image.FromStream(file.InputStream), 200, 200);
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
        public ActionResult UploadWechat(FormCollection form)
        {
            try
            {
                HttpPostedFileBase file = Request.Files["fileField"];
                //等比例缩放图片
                string uploadFolder = "/images/wechat/";
                int userId = User.Identity.GetUserId<int>();
                string imgName = userId + ".jpg";
                string uploadPath = Server.MapPath(uploadFolder);
                string zoomedPicFullPath = uploadPath + imgName;

                // 获取等比例缩放 UploadedImgUrl 后的图片路径
                Image newImg = ImgHandler.ZoomPictureProportionately(Image.FromStream(file.InputStream), 200, 200);
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
        public string GetAvatar(string id)
        {
            return GetImg("avatar", id);
        }

        [AllowAnonymous]
        public string GetAlipayImg(string id)
        {
            return GetImg("alipay", id);
        }

        [AllowAnonymous]
        public string GetWechatImg(string id)
        {
            return GetImg("wechat", id);
        }
        [AllowAnonymous]
        public string GetUserImg(string id)
        {
            return GetImg("user", id);
        }

        private string GetImg(string category, string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                id = User.Identity.GetUserId();
            string path = "~/images/" + category + "/" + id + ".jpg";
            string diskPath = Server.MapPath(path);
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
            int useId = User.Identity.GetUserId<int>();
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
            var userId = User.Identity.GetUserId<int>();
            string path = "~/images/alipay/" + userId + ".jpg";
            string diskPath = Server.MapPath(path);
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
            var userId = User.Identity.GetUserId<int>();
            string path = "~/images/wechat/" + userId + ".jpg";
            string diskPath = Server.MapPath(path);
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

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
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