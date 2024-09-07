using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using WebSolution.Models;
using WebSolution.Utility;

namespace WebSolution.Controllers
{
    public class EcommerceTransferRequestController : Controller
    {
        private AccessFileViewModel accessFileViewModel;
        public EcommerceTransferRequestController()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/AccessFile.xml"));
                //Loop through the selected Nodes.
                foreach (XmlNode node in doc.SelectNodes("/AccessFiles/AccessFile"))
                {
                    accessFileViewModel = (new AccessFileViewModel
                    {
                        IPPort = node["IPPort"].InnerText
                        ,
                        AccessKey = node["AccessKey"].InnerText
                    });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // GET: EcommerceTransferRequest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AddRowForOrder(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRow");
        }
        public ActionResult AddRowForOrderView(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRowView");
        }
        public ActionResult ViewMultiStockTransferRequest(int id)
        {
            return PartialView("_MultiStockTransferRequestView");
        }
        public ActionResult AddEcommerceTransferRequest(int etrId = 0, string eflag = "ed")
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.EcommerceTransferRequestId = etrId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.UserId = Session[SessionCollection.UserId].ToString();
                ViewBag.eflag = eflag;
                return View();
            }

        }


        // Inventory Transfer Request With Multiple Sales Order
        [HttpGet]
        public async Task<ActionResult> TransferRequestMultipleSalesOrder(DateTime? fromdate, DateTime? todate)
        {
            List<DocumentsInvTransECommerce> aList = new List<DocumentsInvTransECommerce>();
            string branch = "";
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                var resultantJson = Json(new { dataList = aList, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
                branch = Session[SessionCollection.Branch].ToString();
                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);

                aList = await GetAllSalesOrderHeader(branch, sfromdate, stodate);


                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }

        public async Task<List<DocumentsInvTransECommerce>> GetAllSalesOrderHeader(string BuisnessUnit, string FromDate, string ToDate)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsInvTransECommerce> documentsViewModels = new List<DocumentsInvTransECommerce>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/SOInventoryTrns?BuisnessUnit=" + BuisnessUnit + "&FromDate=" + FromDate + "&ToDate=" + ToDate);
                //complete_url = objAPIURL + "Api/GetBPDetails";DocNum
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                documentsViewModels = (new JavaScriptSerializer()).Deserialize<List<DocumentsInvTransECommerce>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return documentsViewModels;
        }

        public async Task<ActionResult> GetItemByProcess(string docSreing)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> aList = new List<ItemsViewModel>();
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                var resultantJson = Json(new { dataList = aList, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
                try
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                    complete_url = String.Format(accessFileViewModel.IPPort + "Api/SOInventoryTrnsDetails?DocEntry=" + docSreing);
                    //complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetSalesOrderDetails?ItemHide=Y&DocEntry=" + SaleOrder);
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
                    responseObject = (HttpWebResponse)requestObject.GetResponse();
                    dataStream = responseObject.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    msg = reader.ReadToEnd();
                    aList = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(0);
                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }

        public async Task<ActionResult> SaveEcommerceTransferRequset(TransferRequset itr)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            Stream dataStream;
            StreamReader reader;
            HttpWebRequest HTTP_Request;
            HttpWebResponse HTTP_Response;
            List<ReturnData> returnDatas = new List<ReturnData>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                var resultantJson = Json(new { dataList = returnDatas, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
                try
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                    var myContent = JsonConvert.SerializeObject(itr);
                    complete_url = accessFileViewModel.IPPort + "Api/PostMultiStockTransferRequest";
                    HTTP_Request = (HttpWebRequest)HttpWebRequest.Create(complete_url);
                    HTTP_Request.Method = "POST";
                    HTTP_Request.ContentType = "application/json";
                    HTTP_Request.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                    HTTP_Request.Headers.Add("Branch", branch);
                    HTTP_Request.Headers.Add("UserId", userId);
                    HTTP_Request.Headers.Add("AuthToken", authToken);

                    using (var streamWriter = new StreamWriter(HTTP_Request.GetRequestStream()))
                    {
                        streamWriter.Write(myContent);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    HTTP_Response = (HttpWebResponse)HTTP_Request.GetResponse();

                    dataStream = HTTP_Response.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    msg = reader.ReadToEnd();

                    returnDatas = (new JavaScriptSerializer()).Deserialize<List<ReturnData>>(msg);

                }
                catch
                {
                    ReturnData returnData = new ReturnData();
                    returnData.ReturnMsg = "Network error occured.Please reload the page Or Re-Login";
                    var resultantJson1 = Json(new { dataList = returnData, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                    resultantJson1.MaxJsonLength = int.MaxValue;
                    return resultantJson1;
                }
                await Task.Delay(0);
                var resultantJson = Json(new { dataList = returnDatas[0], UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
        }

        [HttpGet]
        public async Task<ActionResult> Index(DateTime? fromdate, DateTime? todate, string docNum, string docstatus = "")
        {
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var branch = Session[SessionCollection.Branch].ToString();
                var userId = Session[SessionCollection.UserId].ToString();
                if (fromdate == null)
                {
                    fromdate = System.DateTime.Now.AddDays(-1).Date;

                }
                if (todate == null)
                {
                    todate = System.DateTime.Now.Date;

                }

                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);

                List<DocumentsViewModel> aList = await GetAllStockTransferRequestHeader(branch, userId, sfromdate, stodate, docNum, docstatus);

                ViewBag.Branch = branch;
                return View(aList);
            }

        }
        [HttpGet]
        public async Task<ActionResult> MultiStockTransferRequestList_Search(DateTime? fromdate, DateTime? todate, string docNum, string docstatus = "")
        {
            List<DocumentsViewModel> aList = new List<DocumentsViewModel>();
            string branch = "";
            string userId = "";
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                var resultantJson = Json(new { dataList = aList, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
                branch = Session[SessionCollection.Branch].ToString();
                userId = Session[SessionCollection.UserId].ToString();
                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);

                aList = await GetAllStockTransferRequestHeader(branch, userId, sfromdate, stodate, docNum, docstatus);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
        }

        public async Task<List<DocumentsViewModel>> GetAllStockTransferRequestHeader(string BuisnessUnit, string UserCode, string FromDate, string ToDate, string docNum, string Status)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetMultiStockTransferRequestHeader?BuisnessUnit=" + BuisnessUnit + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&Status=" + Status + "&DocNum=" + docNum);
                //complete_url = objAPIURL + "Api/GetBPDetails";DocNum
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                documentsViewModels = (new JavaScriptSerializer()).Deserialize<List<DocumentsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return documentsViewModels;
        }

        [HttpGet]
        public async Task<ActionResult> MultiStockTransferRequestViewData(string DocEntry)
        {
            DocumentsViewModel aList = new DocumentsViewModel();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                aList.UnAutorized = 1;
                return Json(aList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                aList = await GetAllStockTransferRequestHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllStockTransferRequestDetails(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<DocumentsViewModel> GetAllStockTransferRequestHeaderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetMultiStockTransferRequestHeader?DocEntry=" + DocEntry);
                //complete_url = objAPIURL + "Api/GetBPDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                documentsViewModels = (new JavaScriptSerializer()).Deserialize<List<DocumentsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return documentsViewModels[0];
        }

        public async Task<List<ItemsViewModel>> GetAllStockTransferRequestDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetMultiStockTransferRequestDetails?DocEntry=" + DocEntry);
                //complete_url = objAPIURL + "Api/GetBPDetails";
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                itemsViewModels = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return itemsViewModels;
        }

        public List<SalesEmployee> LoadSalesEmployee()
        {
            string complete_url = "", msg = "";
            string BuisnessUnit = Session[SessionCollection.Branch].ToString();
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<SalesEmployee> salesEmployees = new List<SalesEmployee>();
            try
            {
                complete_url = accessFileViewModel.IPPort + "Api/GetSalesEmployeeDetails?BuisnessUnit=" + BuisnessUnit;
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                //requestObject.ContentType = "text/json";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                salesEmployees = (new JavaScriptSerializer()).Deserialize<List<SalesEmployee>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            return salesEmployees;
        }
        public async Task<ActionResult> GetSalesEmployee()
        {
            List<SalesEmployee> salesEmployees = new List<SalesEmployee>();
            salesEmployees = Session[SessionCollection.SaleEmployee] as List<SalesEmployee>;
            if (salesEmployees == null)
            {
                salesEmployees = LoadSalesEmployee();
            }
            await Task.Delay(0);
            return Json(salesEmployees, JsonRequestBehavior.AllowGet);
        }
    }
}