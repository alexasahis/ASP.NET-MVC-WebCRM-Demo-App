﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebCRM.DAL;
using WebCRM.Models;
using WebCRM.ViewModels;

namespace WebCRM.Controllers
{
    public class CompaniesController : Controller
    {
        private WebCRMDBContext db = new WebCRMDBContext();

        // GET: Companies
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetCompaniesList(JQDTblParamModel dTblParams)
        {

            var allCompanies = db.Companies;
            IEnumerable<Company> filteredCompanies;
            if (!string.IsNullOrEmpty(dTblParams.sSearch))
            {
                filteredCompanies = db.Companies
                                      .Where(c => c.Name.Contains(dTblParams.sSearch) ||
                                                  c.Address1.Contains(dTblParams.sSearch) ||
                                                  c.Address2.Contains(dTblParams.sSearch) ||
                                                  c.CityRegion.Contains(dTblParams.sSearch) ||
                                                  c.StateProvince.Contains(dTblParams.sSearch) ||
                                                  c.PostalCode.Contains(dTblParams.sSearch) ||
                                                  c.Country.Contains(dTblParams.sSearch) ||
                                                  c.Phone.Contains(dTblParams.sSearch) ||
                                                  c.Fax.Contains(dTblParams.sSearch));
            }
            else
                filteredCompanies = allCompanies;

            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDir = Request["sSortDir_0"];

            Func<Company, string> sortOrder = (cSort => sortColIndex == 0 ? cSort.ID.ToString() : 
                                                        sortColIndex == 1 ? cSort.Name : 
                                                        sortColIndex == 2 ? cSort.Address1 :
                                                        sortColIndex == 3 ? cSort.CityRegion : 
                                                        sortColIndex == 4 ? cSort.StateProvince :
                                                        sortColIndex == 5 ? cSort.PostalCode :
                                                        sortColIndex == 6 ? cSort.Phone :
                                                        sortColIndex == 7 ? cSort.Fax :
                                                        cSort.Name);

            if (sortDir == "asc")
                filteredCompanies = filteredCompanies.OrderBy(sortOrder);
            else
                filteredCompanies = filteredCompanies.OrderByDescending(sortOrder);

            var displayedCompanies = filteredCompanies.Skip(dTblParams.iDisplayStart).Take(dTblParams.iDisplayLength);

            var totalRecs = allCompanies.Count();

            // TODO: sorting / filtering can get a little kooky (especially with multi-column sorts)
            // explore using the dbContext SQL queries directly
            
            var q = from c in displayedCompanies.AsEnumerable()
                  select new []
                  {
                    c.ID.ToString(),
                    c.Name.ToString(),
                    c.Address1.ToString(),
                    c.CityRegion.ToString(),
                    c.StateProvince.ToString(),
                    c.PostalCode.ToString(),
                    c.Phone.ToString(),
                    c.Fax.ToString(),
                  };


            var dispRecs = q.Count();
            return Json(new
            {
                sEcho = dTblParams.sEcho,
                iTotalRecords = totalRecs,
                iTotalDisplayRecords = dispRecs,
                aaData = q
            },
            JsonRequestBehavior.AllowGet);
        }

        // GET: Companies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // GET: Companies/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Address1,Address2,CityRegion,StateProvince,PostalCode,Country,Phone,Fax")] Company company)
        {
            if (ModelState.IsValid)
            {
                company.CreatedAt = DateTime.Now;
                company.UpdatedAt = DateTime.Now;
                db.Companies.Add(company);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(company);
        }

        // GET: Companies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Address1,Address2,CityRegion,StateProvince,PostalCode,Country,Phone,Fax")] Company company)
        {
            if (ModelState.IsValid)
            {
                company.CreatedAt = DateTime.Now;
                company.UpdatedAt = DateTime.Now;
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Company company = db.Companies.Find(id);
            db.Companies.Remove(company);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Companies/CompanyContacts/5
        [HttpGet, ActionName("CompanyContacts")]
        public ActionResult CompanyContacts(int id)
        {
            Company company = db.Companies.Find(id);
            if (company == null)
                return HttpNotFound();

            IQueryable<ContactListViewModel> q = from contacts in db.Contacts
                                                 from companies in contacts.Companies
                                                 where companies.ID == id
                                                 select new ContactListViewModel()
                                                 {
                                                     ID = contacts.ID,
                                                     Honorific = contacts.Honorific,
                                                     FirstName = contacts.FirstName,
                                                     MiddleName = contacts.MiddleName,
                                                     LastName = contacts.LastName,
                                                     Suffix = contacts.Suffix,
                                                     Title = contacts.Title,
                                                     Phone = contacts.Phone,
                                                     Email = contacts.Email,
                                                     CompanyName = companies.Name
                                                 };

            // return View(db.Contacts.ToList());
            ViewBag.ContactListFrom = "company_view";
            return PartialView("../Contacts/_ContactsList", q.ToList());

        }

        // GET: Companies/CompanyContacts/5
        [HttpGet, ActionName("CompanyContactsDropdownList")]
        public ActionResult CompanyContactsDropdownList(int id)
        {
            Company company = db.Companies.Find(id);
            if (company == null)
                return HttpNotFound();

            IQueryable<ContactListViewModel> q = from contacts in db.Contacts
                                                 from companies in contacts.Companies
                                                 where companies.ID == id
                                                 select new ContactListViewModel()
                                                 {
                                                     ID = contacts.ID,
                                                     Honorific = contacts.Honorific,
                                                     FirstName = contacts.FirstName,
                                                     MiddleName = contacts.MiddleName,
                                                     LastName = contacts.LastName,
                                                     Suffix = contacts.Suffix,
                                                     Title = contacts.Title,
                                                     Phone = contacts.Phone,
                                                     Email = contacts.Email,
                                                     CompanyName = companies.Name
                                                 };

            // return View(db.Contacts.ToList());
            ViewBag.ContactID = new SelectList(q.ToList(), "ID", "FullName");
            return PartialView("../Contacts/_ContactsDropdownList");

        }

        // GET: Companies/ActivityHistory/5
        [HttpGet, ActionName("ActivityHistory")]
        public ActionResult ActivityHistory(int id)
        {
            Company company = db.Companies.Find(id);
            if (company == null)
                return HttpNotFound();

            IQueryable<ActivityHistory> q = from activities in db.ActivityHistory
                                            where activities.CompanyID == id
                                            select activities;

            // return View(db.Contacts.ToList());
            ViewBag.ActivityListFrom = "company_view";
            return PartialView("../ActivityHistories/_ActivityList", q.ToList());

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
