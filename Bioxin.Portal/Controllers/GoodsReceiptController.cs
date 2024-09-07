using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using WebSolution.Models;

namespace WebSolution.Controllers
{
    public class GoodsReceiptController : Controller
    {
        private AccessFileViewModel accessFileViewModel;
        public GoodsReceiptController()
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
        // GET: GoodsReceipt
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

        public ActionResult ViewGoodsReceipt(int id)
        {
            return PartialView("_GoodsReceiptView");
        }
        public ActionResult AddGoodsReceipt(int grId = 0, string eflag = "ed")
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.GoodsReceiptId = grId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.UserId = Session[SessionCollection.UserId].ToString();
                ViewBag.eflag = eflag;
                return View();
            }

        }
        [HttpGet]
        public async Task<ActionResult> Index(DateTime? fromdate, DateTime? todate, string docNum, string docstatus = "")
        {
            string branch = "";
            string userId = "";
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                branch = Session[SessionCollection.Branch].ToString();
                userId = Session[SessionCollection.UserId].ToString();
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

                List<DocumentsViewModel> aList = await GetAllGoodsReceiptHeader(branch, userId, sfromdate, stodate , docNum ,docstatus);

                ViewBag.Branch = branch;
                return View(aList);
            }    
        }
        [HttpGet]
        public async Task<ActionResult> GoodsReceiptList_Search(DateTime? fromdate, DateTime? todate, string docNum, string docstatus = "")
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

                aList = await GetAllGoodsReceiptHeader(branch, userId, sfromdate, stodate, docNum, docstatus);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
        public async Task<List<DocumentsViewModel>> GetAllGoodsReceiptHeader(string BuisnessUnit, string UserCode, string FromDate, string ToDate, string docNum, string Status)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetGoodsReceiptHeader?BuisnessUnit=" + BuisnessUnit + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&Status=" + Status + "&DocNum=" + docNum);
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
        public async Task<ActionResult> SaveGoodsReceipt(GoodsReceipt goodsReceipt)
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
                    // Serialize the object back to JSON with nulls replaced by empty strings
                    var myContent = JsonConvert.SerializeObject(goodsReceipt);
                    //myContent = myContent.Replace(null, "");
                    complete_url = accessFileViewModel.IPPort + "Api/PostGoodsReceipt";
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
        public async Task<ActionResult> GoodsReceiptViewData(string DocEntry)
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
                aList = await GetAllGoodsReceiptHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllGoodsReceiptDetails(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<DocumentsViewModel> GetAllGoodsReceiptHeaderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetGoodsReceiptHeader?DocEntry=" + DocEntry);
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
        public async Task<List<ItemsViewModel>> GetAllGoodsReceiptDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetGoodsReceiptDetails?DocEntry=" + DocEntry);
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
        public ActionResult ViewBatches(int id)
        {
            return PartialView("_BatchView");
        }
        public ActionResult AddRowForBatch(int tr,int rowId)
        {
            TempData["trBSl"] = tr;
            TempData["trBSlrowId"] = rowId;
            return PartialView("_addNewBatchRow");
        }
        [HttpGet]
        public ActionResult ViewSerial(int qty, int id)
        {
            List<SerialViewModel> newList = new List<SerialViewModel>();
            for(var i = 1; i <= qty; i++)
            {
                SerialViewModel serial = new SerialViewModel();
                serial.Stock = i;
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
    }
}