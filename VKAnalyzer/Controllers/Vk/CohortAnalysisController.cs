using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Controllers.Vk
{
    public class CohortAnalysisController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly VkService _vkService;
        private readonly BaseDb _dbContext;

        public CohortAnalysisController()
        {
            _dbContext = new BaseDb();
            _vkService = new VkService();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Result(int id)
        {
            var resultDb = _dbContext.VkCohortAnalyseResults.FirstOrDefault(rest => rest.Id == id);
            var result = new CohortAnalysisResultModel();
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(resultDb.Result))
                {
                    result = (CohortAnalysisResultModel)formatter.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error: {0}", ex));
            }

            return View(result);
        }

        public ActionResult Results()
        {
            var userId = User.Identity.GetUserId();

            var model = _dbContext.VkCohortAnalyseResults.Where(x => x.UserId == userId)
                .OrderByDescending(order => order.CollectionDate)
                .Select(rest => new AnalyseResultsViewModel
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    DateOfCollection = rest.CollectionDate,
                    AnalyseType = "Когортный анализ",
                    GroupId = rest.GroupId
                })
                .ToList();

            return View(model);
        } 

        [HttpPost]
        public ActionResult Analyze(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => _vkService.AnalyzeActivities(model, accessToken, userId));

                ViewBag.Message = "Когортный анализ активностей";

                return View("~/Views/Vk/InProgress.cshtml");
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