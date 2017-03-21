using System.Web.Mvc;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        public ActionResult CohortAnalysis()
        {
            ViewBag.Message = "Cohort analysis";

            return View();
        }
    }
}