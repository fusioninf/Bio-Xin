using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebSolution.Models;

namespace WebSolution.Utility
{
    public class SessionManagement
    {
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            var activeSession = session[SessionCollection.UserId];
            if (activeSession == null)
            {
      
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { action = "Login", controller = "Home" }));
                return;
            }
            //base.OnActionExecuting(filterContext);
        }
    }
}