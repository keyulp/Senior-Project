using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senior_Project.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;

namespace Senior_Project.Controllers
{
    public class UserController : Controller
    {
        //Register Action
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        //Register Post Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "IsEmailVerified, ActivationCode")] User user)
        {
            //Viewbag Variables
            bool Status = false;
            string Message = "";

            //Model Validation
            if(ModelState.IsValid)
            {
                #region Check if Email already exists

                var doesExist = DoesEmailExist(user.EmailId);
                if (doesExist)
                {
                    ModelState.AddModelError("EmailExists", "Email is already in use");
                    return View(user);
                }
                #endregion

                #region Check if Username already exists
                var available = DoesUsernameExist(user.UserName);
                if (available)
                {
                    ModelState.AddModelError("UsernameExists", "Username is already in use");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion

                user.IsEmailVerified = false;

                #region Save Data to Database
                using (SPDatabaseEntities entities = new SPDatabaseEntities())
                {
                    entities.Users.Add(user);
                    entities.SaveChanges();
                }
                #endregion

                #region Send Email to user
                SendVerificationEmail(user.EmailId, user.ActivationCode.ToString());
                Message = "Registration Successfully Completed. Account Activation Link" +
                    " has been sent to your Email Address: " + user.EmailId;
                Status = true;
                #endregion
            }
            else
            {
                Message = "Invalid Request";
            }

            ViewBag.Message = " "+Message;
            ViewBag.Status = Status;
            return View(user);
        }

        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;

            using (SPDatabaseEntities entitites = new SPDatabaseEntities())
            {
                entitites.Configuration.ValidateOnSaveEnabled = false;

                var v = entitites.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    entitites.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }

            ViewBag.Status = Status;
            return View();
        }

        //Login Action
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login Post Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl)
        {
            string message = "";
            using (SPDatabaseEntities entities = new SPDatabaseEntities())
            {
                var v = entities.Users.Where(a => a.EmailId == login.EmailId && a.UserName == login.UserName).FirstOrDefault();
                if(v != null)
                {
                    if(string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(login.EmailId, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
                        {
                            Expires = DateTime.Now.AddMinutes(timeout),
                            HttpOnly = true
                        };
                        Response.Cookies.Add(cookie);


                        if(Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Login", "User");
                        }
                    }
                    else
                    {
                        message = "Invalid Credentials Provided";
                    }
                }
                else
                {
                    message = "Invalid Credentials Provided";
                }
            }

            ViewBag.Message = message;
            return View();
        }


        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");   
        }

        [NonAction]
        public bool DoesEmailExist(string emailId)
        {
            using (SPDatabaseEntities entities = new SPDatabaseEntities())
            {
                var v = entities.Users.Where(a => a.EmailId == emailId).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public bool DoesUsernameExist(string username)
        {
            using (SPDatabaseEntities entities = new SPDatabaseEntities())
            {
                var v = entities.Users.Where(a => a.UserName == username).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationEmail(string emailId, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("gameanimelist@gmail.com", "Game and Anime List");
            var toEmail = new MailAddress(emailId);
            var fromEmailPassword = "aang9713406713";
            string subject = "Your GAL account is successfully created";
            string body = "<br/><br/> We are excited to tell you that your GAL account is" + 
                " successfully created. Please click on the link below to verify your acccount" + 
                " <br/><br/><a href='"+link+"'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                
            smtp.Send(message);
        }

    }
}