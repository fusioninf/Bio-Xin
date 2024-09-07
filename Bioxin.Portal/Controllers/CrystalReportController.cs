using Portal.Controllers;
using Portal.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using WebSolution.Models;

namespace WebSolution.Controllers
{
    [AuthorizeUser]
    public class CrystalReportController :  BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CustomerInvoiceSummary()
        {
            ViewBag.Branch = branch;
            return View();
        }

        public ActionResult SalesOrderReport()
        {
            ViewBag.Branch = branch;
            return View();
        }

        public ActionResult EcommerceSalesDetails()
        {
            ViewBag.Branch = branch;
            return View();
        }

        public ActionResult EcommerceSalesOrderVsInvoice()
        {
            ViewBag.Branch = branch;
            return View();
        }

        public ActionResult ECMPaymentSummary()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult StockBalance()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult SalesInvoiceDetails()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult StocktransferReport()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult POSClosing()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult SalesSummary()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult BackOrder()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult ServiceGain()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult StockLedger()
        {
            ViewBag.Branch = branch;
            return View();
        }


        public ActionResult BranchMonthlySales()
        {
            ViewBag.Branch = branch;
            return View();
        }
        public ActionResult EmployeeSalesTarget()
        {
            ViewBag.Branch = branch;
            return View();
        }

        public async Task<ActionResult> GetBranch()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Branch> branches = new List<Branch>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetBuisnessUnitDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                branches = (new JavaScriptSerializer()).Deserialize<List<Branch>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.CompletedTask;
            return Json(branches, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetGroup()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPGroupDetails");
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetGroupDetails(string GroupCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPGroupDetails?P_GroupCode=" + GroupCode);
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetBusinessPartner(string CardType)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetails?CardType=" + CardType);
                //complete_url = objAPIURL + "Api/GetBPDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetBusinessPartnerByCardCode(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetails?CardCode=" + CardCode);
                //complete_url = objAPIURL + "Api/GetBPDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(businessPartners[0], JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetItem()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetItemDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                items = (new JavaScriptSerializer()).Deserialize<List<Item>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetItemGroup()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetItemeGroupDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                items = (new JavaScriptSerializer()).Deserialize<List<Item>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetWarehouse()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Warehouse> warehouses = new List<Warehouse>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetWarehouseDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                warehouses = (new JavaScriptSerializer()).Deserialize<List<Warehouse>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(warehouses, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetWarehouseALL()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Warehouse> warehouses = new List<Warehouse>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetWarehouseAllDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                warehouses = (new JavaScriptSerializer()).Deserialize<List<Warehouse>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(warehouses, JsonRequestBehavior.AllowGet);
        }
    }
}