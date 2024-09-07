using System;
using System.Web;
using System.Web.Mvc;
using WebSolution.Models;

namespace Portal.Middleware
{
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var userId = httpContext.Session[SessionCollection.UserId];
            return userId != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // If the request is Ajax, return a JSON response
                filterContext.Result = new JsonResult
                {
                    Data = new { 
                        message = "Un-authorized user",
                        unAuthorized = 1,
                        redirectTo = "/Home/Login"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                // If the request is not Ajax, redirect to the login page
                //filterContext.Result = new RedirectResult("~/Home/Login");
            }
        }
    }
}
