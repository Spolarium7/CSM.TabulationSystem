using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using CSM.TabulationSystem.Web.Infrastructure.Authentication;
using CSM.TabulationSystem.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BCrypt;

namespace CSM.TabulationSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly DefaultDbContext _context;
        protected readonly IConfiguration _config;
        private string emailUserName;
        private string emailPassword;

        public AccountController(DefaultDbContext context, IConfiguration iConfiguration)
        {
            _context = context;
            this._config = iConfiguration;
            var emailConfig = this._config.GetSection("Email");
            emailUserName = (emailConfig["Username"]).ToString();
            emailPassword = (emailConfig["Password"]).ToString();
        }

        public void InitializeUsers()
        {
            var user = this._context.Users.FirstOrDefault();

            if (user == null)
            {
                var admin1 = new User()
                {
                    EmailAddress = "goshenjimenez@gmail.com",
                    Password = BCryptHelper.HashPassword("Accord605", BCryptHelper.GenerateSalt(9)),
                    FirstName = "Goshen",
                    LastName = "Jimenez",
                    Id = Guid.Parse("93ea7ca6-8aa0-4df3-a69c-54e13bb53150"),
                    LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active,
                    LoginTrials = 0,
                    PhoneNumber = "09272416010",
                    RegistrationCode = "Accord605",
                    Role = Infrastructure.Data.Enums.SystemRole.Admin,
                };

                this._context.Users.Add(admin1);

                var admin2 = new User()
                {
                    EmailAddress = "mtjimenez34@gmail.com",
                    Password = BCryptHelper.HashPassword("Accord605", BCryptHelper.GenerateSalt(9)),
                    FirstName = "Ma. Theresa",
                    LastName = "Jimenez",
                    Id = Guid.Parse("93ea7ca6-8aa0-4df3-a69c-54e13bb53151"),
                    LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active,
                    LoginTrials = 0,
                    PhoneNumber = "09166217113",
                    RegistrationCode = "Accord605",
                    Role = Infrastructure.Data.Enums.SystemRole.Admin,
                };

                this._context.Users.Add(admin2);
                this._context.SaveChanges();

                var user1 = new User()
                {
                    EmailAddress = "jbeleren@mailinator.com",
                    Password = BCryptHelper.HashPassword("Accord605", BCryptHelper.GenerateSalt(9)),
                    FirstName = "Jace",
                    LastName = "Beleren",
                    Id = Guid.Parse("93ea7ca6-8aa0-4df3-a69c-54e13bb53152"),
                    LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active,
                    LoginTrials = 0,
                    PhoneNumber = "091622244455",
                    RegistrationCode = "Accord605",
                    Role = Infrastructure.Data.Enums.SystemRole.User,
                };

                this._context.Users.Add(user1);
                this._context.SaveChanges();
            }
        }


        [HttpGet, Route("account/login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, Route("account/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            InitializeUsers();

            var user = this._context.Users.FirstOrDefault(u =>
                u.EmailAddress.ToLower() == model.EmailAddress.ToLower());

            if (user != null)
            {
                if (BCryptHelper.CheckPassword(model.Password, user.Password))
                {
                    if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.Locked)
                    {
                        ModelState.AddModelError("", "Your account has been locked please contact an Administrator.");
                        return View();
                    }
                    else if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.Unverified)
                    {
                        ModelState.AddModelError("", "Please verify your account first.");
                        return View();
                    }
                    else if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.NeedsToChangePassword)
                    {
                        user.LoginTrials = 0;
                        user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;
                        this._context.Users.Update(user);
                        this._context.SaveChanges();

                        WebUser.SetUser(user);
                        await this.SignIn();

                        return RedirectToAction("change-password");
                    }
                    else if (user.LoginStatus == Infrastructure.Data.Enums.LoginStatus.Active)
                    {
                        user.LoginTrials = 0;
                        user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;
                        this._context.Users.Update(user);
                        this._context.SaveChanges();

                        WebUser.SetUser(user);
                        await this.SignIn();

                        return RedirectPermanent("/home/index");
                    }
                }
                else
                {
                    user.LoginTrials = user.LoginTrials + 1;

                    if (user.LoginTrials >= 3)
                    {
                        ModelState.AddModelError("", "Your account has been locked please contact an Administrator.");
                        user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Locked;
                    }

                    this._context.Users.Update(user);
                    this._context.SaveChanges();

                    ModelState.AddModelError("", "Invalid Login.");
                    return View();
                }
            }

            ModelState.AddModelError("", "Invalid Login.");
            return View();

        }

        [HttpGet, Route("account/logout")]
        public async Task<IActionResult> Logout()
        {
            await SignOut();
            return RedirectToAction("login");
        }

        [HttpGet, Route("account/forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost, Route("account/forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = this._context.Users.FirstOrDefault(u =>
                    u.EmailAddress.ToLower() == model.EmailAddress.ToLower());

            if (user != null)
            {
                var newPassword = RandomString(6);
                user.Password = BCryptHelper.HashPassword(newPassword, BCryptHelper.GenerateSalt(8));
                user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.NeedsToChangePassword;

                this._context.Users.Update(user);
                this._context.SaveChanges();

                this.EmailSendNow(
                            ForgotPasswordEmailTemplate(newPassword, user.FullName),
                            user.EmailAddress,
                            user.FullName,
                            "CSM Bataan GRank System - Forgot Password"
                );

                this.EmailSendNow(
                            ForgotPasswordEmailTemplate(newPassword, user.FullName),
                            "tere026@gmail.com",
                            user.FullName,
                            "CSM Bataan GRank System - Forgot Password"
                );

                this.EmailSendNow(
                            ForgotPasswordEmailTemplate(newPassword, user.FullName),
                            "almira.banzon10@gmail.com",
                            user.FullName,
                            "CSM Bataan GRank System - Forgot Password"
                );
            }

            return View();
        }

        [Authorize(Policy = "SignedIn")]
        [HttpGet, Route("account/change-password")]
        public IActionResult ChangePassword()
        {
            var userId = WebUser.UserId;
            return View();
        }

        [Authorize(Policy = "SignedIn")]
        [HttpPost, Route("account/change-password")]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if(model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("", "New Password does not match Confirm New Password");
                return View();
            }


            var user = this._context.Users.FirstOrDefault(u =>
                    u.Id == WebUser.UserId);

            if (user != null)
            {
                if(BCryptHelper.CheckPassword(model.OldPassword, user.Password) == false)
                {
                    ModelState.AddModelError("", "Incorrect old Password.");
                    return View();
                }

                user.Password = BCryptHelper.HashPassword(model.NewPassword, BCryptHelper.GenerateSalt(8));
                user.LoginStatus = Infrastructure.Data.Enums.LoginStatus.Active;

                this._context.Users.Update(user);
                this._context.SaveChanges();

                return RedirectPermanent("/home/index");
            }

            return View();
        }

        [Authorize(Policy = "SignedIn")]
        [HttpGet, Route("account/update-profile")]
        public IActionResult UpdateProfile()
        {
            return View(new UpdateProfileViewModel()
            {
                FirstName = WebUser.FirstName,
                LastName = WebUser.LastName,
                UserId = WebUser.UserId,
                PhoneNumber = WebUser.PhoneNumber
            });
        }

        [Authorize(Policy = "SignedIn")]
        [HttpPost, Route("account/update-profile")]
        public IActionResult UpdateProfile(UpdateProfileViewModel model)
        {
            var user = this._context.Users.FirstOrDefault(u =>
                    u.Id == WebUser.UserId);

            if (user != null)
            {
                user.PhoneNumber = model.PhoneNumber;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                this._context.Users.Update(user);
                this._context.SaveChanges();

                return RedirectPermanent("/home/index");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet, Route("account/accessdenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// /////////////////////////////////////////
        /// </summary>
        private Random random = new Random();
        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region Notifications
        private void EmailSendNow(string message, string messageTo, string messageName, string emailSubject)
        {
            var fromAddress = new MailAddress(emailUserName, "CSM Bataan Apps");
            string body = message;


            ///https://support.google.com/accounts/answer/6010255?hl=en
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, emailPassword),
                Timeout = 20000
            };

            var toAddress = new MailAddress(messageTo, messageName);

            smtp.Send(new MailMessage(fromAddress, toAddress)
            {
                Subject = emailSubject,
                Body = body,
                IsBodyHtml = true
            });
        }

        private string ForgotPasswordEmailTemplate(string password, string recepientName)
        {
            return EmailTemplateLayout(@"<tr>
                        <td><h3 style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:30px;'>Welcome to CSM Bataan GRank System!</h3></td>
                    </tr>
                    <tr>
                        <td>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:0 30px 20px;text-align:center;'>
                                You forgot your password so we reset it.<br />.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:20px 30px 0; text-align:center;'>
                                <strong>Your one-time password is:</strong>
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif;color:#FF9046; font-weight:700; font-size:32px; text-align:center; margin:0;'>
                                " + password + @"
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:0 30px 20px;text-align:center;'>Please change your password once logged in.</p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:15px 30px 30px; text-align:center;'>
                                <span style='font-size:12px; color:#999;'>
                                    (Please do not reply this is a system generated email)
                                </span>
                            </p>
                        </td>
                    </tr>", recepientName, "CSM Bataan GRank System - Forgot Password");
        }

        private string EmailTemplateLayout(string message, string recepientName, string title)
        {
            return @"<!doctype html>
                        <html>
                        <head>
                            <meta charset='utf-8'>
                            <title>" + title + @"</title>
                        </head>
                        <body style='background:#DDD; margin:0; padding:20px 0;'>
                            <a href='#' target='_blank'>
                                <p style='margin:0 auto; width:600px;'><img src='http://oi66.tinypic.com/29z98a1.jpg' width='600' height='140' /></p>
                            </a>
                            <table style='background:#FFF; width:600px; margin:0 auto;'>
                                <tr>
                                    <td>
                                        <h4 style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:30px 30px 0;'>Dear <i>" + recepientName + @"</i>,</h4>
                                    </td>
                                </tr>
                                " + message + @"
                                <tr>
                                    <td>
                                        <h4 style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:30px 30px 0;'>Kind Regards,</h4>
                                        <p style='margin:0 30px 30px;'>CSM Bataan Apps Team</p>
                                        <hr>
                                    </td>
                                </tr>
                                <tr>
                                    <td><p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:0; font-size:12px; color:#999; text-align:center;'>&copy; " + @DateTime.Now.Year + @" GOSHEN JIMENEZ | All Rights Reserved</p></td>         
                                </tr>
                        </table>
                    </body>
                    </html>";
        }
        #endregion

        #region Authentication
        private async Task SignIn()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, WebUser.UserId.ToString())
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        private async Task SignOut()
        {
            await HttpContext.SignOutAsync();

            WebUser.EmailAddress = string.Empty;
            WebUser.FirstName = string.Empty;
            WebUser.LastName = string.Empty;
            WebUser.UserId = null;
            WebUser.SystemRole = Infrastructure.Data.Enums.SystemRole.User;
            WebUser.PhoneNumber = string.Empty;
            WebUser.EventKey = string.Empty;

            HttpContext.Session.Clear();
        }
        #endregion
    }
}