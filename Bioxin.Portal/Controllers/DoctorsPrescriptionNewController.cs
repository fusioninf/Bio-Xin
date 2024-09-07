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

namespace WebSolution.Controllers
{
    public class DoctorsPrescriptionNewController : Controller
    {
        private AccessFileViewModel accessFVM;
        public DoctorsPrescriptionNewController()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/AccessFile.xml"));
                //Loop through the selected Nodes.
                foreach (XmlNode node in doc.SelectNodes("/AccessFiles/AccessFile"))
                {
                    accessFVM = (new AccessFileViewModel
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

        // GET: DoctorsPrescriptionNew
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Index(DateTime? fromdate, DateTime? todate,string patientCode,string patientName,string patientMobile, string doctorName,string docNum)
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
                    fromdate = System.DateTime.Now.Date;
                }
                if (todate == null)
                {
                    todate = System.DateTime.Now.Date;
                }

                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);

                List<DocumentsViewModel> aList = await GetAllDoctorsPrescriptionHeader(branch, userId, sfromdate, stodate, patientCode, patientName, patientMobile, doctorName, docNum);

                ViewBag.Branch = branch;

                return View(aList);
            }

        }
        [HttpGet]
        public async Task<ActionResult> DoctorsPrescriptionList_Search(DateTime? fromdate, DateTime? todate, string patientCode, string patientName, string patientMobile, string doctorName, string docNum)
        {
            List<DocumentsViewModel> aList = new List<DocumentsViewModel>();
            string branch = "";
            string userId = "";
            string authToken = "";
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
                authToken = Session[SessionCollection.AuthToken].ToString();
                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);
                string stodate = String.Format("{0:yyyyMMdd}", todate);

                aList = await GetAllDoctorsPrescriptionHeader(branch, userId, sfromdate, stodate, patientCode, patientName, patientMobile, doctorName, docNum);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
                //return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<List<DocumentsViewModel>> GetAllDoctorsPrescriptionHeader(string BuisnessUnit, string UserCode, string FromDate, string ToDate, string patientCode, string patientName, string patientMobile, string doctorName, string docNum)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetDoctorsPrescriptionHeader?BuisnessUnit=" + BuisnessUnit + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&CardCode=" + patientCode + "&CardName=" + patientName + "&Mobile=" + patientMobile + "&DoctorName=" + doctorName + "&DocNum=" + docNum);
               
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
            return documentsViewModels;
        }
        public ActionResult ViewDoctorsPrescription(int id)
        {
            return PartialView("_DoctorsPrescriptionView");
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
        public ActionResult AddRowForPatienTestOrder(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewPatienTestOrderRow");
        }
        public ActionResult AddRowForPatienTestOrderView(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewPatienTestOrderRowView");
        }
        [HttpGet]
        public async Task<ActionResult> DoctorsPrescriptionViewData(string DocEntry)
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
                aList = await GetAllDoctorsPrescriptionHeaderByDocEntry(DocEntry);
                aList.UnAutorized = 0;
                aList.itemsViewModels = await GetAllDoctorsPrescriptionDetails(DocEntry);
                aList.testsViewModels = await GetAllDoctorsPrescriptionTestDetails(DocEntry);
                return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<DocumentsViewModel> GetAllDoctorsPrescriptionHeaderByDocEntry(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetDoctorsPrescriptionHeader?DocEntry=" + DocEntry);
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
        public async Task<List<ItemsViewModel>> GetAllDoctorsPrescriptionDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> itemsViewModels = new List<ItemsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetDoctorsPrescriptionDetail?DocEntry=" + DocEntry);
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
        public async Task<List<TestsViewModel>> GetAllDoctorsPrescriptionTestDetails(string DocEntry)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<TestsViewModel> testsViewModels = new List<TestsViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetPatientHistoryDetails?DocEntry=" + DocEntry);
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
                testsViewModels = (new JavaScriptSerializer()).Deserialize<List<TestsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return testsViewModels;
        }
        public ActionResult AddDoctorsPrescription(int dpId = 0, string eflag = "ed")
        {
            string branch = "";
            string userId = "";
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.DoctorsPrescriptionId = dpId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.eflag = eflag;
                ViewBag.empId = Session[SessionCollection.EmpId].ToString();
                ViewBag.empType = Session[SessionCollection.EmpType].ToString();
                return View();
            }
        }
        public async Task<ActionResult> SaveDoctorsPrescription(DoctorsPrescription itr)
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
                    complete_url = accessFVM.IPPort + "Api/PostDoctorsPrescription";
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
                //return Json(returnDatas[0], JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> GetSalesInvoiceHeaderByCardCode(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> documents = new List<DocumentsViewModel>();
            try
            {
                //complete_url = String.Format(accessFVM.IPPort + "Api/GetSaleInvoiceHeader?DocStatus=O&CardCode=" + CardCode);
                complete_url = String.Format(accessFVM.IPPort + "Api/GetSaleInvoiceHeader?CardCode=" + CardCode);
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
            return Json(documents, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> GetBillToAddress(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPBilltoAddressDetails?CardType=C&CardCode=" + CardCode);
                //complete_url = objAPIURL + "Api/GetBPBilltoAddressDetails";
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
        public async Task<ActionResult> GetShipToAddress(string CardCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPShiptoAddressDetails?CardType=C&CardCode=" + CardCode);
                //complete_url = objAPIURL + "Api/GetBPBilltoAddressDetails";
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

        public async Task<ActionResult> AutoCompleteItemName(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemNameSearch=" + prefix;
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
        public async Task<ActionResult> AutoCompleteItem(string prefix)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
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
                    complete_url = accessFVM.IPPort + "Api/GetItemDetails?ItemCodeSearch=" + prefix;
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
        public async Task<ActionResult> GetPatientTestItem()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetPatientHistoryTestCode";
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
        public async Task<ActionResult> GetDoctor()
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Employee> employees = new List<Employee>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFVM.IPPort + "Api/GetEmployeeDetails?Doctor=Y&BuisnessUnit=" + branch);
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
                employees = (new JavaScriptSerializer()).Deserialize<List<Employee>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetBreakfast()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetPrescriptionBreakFastList";
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
        public async Task<ActionResult> GetLunch()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetPrescriptionLunchList";
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
        public async Task<ActionResult> GetDinner()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<Item> items = new List<Item>();
            try
            {
                complete_url = accessFVM.IPPort + "Api/GetPrescriptionDinnerList";
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
    }
}