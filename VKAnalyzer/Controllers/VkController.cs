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
using VKAnalyzer.Models.VKModels.Memas;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly VkService _vkService;
        private readonly BaseDb _dbContext;

        public VkController()
        {
            _vkService = new VkService();
            _dbContext = new BaseDb();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListOfResults()
        {
            var userId = User.Identity.GetUserId();
            var model = new ListOfResults();

            model.CohortAnalyseResults = _dbContext.VkCohortAnalyseResults.Where(x => x.UserId == userId)
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

            model.MemasAnalyzeResults = _dbContext.VkMemasAnalyzeResults.Where(x => x.UserId == userId)
                .OrderByDescending(order => order.CollectionDate)
                .Select(rest => new AnalyseResultsViewModel()
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    DateOfCollection = rest.CollectionDate,
                    AnalyseType = "Анализ мемасов",
                })
                .ToList();

            return View(model);
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

        public ActionResult MemasResult(int id)
        {
            var resultDb = _dbContext.VkMemasAnalyzeResults.FirstOrDefault(rest => rest.Id == id);
            var result = new MemasAnalyzeResultModel();
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(resultDb.Result))
                {
                    result = (MemasAnalyzeResultModel)formatter.Deserialize(ms);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error: {0}", ex));
            }

            return View("~/Views/Vk/Memas/Result.cshtml", result);
        }

        public FileResult Download(string path)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = "Список пользователей.txt";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpPost]
        public ActionResult AnalyzeActivities(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => _vkService.AnalyzeActivities(model, accessToken, userId));

                ViewBag.Message = "Когортный анализ активностей";

                return View();
            }

            return RedirectToAction("Index");
        }

        public ActionResult VkCohortAnalyseOfSales()
        {
            var accessToken = GetCurrentUserAccessToken();
            var targets = _vkService.GetTargetsGroups(accessToken);
            ViewData["AllTargetGroups"] = from target in targets select new SelectListItem { Text = target.name, Value = target.id };

            return View("~/Views/Vk/CohortAnalyseOfSales/Index.cshtml");
        }

        [HttpPost]
        public ActionResult VkCohortAnalyseOfSales(VkAnalyseSalesModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => _vkService.AnalyzeSales(model, accessToken, userId));

                ViewBag.Message = "Когортный анализ продаж";

                return View("~/Views/Vk/CohortAnalyseOfSales/AnalyseInProgress.cshtml");
            }

            return RedirectToAction("Index");
        }

        public ActionResult MemasAnalyze()
        {
            return View("~/Views/Vk/Memas/Index.cshtml");
        }

        [HttpPost]
        public ActionResult AnalyzeMemas()
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => _vkService.AnalyzeMemas(accessToken, userId));

                ViewBag.Message = "Анализатор мемасов";

                return View("~/Views/Vk/CohortAnalysis.cshtml");
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