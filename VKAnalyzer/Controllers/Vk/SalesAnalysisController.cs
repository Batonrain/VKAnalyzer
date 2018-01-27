﻿using System.Linq;
using System.Web.Mvc;
using Hangfire;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Controllers.Vk
{
    public class SalesAnalysisController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly VkService _vkService;
        private readonly BaseDb _dbContext;

        public SalesAnalysisController()
        {
            _dbContext = new BaseDb();
            _vkService = new VkService();
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

        public string GetAccounts()
        {
            var accessToken = GetCurrentUserAccessToken();
            return _vkService.GetAccounts(accessToken);
        }

        public string GetClients(string accountId)
        {
            var accessToken = GetCurrentUserAccessToken();
            return _vkService.GetClients(accountId, accessToken);
        }

        public string GetTargetGroups(string accountId, string clientId)
        {
            var accessToken = GetCurrentUserAccessToken();
            return _vkService.GetTargetGroups(accountId, clientId, accessToken);
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