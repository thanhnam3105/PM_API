using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TOS.Web.Models;
using System.Security.Claims;
using DocumentFormat.OpenXml.Office.CustomXsn;
using System.Globalization;

namespace TOS.Web.Controllers.Helpers
{
    public class ActionFilterService : System.Web.Http.Filters.ActionFilterAttribute
    {
        /// <summary>
        /// Action before execute api
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="HttpResponseException"></exception>
        public override void OnActionExecuting(HttpActionContext context)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var ctlName = context.ControllerContext.ControllerDescriptor.ControllerName;
            var actionName = context.ActionDescriptor.ActionName;
            var common = new CommonController();
            // Get the scope claim. Ensure that the issuer is for the correct Auth0 domain
            if (!(ctlName == "Account" && (actionName == "Login" || actionName == "IsValidLogin")))
            {
                var isValid = true;
                var token = context.Request.Headers.Authorization?.Parameter?.ToString();
                if (token != null)
                {
                    var account = new AccountController();
                    isValid = account.IsValidToken(token);
                }

                if (token == null || !isValid)
                {
                    var message = "TokenInvalid";
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(message),
                        ReasonPhrase = "Token Exception!"
                    });
                }
            }
        }

        /// <summary>
        /// Action after execute api
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) 
        {
            if(actionExecutedContext.Response != null)
            {
                var objectContent = actionExecutedContext.Response.Content as ObjectContent;

                // Get and check type api result
                var baseType = objectContent?.ObjectType?.BaseType?.Name;
                if (baseType == "ApiResult`1")
                {
                    // Convert type to json
                    var jsonData = JsonConvert.SerializeObject(objectContent.Value);

                    // Convert json to jobject
                    JObject value = JObject.Parse(jsonData);

                    // Get type of jobject
                    Type targetType = value.GetType();

                    // Set new type, value for response content
                    var jObectContent = new ObjectContent(targetType, value, new JsonMediaTypeFormatter(), (string)null);
                    actionExecutedContext.Response.Content = jObectContent;

                    base.OnActionExecuted(actionExecutedContext);
                }
            }
        }
    }
}