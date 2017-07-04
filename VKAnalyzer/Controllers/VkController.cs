using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
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
                    DateOfCollection = rest.CollectionDate.ToString(),
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
                // removed error handling logic!
            }

            return View(result);
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

                var binaryFormatter = new BinaryFormatter();

                using (var ms = new MemoryStream())
                {
                    binaryFormatter.Serialize(ms, result);
                    byte[] rr = ms.GetBuffer();

                    var cntx = new BaseDb();
                    cntx.VkCohortAnalyseResults.Add(new VkCohortAnalyseResult
                    {
                        UserId = User.Identity.GetUserId(),
                        CollectionDate = DateTime.Now,
                        GroupId = model.GroupId,
                        Result = rr
                    });
                    cntx.SaveChanges();
                }

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