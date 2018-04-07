using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Services.VK;
using VKAnalyzer.Services.VK.CohortAndSale;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly VkService _vkService;
        private readonly VkDatabaseService _vkDatabaseService;

        public VkController(VkService vkService, VkDatabaseService vkDatabaseService)
        {
            _vkService = vkService;
            _vkDatabaseService = vkDatabaseService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Results()
        {
            return View();
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
            return _vkDatabaseService.GetTargetGroups(accountId, clientId, accessToken);
        }

        public string GetCities(int country = 1)
        {
            var accessToken = GetCurrentUserAccessToken();
            return _vkDatabaseService.GetCities(accessToken, country);
        }

        public string GetUnivercity(int country = 1, int city = 1)
        {
            var accessToken = GetCurrentUserAccessToken();
            return _vkDatabaseService.GetUniversities(accessToken, country, city);
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