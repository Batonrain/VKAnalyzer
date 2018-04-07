using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Hangfire;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels.Memas;
using VKAnalyzer.Services.VK;
using VKAnalyzer.Services.VK.CohortAndSale;

namespace VKAnalyzer.Controllers.Vk
{
    public class MemologyController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly VkService _vkService;
        private readonly BaseDb _dbContext;

        public MemologyController(BaseDb baseDb, VkService vkService)
        {
            _dbContext = baseDb;
            _vkService = vkService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Analyze()
        {
            if (ModelState.IsValid)
            {
                var accessToken = GetCurrentUserAccessToken();
                var userId = User.Identity.GetUserId();
                BackgroundJob.Enqueue(() => _vkService.AnalyzeMemas(accessToken, userId));

                ViewBag.Message = "Анализатор мемасов";

                return View("~/Views/Vk/InProgress.cshtml");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Result(int id)
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
                Logger.Error(string.Format("Error: {0}", ex));
            }

            return View("~/Views/Memology/Result.cshtml", result);
        }

        public FileResult Download(string path)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = "Список пользователей.txt";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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