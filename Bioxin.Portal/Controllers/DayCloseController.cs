using Newtonsoft.Json;
using Portal.Controllers;
using Portal.Middleware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebSolution.Models;
using WebSolution.Models.Constants;
using WebSolution.Models.DayClose;

namespace WebSolution.Controllers
{
    public class DayCloseController : BaseController
    {
        [AuthorizeUser]
        public ActionResult Index()
        {
            ViewBag.Branch = branch;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Search(DateTime? fromDate, DateTime? toDate)
        {
            string sfromdate = String.Format("{0:yyyyMMdd}", fromDate);
            string stodate = String.Format("{0:yyyyMMdd}", toDate);
            var result = new List<DayCloseResponseDto>();
            string url = string.Format("Api/GetDayCloseHeaders?BuisnessUnit={0}&FromDate={1}&ToDate={2}", branch, sfromdate, stodate);
            result = await CallApi<List<DayCloseResponseDto>>(url, RequestMethods.GET);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [AuthorizeUser]
        public ActionResult Add()
        {
            ViewBag.Branch = branch;
            ViewBag.UserId = userId;
            return View();
        }

        public async Task<ActionResult> GetDailyAllRecivedDetails(DateTime? closingDate)
        {
            List<ReceivedResponseDto> closeDay = await FetchReceives(closingDate);

            await Task.Delay(0);
            return Json(closeDay, JsonRequestBehavior.AllowGet);
        }

    

        public async Task<ActionResult> GetDailyBankRecivedDetails(DateTime? closingDate)
        {
            List<BankReceiveResponseDto> closeDay = await FetchBankReceives(closingDate);

            await Task.Delay(0);
            return Json(closeDay, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetDailyClosingStockNoteDetails()
        {
            string complete_url = "Api/GetDailyClosingStockNoteDetails";
            var result = CallApi<List<CloseDay>>(complete_url, RequestMethods.GET);

            await Task.Delay(0);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> SaveDayClose(CloseDay closeday)
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
                    // Iterate through the properties and replace null values with empty strings
                    foreach (var property in closeday.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) && property.GetValue(closeday) == null)
                        {
                            property.SetValue(closeday, "");
                        }
                    }
                    var myContent = JsonConvert.SerializeObject(closeday);
                    complete_url = accessFVM.IPPort + "Api/PostDayClosing";
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
        public async Task<ActionResult> GetDayClose(string fromdate)
        {
            var complete_url = $"Api/GetDayClose?BuisnessUnit={branch}&ClosingDate={fromdate}";
            var result = CallApi<List<CloseDay>>(complete_url, RequestMethods.GET);

            await Task.Delay(0);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Details(long docKey)
        {
            var complete_url = $"Api/DayClose/Details?docKey={docKey}";
            var result = await CallApi<List<DayCloseLineResponseDto>>(complete_url, RequestMethods.GET);

            var firstRow = result[0];
            var date = DateTime.Parse(firstRow.DayCloseDate, new CultureInfo("en-GB"));
            var responseDto = new DayCloseDetailsResponseDto()
            {
                DayCloseDate = firstRow.DayCloseDate,
                BranchName = firstRow.BranchName,
                CashAmount = firstRow.CashAmount,
                Code = firstRow.Code,
                BranchId = firstRow.BranchId,
                DocEntry = firstRow.DocEntry,
                ExtraCash = firstRow.ExtraCash,
                OtherAmount = firstRow.OtherAmount,
                Remarks = firstRow.Remarks,
                Status = firstRow.Status,
                Lines = result,
                Banks = await FetchBankReceives(date),
                Receiveds = await FetchReceives(date)
            };
            await Task.Delay(0);
            return Json(responseDto, JsonRequestBehavior.AllowGet);
        }

        private async Task<List<BankReceiveResponseDto>> FetchBankReceives(DateTime? closingDate)
        {
            string sClosingDate = String.Format("{0:yyyyMMdd}", closingDate);
  
            var complete_url = $"Api/GetDailyBankRecivedDetails?BuisnessUnit={branch}&ClosingDate={sClosingDate}";
            var closeDay = await CallApi<List<BankReceiveResponseDto>>(complete_url, RequestMethods.GET);
            return closeDay;
        }

        private async Task<List<ReceivedResponseDto>> FetchReceives(DateTime? closingDate)
        {
            string sClosingDate = String.Format("{0:yyyyMMdd}", closingDate);
            var complete_url = $"Api/GetDailyAllRecivedDetails?BuisnessUnit={branch}&ClosingDate={sClosingDate}";
            var closeDay = await CallApi<List<ReceivedResponseDto>>(complete_url, RequestMethods.GET);
            return closeDay;
        }
    }
}