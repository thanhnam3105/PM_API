using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Tos.Web.Controllers.Helpers;
using TOS.Web.Controllers.Helpers;
using TOS.Web.Properties;

namespace TOS.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Filters.Add(new ActionFilterService());

            // コントローラー名、パラメーター名(省略可能) によるマッピング
            config.Routes.MapHttpRoute(
                name: "DefaultApiWithId",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { id = @"\d+" });

            // コントローラー名、アクション名によるマッピング
            config.Routes.MapHttpRoute(
                name: "DefaultApiWithAction",
                routeTemplate: "api/{controller}/{action}");

            // コントローラー名 (Get) によるマッピング
            config.Routes.MapHttpRoute(
                name: "DefaultApiGet",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Get" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });

            // コントローラー名 (Post) によるマッピング
            config.Routes.MapHttpRoute(
                name: "DefaultApiPost",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Post" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            // コントローラー名 (Put) によるマッピング
            config.Routes.MapHttpRoute(
                name: "DefaultApiPut",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Put" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) });

            // コントローラー名 (Delete) によるマッピング
            config.Routes.MapHttpRoute(
                name: "DefaultApiDelete",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Delete" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) });

            config.Routes.MapHttpRoute(
                name: "AuthApi",
                routeTemplate: "auth",
                defaults: new { action = "Post", controller = "Auth" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            // OData Query の有効化
            //config.EnableQuerySupport();

            // 認証フィルターの追加
            //config.Filters.Add(new AuthenticationFilterAttribute());

            // 例外フィルターの追加
            //config.Filters.Add(new LoggingExceptionFilterAttribute());

            JsonSerializerSettings jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            // タイムゾーンにUTCを指定
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            jsonSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}
