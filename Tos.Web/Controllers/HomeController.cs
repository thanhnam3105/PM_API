using System.Web.Mvc;

namespace TOS.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public string Get()
        {
            return "";
        }
    }
}
