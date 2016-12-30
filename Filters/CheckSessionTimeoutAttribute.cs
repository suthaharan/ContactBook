using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Contact2015.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CheckSessionTimeoutAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            var user = session["_userId"];
            if (((user == null) && (!session.IsNewSession)) || (session.IsNewSession) || (null == filterContext.HttpContext.Session))
            {
                //redirect to login page
                var url = new System.Web.Mvc.UrlHelper(filterContext.RequestContext);
                var loginUrl = url.Content("~/user/logout");
                //filterContext.HttpContext.Response.Redirect(loginUrl,true);
                filterContext.Result = new RedirectResult(loginUrl);
                return;
            }
        }
    }
}