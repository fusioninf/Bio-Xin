using Newtonsoft.Json;
using Portal.Controllers;
using Portal.Middleware;
using RestSharp;
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
using WebSolution.Models.Constants;

namespace WebSolution.Controllers
{
    public class SalesOrderNewController : BaseController
    {
        public async Task<ActionResult> GetprintPage()
        {
            await Task.Delay(0);
            string userId = "";
            var activeSession = Session[SessionCollection.UserId];
            if(activeSession != null)
            {
                userId = Session[SessionCollection.UserId].ToString();
            }
            accessFVM.UserId = userId;
            return Json(accessFVM, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [AuthorizeUser]
        public async Task<ActionResult> Index()
        {
            if (userId == null)
                return RedirectToAction("Login", "Home");
            
            else
                return View();
        }
        [HttpGet]
        public async Task<ActionResult> SalesOrderList_Search(DateTime? fromdate, DateTime? todate, string DocNum, string customerName, string CustomerMobile, string salesEmployeeId)
        {
            List<DocumentsViewModel> aList = new List<DocumentsViewModel>();

            if (userId == null)
            {
                var resultantJson = Json(new { dataList = aList, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
             
                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);

                aList = await GetAllSalesOrderHeader(branch, userId, sfromdate, stodate, DocNum, customerName, CustomerMobile, salesEmployeeId);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
        }
        public async Task<List<DocumentsViewModel>> GetAllSalesOrderHeader(string BuisnessUnit, string UserCode, string FromDate, string ToDate, string DocNum, string customerName, string CustomerMobile, string salesEmployeeId)
        {
            var complete_url = $"Api/GetSalesOrderHeader?BuisnessUnit={BuisnessUnit}&FromDate={FromDate}&ToDate={ToDate}&DocNo={DocNum}&CardName={customerName}&MobileNo={CustomerMobile}&salesEmployeeId={salesEmployeeId}";
            return await CallApi<List<DocumentsViewModel>>(complete_url, RequestMethods.GET);
        }
        public ActionResult ViewSalesOrder(int id)
        {
            return PartialView("_SalesOrderView");
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
        [HttpGet]
        public async Task<ActionResult> SalesOrderViewData(string DocEntry)
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
                aList = await GetAllSalesOrderHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllSalesOrderDetails(DocEntry);
                aList.paymentViews = await GetAllPaymentDetailsOrder(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<DocumentsViewModel> GetAllSalesOrderHeaderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderHeader?DocEntry=" + DocEntry);
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                requestObject.ContentType = "application/json";
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
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
        public async Task<List<ItemsViewModel>> GetAllSalesOrderDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            string ItemHide = "N";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderDetails?DocEntry=" + DocEntry + "&ItemHide=" + ItemHide);
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
                itemsViewModels = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return itemsViewModels;
        }
        public async Task<List<PaymentView>> GetAllPaymentDetailsOrder(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<PaymentView> payments = new List<PaymentView>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetPaymentDetailsOrder?DocEntry=" + DocEntry);
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
                payments = (new JavaScriptSerializer()).Deserialize<List<PaymentView>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return payments;
        }
        public ActionResult AddSalesOrder(int soId = 0, string eflag = "")
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.SalesOrderId = soId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.eflag = eflag;
                return View();
            }
        }
        public async Task<ActionResult> SaveSalesOrder1(SalesOrderNew itr)
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
                    // Iterate through the properties and replace null values with empty strings
                    foreach (var property in itr.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) && property.GetValue(itr) == null)
                        {
                            property.SetValue(itr, "");
                        }
                    }
                    // Serialize the object back to JSON with nulls replaced by empty strings
                    var myContent = JsonConvert.SerializeObject(itr);
                    //myContent = myContent.Replace(null, "");
                    complete_url = accessFVM.IPPort + "Api/PostECommerceSalesOrderWithAdvancePayment";  //"Api/PostSalesOrderWithAdvancePayment";
                    HTTP_Request = (HttpWebRequest)HttpWebRequest.Create(complete_url);
                    HTTP_Request.Method = "POST";
                    HTTP_Request.ContentType = "application/json";
                    HTTP_Request.Headers.Add("AccessKey", accessFVM.AccessKey);
                    HTTP_Request.Headers.Add("Branch", branch);
                    HTTP_Request.Headers.Add("UserId", userId);
                    HTTP_Request.Headers.Add("AuthToken", authToken);
                    //HTTP_Request.KeepAlive = false;

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
                 catch(Exception ex)
                {
                    //return Json(new ReturnData { ReturnMsg = ex.Message }, JsonRequestBehavior.AllowGet);
                    ReturnData returnData = new ReturnData();
                    returnData.ReturnMsg = ex.Message;
                    var resultantJson1 = Json(new { dataList = returnData, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                    resultantJson1.MaxJsonLength = int.MaxValue;
                    return resultantJson1;
                }
                await Task.Delay(0);
                var resultantJson = Json(new { dataList = returnDatas[0], UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(returnDatas[0], JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SaveSalesOrder(SalesOrderNew model)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "";
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
                    string data = JsonConvert.SerializeObject(model);
                    complete_url = accessFVM.IPPort + "Api/PostECommerceSalesOrderWithAdvancePayment";
                    var client = new RestClient(complete_url);
                    var request = new RestRequest(Method.POST);
                    request.AddJsonBody(data);
                    request.AddHeader("accept", "application/json");
                    request.AddHeader("AccessKey", accessFVM.AccessKey);
                    request.AddHeader("Branch", branch);
                    request.AddHeader("UserId", userId);
                    request.AddHeader("AuthToken", authToken);
                    //request.Timeout = 300000;
                    //request.Timeout = 3000000;
                    // Make the asynchronous POST request
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IRestResponse response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        // Deserialize the response content
                        var msg = response.Content;
                        returnDatas = (new JavaScriptSerializer()).Deserialize<List<ReturnData>>(msg);
                    }
                    else
                    {
                        ReturnData returnData = new ReturnData();
                        returnData.ReturnMsg = "Network error occured.Please reload the page Or Re-Login";
                        var resultantJson1 = Json(new { dataList = returnData, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                        resultantJson1.MaxJsonLength = int.MaxValue;
                        return resultantJson1;
                    }
                }
                catch (Exception ex)
                {
                    ReturnData returnData = new ReturnData();
                    returnData.ReturnMsg ="Network error occured.Please reload the page Or Re-Login";
                    var resultantJson1 = Json(new { dataList = returnData, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                    resultantJson1.MaxJsonLength = int.MaxValue;
                    return resultantJson1;
                }

                var resultantJson = Json(new { dataList = returnDatas[0], UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
          
        }
        public async Task<ActionResult> UpdateSalesOrder(SalesOrderNew itr)
        {
            string complete_url = "", msg = "";
            Stream dataStream;
            StreamReader reader;
            HttpWebRequest HTTP_Request;
            HttpWebResponse HTTP_Response;
            List<ReturnData> returnDatas = new List<ReturnData>();
            try
            {
                // Iterate through the properties and replace null values with empty strings
                foreach (var property in itr.GetType().GetProperties())
                {
                    if (property.PropertyType == typeof(string) && property.GetValue(itr) == null)
                    {
                        property.SetValue(itr, "");
                    }
                }
                // Serialize the object back to JSON with nulls replaced by empty strings
                var myContent = JsonConvert.SerializeObject(itr);
                //myContent = myContent.Replace(null, "");
                complete_url = accessFVM.IPPort + "Api/UpdateSalesOrder";
                HTTP_Request = (HttpWebRequest)HttpWebRequest.Create(complete_url);
                HTTP_Request.Method = "POST";
                HTTP_Request.ContentType = "application/json";
                HTTP_Request.Headers.Add("AccessKey", accessFVM.AccessKey);
                HTTP_Request.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                HTTP_Request.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                HTTP_Request.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());

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
                throw;
            }
            await Task.Delay(0);
            return Json(returnDatas[0], JsonRequestBehavior.AllowGet);
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

            await Task.Delay(0);
            return Json(branches, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> AutoCompleteBusinessPartner(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string CardType = "C";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<AutoCompleteBP> autoCompletes = new List<AutoCompleteBP>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = autoCompletes, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetails?CardType=" + CardType + "&MobileNoSearch=" + prefix);
                    //complete_url = objAPIURL + "Api/GetBPDetails";
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
                    responseObject = (HttpWebResponse)requestObject.GetResponse();
                    dataStream = responseObject.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    msg = reader.ReadToEnd();
                    autoCompletes = (new JavaScriptSerializer()).Deserialize<List<AutoCompleteBP>>(msg);
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(0);
                var resultantJson = Json(new { dataList = autoCompletes, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(autoCompletes, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> AutoCompleteCustomerName(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string CardType = "C";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<AutoCompleteBP> autoCompletes = new List<AutoCompleteBP>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = autoCompletes, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetails?CardType=" + CardType + "&CardNameSearch=" + prefix);
                    //complete_url = objAPIURL + "Api/GetBPDetails";
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
                    responseObject = (HttpWebResponse)requestObject.GetResponse();
                    dataStream = responseObject.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    msg = reader.ReadToEnd();
                    autoCompletes = (new JavaScriptSerializer()).Deserialize<List<AutoCompleteBP>>(msg);
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(0);
                var resultantJson = Json(new { dataList = autoCompletes, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(autoCompletes, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteCustomerCode(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string CardType = "C";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<AutoCompleteBP> autoCompletes = new List<AutoCompleteBP>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = autoCompletes, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetails?CardType=" + CardType + "&CardCodeSearch=" + prefix);
                    //complete_url = objAPIURL + "Api/GetBPDetails";
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
                    responseObject = (HttpWebResponse)requestObject.GetResponse();
                    dataStream = responseObject.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    msg = reader.ReadToEnd();
                    autoCompletes = (new JavaScriptSerializer()).Deserialize<List<AutoCompleteBP>>(msg);
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(0);
                var resultantJson = Json(new { dataList = autoCompletes, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(autoCompletes, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteItem(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "Y";
            string selleItem = "Y";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                var resultantJson = Json(new { dataList = items, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemCodeSearch=" + prefix + "&InventoryItem=" + inventoryItem + "&SelleItem=" + selleItem;
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
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
                var resultantJson = Json(new { dataList = items, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(items, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteItemName(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "Y";
            string selleItem = "Y";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = items, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemNameSearch=" + prefix + "&InventoryItem=" + inventoryItem + "&SelleItem=" + selleItem;
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
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
                var resultantJson = Json(new { dataList = items, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(items, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteService(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "N";
            string selleItem = "Y";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = items, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemCodeSearch=" + prefix + "&InventoryItem=" + inventoryItem + "&SelleItem=" + selleItem;
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
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
                var resultantJson = Json(new { dataList = items, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(items, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteServiceName(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "N";
            string selleItem = "Y";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = items, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemNameSearch=" + prefix + "&InventoryItem=" + inventoryItem + "&SelleItem=" + selleItem;
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
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
                var resultantJson = Json(new { dataList = items, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(items, JsonRequestBehavior.AllowGet);
            }

        }

        public async Task<ActionResult> AutoCompleteServiceSo(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "N";
            string selleItem = "Y";
            string FromPage = "SO";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = items, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemCodeSearch=" + prefix + "&InventoryItem=" + inventoryItem + "&SelleItem=" + selleItem + "&FromPage=" + FromPage;
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
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
                var resultantJson = Json(new { dataList = items, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(items, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteServiceNameSo(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "N";
            string selleItem = "Y";
            string FromPage = "SO";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = items, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemNameSearch=" + prefix + "&InventoryItem=" + inventoryItem + "&SelleItem=" + selleItem + "&FromPage=" + FromPage;
                    requestObject = WebRequest.Create(complete_url);
                    requestObject.Method = "GET";
                    //requestObject.ContentType = "text/json";
                    requestObject.ContentType = "application/json";
                    //requestObject.Timeout = 30;
                    requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                    requestObject.Headers.Add("Branch", branch);
                    requestObject.Headers.Add("UserId", userId);
                    requestObject.Headers.Add("AuthToken", authToken);
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
                var resultantJson = Json(new { dataList = items, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(items, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> AutoCompleteSalesOrder(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> dvm = new List<DocumentsViewModel>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = dvm, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
                try
                {
                    complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderHeader?DocNo=" + prefix);
                    //complete_url = String.Format(objAPIURL + "Api/GetSalesOrderHeader?BuisnessUnit=" + BuisnessUnit + "&FromDate=" + FromDate + "&ToDate=" + ToDate);
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
                    dvm = (new JavaScriptSerializer()).Deserialize<List<DocumentsViewModel>>(msg);
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(0);
                var resultantJson = Json(new { dataList = dvm, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(documentsViewModels, JsonRequestBehavior.AllowGet);
            }

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
        public async Task<ActionResult> GetDoctorsPrescriptionHeaderByCardCode(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetDoctorsPrescriptionHeader?DocStatus=O&CardCode=" + CardCode);
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
        public async Task<ActionResult> GetBPContactPerson(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ContactPerson> contactPersons = new List<ContactPerson>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPContactPersonDetails?CardCode=" + CardCode);
                // complete_url = objAPIURL + "Api/GetBPContactPersonDetails?UserCode=" + userid";
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
                contactPersons = (new JavaScriptSerializer()).Deserialize<List<ContactPerson>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(contactPersons, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> GetSalesChannel()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<InventoryTransType> salesChannel = new List<InventoryTransType>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetSalesChannelData";
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
                salesChannel = (new JavaScriptSerializer()).Deserialize<List<InventoryTransType>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(salesChannel, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> GetMainWarehouse(string WhsType)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Warehouse> warehouses = new List<Warehouse>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetWarehouseDetails?WhsType=" + WhsType);
                //complete_url = objAPIURL + "Api/GetWarehouseDetails";
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
        public async Task<ActionResult> GetItemByItemCode(string ItemCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetItemDetails?ItemCode=" + ItemCode);
                //complete_url = objAPIURL + "Api/GetItemDetails";
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
            return Json(items[0], JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetItemByItemCodeCardCode(string ItemCode, string CardCode, string HappyHrs)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetItemDetails?ItemCode=" + ItemCode + "&CardCode=" + CardCode + "&HappyHrs=" + HappyHrs);
                //complete_url = objAPIURL + "Api/GetItemDetails";
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
            return Json(items[0], JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetItemeWareHouseWiseStock(string ItemCode, string WhsCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetItemeWareHouseWiseStock?ItemCode=" + ItemCode + "&WhsCode=" + WhsCode);
                //complete_url = objAPIURL + "Api/GetItemDetails";
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
            return Json(items[0], JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDiscBasedOnCustomer(string CardCode, string PostingDate)
        {
            string complete_url = "", msg = "";
            string BusinessUnit = Session[SessionCollection.Branch].ToString();
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Discount> discounts = new List<Discount>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/DiscBasedOnCustomer?CardCode=" + CardCode + "&BusinessUnit=" + BusinessUnit + "&PostingDate=" + PostingDate);
                //complete_url = objAPIURL + "Api/GetItemDetails";
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
                discounts = (new JavaScriptSerializer()).Deserialize<List<Discount>>(msg);
            }
            catch (Exception ex)
            {
                return Json(discounts, JsonRequestBehavior.AllowGet);
            }

            await Task.Delay(0);
            return Json(discounts, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDiscBasedOnItem(DiscBasedOnItem discBased)
        {
            string complete_url = "", msg = "";
            Stream dataStream;
            StreamReader reader;
            HttpWebRequest HTTP_Request;
            HttpWebResponse HTTP_Response;
            //discBased.PostingTime=DateTime.Now.AddHours()
            List<Discount> discounts = new List<Discount>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = discounts, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
            else
            {
                try
                {
                    var myContent = JsonConvert.SerializeObject(discBased);
                    //myContent = myContent.Replace(null, "");
                    complete_url = accessFVM.IPPort + "Api/DiscBasedOnItem";
                    HTTP_Request = (HttpWebRequest)HttpWebRequest.Create(complete_url);
                    HTTP_Request.Method = "POST";
                    HTTP_Request.ContentType = "application/json";
                    HTTP_Request.Headers.Add("AccessKey", accessFVM.AccessKey);
                    HTTP_Request.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                    HTTP_Request.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                    HTTP_Request.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());

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

                    discounts = (new JavaScriptSerializer()).Deserialize<List<Discount>>(msg);
                }
                catch (Exception ex)
                {
                    Discount discount = new Discount();
                    discount.ReturnMsg = "Network error occured.Please reload the page Or Re-Login";
                    var resultantJson1 = Json(new { dataList = discount, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                    resultantJson1.MaxJsonLength = int.MaxValue;
                    return resultantJson1;
                }
                await Task.Delay(0);
                var resultantJson = Json(new { dataList = discounts, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
        public async Task<ActionResult> GetSalesEmployee()
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
                complete_url = accessFVM.IPPort + "Api/GetSalesEmployeeDetails?BuisnessUnit=" + BuisnessUnit;
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
                salesEmployees = (new JavaScriptSerializer()).Deserialize<List<SalesEmployee>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(salesEmployees, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetUPI()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetUPIList";
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
        public async Task<ActionResult> GetDoctorsPrescriptionHeader(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetDoctorsPrescriptionHeader?DocEntry=" + DocEntry);
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
                itemsViewModels = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(itemsViewModels[0], JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetAllDoctorsPrescriptionDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                //complete_url = String.Format(objAPIURL + "Api/GetSalesOrderDetails?DocEntry=" + DocEntry);
                complete_url = String.Format(accessFVM.IPPort + "Api/GetDoctorsPrescriptionDetail?DocEntry=" + DocEntry);
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
                itemsViewModels = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(itemsViewModels, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetCustomerVoucher(string VoucherType, string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<CustomerVoucherBalance> voucherBalances = new List<CustomerVoucherBalance>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetCustomerVoucherBalance?VoucherType=" + VoucherType + "&CardCode=" + CardCode);
                //complete_url = objAPIURL + "Api/GetWarehouseDetails";
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
                voucherBalances = (new JavaScriptSerializer()).Deserialize<List<CustomerVoucherBalance>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(voucherBalances, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRowForPaymentMode(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewPaymentMode");
        }
        public ActionResult AddRowForPaymentModeView(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewPaymentModeView");
        }
        public async Task<ActionResult> GetCustomerVoucherBalance(string VoucherType, string CardCode,int CardId)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<CustomerVoucherBalance> voucherBalances = new List<CustomerVoucherBalance>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetCustomerVoucherBalance?VoucherType=" + VoucherType + "&CardCode=" + CardCode + "&CardId=" + CardId);
                //complete_url = objAPIURL + "Api/GetWarehouseDetails";
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
                voucherBalances = (new JavaScriptSerializer()).Deserialize<List<CustomerVoucherBalance>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(voucherBalances[0], JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewMemberRegistration()
        {
            ViewBag.BranchMem = Session[SessionCollection.Branch].ToString();
            return PartialView("_AddMemberRegistration");
        }
        public async Task<ActionResult> SalesOrderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documents = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderHeader?DocEntry=" + DocEntry);
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
                documents = (new JavaScriptSerializer()).Deserialize<List<DocumentsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(documents[0], JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFollowUpBy()
        {
            var BuisnessUnit = Session[SessionCollection.Branch].ToString();
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Employee> employees = new List<Employee>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetEmployeeDetails?BuisnessUnit=" + BuisnessUnit);
                //complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetEmployeeDetails");
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
                employees = (new JavaScriptSerializer()).Deserialize<List<Employee>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(employees, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> TreatmentFollowUp(DateTime? fromdate, DateTime? todate, string SalesNo, string CustomerName,string CustomerMobile, string TreatmentName, string FollowUpBy)
        {
            var userAccess = Session[SessionCollection.UserId];
            if (userAccess == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (fromdate == null)
                {
                    fromdate = System.DateTime.Now.Date;

                }
                if (todate == null)
                {
                    todate = System.DateTime.Now.Date;

                }

                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);
                string BuisnessUnit = Session[SessionCollection.Branch].ToString();
                string UserCode = Session[SessionCollection.UserId].ToString();

                List<TreatmentFolloupDetail> aList = await GetSalesOrderHeaderTreatmentFollowUp(BuisnessUnit, UserCode, sfromdate, stodate, SalesNo, CustomerName, CustomerMobile, TreatmentName, FollowUpBy);
                ViewBag.Branch = BuisnessUnit;
                return View(aList);
            }
        }

        [HttpGet]
        public async Task<ActionResult> TreatmentFollowUpList_Search(DateTime? fromdate, DateTime? todate, string SalesNo, string CustomerName, string CustomerMobile, string TreatmentName, string FollowUpBy)
        {
            List<TreatmentFolloupDetail> aList = new List<TreatmentFolloupDetail>();
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
                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);
                branch = Session[SessionCollection.Branch].ToString();
                userId = Session[SessionCollection.UserId].ToString();
                aList = await GetSalesOrderHeaderTreatmentFollowUp(branch, userId, sfromdate, stodate, SalesNo, CustomerName, CustomerMobile, TreatmentName, FollowUpBy);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
        }
        public async Task<List<TreatmentFolloupDetail>> GetSalesOrderHeaderTreatmentFollowUp(string BuisnessUnit, string UserCode, string FromDate, string ToDate, string SalesNo, string CustomerName, string CustomerMobile, string TreatmentName, string FollowUpBy)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<TreatmentFolloupDetail> treatmentFolloups = new List<TreatmentFolloupDetail>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/TreatmentFolloupDetails?BusinessUnit=" + BuisnessUnit +  "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&SONO=" + SalesNo + "&CardName=" + CustomerName + "&Mobile=" + CustomerMobile + "&Treatment=" + TreatmentName + "&FollowedBy=" + FollowUpBy);
                //complete_url = objAPIURL + "Api/GetBPDetails";"&User=" + UserCode +
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
                treatmentFolloups = (new JavaScriptSerializer()).Deserialize<List<TreatmentFolloupDetail>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return treatmentFolloups;
        }


        public async Task<ActionResult> SaveTreatmentFollow(Activitys activitys)
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
                    var myContent = JsonConvert.SerializeObject(activitys);
                    complete_url = accessFVM.IPPort + "Api/PostActivity";
                    HTTP_Request = (HttpWebRequest)HttpWebRequest.Create(complete_url);
                    HTTP_Request.Method = "POST";
                    HTTP_Request.ContentType = "application/json";
                    HTTP_Request.Headers.Add("AccessKey", accessFVM.AccessKey);
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

        public ActionResult AddExchange(int exId = 0)
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.ExchangeOrderId = exId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                return View();
            }

        }
        // Start Service Exchange
        public ActionResult AddRowForExchange(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_AddExExchangeRow");
        }
        public ActionResult AddRowForNewExchange(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_AddNewExchangeRow");
        }

        [HttpGet]
        public async Task<ActionResult> SalesExchangeViewData(string DocEntry)
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
                aList = await GetAllSalesOrderHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllSalesExchangeDetails(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<List<ItemsViewModel>> GetAllSalesExchangeDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            string ItemHide = "N";
            string Status = "O";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderDetails?DocEntry=" + DocEntry + "&ItemHide=" + ItemHide + "&Status=" + Status);
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
                itemsViewModels = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return itemsViewModels;
        }
        public async Task<ActionResult> SaveExchangeOrder(SalesInvoice so)
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
                    var myContent = JsonConvert.SerializeObject(so);
                    complete_url = accessFVM.IPPort + "Api/PostServiceExchangeWithPayment";  //"Api/PostInvoice";
                    HTTP_Request = (HttpWebRequest)HttpWebRequest.Create(complete_url);
                    HTTP_Request.Method = "POST";
                    HTTP_Request.ContentType = "application/json";
                    HTTP_Request.Headers.Add("AccessKey", accessFVM.AccessKey);
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
        // End Service Exchange
    }
}