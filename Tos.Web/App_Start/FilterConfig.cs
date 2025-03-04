using System.Web;
using System.Web.Mvc;
using TOS.Web.Controllers;

namespace TOS.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
