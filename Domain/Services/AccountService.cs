using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using Domain.DAL;
using Domain.Helper;
using Domain.Models;
using Microsoft.AspNet.Identity;

namespace Domain.Services
{
    public class AccountService:IDisposable
    {
        readonly BlogContext _context = new BlogContext();
        const string IdentityProviderClaimType =
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

        const string DefaultIdentityProviderClaimValue = "ASP.NET Identity";

        public User Find(string email)
        {
            return _context.Users.FirstOrDefault(x => x.Email == email);
        }

        public User Find(int userId)
        {
            return _context.Users.FirstOrDefault(x => x.UserId == userId);
        }

        public void Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public bool Exist(string email)
        {
            return _context.Users.Count(x => x.Email == email) > 0;
        }

        public User Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public ClaimsIdentity CreateIdentity(User user, string authenticationType)
        {
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            identity.AddClaim(new Claim(IdentityProviderClaimType, DefaultIdentityProviderClaimValue,
                ClaimValueTypes.String));
            if (user.Email.Equals("1176807665@qq.com"))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            }
            return identity;
        }

        public OperationResult<string> GenerateCode(int userId, string newEmail)
        {
            var user = Find(userId);
            if (Exist(newEmail))
            {
                return new OperationResult<string>(false, "该邮箱已注册");
            }
            var securityToken = new SecurityToken(Encoding.Unicode.GetBytes(user.SecurityStamp));
            int code = Rfc6238AuthenticationService.GenerateCode(securityToken, newEmail);
            EmailService.SendEmail(EmailType.ResetEmail, newEmail, user.UserName, "", code.ToString());
            return new OperationResult<string>(true, "验证码已发送至你的新邮箱");
        }
        public User ValidateCode(int userId, int code, string newEmail)
        {
            var user = Find(userId);
            var securityToken = new SecurityToken(Encoding.Unicode.GetBytes(user.SecurityStamp));
            if (Rfc6238AuthenticationService.ValidateCode(securityToken, code, newEmail))
            {
                user.Email = newEmail;
                Update(user);
                return user;
            }
            return null;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}