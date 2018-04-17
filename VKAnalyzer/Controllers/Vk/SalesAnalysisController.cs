using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Hangfire;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Services.VK;
using VKAnalyzer.Services.VK.CohortAndSale;

namespace VKAnalyzer.Controllers.Vk
{
    public class SalesAnalysisController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly VkService _vkService;
        private readonly BaseDb _dbContext;

        public SalesAnalysisController(VkService vkService)
        {
            _dbContext = new BaseDb();
            _vkService = vkService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Analyze(VkAnalyseSalesModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => _vkService.AnalyzeSales(model, accessToken, userId));

                ViewBag.Message = "Когортный анализ продаж";

                return View("~/Views/Vk/InProgress.cshtml");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Result(int id)
        {
            var resultDb = _dbContext.VkCohortSalesAnalyseResults.FirstOrDefault(rest => rest.Id == id);
            var result = new SalesActivitiesRetargetResult();
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(resultDb.Result))
                {
                    result = (SalesActivitiesRetargetResult)formatter.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error: {0}", ex));
            }

            return View(result);
        }

        public ActionResult ResultWithList(int id)
        {
            var resultDb = _dbContext.VkCohortSalesAnalyseWithListResults.FirstOrDefault(rest => rest.Id == id);
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

            var model = _dbContext.VkCohortSalesAnalyseResults.Where(x => x.UserId == userId)
                .OrderByDescending(order => order.CollectionDate)
                .Select(rest => new AnalyseResultsViewModel()
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    DateOfCollection = rest.CollectionDate,
                    AnalyseType = "Анализ продаж с ретаргетом",
                    Type = Types.CohortWithRetarget
                })
                .ToList();

            var listModels = _dbContext.VkCohortSalesAnalyseWithListResults.Where(x => x.UserId == userId)
                .OrderByDescending(order => order.CollectionDate)
                .Select(rest => new AnalyseResultsViewModel()
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    DateOfCollection = rest.CollectionDate,
                    AnalyseType = "Анализ продаж со списком покупателей",
                    Type = Types.CohortWithList
                })
                .ToList();

            model.AddRange(listModels);

            return View(model);
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