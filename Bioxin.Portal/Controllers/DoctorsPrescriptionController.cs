using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebSolution.Controllers
{
    public class DoctorsPrescriptionController : Controller
    {
        // GET: DoctorsPrescription
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult Prescription()
        {
            return View();
        }
        public ActionResult AddRowForOrder(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRow");
        }
        public ActionResult DailyOperationCheckList()
        {
            return View();
        }
        public ActionResult TherapistCheckList()
        {
            return View();
        }
        public ActionResult EmployeeActivity()
        {
            return View();
        }
    }
}