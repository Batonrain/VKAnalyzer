using System;
using System.Collections.Generic;
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
using VKAnalyzer.Models.VKModels.Memas;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private VkService vkService;
        private VkMemasService _vkMemasService;
        private CohortAnalyser cohortAnalyser;
        private BaseDb _dbContext;

        public VkController()
        {
            vkService = new VkService();
            _vkMemasService = new VkMemasService();
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
            var analyzeModels = vkService.GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate, null, false);

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

        public ActionResult VkCohortAnalyseOfSales()
        {
            return View("~/Views/Vk/CohortAnalyseOfSales/Index.cshtml");
        }

        [HttpPost]
        public ActionResult VkCohortAnalyseOfSales(VkCohortAnalyseOfSalesModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => AnalyzeSalesAndSaveData(model, accessToken, userId));

                ViewBag.Message = "Когортный анализ продаж";

                return View();
            }

            return RedirectToAction("Index");
        }

        public void AnalyzeSalesAndSaveData(VkCohortAnalyseOfSalesModel model, string accessToken, string userId)
        {
            vkService.AccessToken = accessToken;
            var listOfBuyers = ConvertstringToList(model.ListOfBuyers);
            var analyzeModels = vkService.GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate, listOfBuyers, false);


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
                BackgroundJob.Enqueue(() => AnalyzeAndSaveMemasData(accessToken, userId));

                ViewBag.Message = "Анализатор мемасов";

                return View("~/Views/Vk/CohortAnalysis.cshtml");
            }

            return RedirectToAction("Index");
        }

        public void AnalyzeAndSaveMemasData(string accessToken, string userId)
        {
            vkService.AccessToken = accessToken;
            var result = _vkMemasService.Analyze(accessToken);

            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkMemasAnalyzeResults.Add(new VkMemasAnalyzeResult
                {
                    UserId = userId,
                    Name = string.Format("Анализатор мемасов за {0}", DateTime.Now),
                    CollectionDate = DateTime.Now,
                    Result = rr
                });
                cntx.SaveChanges();
            }
        }

        private IEnumerable<string> ConvertstringToList(string input)
        {
            return input.Replace("\r\n","").Split(';').ToList();
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