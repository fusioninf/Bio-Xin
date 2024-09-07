using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebSolution.Models;
using System.Net.Http.Headers;

namespace WebSolution.Controllers
{
    public class SalesOrderController : Controller
    {
        ResultResponse aResponse = new ResultResponse();
        Uri baseAddress = new Uri("http://localhost:55189/");
        
        public ActionResult ViewOrder(int id)
        {
            return PartialView("_SalesOrderView");
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult AddRowForOrder(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRow");
        }
        public ActionResult SalesorderEntry(int soId = 0, string eflag = "ed")
        {
            ViewBag.SalesOrderId = soId;
            ViewBag.eflag = eflag;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SaveSalesOrder(SalesOrder salesOrder)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var myContent = JsonConvert.SerializeObject(salesOrder);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = new HttpResponseMessage();
                if (salesOrder.DocEntry > 0)
                {
                  response = client.PostAsync("SalesOrder/UpdateSalesOrder/", byteContent).Result;
                }
                else
                {
                  response = client.PostAsync("SalesOrder/AddSalesOrder/", byteContent).Result;
                }
              
              
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Msg = "Submitted Successfully";
                    aResponse.IsSuccessStatusCode = response.IsSuccessStatusCode;
                    aResponse.pk = 1;
                }
                else
                {
                    aResponse.IsSuccessStatusCode = false;
                    aResponse.pk = 0;
                }

                await Task.Delay(0);
                return Json(aResponse, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult AddRowForOrderSap(int tr)
        {
            TempData["trSl"] = tr;
            return PartialView("_addNewOrderRowSap");
        }

        [HttpPost]
        public async Task<ActionResult> GetSalesOrderById(SalesOrder salesOrder)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var myContent = JsonConvert.SerializeObject(salesOrder);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = client.PostAsync("SalesOrder/GetSalesOrderById/", byteContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    salesOrder = JsonConvert.DeserializeObject<SalesOrder>(data);
                }
                else
                {
                    Console.WriteLine("Internal server Error");
                }
            }
            await Task.Delay(0);
            return Json(salesOrder, JsonRequestBehavior.AllowGet);
        }

    }
}