using Portal.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebSolution.Models;
using WebSolution.Models.Constants;

namespace WebSolution.Controllers
{
    public class CommonController : BaseController
    {
        public async Task<ActionResult> GetItemByItemCodeCardCode(string ItemCode, string CardCode, string HappyHrs)
        {
            var complete_url = $"Api/GetItemDetails?ItemCode={ItemCode}&CardCode={CardCode}&HappyHrs={HappyHrs}";
            var result = await CallApi<List<Item>>(complete_url, RequestMethods.GET);

            return Json(result[0], JsonRequestBehavior.AllowGet);

        }
    }
}