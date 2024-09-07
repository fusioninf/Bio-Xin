using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using WebSolution.Models;

namespace WebSolution.Controllers
{
    public class InventoryTransferController : Controller
    {
        private AccessFileViewModel accessFileViewModel;
        public InventoryTransferController()
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
        public ActionResult ViewInventoryTransfer(int id)
        {
            return PartialView("_InventoryTransferView");
        }
        public ActionResult AddInventoryTransfer(int itId = 0, string eflag = "ed")
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.InventoryTransferId = itId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.UserId = Session[SessionCollection.UserId].ToString();
                ViewBag.eflag = eflag;
                return View();
            }
        }
        public async Task<ActionResult> GetStockTransferRequestHeader()
        {
            string BuisnessUnit = Session[SessionCollection.Branch].ToString();
            string Status = "O";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetStockTransferRequestHeader?BuisnessUnit=" + BuisnessUnit + "&Status=" + Status);
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
            return Json(documentsViewModels, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SaveTransfer1(Transfer model)
        {
                try
                {
                    string data = JsonConvert.SerializeObject(model);
                    string completeUrl = accessFileViewModel.IPPort + "Api/PostStockTransfer";
                    var client = new RestClient(completeUrl);
                    var request = new RestRequest(Method.POST);
                    request.AddJsonBody(data);
                    request.AddHeader("accept", "application/json");
                    request.AddHeader("AccessKey", accessFileViewModel.AccessKey);
                    request.AddHeader("Branch", Session[SessionCollection.Branch].ToString());
                    request.AddHeader("UserId", Session[SessionCollection.UserId].ToString());
                    request.AddHeader("AuthToken", Session[SessionCollection.AuthToken].ToString());
                    // Make the asynchronous POST request
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IRestResponse response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        // Deserialize the response content
                        var msg = response.Content;
                        var returnDatas = JsonConvert.DeserializeObject<List<ReturnData>>(msg);
                        return Json(returnDatas.FirstOrDefault(), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // Handle unsuccessful response here, if needed
                        return Json(new ReturnData { ReturnMsg = "API request failed." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions here
                    return Json(new ReturnData { ReturnMsg = ex.Message }, JsonRequestBehavior.AllowGet);
                }
        }
        public async Task<ActionResult> SaveTransfer(Transfer model)
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
                    foreach (var property in model.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) && property.GetValue(model) == null)
                        {
                            property.SetValue(model, "");
                        }
                    }
                    // Serialize the object back to JSON with nulls replaced by empty strings
                    var myContent = JsonConvert.SerializeObject(model);
                    //myContent = myContent.Replace(null, "");
                    complete_url = accessFileViewModel.IPPort + "Api/PostStockTransfer";  //"Api/PostSalesOrderWithAdvancePayment";
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
                //return Json(returnDatas[0], JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        //public async Task<ActionResult> Index(DateTime? fromdate, DateTime? todate)
        //{
        //    var activeSession = Session[SessionCollection.UserId];
        //    if (activeSession == null)
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }
        //    else
        //    {
        //        if (fromdate == null)
        //        {
        //            fromdate = System.DateTime.Now.AddDays(-1).Date;

        //        }
        //        if (todate == null)
        //        {
        //            todate = System.DateTime.Now.Date;

        //        }

        //        string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
        //        string stodate = String.Format("{0:yyyyMMdd}", todate);

        //        List<DocumentsViewModel> aList = await GetAllStockTransferHeader(Session[SessionCollection.Branch].ToString(), Session[SessionCollection.UserId].ToString(), sfromdate, stodate);
        //        ViewBag.Branch = Session[SessionCollection.Branch].ToString();
        //        return View(aList);
        //    }

        //}
        public async Task<ActionResult> Index()
        {
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                await Task.Delay(0);
                return View();
            }
           
        }
        [HttpGet]
        public async Task<ActionResult> GetCurrentDayData(DateTime? fromdate, DateTime? todate)
        {
            string branch = "";
            string userId = "";
            if (fromdate == null)
            {
                fromdate = System.DateTime.Now.AddDays(-1).Date;

            }
            if (todate == null)
            {
                todate = System.DateTime.Now.Date;

            }
            branch = Session[SessionCollection.Branch].ToString();
            userId = Session[SessionCollection.UserId].ToString();
            string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
            string stodate = String.Format("{0:yyyyMMdd}", todate);

            List<DocumentsViewModel> aList = await GetAllStockTransferHeader(branch, userId, sfromdate, stodate);
            return PartialView("_InventoryTransferList", aList);
        }

        [HttpGet]
        public async Task<ActionResult> InventoryTransferList_Search(DateTime? fromdate, DateTime? todate)
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

                aList = await GetAllStockTransferHeader(branch, userId, sfromdate, stodate);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
        public async Task<List<DocumentsViewModel>> GetAllStockTransferHeader(string BuisnessUnit, string UserCode, string FromDate, string ToDate)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetStockTransferHeader?BuisnessUnit=" + BuisnessUnit + "&FromDate=" + FromDate + "&ToDate=" + ToDate);
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
            return documentsViewModels;
        }
        [HttpGet]
        public async Task<ActionResult> InventoryTransferViewData(string DocEntry)
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
                aList = await GetAllStockTransferHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllStockTransferDetails(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<DocumentsViewModel> GetAllStockTransferHeaderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetStockTransferHeader?DocEntry=" + DocEntry);
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
        public async Task<List<ItemsViewModel>> GetAllStockTransferDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetStockTransferDetail?DocEntry=" + DocEntry);
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
        [HttpGet]
        public async Task<ActionResult> InventoryTransferReceipt()
        {
            string branch = "";
            string userId = "";
            var userAccess = Session[SessionCollection.UserId];
            if (userAccess == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                branch = Session[SessionCollection.Branch].ToString();
                userId = Session[SessionCollection.UserId].ToString();
                List<DocumentsViewModel> aList = await GetAllStockTransfertoReceiptfromIntransit(branch);
                ViewBag.Branch = branch;
                return View(aList);
            }
 
        }
        public async Task<List<DocumentsViewModel>> GetAllStockTransfertoReceiptfromIntransit(string BuisnessUnit)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetStockTransfertoReceiptfromIntransit?BuisnessUnit=" + BuisnessUnit);
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
            return documentsViewModels;
        }
        public async Task<ActionResult> SaveTransferReceipt(List<InventoryTransferReceipt> things)
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
                    foreach (var single in things)
                    {
                        var myContent = JsonConvert.SerializeObject(single);
                        //myContent = myContent.Replace(null, "");
                        complete_url = accessFileViewModel.IPPort + "Api/PostStockTransferReceiptFromIntransit";
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
                //return Json(returnDatas[0], JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public async Task<ActionResult> ViewBatches(string ItemCode, string WareHouse,int id)
        {
            List<BatchesViewModel> newList = new List<BatchesViewModel>();
            List<BatchesViewModel> aList = await GetAllBatchesItem(ItemCode, WareHouse);
            foreach(var slist in aList)
            {
                BatchesViewModel batches = new BatchesViewModel();
                batches.ItemCode = slist.ItemCode;
                batches.ItemName = slist.ItemName;
                batches.BatchNum = slist.BatchNum;
                batches.ExpDate = slist.ExpDate;
                batches.InDate = slist.InDate;
                batches.Stock = slist.Stock;
                batches.RowId = id;
                newList.Add(batches);
            }
            return PartialView("_BatchView", newList);
        }
        public async Task<List<BatchesViewModel>> GetAllBatchesItem(string ItemCode, string WareHouse)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BatchesViewModel> batches = new List<BatchesViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetItemWiseBatchDetails?ItemCode=" + ItemCode + "&WareHouse=" + WareHouse);
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
                batches = (new JavaScriptSerializer()).Deserialize<List<BatchesViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return batches;
        }
        [HttpGet]
        public async Task<ActionResult> ViewSerial(string ItemCode, string WareHouse, int id)
        {
            List<SerialViewModel> newList = new List<SerialViewModel>();
            List<SerialViewModel> aList = await GetAllSerialItem(ItemCode, WareHouse);
            foreach (var slist in aList)
            {
                SerialViewModel serial = new SerialViewModel();
                serial.ItemName = slist.ItemName;
                serial.IntrSerial = slist.IntrSerial;
                serial.SysSerial = slist.SysSerial;
                serial.SuppSerial = slist.SuppSerial;
                serial.ExpDate = slist.ExpDate;
                serial.Stock = slist.Stock;
                serial.InDate = slist.InDate;
                serial.RowId = id;
                newList.Add(serial);
            }
            return PartialView("_SerialView", newList);
        }
        public async Task<List<SerialViewModel>> GetAllSerialItem(string ItemCode, string WareHouse)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<SerialViewModel> serials = new List<SerialViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetItemWiseSerialDetails?ItemCode=" + ItemCode + "&WareHouse=" + WareHouse);
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
                serials = (new JavaScriptSerializer()).Deserialize<List<SerialViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return serials;
        }
        public async Task<ActionResult> GetDeliveryConfirmStatus()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DeliveryConfirmStatus> deliveryConfirms = new List<DeliveryConfirmStatus>();
            try
            {
                complete_url = accessFileViewModel.IPPort + "Api/GetDeliveryConfirmStatus";
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
                deliveryConfirms = (new JavaScriptSerializer()).Deserialize<List<DeliveryConfirmStatus>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(deliveryConfirms, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetDeliveryChannel()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DeliveryConfirmStatus> deliveryConfirms = new List<DeliveryConfirmStatus>();
            try
            {
                complete_url = accessFileViewModel.IPPort + "Api/GetDeliveryChannelData";
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
                deliveryConfirms = (new JavaScriptSerializer()).Deserialize<List<DeliveryConfirmStatus>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(deliveryConfirms, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetDeliveryAgent()
        {
           var BuisnessUnit= Session[SessionCollection.Branch].ToString();
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Employee> employees = new List<Employee>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetEmployeeDetails?BuisnessUnit=" + BuisnessUnit);
                //complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetEmployeeDetails");
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
                employees = (new JavaScriptSerializer()).Deserialize<List<Employee>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetThana()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetThanaMaster");
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
                businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetWarehouseHead()
        {
            string complete_url = "", msg = "";
            string BusinessUnit = Session[SessionCollection.Branch].ToString();
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Warehouse> warehouses = new List<Warehouse>();
            try
            {
                complete_url = accessFileViewModel.IPPort + "Api/GetWarehouseDetailsAll?BusinessUnit=" + BusinessUnit;
                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                requestObject.ContentType = "application/json";
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
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

        [HttpGet]
        public async Task<ActionResult> EcommerceCourierInfoUpdate()
        {
            var userAccess = Session[SessionCollection.UserId];
            if (userAccess == null)
                return RedirectToAction("Login", "Home");
            else
                return View();
        }

        [HttpGet]
        public async Task<ActionResult> EcommerceCourierInfoList_Search(DateTime? fromdate, DateTime? todate, string SalesNo, string CardCode, string MobileNo, string DeliveryChannel, string DeliveryAgent, string Thana, string DeliveryStatus, int PageSize = 50, int PageNumber = 1)
        {
            List<DocumentsInvTransECommerce> aList = new List<DocumentsInvTransECommerce>();
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
                string BuisnessUnit = Session[SessionCollection.Branch].ToString();
                string UserCode = Session[SessionCollection.UserId].ToString();
                aList = await GetStockTransferHeaderECommerce(BuisnessUnit, sfromdate, stodate, SalesNo, CardCode, MobileNo, DeliveryChannel, DeliveryAgent, Thana, DeliveryStatus, PageSize, PageNumber);

                var resultantJson = Json(new { dataList = aList, totalRows = aList.Count > 0 ? aList.FirstOrDefault()?.TotalRows : 0, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
        public async Task<List<DocumentsInvTransECommerce>> GetStockTransferHeaderECommerce(string BuisnessUnit, string FromDate, string ToDate, string SalesNo, string CardCode, string MobileNo, string DeliveryChannel, string DeliveryAgent, string Thana, string DeliveryStatus, int PageSize, int PageNumber)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsInvTransECommerce> documentsInvTransEs = new List<DocumentsInvTransECommerce>();
            try
            {
                complete_url = String.Format(
                    accessFileViewModel.IPPort + "Api/GetECommerceCorrierTrackHeader?BuisnessUnit={0}&FromDate={1}&ToDate={2}&SODocNum={3}&CardCode={4}&Mobile={5}&DelChannel={6}&DelAgent={7}&Area={8}&Status={9}&PageSize={10}&PageNumber={11}",
                    Uri.EscapeDataString(BuisnessUnit),
                    Uri.EscapeDataString(FromDate),
                    Uri.EscapeDataString(ToDate),
                    Uri.EscapeDataString(SalesNo),
                    Uri.EscapeDataString(CardCode),
                    Uri.EscapeDataString(MobileNo),
                    Uri.EscapeDataString(DeliveryChannel),
                    Uri.EscapeDataString(DeliveryAgent),
                    Uri.EscapeDataString(Thana),
                    Uri.EscapeDataString(DeliveryStatus),
                    PageSize,
                    PageNumber
                );

                requestObject = WebRequest.Create(complete_url);
                requestObject.Method = "GET";
                requestObject.ContentType = "application/json";
                requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
                requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
                requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
                requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
                responseObject = (HttpWebResponse)requestObject.GetResponse();
                dataStream = responseObject.GetResponseStream();
                reader = new StreamReader(dataStream);
                msg = reader.ReadToEnd();
                documentsInvTransEs = (new JavaScriptSerializer()).Deserialize<List<DocumentsInvTransECommerce>>(msg);
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                throw;
            }
            finally
            {
                responseObject?.Close();
            }

            await Task.Delay(0);
            return documentsInvTransEs;
        }

        public async Task<ActionResult> SaveEcommerceCourierInfo(List<StockTransferECommerce> things)
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
                    foreach (var single in things)
                    {
                        var myContent = JsonConvert.SerializeObject(single);
                        //myContent = myContent.Replace(null, "");
                        complete_url = accessFileViewModel.IPPort + "Api/UpdateECommerceCourierInfo";
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
    }
}