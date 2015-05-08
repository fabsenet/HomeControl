using System;
using System.Diagnostics;
using System.Web.Mvc;
using Serilog;

namespace Web.Controllers
{
    public class LogAction :ActionFilterAttribute
    {
        private const string Key = "LogAction_Stopwatch";
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
               filterContext.HttpContext.Items[Key] = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
           Log.ForContext<LogAction>()
                .Verbose("Executing {@Action} on {@Controller} took {elapsed} ms"
               , filterContext.ActionDescriptor.ActionName
               , filterContext.ActionDescriptor.ControllerDescriptor.ControllerName
               , ((Stopwatch)filterContext.HttpContext.Items[Key]).Elapsed);
        }
    }
}