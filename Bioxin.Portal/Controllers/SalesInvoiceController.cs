using Newtonsoft.Json;
using Portal.Controllers;
using Portal.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebSolution.Models;
using WebSolution.Models.Constants;

namespace WebSolution.Controllers
{
    public class SalesInvoiceController : BaseController
    {
      
        // GET: SalesInvoice
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AuthorizeUser]
        public async Task<ActionResult> Index(DateTime? fromdate, DateTime? todate, string cardCode, string cardName, string mobileNo, string docNo)
        {
            if (fromdate == null) fromdate = System.DateTime.Now.Date;
            if (todate == null) todate = System.DateTime.Now.Date;
           
            string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
            string stodate = String.Format("{0:yyyyMMdd}", todate);

            List<DocumentsViewModel> aList = await GetAllSalesInvoiceHeader(branch, userId, sfromdate, stodate, cardCode, cardName, mobileNo, docNo, "");

            ViewBag.Branch = branch;

            return View(aList);

        }
        [HttpGet]
        public async Task<ActionResult> SalesInvoiceList_Search(DateTime? fromdate, DateTime? todate, string cardCode, string cardName, string mobileNo, string docNo, string salesEmployeeId)
        {
            List<DocumentsViewModel> aList = new List<DocumentsViewModel>();
           
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

                aList = await GetAllSalesInvoiceHeader(branch, userId, sfromdate, stodate, cardCode, cardName, mobileNo, docNo, salesEmployeeId);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
        public async Task<List<DocumentsViewModel>> GetAllSalesInvoiceHeader(string BuisnessUnit, string UserCode, string FromDate, string ToDate, string CardCode, string CardName, string MobileNo, string DocNo, string salesEmployeeId)
        {
            var url = $"Api/GetSaleInvoiceHeader?BuisnessUnit={BuisnessUnit}&FromDate={FromDate}&ToDate={ToDate}&CardCode={CardCode}&CardName={CardName}&MobileNo={MobileNo}&DocNo={DocNo}&salesEmployeeId={salesEmployeeId}";
            var result = await CallApi<List<DocumentsViewModel>>(url, RequestMethods.GET);
            return result;
        }
        public ActionResult ViewSalesInvoice(int id)
        {
            return PartialView("_SalesInvoiceView");
        }
        public ActionResult AddRowForInvoice(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRow");
        }
        public ActionResult AddRowForPaymentMode(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewPaymentMode");
        }
        public ActionResult AddRowForInvoiceView(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRowView");
        }
        public ActionResult AddRowForPaymentModeView(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewPaymentModeView");
        }
        [HttpGet]
        public async Task<ActionResult> SalesInvoiceViewData(string DocEntry)
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
                aList = await GetAllSalesInvoiceHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllSalesInvoiceDetails(DocEntry);
                aList.paymentViews = await GetAllPaymentDetailsOrder(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<DocumentsViewModel> GetAllSalesInvoiceHeaderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSaleInvoiceHeader?DocEntry=" + DocEntry);
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
                documentsViewModels = (new JavaScriptSerializer()).Deserialize<List<DocumentsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return documentsViewModels[0];
        }
        public async Task<List<ItemsViewModel>> GetAllSalesInvoiceDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSaleInvoiceDetails?DocEntry=" + DocEntry);
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
                complete_url = String.Format(accessFVM.IPPort + "Api/GetPaymentDetailsInvoice?DocEntry=" + DocEntry);
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
        public async Task<ActionResult> GetSalesOrderHeader(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderHeader?DocEntry=" + DocEntry);
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
        public async Task<ActionResult> GetAllSalesOrderDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderDetailsCopy?ItemHide=N&Status=O&DocEntry=" + DocEntry);
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
                itemsViewModels = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }
            await Task.Delay(0);
            return Json(itemsViewModels, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetAllSalesOrderDetailsUpdate(string DocEntry)
        {
            string complete_url = "";
        
            complete_url = "Api/GetSalesOrderDetailsCopy?ItemHide=N&Status=O&DocEntry=" + DocEntry;
            List<ItemsViewModel> itemsViewModels = await CallApi<List<ItemsViewModel>>(complete_url, RequestMethods.GET);
           
            var warehouse = await CallApi<List<Warehouse>>($"Api/GetWarehouseAllDetails?WhsType=N&BuisnessUnit={branch}", RequestMethods.GET);

            foreach (var item in itemsViewModels)
            {
                item.BatchDetails = await GetAllBatchesItem(item.ItemCode, warehouse[0].WhsCode);
            }

            return Json(new
            {
                warehouse = warehouse,
                items = itemsViewModels
            }, JsonRequestBehavior.AllowGet);

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

        [HttpGet]
        public async Task<ActionResult> ViewBatches(string ItemCode, string WareHouse, int id)
        {
            List<BatchesViewModel> aList = await GetAllBatchesItem(ItemCode, WareHouse);

            return Json(aList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ViewBatchesPage(string ItemCode, string WareHouse, int id)
        {
            List<BatchesViewModel> newList = new List<BatchesViewModel>();
            List<BatchesViewModel> aList = await GetAllBatchesItem(ItemCode, WareHouse);
            foreach (var slist in aList)
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
            var complete_url = $"Api/GetItemWiseBatchDetails?ItemCode={ItemCode}&WareHouse={WareHouse}";
            return await CallApi<List<BatchesViewModel>>(complete_url, RequestMethods.GET);
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
                complete_url = String.Format(accessFVM.IPPort + "Api/GetItemWiseSerialDetails?ItemCode=" + ItemCode + "&WareHouse=" + WareHouse);
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
                serials = (new JavaScriptSerializer()).Deserialize<List<SerialViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return serials;
        }

        [AuthorizeUser]
        public ActionResult AddSalesInvoice(int siId = 0, string eflag = "")
        {
            ViewBag.SalesInvoiceId = siId;
            ViewBag.Branch = Session[SessionCollection.Branch].ToString();
            ViewBag.eflag = eflag;
            return View();
        }
        public async Task<ActionResult> SaveSalesInvoice(SalesInvoice itr)
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
                    complete_url = accessFVM.IPPort + "Api/PostInvoiceWithPayment";  //"Api/PostInvoice";
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
                catch (Exception ex)
                {
                    ReturnData returnData = new ReturnData();
                    returnData.ReturnMsg ="Network error occured.Please reload the page Or Re-Login";
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
        public async Task<ActionResult> GetSalesOrderHeaderByCardCode(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesOrderHeader?DocStatus=O&CardCode=" + CardCode);
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
                //complete_url = objAPIURL + "Api/GetWarehouseDetails;
                //complete_url = objAPIURL + "Api/GetWarehouseDetails?WhsType=N";
                complete_url = accessFVM.IPPort + "Api/GetWarehouseAllDetails?WhsType=N";  //?BuisnessUnit=servicebranch&WhsType=N
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
        public async Task<ActionResult> GetWarehouseRow()
        {
            var result = await CallApi<List<Warehouse>>($"Api/GetWarehouseAllDetails?WhsType=N&BuisnessUnit={branch}", RequestMethods.GET);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetWarehouseServiceBranchRow(string Branch)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Warehouse> warehouses = new List<Warehouse>();
            try
            {
                //complete_url = objAPIURL + "Api/GetWarehouseDetails;
                //complete_url = objAPIURL + "Api/GetWarehouseDetails?WhsType=N";
                complete_url = accessFVM.IPPort + "Api/GetWarehouseAllDetails?WhsType=N&BuisnessUnit=" + Branch;  //?BuisnessUnit=servicebranch&WhsType=N
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
        public async Task<ActionResult> GetItemByItemCodeCardCode(string ItemCode,string CardCode,string HappyHrs)
        {
            var complete_url = $"Api/GetItemDetails?ItemCode={ItemCode}&CardCode={CardCode}&HappyHrs={HappyHrs}";
            var result = await CallApi<List<Item>>(complete_url, RequestMethods.GET);

          
            var warehouse = await CallApi<List<Warehouse>>($"Api/GetWarehouseAllDetails?WhsType=N&BuisnessUnit={branch}", RequestMethods.GET);

            List<BatchesViewModel> batches = await GetAllBatchesItem(ItemCode, warehouse[0].WhsCode);

            return Json(new
            {
                ItemDetails = result[0],
                Batches = batches,
                Warehouses = warehouse
            }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> GetPaymentMethod()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<PaymentMethod> payments = new List<PaymentMethod>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetPaymentMethod";
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
                payments = (new JavaScriptSerializer()).Deserialize<List<PaymentMethod>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(payments, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetPaymentBankCodes(string PaymentMethod)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<PaymentBankCode> paymentBanks = new List<PaymentBankCode>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetPaymentBankCodes?PaymentMethod=" + PaymentMethod);
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
                paymentBanks = (new JavaScriptSerializer()).Deserialize<List<PaymentBankCode>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(paymentBanks, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewMemberRegistration()
        {
            ViewBag.BranchMem = Session[SessionCollection.Branch].ToString();
            return PartialView("_AddMemberRegistration");
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
                ViewBag.ExchangeInvoiceId = exId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                return View();
            }
        }
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
                aList = await GetAllSalesInvoiceHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllSalesExchangeDetails(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<List<ItemsViewModel>> GetAllSalesExchangeDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSalesInvoiceCopyforExchangeItem?DocEntry=" + DocEntry);
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
        public async Task<ActionResult> SaveExchangeInvoice(SalesInvoice itr)
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
                    //myContent = myContent.Replace(null, "");
                    complete_url = accessFVM.IPPort + "Api/PostItemExchangeWithPayment";  //"Api/PostInvoice";
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

        public async Task<ActionResult> AutoCompleteService(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "N";
            string selleItem = "Y";
            string FromPage = "SI";
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
        public async Task<ActionResult> AutoCompleteServiceName(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string inventoryItem = "N";
            string selleItem = "Y";
            string FromPage = "SI";
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
    }
}