using Portal.Controllers;
using Portal.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebSolution.Models;

namespace WebSolution.Controllers
{
    public class HomeController : BaseController
    {
        [AuthorizeUser]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [AuthorizeUser]
        public ActionResult Logout()
        {
            try
            {
               string ms= LogOutCheck();
            }
            catch
            {

            }
            finally
            {
                Session.Clear();
                Session.Abandon();
                Session.RemoveAll();
            }
            return View("Login");
        }
        public ActionResult Login()
        {
            var data = TempData["message"];
            ViewData["LoginMsg"] = data;
            return View("Login");
        }
        public ActionResult LoginAction(LoginUser login)
        {
            string issuccess = check_user_password(login.userName, login.password);

            if (issuccess == "0000")
            {
                Session[SessionCollection.MenuList] = GetMenuStepOne(login.userName);
                //GetBranch();
                //GetGroup();
                //GetSalesEmployee();
                //GetCountry();
                //GetOccupation();
                //GetRelationShip();
                //GetPaymentTerms();
                //GetBankDetails();
                return RedirectToAction("Index", "Home");
            }
            else
            {

                TempData["message"] = issuccess;
                return RedirectToAction("Login", "Home");

            }
        }


        public string check_user_password(string userid, string password)
        {
            string msg_code = "", msg_desc = "";

            string line;
            try
            {
                string api_url = String.Format(accessFVM.IPPort + "Api/CheckPassword?UserId=" + userid + "&Password=" + password);
                WebRequest requestObject;
                requestObject = WebRequest.Create(api_url);
                requestObject.Method = "GET";
                requestObject.ContentType = "application/json";
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);

                HttpWebResponse responseObject = null;
                responseObject = (HttpWebResponse)requestObject.GetResponse();


                using (Stream stream = responseObject.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    line = reader.ReadToEnd();
                    reader.Close();
                }
                List<userlogin_message_from_API> message = (new JavaScriptSerializer()).Deserialize<List<userlogin_message_from_API>>(line);
                foreach (userlogin_message_from_API m in message)
                {
                    msg_code = m.ReturnCode;
                    msg_desc = m.ReturnMsg;
                    Session[SessionCollection.UserId] = userid;
                    Session[SessionCollection.UserType] = m.UserType;
                    Session[SessionCollection.Branch] = m.Branch;
                    Session[SessionCollection.AuthToken] = m.AuthToken;
                    Session[SessionCollection.EmpId] = m.EmpId;
                    Session[SessionCollection.EmpType] = m.EmpType;
                }
                if(msg_code != "0000")
                {
                    msg_code = msg_desc;
                }
                return msg_code;
            }
            catch (Exception ex)
            {
                return  ex.Message ;
            }
        }

        public string LogOutCheck()
        {
            string msg_code = "", msg_desc = "";

            string line;
            try
            {
                var access = Session[SessionCollection.UserId];
                string branch = "";
                string userId = "";
                string authToken = "";
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                //string api_url = String.Format(objAPIURL.get_api_url() +"Api/CheckPassword?UserCode=SAPUser1&Password=Bioxin@123");
                string api_url = String.Format(accessFVM.IPPort + "Api/LogOut");
                WebRequest requestObject;
                requestObject = WebRequest.Create(api_url);
                requestObject.Method = "GET";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", branch);
                requestObject.Headers.Add("UserId", userId);
                requestObject.Headers.Add("AuthToken", authToken);

                HttpWebResponse responseObject = null;
                responseObject = (HttpWebResponse)requestObject.GetResponse();

                using (Stream stream = responseObject.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    line = reader.ReadToEnd();
                    reader.Close();
                }
                List<userlogin_message_from_API> message = (new JavaScriptSerializer()).Deserialize<List<userlogin_message_from_API>>(line);
                foreach (userlogin_message_from_API m in message)
                {
                    msg_code = m.ReturnCode;
                    msg_desc = m.ReturnMsg;
                }
                //return (msg_code == "0000" ? "" : msg_desc);
                return msg_code;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public IEnumerable<MenuStepOne> GetMenuStepOne(string loginUserId)
        {
            try
            {

                List<MenuStepOne> aMenuStepOneList = new List<MenuStepOne>();

                IEnumerable<MenuOperation> menuOperationsList = GetAllMenuUserWise(loginUserId);

                IEnumerable<MenuOperation> tempStepOneList = menuOperationsList.Where(x => x.MenuStep == 1 && x.ParantId == 0);
                MenuStepOne aMenuStepOne;
                foreach (var aItem in tempStepOneList)
                {
                    aMenuStepOne = new MenuStepOne();
                    aMenuStepOne.SL = aItem.SL;
                    aMenuStepOne.MenuName = aItem.MenuName;
                    aMenuStepOne.ControllerName = aItem.ControllerName;
                    aMenuStepOne.ActionName = aItem.ActionName;
                    aMenuStepOne.ParantMenu = aItem.ParantMenu;
                    aMenuStepOne.MenuStepTwoList = GetMenuStepTwo(aItem.SL, menuOperationsList);
                    aMenuStepOneList.Add(aMenuStepOne);
                }
                return aMenuStepOneList;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private List<MenuStepTwo> GetMenuStepTwo(int ParantId, IEnumerable<MenuOperation> menuOperationsList)
        {
            try
            {
                List<MenuStepTwo> aStepTwoList = new List<MenuStepTwo>();

                IEnumerable<MenuOperation> tempStepOneList = menuOperationsList.Where(x => x.MenuStep == 2 && x.ParantId == ParantId);
                MenuStepTwo aMenuStepTwo;
                foreach (var aItem in tempStepOneList)
                {
                    aMenuStepTwo = new MenuStepTwo();
                    aMenuStepTwo.SL = aItem.SL;
                    aMenuStepTwo.MenuName = aItem.MenuName;
                    aMenuStepTwo.ControllerName = aItem.ControllerName;
                    aMenuStepTwo.ActionName = aItem.ActionName;
                    aMenuStepTwo.ParantMenu = aItem.ParantMenu;
                    aStepTwoList.Add(aMenuStepTwo);
                }


                return aStepTwoList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<MenuOperation> GetAllMenuUserWise(string loginUserId)
        {
            string line;
            try
            {
                string branch = "";
                string userId = "";
                string authToken = "";
                var access = Session[SessionCollection.UserId];
                if (access != null)
                {
                    branch = Session[SessionCollection.Branch].ToString();
                    userId = Session[SessionCollection.UserId].ToString();
                    authToken = Session[SessionCollection.AuthToken].ToString();
                }
                string api_url = String.Format(accessFVM.IPPort + "Api/GetUserWiseMenuDetails");
                WebRequest requestObject;
                requestObject = WebRequest.Create(api_url);
                requestObject.Method = "GET";
                requestObject.ContentType = "application/json";
                //requestObject.Timeout = 30;
                requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
                requestObject.Headers.Add("Branch", branch);
                requestObject.Headers.Add("UserId", userId);
                requestObject.Headers.Add("AuthToken", authToken);

                HttpWebResponse responseObject = null;
                responseObject = (HttpWebResponse)requestObject.GetResponse();

                using (Stream stream = responseObject.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    line = reader.ReadToEnd();
                    reader.Close();
                }
                List<MenuOperation> aMenuOperations = (new JavaScriptSerializer()).Deserialize<List<MenuOperation>>(line);

                return aMenuOperations;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult MenuGeneration()
        {
            try
            {
                return PartialView("~/Views/Shared/_Partial_Menu_Leyout.cshtml",
                           (List<MenuStepOne>)Session[SessionCollection.MenuList]);
            }
            catch (Exception exception)
            {
                string message = exception.ToString();

                return RedirectToAction("Login", "Home");
            }
        }

        public void GetBranch()
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
            Session[SessionCollection.BuisnessUnit] = branches;
        }
        public void GetGroup()
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

            Session[SessionCollection.BPGroup] = businessPartners;
        }
        //public async Task<ActionResult> GetGroupDetails(string GroupCode)
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetBPGroupDetails?P_GroupCode=" + GroupCode);
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
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
        public void GetSalesEmployee()
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

            Session[SessionCollection.SaleEmployee] = salesEmployees;
        }
        public void GetCountry()
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

            Session[SessionCollection.Country] = businessPartners;
        }
        //public async Task<ActionResult> GetState(string CountryCode)
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetStateDetails?CountryCode=" + CountryCode);
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
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
        //public async Task<ActionResult> GetThana(string DistrictCode)
        //{
        //    string complete_url = "", msg = "";
        //    HttpWebResponse responseObject = null;
        //    Stream dataStream;
        //    StreamReader reader;
        //    WebRequest requestObject;
        //    List<BusinessPartnerViewModel> businessPartners = new List<BusinessPartnerViewModel>();
        //    try
        //    {
        //        complete_url = String.Format(accessFileViewModel.IPPort + "Api/GetThanaMaster?DistrictCode=" + DistrictCode);
        //        requestObject = WebRequest.Create(complete_url);
        //        requestObject.Method = "GET";
        //        //requestObject.ContentType = "text/json";
        //        requestObject.ContentType = "application/json";
        //        //requestObject.Timeout = 30;
        //        requestObject.Headers.Add("AccessKey", accessFileViewModel.AccessKey);
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
        public void GetOccupation()
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

            Session[SessionCollection.Occupation] = businessPartners;
        }
        public void GetRelationShip()
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

            Session[SessionCollection.Relation] = businessPartners;
        }
        public void GetPaymentTerms()
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

            Session[SessionCollection.PaymentTerms] = businessPartners;
        }
        public void GetBankDetails()
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

            Session[SessionCollection.HouseBank] = businessPartners;
        }
    }
}