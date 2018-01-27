using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly BaseDb _dbContext;

        public VkController()
        {
            _dbContext = new BaseDb();
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
    }
}