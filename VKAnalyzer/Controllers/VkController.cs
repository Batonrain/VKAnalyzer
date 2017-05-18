using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
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

        [HttpPost]
        public ActionResult CohortAnalysis(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                var analyzeModels = vkService.GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate);

                var result = cohortAnalyser.Analyze(analyzeModels, model.Step, model.StartDate,
                    model.EndDate, model.GroupId);
                
                ViewBag.Message = "Cohort analysis";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        

    }
}