using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Hangfire;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
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
        private BaseDb _dbContext;

        public VkController()
        {
            vkService = new VkService();
            _dbContext = new BaseDb();
            cohortAnalyser = new CohortAnalyser();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListOfResults()
        {
            var userId = User.Identity.GetUserId();

            var results = _dbContext.VkCohortAnalyseResults.Where(x => x.UserId == userId)
                .OrderByDescending(order => order.CollectionDate)
                .Select(rest => new AnalyseResultsViewModel()
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    DateOfCollection = rest.CollectionDate,
                    AnalyseType = "Когортный анализ",
                    GroupId = rest.GroupId
                })
                .ToList();

            return View(results);
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
                _logger.Error(string.Format("Error: {0}",ex));
            }

            return View(result);
        }

        [HttpPost]
        public ActionResult CohortAnalysis(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => AnalyzeAndSaveData(model, accessToken, userId));

                ViewBag.Message = "Когортный анализ активностей";

                return View();
            }

            return RedirectToAction("Index");
        }

        public void AnalyzeAndSaveData(CohortAnalysysInputModel model, string accessToken, string userId)
        {
            vkService.AccessToken = accessToken;
            var analyzeModels = vkService.GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate, model.ExcludeUsers);

            var result = cohortAnalyser.Analyze(analyzeModels, model.Step, model.StartDate,
                model.EndDate, model.GroupId);

            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkCohortAnalyseResults.Add(new VkCohortAnalyseResult
                {
                    UserId = userId,
                    Name = model.Name,
                    CollectionDate = DateTime.Now,
                    GroupId = model.GroupId,
                    Result = rr
                });
                cntx.SaveChanges();
            }
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