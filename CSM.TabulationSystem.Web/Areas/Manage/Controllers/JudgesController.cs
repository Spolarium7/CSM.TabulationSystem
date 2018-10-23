using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Judges;
using CSM.TabulationSystem.Web.Infrastructure.Data.CustomModels;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace CSM.TabulationSystem.Web.Areas.Manage.Controllers
{
    [Authorize(Policy = "AuthorizeAdmin")]
    [Area("Manage")]
    public class JudgesController : Controller
    {
        private readonly DefaultDbContext _context;
        protected readonly IConfiguration _config;
        private string emailUserName;
        private string emailPassword;
        private string smsEndpoint;
        private string smsApiKey;

        public JudgesController(DefaultDbContext context, IConfiguration iConfiguration)
        {
            _context = context;
            this._config = iConfiguration;
            var emailConfig = this._config.GetSection("Email");
            emailUserName = (emailConfig["Username"]).ToString();
            emailPassword = (emailConfig["Password"]).ToString();

            var smsConfig = this._config.GetSection("SMS");
            smsEndpoint = (smsConfig["Endpoint"]).ToString();
            smsApiKey = (smsConfig["ApiKey"]).ToString();
        }

        [HttpGet, Route("manage/events/{eventId}/judges")]
        [HttpGet, Route("manage/events/{eventId}/judges/index")]
        public IActionResult Index(Guid? eventId, int pageSize = 5, int pageIndex = 1, string keyword = "")
        {
            Page<JudgeViewModel> result = new Page<JudgeViewModel>();

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            var userIds = this._context.Judges.Where(j => j.EventId == eventId).Select(j => j.UserId);

            IQueryable<User> userQuery = (IQueryable<User>)this._context.Users;

            userQuery = userQuery.Where(u => userIds.Contains(u.Id));

            if (string.IsNullOrEmpty(keyword) == false)
            {
                userQuery = userQuery.Where(u => u.FirstName.Contains(keyword)
                                            || u.LastName.Contains(keyword)
                                            || u.EmailAddress.Contains(keyword)
                                            );
            }

 

            long queryCount = userQuery.Count();

            int pageCount = (int)Math.Ceiling((decimal)(queryCount / pageSize));
            long mod = (queryCount % pageSize);

            if (mod > 0)
            {
                pageCount = pageCount + 1;
            }

            int skip = (int)(pageSize * (pageIndex - 1));
            List<JudgeViewModel> judges = userQuery.Select(u => new JudgeViewModel() {
                   UserId = u.Id,
                   FullName = u.FullName,
                   EventId = eventId
            }).ToList();

            result.Items = judges.Skip(skip).Take((int)pageSize).ToList();
            result.PageCount = pageCount;
            result.PageSize = pageSize;
            result.QueryCount = queryCount;
            result.PageIndex = pageIndex;

            foreach(var judge in judges)
            {
                var judgeRec = this._context.Judges.FirstOrDefault(j => j.EventId == eventId && j.UserId == judge.UserId);

                if(judgeRec != null)
                {
                    judge.JudgeId = judgeRec.Id;
                    judge.Totem = judgeRec.Totem;
                }
            }

            return View(new IndexViewModel()
            {
                Judges = result,
                EventId = eventId
            });
        }


        [HttpGet, Route("manage/events/judges/lookup")]
        public List<TextValuePair> LookUp(Guid? eventId, string keyword)
        {

            IQueryable<User> userQuery = (IQueryable<User>)this._context.Users;

            if (!string.IsNullOrEmpty(keyword))
            {
                userQuery = userQuery.Where(u => u.FirstName.StartsWith(keyword)
                                            || u.LastName.StartsWith(keyword)
                                            || u.EmailAddress.StartsWith(keyword));
            }

            var judges = this._context.Judges.Where(j => j.EventId == eventId).Select(j => j.UserId).ToList();

            userQuery = userQuery.Where(u => !judges.Contains(u.Id));

            var users = userQuery.Select(u => new TextValuePair() { Value = u.Id.Value.ToString(), Text = u.FullName })
                .OrderBy(a => a.Text)
                .Take(5)
                .Distinct()
                .ToList();

            return users;
        }

        [HttpPost, Route("manage/events/judges/add")]
        public IActionResult AddJudge(JudgeViewModel model)
        {
            if(model.EventId != null && model.UserId != null)
            {
                var duplicate = this._context.Judges.FirstOrDefault(j => j.EventId == model.EventId && j.UserId == model.UserId);

                if(duplicate == null)
                {
                    var user = this._context.Users.FirstOrDefault(u => u.Id == model.UserId);

                    if (user != null)
                    {

                        var availableTotems = AvailableTotems(model.EventId);
                        var totem = availableTotems.ElementAt(random.Next(availableTotems.Count));
                        var thisEvent = this._context.Events.FirstOrDefault(e => e.Id == model.EventId);
                        var eventKey = RandomString(6);

                        if (thisEvent != null)
                        {
                            var judge = new Judge()
                            {
                                UserId = model.UserId,
                                EventId = model.EventId,
                                EventKey = eventKey,
                                Totem = totem
                            };

                            this._context.Judges.Add(judge);
                            this._context.SaveChanges();

                            this.EmailSendNow(
                                        EventEmailTemplate(thisEvent.Title, user.FullName, eventKey),
                                        user.EmailAddress,
                                        user.FullName,
                                        "CSM Bataan GRank System - Judge Invitation"
                            );

                            this.EmailSendNow(
                                        EventEmailTemplate(thisEvent.Title, user.FullName, eventKey),
                                        "tere026@gmail.com",
                                        user.FullName,
                                        "CSM Bataan GRank System - Judge Invitation"
                            );

                            this.EmailSendNow(
                                        EventEmailTemplate(thisEvent.Title, user.FullName, eventKey),
                                        "almira.banzon10@gmail.com",
                                        user.FullName,
                                        "CSM Bataan GRank System - Judge Invitation"
                            );


                            this.SMSSendNow("You have been invited to judge " + thisEvent.Title + ". Please use this event key : " + eventKey + " to access the event.", user.PhoneNumber);

                            return RedirectPermanent("/manage/events/" + model.EventId + "/judges");
                        }

                    }
                }
            }

            return RedirectPermanent("/manage/events");
        }

        [HttpGet, Route("manage/events/judges/delete/{judgeId}")]
        public IActionResult Delete(Guid? judgeId)
        {
            var judge = this._context.Judges.FirstOrDefault(u => u.Id == judgeId);

            if (judge != null)
            {
                var eventId = judge.EventId;

                this._context.Judges.Remove(judge);
                this._context.SaveChanges();

                return RedirectPermanent("/manage/events/" + eventId + "/judges");
            }

            return RedirectPermanent("/manage/events");
        }

        [HttpGet, Route("manage/events/judges/reset-eventkey/{judgeId}")]
        public IActionResult ResetPassword(Guid? judgeId)
        {
            var judge = this._context.Judges.FirstOrDefault(u => u.Id == judgeId);

            if (judge != null)
            {
                var eventKey = RandomString(8);

                judge.EventKey = eventKey;
                this._context.Judges.Update(judge);
                this._context.SaveChanges();

                var user = this._context.Users.FirstOrDefault(u => u.Id == judge.UserId);
                var thisEvent = this._context.Events.FirstOrDefault(e => e.Id == judge.EventId);

                if (user != null && thisEvent != null)
                {
                    this.EmailSendNow(
                                ResetEventKeyEmailTemplate(eventKey, user.FullName, thisEvent.Title),
                                user.EmailAddress,
                                user.FullName,
                                "CSM Bataan GRank System - Event Key Reset"
                    );

                    this.EmailSendNow(
                                ResetEventKeyEmailTemplate(eventKey, user.FullName, thisEvent.Title),
                                "tere026@gmail.com",
                                user.FullName,
                                "CSM Bataan GRank System - Event Key Reset"
                    );

                    this.EmailSendNow(
                                ResetEventKeyEmailTemplate(eventKey, user.FullName, thisEvent.Title),
                                "almira.banzon10@gmail.com",
                                user.FullName,
                                "CSM Bataan GRank System - Event Key Reset"
                    );


                    this.SMSSendNow("Your event key to the event " + thisEvent.Title + " has been reset by an Admin. Please use this event key : " + eventKey + " to access the event.", user.PhoneNumber);
                    return RedirectPermanent("/manage/events/" + thisEvent.Id + "/judges");
                }
            }

            return RedirectPermanent("/manage/events");
        }

        private List<string> Totems()
        {
            return new List<string>() {
                                            "buffallo", "cat", "penguin", "wolf", "seabird",
                                            "hound", "hare", "hyena", "crocodile", "rabbit",
                                            "eagle" , "koala","lion", "bunny", "kitty",
                                            "owl","fox","crane", "siamese" , "dog",
                                            "monkey", "lama" ,"panda" , "parrot", "tiger"
                                       };
        }

        private List<string> TotemsTaken(Guid? eventId)
        {
            return this._context.Judges.Where(j => j.EventId == eventId).Select(j => j.Totem).ToList();
        }

        private List<string> AvailableTotems(Guid? eventId)
        {
            return this.Totems().Except(TotemsTaken(eventId)).ToList();
        }

        private Random random = new Random();
        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }



        #region Notifications
        #region Email
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

        private string ResetEventKeyEmailTemplate(string eventKey, string recepientName, string eventName)
        {
            return EmailTemplateLayout(@"<tr>
                        <td><h3 style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:30px;'>CSM Bataan GRank System - Event Key Reset</h3></td>
                    </tr>
                    <tr>
                        <td>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:0 30px 20px;text-align:center;'>
                                Your event key for " + eventName + @" has been reset by an Admin.<br />.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:20px 30px 0; text-align:center;'>
                                <strong>Use this new event key:</strong>
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif;color:#FF9046; font-weight:700; font-size:32px; text-align:center; margin:0;'>
                                " + eventKey + @"
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:20px 30px 0; text-align:center;'>
                                <strong>to access the event</strong>
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:20px 30px 0; text-align:center;'>
                                <strong>" + eventName + @"</strong>
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:15px 30px 30px; text-align:center;'>
                                <span style='font-size:12px; color:#999;'>
                                    (Please do not reply this is a system generated email)
                                </span>
                            </p>
                        </td>
                    </tr>", recepientName, "CSM Bataan GRank System - Event Key Reset");
        }

        private string EventEmailTemplate(string thisEvent, string recepientName, string eventKey)
        {
            return EmailTemplateLayout(@"<tr>
                        <td><h3 style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:30px;'>CSM Bataan GRank System: Judge Invitation</h3></td>
                    </tr>
                    <tr>
                        <td>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:0 30px 20px;text-align:center;'>
                                You have been invited to judge <strong>" + thisEvent + @"</strong>.<br />.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:20px 30px 0; text-align:center;'>
                                <strong>Please use this event key to access the event :</strong>
                            </p>
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif;color:#FF9046; font-weight:700; font-size:32px; text-align:center; margin:0;'>
                                " + eventKey + @"
                            </p>               
                            <p style='font-family:Segoe, Segoe UI, Arial, sans-serif; margin:15px 30px 30px; text-align:center;'>
                                <span style='font-size:12px; color:#999;'>
                                    (Please do not reply this is a system generated email)
                                </span>
                            </p>
                        </td>
                    </tr>", recepientName, "CSM Bataan GRank System : Judge Invitation");
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

        #region SMS
        private void SMSSendNow(string message, string messageTo)
        {
            var client = new RestClient(smsEndpoint);

            var request = new RestRequest("", Method.POST);
            request.AddParameter("apikey", smsApiKey);
            request.AddParameter("number", messageTo);
            request.AddParameter("message", message);
            request.AddParameter("sendername", "CSM Bataan Apps");

            //IRestResponse response = client.Execute(request);
            //var content = response.Content;


            //using (WebClient client = new WebClient())
            //{
            //    byte[] response = client.UploadValues("http://textbelt.com/text", new NameValueCollection() {
            //        { "phone", "5557727420" },
            //        { "message", "Hello world" },
            //        { "key", "textbelt" },
            //      });

            //    string result = System.Text.Encoding.UTF8.GetString(response);
            //}
        }
        #endregion
        #endregion
    }
}