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
    public class ActivityController : Controller
    {
        private AccessFileViewModel accessFileViewModel;
        public ActivityController()
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
        public ActionResult ViewActivity(int id)
        {
            return PartialView("_ActivityView");
        }
        public ActionResult AddActivity(int aId = 0, string eflag = "ed")
        {
            string branch = "";
            string userId = "";
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                //TempData["message"] = "Session Out, Please Login Again";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                branch = Session[SessionCollection.Branch].ToString();
                userId = Session[SessionCollection.UserId].ToString();
                ViewBag.ActivityId = aId;
                ViewBag.Branch = branch;
                ViewBag.UserId = userId;
                ViewBag.eflag = eflag;
                return View();

            }
        }
        public async Task<ActionResult> GetActivityTypeMaster()
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ActivityTypeMaster> activityTypeMasters = new List<ActivityTypeMaster>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetActivityTypeMaster");
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                activityTypeMasters = (new JavaScriptSerializer()).Deserialize<List<ActivityTypeMaster>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(activityTypeMasters, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetActivityMaster(string ActivityType)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ActivityMaster> activityMasters = new List<ActivityMaster>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetActivityMaster?ActivityTypeCode=" + ActivityType);
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                activityMasters = (new JavaScriptSerializer()).Deserialize<List<ActivityMaster>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(activityMasters, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetActivityStatusMaster()
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ActivityStatusMaster> activityStatuses = new List<ActivityStatusMaster>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetActivityStatusMaster");
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                activityStatuses = (new JavaScriptSerializer()).Deserialize<List<ActivityStatusMaster>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(activityStatuses, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetAssinedTo()
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
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetEmployeeDetails");
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                employees = (new JavaScriptSerializer()).Deserialize<List<Employee>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetDailyOpertaionStatus()
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DailyOpertaionStatus> dailyOpertaions = new List<DailyOpertaionStatus>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetDailyOpertaionStatus");
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                dailyOpertaions = (new JavaScriptSerializer()).Deserialize<List<DailyOpertaionStatus>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(dailyOpertaions, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetActivitySession()
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ActivitySession> activitySessions = new List<ActivitySession>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetActivitySession");
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                activitySessions = (new JavaScriptSerializer()).Deserialize<List<ActivitySession>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(activitySessions, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetSalesOrder(string CardCode)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentsViewModel> salesOrder = new List<DocumentsViewModel>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetSalesOrderHeader?CardCode=" + CardCode);
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                salesOrder = (new JavaScriptSerializer()).Deserialize<List<DocumentsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(salesOrder, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetItemBySalesOrder(int SaleOrder)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<ItemsViewModel> salesItems = new List<ItemsViewModel>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetSalesOrderDetails?DocEntry=" + SaleOrder);
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                salesItems = (new JavaScriptSerializer()).Deserialize<List<ItemsViewModel>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return Json(salesItems, JsonRequestBehavior.AllowGet);
        }

        //[SessionCheck]
        [HttpGet]
        public async Task<ActionResult> Index(DateTime? fromdate, DateTime? todate)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            var activeSession = Session[SessionCollection.UserId];
            if (activeSession == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                branch = Session[SessionCollection.Branch].ToString();
                userId = Session[SessionCollection.UserId].ToString();
                authToken = Session[SessionCollection.AuthToken].ToString();
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

                List<DocumentActivity> aList = await GetAllActivity(branch, userId, sfromdate, stodate);

                ViewBag.Branch = branch;
                return View(aList);
            }

        }
        [HttpGet]
        public async Task<ActionResult> ActivityList_Search(DateTime? fromdate, DateTime? todate)
        {
            List<DocumentActivity> aList = new List<DocumentActivity>();
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

                 aList = await GetAllActivity(branch, userId, sfromdate, stodate);

                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }

        public async Task<List<DocumentActivity>> GetAllActivity(string BuisnessUnit, string UserCode, string FromDate, string ToDate)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentActivity> documentActivities = new List<DocumentActivity>();
            try
            {
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetActivityList?BuisnessUnit=" + BuisnessUnit + "&User=" + UserCode + "&FromDate=" + FromDate + "&ToDate=" + ToDate);
                //complete_url = objAPIURL + "Api/GetBPDetails";
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
                documentActivities = (new JavaScriptSerializer()).Deserialize<List<DocumentActivity>>(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(0);
            return documentActivities;
        }

        public async Task<ActionResult> SaveActivity(Activitys activitys)
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
                    complete_url = accessFileViewModel.IPPort + "Api/PostActivity";
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
                    throw;
                }
                await Task.Delay(0);
                var resultantJson = Json(new { dataList = returnDatas[0], UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }
        }
        [HttpGet]
        public async Task<ActionResult> ActivityViewData(string ActivityCode)
        {
            string branch = "";
            string userId = "";
            string authToken = "";
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<DocumentActivity> documentActivities = new List<DocumentActivity>();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {

                var resultantJson = Json(new { dataList = documentActivities, UnAutorized = 1 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            } else
            {
                try
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                    complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetActivityList?ActivityCode=" + ActivityCode);
                    //complete_url = objAPIURL + "Api/GetBPDetails";
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
                    documentActivities = (new JavaScriptSerializer()).Deserialize<List<DocumentActivity>>(msg);
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(0);

                var resultantJson = Json(new { dataList = documentActivities[0], UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
    }
}