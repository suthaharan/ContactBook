using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Contact2015.Models;
using Contact2015.Models.ViewModels;
using Contact2015.Helpers;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Contact2015.Filters;

namespace Contact2015.Controllers
{
    [CheckSessionTimeout]
    public class ContactController : Controller
    {
        private Contacts2015Entities _dbContext = new Contacts2015Entities();




        #region Manage Contacts
        // MANAGE USERS

        public ActionResult Manage()
        {
            var _sessId = Int32.Parse(Session["_userId"].ToString());
            var _sessEmail = Session["_userEmail"].ToString();
            var _sessUserType = Int32.Parse(Session["_userType"].ToString()); 

            try
            {
                // add conditional statement in query to pull contact only on the signed in user
                // admin can see all contacts while public can see only their contacts

                var _contactResults = (IList<vmContact>)
                                (from e in _dbContext.contacts
                                 where e.userid == _sessId
                                 select new vmContact
                                 {
                                     id = e.id,
                                     txtfirst = e.firstName,
                                     txtlast = e.lastName,
                                     txtphone = e.phoneNumber,
                                     txtcity = e.city,
                                     txtpostal = e.postalCode,
                                     txtprovince = e.province,
                                     txtstreet = e.streetName,
                                     selcountry = e.country,
                                     notes = e.notes
                                 }).ToList();

                if ((_sessUserType == 1) || (_sessUserType == 99))
                {
                    _contactResults = (IList<vmContact>)
                        (from e in _dbContext.contacts
                         select new vmContact
                         {
                             id = e.id,
                             txtfirst = e.firstName,
                             txtlast = e.lastName,
                             txtphone = e.phoneNumber,
                             txtcity = e.city,
                             txtpostal = e.postalCode,
                             txtprovince = e.province,
                             txtstreet = e.streetName,
                             selcountry = e.country,
                             notes = e.notes
                         }).ToList();
                }

                var RSListCount = _contactResults.Count();

                if (RSListCount != 0)
                {
                    foreach (var i in _contactResults)
                    {
                        i.txtfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(i.txtfirst);
                        i.txtlast = CryptoDecrypt.AES_256_WebKey_Decrypt(i.txtlast);
                        i.txtphone = CryptoDecrypt.AES_256_WebKey_Decrypt(i.txtphone);
                    }
                }

                ViewBag.RSListCount = RSListCount;


                if (null == _contactResults)
                {
                    return View();
                }
                return View(_contactResults);


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
        public ActionResult Manage(vmContact _vmContact)
        {
            try
            {

                var _sessId = Int32.Parse(Session["_userId"].ToString());
                var _sessEmail = Session["_userEmail"].ToString();

                if (ModelState.IsValid)
                {

                        contact myNewContact = new contact();
                        myNewContact.firstName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmContact.txtfirst);
                        myNewContact.lastName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmContact.txtlast);
                        myNewContact.phoneNumber = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmContact.txtphone);
                        myNewContact.streetName = _vmContact.txtstreet;
                        myNewContact.city = _vmContact.txtcity;
                        myNewContact.province = _vmContact.txtprovince;
                        myNewContact.postalCode = _vmContact.txtpostal;
                        myNewContact.country = _vmContact.selcountry;
                        myNewContact.notes = _vmContact.notes;
                        myNewContact.status = 1;
                        myNewContact.userid = _sessId; // Id of the user who adds this contact
                        myNewContact.modified = DateTime.Now;
                        myNewContact.created = DateTime.Now;

                        _dbContext.contacts.Add(myNewContact);
                        _dbContext.SaveChanges();

                        ViewBag.Success = 1; // 0 - Error
                        ViewBag.MessageTitle = "Success";
                        ViewBag.Message = "New contact has been added to the system.";

                }
                else
                {
                    ViewBag.Success = 0; // 0 - Error
                    ViewBag.MessageTitle = "Invalid model state";
                    ViewBag.Message = "We cannot process the input.";

                    ViewBag.firstName = _vmContact.txtfirst;
                    ViewBag.lastName = _vmContact.txtlast;
                    ViewBag.phonenumber = _vmContact.txtphone;
                    ViewBag.streetName = _vmContact.txtstreet;
                    ViewBag.city = _vmContact.txtcity;
                    ViewBag.province = _vmContact.txtprovince;
                    ViewBag.postalCode = _vmContact.txtpostal;
                    ViewBag.country = _vmContact.selcountry;
                    ViewBag.notes = _vmContact.notes;

                    ModelState.AddModelError("", "Invalid Input");
                }

                // Populate the values for the grid
                // add conditional statement in query to pull contact only on the signed in user
                var _contactResults = (IList<vmContact>)
                            (from e in _dbContext.contacts
                             where e.userid == _sessId
                             select new vmContact
                             {
                                 id = e.id,
                                 txtfirst = e.firstName,
                                 txtlast = e.lastName,
                                 txtphone = e.phoneNumber,
                                 txtcity = e.city,
                                 txtpostal = e.postalCode,
                                 txtprovince = e.province,
                                 txtstreet = e.streetName,
                                 selcountry = e.country,
                                 notes = e.notes
                             }).ToList();
                var RSListCount = _contactResults.Count();

                if (RSListCount != 0)
                {
                    foreach (var i in _contactResults)
                    {
                        i.txtfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(i.txtfirst);
                        i.txtlast = CryptoDecrypt.AES_256_WebKey_Decrypt(i.txtlast);
                        i.txtphone = CryptoDecrypt.AES_256_WebKey_Decrypt(i.txtphone);
                    }
                }

                ViewBag.RSListCount = RSListCount;

                if (null == _contactResults)
                {
                    return View();
                }
                return View(_contactResults);

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

        #endregion

        // EDIT Users now accepting values via query string for editing user input
        // GET: /User/Edit/5
        public ActionResult Edit(int id = 0)
        {
            try
            {

                var _contactResults = (from e in _dbContext.contacts
                                    where e.id == id
                                    select new vmContact
                                    {
                                        id = e.id,
                                        txtfirst = e.firstName,
                                        txtlast = e.lastName,
                                        txtphone = e.phoneNumber,
                                        txtcity = e.city,
                                        txtpostal = e.postalCode,
                                        txtprovince = e.province,
                                        txtstreet = e.streetName,
                                        selcountry = e.country,
                                        notes = e.notes
                                    }).FirstOrDefault();


                if (null == _contactResults)
                {
                    return View("manage");
                }

                _contactResults.txtfirst = CryptoDecrypt.AES_256_WebKey_Decrypt(_contactResults.txtfirst);
                _contactResults.txtlast = CryptoDecrypt.AES_256_WebKey_Decrypt(_contactResults.txtlast);
                _contactResults.txtphone = CryptoDecrypt.AES_256_WebKey_Decrypt(_contactResults.txtphone);
                ViewBag.SuccessMessage = "hello";
                return View(_contactResults);


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
        public ActionResult Edit(vmContact _vmContact)
        {
            try
            {
                var _sessId = Int32.Parse(Session["_userId"].ToString());
                var _sessEmail = Session["_userEmail"].ToString();
                var _contactResults = (from e in _dbContext.contacts
                                       where e.id == _vmContact.id
                                       select new vmContact
                                       {
                                           id = e.id,
                                           txtfirst = e.firstName,
                                           txtlast = e.lastName,
                                           txtphone = e.phoneNumber,
                                           txtcity = e.city,
                                           txtpostal = e.postalCode,
                                           txtprovince = e.province,
                                           txtstreet = e.streetName,
                                           selcountry = e.country,
                                           notes = e.notes
                                       }).FirstOrDefault();


                if (null == _contactResults)
                {
                    //TempData["Success"] = 0; // 0 - Error
                    //TempData["MessageTitle"] = "Update contact";
                    //TempData["Message"] = "Contact details has been successfully updated.";
                    //Add suitable error message for viewbag
                    return View("manage");
                }
                else
                {

                    var _contactObj = (from s in _dbContext.contacts where s.id == _vmContact.id select s).FirstOrDefault();
                    _contactObj.firstName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmContact.txtfirst);
                    _contactObj.lastName = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmContact.txtlast);
                    _contactObj.phoneNumber = CryptoDecrypt.AES_256_WebKey_Encrypt(_vmContact.txtphone);
                    _contactObj.streetName = _vmContact.txtstreet;
                    _contactObj.city = _vmContact.txtcity;
                    _contactObj.province = _vmContact.txtprovince;
                    _contactObj.postalCode = _vmContact.txtpostal;
                    _contactObj.country = _vmContact.selcountry;
                    _contactObj.userid = _sessId;
                    _contactObj.notes = _vmContact.notes;
                    _contactObj.modified = DateTime.Now;

                    // _dbContext.SaveChanges();

                    _dbContext.Entry(_contactObj).State = EntityState.Modified;
                    _dbContext.SaveChanges();

                    TempData["Success"] = 0; // 0 - Error
                    TempData["MessageTitle"] = "Update contact";
                    TempData["Message"] = "Contact details has been successfully updated.";

                    return RedirectToAction("manage", "contact");

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

        public ActionResult Delete(int id = 0)
        {
            try
            {

                contact _contacts = _dbContext.contacts.Find(id);
                _dbContext.contacts.Remove(_contacts);
                _dbContext.SaveChanges();

                TempData["Success"] = 1; // 0 - Error
                TempData["MessageTitle"] = "Remove contact";
                TempData["Message"] = "Contact has been successfully removed from the system.";

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





        protected override void Dispose(bool disposing)
        {
            _dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}