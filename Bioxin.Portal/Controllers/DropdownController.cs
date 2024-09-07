using Portal.Controllers;
using Portal.Middleware;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebSolution.Models;
using WebSolution.Models.Constants;

namespace WebSolution.Controllers
{
    public class DropdownController : BaseController
    {
        [AuthorizeUser]
        public async Task<ActionResult> GetSalesEmployee()
        {
            var salesEmployees = Session[SessionCollection.SaleEmployee] as List<SalesEmployee>;

            if (salesEmployees == null || salesEmployees.Count == 0)
            {
                var completeUrl = $"Api/GetSalesEmployeeDetails?BuisnessUnit={branch}";
                salesEmployees = await CallApi<List<SalesEmployee>>(completeUrl, RequestMethods.GET);
                Session[SessionCollection.SaleEmployee] = salesEmployees;
            }

            // Return the sales employees as JSON
            return Json(salesEmployees, JsonRequestBehavior.AllowGet);
        }

    }
}