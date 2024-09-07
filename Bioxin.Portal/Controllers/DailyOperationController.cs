using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebSolution.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebSolution.Controllers
{
    public class DailyOperationController : Controller
    {
        ResultResponse aResponse = new ResultResponse();
        Uri baseAddress = new Uri("http://localhost:55189/");
        // GET: DailyOperation
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AddRowForOrder(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRow");
        }
        public ActionResult AddDailyOperation(int dailyOperationId = 0, string eflag = "ed")
        {
            ViewBag.DailyOperationId = dailyOperationId;
            ViewBag.eflag = eflag;
            return View();
        }

}
}