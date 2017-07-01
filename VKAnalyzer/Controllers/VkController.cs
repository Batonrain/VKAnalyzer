using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Services;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private VkService vkService;
        private CohortAnalyser cohortAnalyser;

        public VkController()
        {
            vkService = new VkService();

            cohortAnalyser = new CohortAnalyser();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Result(string code, string state)
        {
            return View();
        }

        [HttpPost]
        public ActionResult CohortAnalysis(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                vkService.AccessToken = GetCurrentUserAccessToken();
                var analyzeModels = vkService.GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate);

                var result = cohortAnalyser.Analyze(analyzeModels, model.Step, model.StartDate,
                    model.EndDate, model.GroupId);
                
                ViewBag.Message = "Когортный анализ активностей";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        private string GetCurrentUserAccessToken()
        {
            var context = new BaseDb();
            var userId = User.Identity.GetUserId();
            var result = context.UserAccessTokens.FirstOrDefault(us => us.VkUserId == userId);

            return result.AccessToken;
        }

    }
}