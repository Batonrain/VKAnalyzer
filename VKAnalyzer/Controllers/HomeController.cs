using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.DBContexts;

namespace VKAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var cntx = new BaseDb();
            var result = cntx.VkCohortAnalyseResults.FirstOrDefault(x => x.UserId == userId);

            var set = new CohortAnalysisResultModel();
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(result.Result))
                {

                    set = (CohortAnalysisResultModel)formatter.Deserialize(ms);

                }
            }
            catch (Exception ex)
            {
                // removed error handling logic!
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}