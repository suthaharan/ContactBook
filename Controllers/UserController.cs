using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Contact2015.Models;
using Contact2015.Models.ViewModels;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Contact2015.Helpers;
using System.Web.Configuration;
using System.Text;
using System.Security.Cryptography;
using Contact2015.Filters;

namespace Contact2015.Controllers
{
    public class UserController : Controller
    {
        private Contacts2015Entities _dbContext = new Contacts2015Entities();



        public ActionResult Login()
        {
            if (Session["_userHash"] != null)
            {
                return RedirectToAction("index", "contact");
            }
            return View();
        }


        [HttpPost]
        public ActionResult Login(vmLogin _vmLogin)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var _frmEmail = _vmLogin.txtemail.ToString();
                    var _frmPassword = _vmLogin.txtpassword.ToString();
                    var encrypPass = CryptoDecrypt.SHA256Pass(_frmPassword);

                    var _vmLoginUser = _dbContext.users.SingleOrDefault(u => (u.email == _frmEmail) && (u.password == encrypPass));
                    if (null == _vmLoginUser)
                    {
                        ModelState.AddModelError("", "Invalid credentials");
                        return View();
                    }
                    Session["_userEmail"] = _frmEmail;
                    Session["_userId"] = _vmLoginUser.id;
                    Session["_userType"] = _vmLoginUser.usertype;

                    return RedirectToAction("manage", "contact");
                }
                else
                {
                   ModelState.AddModelError("", "Invalid login entry");
                   return View();
                }

                
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }

        }



        private bool isValidUserAccount(string passString)
        {
            var _passString = passString.ToLower().Trim().ToString();

            if (!String.IsNullOrEmpty(_passString))
            {
                var isPresent = _dbContext.users.FirstOrDefault(c => c.email == _passString);
                if (!String.IsNullOrEmpty(isPresent.id.ToString()))
                {
                    return true;
                }
                return false;
            }
            return false;
        }


        public ActionResult ForgotPassword() 
        {
            return View();
        }


        [HttpPost]
        public ActionResult ForgotPassword(vmForgotPassword vmforgot)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var isValidEmail = isValidUserAccount(vmforgot.email.ToLower().Trim().ToString());

                    if (isValidEmail)
                    {
                        ViewBag.MessageTitle = "Forgot Password";
                        ViewBag.Message = "We received your request to reset your password. Please check your email for instructions.";
                        var idhash = Guid.NewGuid();

                        var forgotpassUser = _dbContext.forgotPasses.Create();
                        var frmEmail = vmforgot.email.ToLower().Trim().ToString();
                        var fpPass = RandomPassword.Generate(8, 10);
                        forgotpassUser.email = frmEmail;
                        forgotpassUser.tempPassword = fpPass;
                        forgotpassUser.status = 0;
                        forgotpassUser.modified = DateTime.Now;
                        forgotpassUser.created = DateTime.Now;
                        _dbContext.forgotPasses.Add(forgotpassUser);
                        _dbContext.SaveChanges();


                        // send out email with hash
                        string emailTemplatePath = Server.MapPath("~/Helpers/emailTemplates/ForgotPassword.txt");
                        var fpurl = WebConfigurationManager.AppSettings["BASEURL"] + "/user/resetpassword/";
                        var emailBody = new StringBuilder(System.IO.File.ReadAllText(emailTemplatePath, System.Text.Encoding.UTF8));
                        emailBody.Replace("<FP_URL>", fpurl);
                        emailBody.Replace("<FP_PASS>", fpPass);
                        emailBody.Replace("<EMAILURL>", WebConfigurationManager.AppSettings["EMAILURL"]);
                        string defaultEmail = WebConfigurationManager.AppSettings["DEFAULTEMAIL"];
                        var emailData = new Dictionary<string, string>()
                        {
                            {"To", frmEmail},
                            {"Subject", "Reset Password"},
                            {"Body", emailBody.ToString()},
                            {"From", defaultEmail},
                            {"FromDescription", "Contacts Inc."}
                        };

                        try
                        {
                            MailHelper.SendMail(emailData);
                            ViewBag.Success = 1; // 0 - Error
                            ViewBag.MessageTitle = "Forgot Password";
                            ViewBag.Message = "Please check your email.";
                            return View();
                        }
                        catch (Exception ex)
                        {
                            #region Show error message

                            ModelState.AddModelError("", "Error in sending email. " + ex.Message);
                            return View();

                            #endregion
                        }


                    }
                        ViewBag.Success = 2; // 0 - Error
                        ViewBag.MessageTitle = "Forgot Password";
                        ViewBag.Message = "No record found matching the email address provided.";
                        //ModelState.AddModelError("", "No record found matching the email address provided. Please try again.");
                        return View();
                    
                    


                }
                else
                {
                    ModelState.AddModelError("", "Invalid form submission.");
                    return View();
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Not a valid form submission");
                return View();
            }
        }



        public ActionResult ResetPassword() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(vmReset _vmReset)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var _frmEmail = _vmReset.txtremail.ToString();
                    var _frmOldPassword = _vmReset.txtroldpwd.ToString();
                    var _frmPassword1 = _vmReset.txtrpwd1.ToString();
                    var _frmPassword2 = _vmReset.txtrpwd2.ToString();

                    var encrypPass = _frmOldPassword;
                    if (_frmPassword1 != _frmPassword2)
                    {
                        ViewBag.Success = 2; // 2 - Error
                        ViewBag.MessageTitle = "Reset Password";
                        ViewBag.Message = "Please enter valid data.";
                        return View();

                    }

                    var _vmFPUser = _dbContext.forgotPasses.SingleOrDefault(u => (u.email == _frmEmail) && (u.tempPassword == encrypPass));
                    if (null == _vmFPUser)
                    {
                        ModelState.AddModelError("", "Invalid credentials");
                        return View();
                    }
                    else
                    {

                        _vmFPUser.status = 1;
                        _vmFPUser.modified = DateTime.Now;

                        _dbContext.Entry(_vmFPUser).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        var _vmUser = _dbContext.users.SingleOrDefault(u => (u.email == _frmEmail));
                        _vmUser.password = CryptoDecrypt.SHA256Pass(_frmPassword1);
                        _dbContext.Entry(_vmUser).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        ViewBag.Success = 1; // 2 - Error
                        ViewBag.MessageTitle = "Reset Password";
                        ViewBag.Message = "Password updated successfully.";
                        return RedirectToAction("login", "user");

                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login entry");
                    return View();
                }

            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View();
            }   

        }

        [CheckSessionTimeout]
        public ActionResult MyProfile() 
        {
            var _sessId = Int32.Parse(Session["_userId"].ToString());
            var _sessEmail = Session["_userEmail"].ToString();

            try
            {

                var _userResults = (from e in _dbContext.users
                                    where e.id == _sessId
                                    select new vmUser
                                    {
                                        id = e.id,
                                        userfirst = e.firstName,
                                        userlast = e.lastName,
                                        useremail = e.email,
                                        userpass = e.password,
                                        selusertype = e.usertype
                                    }).FirstOrDefault();


                if (null == _userResults)
                {
                    return View("myprofile");
                }

                _userResults.userfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(_userResults.userfirst);
                _userResults.userlast = CryptoDecrypt.AES_256_WebKey_Decrypt(_userResults.userlast);
                return View(_userResults);


            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }   

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyProfile(vmUser _vmUser) 
        {
            try
            {
                var _sessId = Int32.Parse(Session["_userId"].ToString());
                var _sessEmail = Session["_userEmail"].ToString();

                // Check if valid email address 
                if (_sessEmail != _vmUser.useremail)
                {
                    var isValidEmail = isValidUserAccount(_vmUser.useremail.ToLower().Trim().ToString());
                    if (!isValidEmail)
                    {
                        ViewBag.UserExist = "Yes";
                        return View();
                    }
                }

                var _userResults = (from e in _dbContext.users
                                    where e.id == _vmUser.id
                                    select new vmUser
                                    {
                                        id = e.id,
                                        userfirst = e.firstName,
                                        userlast = e.lastName,
                                        useremail = e.email,
                                        userpass = e.password,
                                        selusertype = e.usertype
                                    }).FirstOrDefault();


                if (null == _userResults)
                {
                    //Add suitable error message for viewbag
                    return View("manage");
                }
                else
                {

                    var _userObj = (from s in _dbContext.users where s.id == _vmUser.id select s).FirstOrDefault();
                    _userObj.firstName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmUser.userfirst);
                    _userObj.lastName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmUser.userlast);
                    _userObj.email = _vmUser.useremail;
                    if (!string.IsNullOrEmpty(_vmUser.userpass))
                    {
                        _userObj.password = CryptoDecrypt.SHA256Pass(_vmUser.userpass);
                    }
                    _userObj.modified = DateTime.Now;

                    // _dbContext.SaveChanges();

                    _dbContext.Entry(_userObj).State = EntityState.Modified;
                    _dbContext.SaveChanges();

                    return RedirectToAction("myprofile", "user");

                }

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }
        }


        #region Manage Users
        // MANAGE USERS
        [CheckSessionTimeout]
        public ActionResult Manage()  
        {

            try
            {

                var _sessId = Int32.Parse(Session["_userId"].ToString());
                var _sessEmail = Session["_userEmail"].ToString();
                var _sessUserType = Int32.Parse(Session["_userType"].ToString()); 


                var _userResults = (IList<vmUser>)
                        (from e in _dbContext.users
                         select new vmUser
                         {
                             id = e.id,
                             userfirst = e.firstName,
                             userlast = e.lastName,
                             useremail = e.email,
                             selusertype = e.usertype
                         }).ToList();

            
            var RSListCount = _userResults.Count();

            if (RSListCount != 0)
            {
                foreach (var i in _userResults)
                {
                    i.userfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(i.userfirst);
                    i.userlast = CryptoDecrypt.AES_256_WebKey_Decrypt(i.userlast);
                }
            }

            ViewBag.RSListCount = RSListCount;


            if (null == _userResults)
            {
                return View();
            }
            return View(_userResults);


            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }  


        }

        // MANAGE USERS accepting POST INPUT to add users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(vmUser _vmUser)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var isValidEmail = isValidUserAccount(_vmUser.useremail.ToLower().Trim().ToString());
                    if (!isValidEmail)
                    {
                        ViewBag.Success = 0; // 0 - Error
                        ViewBag.MessageTitle = "User exist";
                        ViewBag.Message = "We cannot add this user at this time.";

                    }
                    else
                    {
                        user myUser = new user();
                        myUser.firstName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmUser.userfirst);
                        myUser.lastName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmUser.userlast);
                        myUser.email = _vmUser.useremail;
                        myUser.password = CryptoDecrypt.SHA256Pass(_vmUser.userpass);
                        myUser.active = 1;
                        myUser.usertype = _vmUser.selusertype; // _vmUser.selusertype 99 - SA, 1 - A, 2 - public
                        myUser.modified = DateTime.Now;
                        myUser.created = DateTime.Now;

                        _dbContext.users.Add(myUser);
                        _dbContext.SaveChanges();

                        ViewBag.Success = 1; // 0 - Error
                        ViewBag.MessageTitle = "Success";
                        ViewBag.Message = "New user has been added to the system.";
                    }

                }
                else
                {
                    ViewBag.Success = 0; // 0 - Error
                    ViewBag.MessageTitle = "Invalid model state";
                    ViewBag.Message = "We cannot process the input.";

                    ViewBag.firstName = _vmUser.userfirst;
                    ViewBag.lastName = _vmUser.userlast;
                    ViewBag.email = _vmUser.useremail;

                    ModelState.AddModelError("", "Invalid Input");
                }

                // Populate the values for the grid
                var _userResults = (IList<vmUser>)
                            (from e in _dbContext.users
                             select new vmUser
                             {
                                 id = e.id,
                                 userfirst = e.firstName,
                                 userlast = e.lastName,
                                 useremail = e.email,
                                 selusertype = e.usertype
                             }).ToList();
                var RSListCount = _userResults.Count();

                if (RSListCount != 0)
                {
                    foreach (var i in _userResults)
                    {
                        i.userfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(i.userfirst);
                        i.userlast = CryptoDecrypt.AES_256_WebKey_Decrypt(i.userlast);
                    }
                }

                ViewBag.RSListCount = RSListCount;

                if (null == _userResults)
                {
                    return View();
                }
                return View(_userResults);
                
            }

            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.Success = 0; // 0 - Error
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }  
        }

        #endregion

        // EDIT Users now accepting values via query string for editing user input
        // GET: /User/Edit/5
        [CheckSessionTimeout]
        public ActionResult Edit(int id = 0)
        {
            try
            {

                var _userResults = (from e in _dbContext.users
                     where e.id == id
                     select new vmUser
                     {
                         id = e.id,
                         userfirst = e.firstName,
                         userlast = e.lastName,
                         useremail = e.email,
                         userpass = e.password,
                         selusertype = e.usertype
                     }).FirstOrDefault();


                if (null == _userResults)
                {
                    return View("manage");
                }

                _userResults.userfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(_userResults.userfirst);
                _userResults.userlast = CryptoDecrypt.AES_256_WebKey_Decrypt(_userResults.userlast);

                return View(_userResults);


            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }   
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vmUser _vmUser)
        {
            try
            {


                // Check if valid email address 
                //var isValidEmail = isValidUserAccount(_vmUser.useremail.ToLower().Trim().ToString());
                //if (!isValidEmail)
                //{
                //    ViewBag.UserExist = "Yes";
                //    return View();
                //}


                var _userResults = (from e in _dbContext.users
                                    where e.id == _vmUser.id
                                    select new vmUser
                                    {
                                        id = e.id,
                                        userfirst = e.firstName,
                                        userlast = e.lastName,
                                        useremail = e.email,
                                        userpass = e.password,
                                        selusertype = e.usertype
                                    }).FirstOrDefault();


                if (null == _userResults)
                {

                    //Add suitable error message for viewbag
                    return View("manage");
                }
                else
                {

                    var _userObj = (from s in _dbContext.users where s.id == _vmUser.id select s).FirstOrDefault();
                    _userObj.firstName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmUser.userfirst);
                    _userObj.lastName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmUser.userlast);
                    _userObj.email = _vmUser.useremail;
                    if (!string.IsNullOrEmpty(_vmUser.userpass))
                    {
                        _userObj.password = CryptoDecrypt.SHA256Pass(_vmUser.userpass);
                    }
                    _userObj.modified = DateTime.Now;

                    _dbContext.Entry(_userObj).State = EntityState.Modified;
                    _dbContext.SaveChanges();


                    TempData["Success"] = 1; // 0 - Error
                    TempData["MessageTitle"] = "Update user";
                    TempData["Message"] = "User details has been successfully updated.";

                    return RedirectToAction("manage", "user");

                }
                
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }
        }


        //
        // GET: /User/Delete/5
        [CheckSessionTimeout]
        public ActionResult Delete(int id = 0)
        {
            try
            {

                user user = _dbContext.users.Find(id);
                _dbContext.users.Remove(user);
                _dbContext.SaveChanges();

                TempData["Success"] = 1; // 0 - Error
                TempData["MessageTitle"] = "Remove user";
                TempData["Message"] = "User has been successfully removed from the system.";

                return RedirectToAction("manage");


            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Exception";
                ViewBag.Message = ex.Message.ToString();
                return View("Error");
            }   
        }

     

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("login", "user");
        }

        public ActionResult LogoutMessage()
        {
            ViewBag.MessageTitle = "Sign Out";
            ViewBag.Message = "Please sign in now. Your session has ended.";
            return View("_Message");
        }

        protected override void Dispose(bool disposing)
        {
            _dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}