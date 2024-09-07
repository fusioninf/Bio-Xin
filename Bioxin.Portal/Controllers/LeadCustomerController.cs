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
    public class LeadCustomerController : Controller
    {
        private AccessFileViewModel accessFVM;
        public LeadCustomerController()
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
        // GET: LeadCustomer
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
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetailsAll?CardType=L&FDate=" + FromDate + "&Mobile=" + phone );
                //complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetailsAll?CardType=L&FDate=" + FromDate + "&Mobile=" + phone + "&GroupCode=" + strGroupCode);
                //complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetailsAll?FDate=" + FromDate + "&Mobile=" + phone + "&GroupCode=" + strGroupCode);
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
                complete_url = String.Format(accessFVM.IPPort + "Api/GetBPDetailsAll?CardCode=" + CardCode);
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
    }
}