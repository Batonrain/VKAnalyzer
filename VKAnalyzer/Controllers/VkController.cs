using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly BaseDb _dbContext;
        private readonly VkService _vkService;
        private readonly VkDatabaseService _vkDatabaseService;

        public VkController()
        {
            _dbContext = new BaseDb();
            _vkService = new VkService();
            _vkDatabaseService = new VkDatabaseService();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Results()
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

            model.CohortSalesAnalyseResults = _dbContext.VkCohortSalesAnalyseResults.Where(x => x.UserId == userId)
                .OrderByDescending(order => order.CollectionDate)
                .Select(rest => new AnalyseResultsViewModel()
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    DateOfCollection = rest.CollectionDate,
                    AnalyseType = "Анализ продаж",
                })
                .ToList();

            return View(model);
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

        public string GetCities(int country = 1)
        {
            var accessToken = GetCurrentUserAccessToken();
            return _vkDatabaseService.GetCities(accessToken, country);
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