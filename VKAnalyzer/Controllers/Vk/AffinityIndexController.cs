using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Hangfire;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels.AffinityIndex;
using VKAnalyzer.Services.Interfaces;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Controllers.Vk
{
    public class AffinityIndexController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AffinityIndexService _affinityIndexService;

        public AffinityIndexController(AffinityIndexService affinityIndexService)
        {
            _affinityIndexService = affinityIndexService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Start(AffinityIndexOptionsModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                var audiencesUnderAnalysis = new List<AffinityIndexOptionsAuditoryModel>() {model.Auditory1};
                var comparativeAudience = new List<AffinityIndexOptionsAuditoryModel>() { model.Auditory2 };

                BackgroundJob.Enqueue(() => _affinityIndexService.Start(audiencesUnderAnalysis, model.Auditory2, model.AccountId, model.ClientId, accessToken, userId));

                ViewBag.Message = "Аффинити Индекс";

                return View("~/Views/Vk/InProgress.cshtml");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Result(int id)
        {
            return View();
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