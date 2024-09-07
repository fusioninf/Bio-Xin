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
    public class MemberRegistrationController : Controller
    {
        private AccessFileViewModel accessFVM;
        public MemberRegistrationController()
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

        // GET: MemberRegistration
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Index(DateTime? fromdate, string phone, int? groupcode)
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

                if (groupcode == null)
                {
                    groupcode = 0;
                }

                string sfromdate = String.Format("{0:yyyyMMdd}", fromdate);

                List<BusinessPartnerViewModel> aList = await GetAllMemberRegistration(branch, userId, sfromdate, phone, groupcode);

                ViewBag.Branch = branch;

                return View(aList);
            }

        }
        [HttpGet]
        public async Task<ActionResult> MemberRegistrationList_Search(DateTime? fromdate, string phone, int? groupcode)
        {
            List<BusinessPartnerViewModel> aList = new List<BusinessPartnerViewModel>();
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

                aList = await GetAllMemberRegistration(branch, userId, sfromdate, phone, groupcode);


                var resultantJson = Json(new { dataList = aList, UnAutorized = 0 }, JsonRequestBehavior.AllowGet);
                resultantJson.MaxJsonLength = int.MaxValue;
                return resultantJson;
            }

        }
        public async Task<List<BusinessPartnerViewModel>> GetAllMemberRegistration(string BuisnessUnit, string UserCode, string FromDate, string phone, int? GroupCode)
        {
            string strGroupCode = "";
            if (GroupCode == 0)
            {
                strGroupCode = "";
            }
            else
            {
                strGroupCode = GroupCode.ToString();
            }
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                //complete_url = String.Format(objAPIURL + "Api/GetBPDetails?BuisnessUnit=" + BuisnessUnit + "&UserCode=" + UserCode + "&FromDate=" + FromDate + "&Mobile=" + phone + "&GroupCode=" + GroupCode);
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetails?FDate=" + FromDate + "&Mobile=" + phone + "&GroupCode=" + strGroupCode);
                //complete_url = String.Format(objAPIURL + "Api/GetBPDetails");
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
                BusinessPartnerViewModel businessPartner = new BusinessPartnerViewModel();
                businessPartner.ReturnMsg = "Network error occured.Please reload the page Or Re-Login";
                businessPartner.ReturnCode = "-44444";
                businessPartners.Add(businessPartner);
                return businessPartners;

            }
          
            await Task.Delay(0);
            return businessPartners;
        }
        public ActionResult ViewMemberRegistration(int id)
        {
            return PartialView("_MemberRegistrationView");
        }
        [HttpGet]
        public async Task<ActionResult> MemberRegistrationViewData(string CardCode)
        {
            BusinessPartnerViewModel aList = new BusinessPartnerViewModel();
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                aList.UnAutorized = 1;
                return Json(aList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                aList = await GetMemberRegistrationDetails(CardCode);
                aList.UnAutorized = 0;
                return Json(aList, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<BusinessPartnerViewModel> GetMemberRegistrationDetails(string CardCode)
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
            return businessPartners[0];
        }

        public ActionResult AddMemberRegistration(string memId = "", string eflag = "ed")
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.MemberId = memId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.eflag = eflag;
                return View();
            }
        }
  
        public async Task<ActionResult> SaveMemberRegistration(MemberRegistration itr)
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
                    complete_url = accessFVM.IPPort + "Api/PostBP";
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

        public List<Branch> LoadBranch()
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

            return branches;
        }
        public async Task<ActionResult> GetBranch()
        {
            List<Branch> branches = new List<Branch>();
            branches = Session[SessionCollection.BuisnessUnit] as List<Branch>;
            if (branches == null)
            {
                branches = LoadBranch();
            }
            await Task.Delay(0);
            return Json(branches, JsonRequestBehavior.AllowGet);
        }


        public List<BusinessPartnerViewModel> LoadGroup()
        {
            string complete_url = "", msg = "";
            string GroupType = "C";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPGroupDetails?P_GroupType=" + GroupType);
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
            return businessPartners;
        }
        public async Task<ActionResult> GetGroup()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.BPGroup] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadGroup();
            }
            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> GetGroup()
        //{
        //    string complete_url = "", msg = "";
        //    string GroupType = "C";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFVM.IPPort + "Api/GetBPGroupDetails?P_GroupType=" + GroupType);
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(businessPartners, JsonRequestBehavior.AllowGet);
        //}
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
        //public async Task<ActionResult> GetSalesEmployee()
        //{
        //    string complete_url = "", msg = "";
        //    string BuisnessUnit = Session[SessionCollection.Branch].ToString();
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<SalesEmployee> salesEmployees = new List<SalesEmployee>();
        //    try
        //    {
        //        complete_url = accessFVM.IPPort + "Api/GetSalesEmployeeDetails?BuisnessUnit=" + BuisnessUnit;
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        salesEmployees = (new JavaScriptSerializer()).Deserialize<List<SalesEmployee>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(salesEmployees, JsonRequestBehavior.AllowGet);
        //}

        public List<BusinessPartnerViewModel> LoadCountry()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetCountryDetails");
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
            return businessPartners;
        }
        public async Task<ActionResult> GetCountry()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.Country] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadCountry();
            }
            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> GetCountry()
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFVM.IPPort + "Api/GetCountryDetails");
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(businessPartners, JsonRequestBehavior.AllowGet);
        //}
        public async Task<ActionResult> GetState(string CountryCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetStateDetails?CountryCode=" + CountryCode);
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
        public async Task<ActionResult> GetThana(string DistrictCode)
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetThanaMaster?DistrictCode=" + DistrictCode);
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
        public List<BusinessPartnerViewModel> LoadOccupation()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetCustomerOccupation");
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
            return businessPartners;
        }
        public async Task<ActionResult> GetOccupation()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.Occupation] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadOccupation();
            }
            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> GetOccupation()
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFVM.IPPort + "Api/GetCustomerOccupation");
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(businessPartners, JsonRequestBehavior.AllowGet);
        //}

        public List<BusinessPartnerViewModel> LoadRelationShip()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetCustomerRelationShip");
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

            return businessPartners;
        }
        public async Task<ActionResult> GetRelationShip()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.Relation] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadRelationShip();
            }
            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> GetRelationShip()
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFVM.IPPort + "Api/GetCustomerRelationShip");
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(businessPartners, JsonRequestBehavior.AllowGet);
        //}

        public List<BusinessPartnerViewModel> LoadPaymentTerms()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetPaymentTerms");
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
            return businessPartners;
        }
        public async Task<ActionResult> GetPaymentTerms()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.PaymentTerms] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadPaymentTerms();
            }
            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> GetPaymentTerms()
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFVM.IPPort + "Api/GetPaymentTerms");
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(businessPartners, JsonRequestBehavior.AllowGet);
        //}

        public List<BusinessPartnerViewModel> LoadBankDetails()
        {
            string complete_url = "", msg = "";
            HttpWebResponse responseObject = null;
            Stream dataStream;
            StreamReader reader;
            WebRequest requestObject;
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            try
            {
                complete_url = String.Format(accessFVM.IPPort + "Api/GetHouseBankDetails");
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

            return businessPartners;
        }
        public async Task<ActionResult> GetBankDetails()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.HouseBank] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadBankDetails();
            }
            await Task.Delay(0);
            return Json(businessPartners, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> GetBankDetails()
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFVM.IPPort + "Api/GetHouseBankDetails");
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
        //        requestObject.Headers.Add("Branch", Session[SessionCollection.Branch].ToString());
        //        requestObject.Headers.Add("UserId", Session[SessionCollection.UserId].ToString());
        //        requestObject.Headers.Add("AuthToken", Session[SessionCollection.AuthToken].ToString());
        //        responseObject = (HttpWebResponse)requestObject.GetResponse();
        //        dataStream = responseObject.GetResponseStream();
        //        reader = new StreamReader(dataStream);
        //        msg = reader.ReadToEnd();
        //        businessPartners = (new JavaScriptSerializer()).Deserialize<List<BusinessPartnerViewModel>>(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    await Task.Delay(0);
        //    return Json(businessPartners, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult AddMemberRegistrationNew(string memId = "", string eflag = "ed")
        {
            var access = Session[SessionCollection.UserId];
            if (access == null)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.MemberId = memId;
                ViewBag.Branch = Session[SessionCollection.Branch].ToString();
                ViewBag.eflag = eflag;
                ViewBag.ddlGroup = new SelectList(GetGroupNew(), "GroupCode", "GroupName");
                ViewBag.ddlOccupation = new SelectList(GetOccupationNew(), "Value", "Description");
                ViewBag.ddlRelationship = new SelectList(GetRelationShipNew(), "Value", "Description");
                ViewBag.ddlPaymentTerms = new SelectList(GetPaymentTermsNew(), "PaymentTermsGrpCode", "PaymentTermsGrpName");
                //ViewBag.ddlBankName = new SelectList(GetBankDetailsNew(), "BankCode", "BankName");
                ViewBag.ddlBranch = new SelectList(GetBranchNew(), "PrcCode", "PrcName");
                ViewBag.ddlSalesEmployee = new SelectList(GetSalesEmployeeNew(), "SlpCode", "SlpName");
                //
                return View();
            }
        }
        public List<BusinessPartnerViewModel> GetGroupNew()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.BPGroup] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadGroup();
            }
            return businessPartners;
        }
        public List<Branch> GetBranchNew()
        {
            List<Branch> branches = new List<Branch>();
            branches = Session[SessionCollection.BuisnessUnit] as List<Branch>;
            if (branches == null)
            {
                branches = LoadBranch();
            }
            return branches;
        }
        public List<SalesEmployee> GetSalesEmployeeNew()
        {
            List<SalesEmployee> salesEmployees = new List<SalesEmployee>();
            salesEmployees = Session[SessionCollection.SaleEmployee] as List<SalesEmployee>;
            if (salesEmployees == null)
            {
                salesEmployees = LoadSalesEmployee();
            }
            return salesEmployees;
        }
        public List<BusinessPartnerViewModel> GetOccupationNew()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.Occupation] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadOccupation();
            }
            return businessPartners;
        }
        public List<BusinessPartnerViewModel> GetRelationShipNew()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.Relation] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadRelationShip();
            }
            return businessPartners;
        }
        public List<BusinessPartnerViewModel> GetPaymentTermsNew()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.PaymentTerms] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadPaymentTerms();
            }
            return businessPartners;
        }
        public List<BusinessPartnerViewModel> GetBankDetailsNew()
        {
            List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
            businessPartners = Session[SessionCollection.HouseBank] as List<BusinessPartnerViewModel>;
            if (businessPartners == null)
            {
                businessPartners = LoadBankDetails();
            }
            return businessPartners;
        }
    }
}