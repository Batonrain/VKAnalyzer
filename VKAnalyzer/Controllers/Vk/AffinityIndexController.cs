using System.Web.Mvc;
using NLog;
using VKAnalyzer.Services.Interfaces;

namespace VKAnalyzer.Controllers.Vk
{
    public class AffinityIndexController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IAffinityIndexService _affinityIndexService;

        public AffinityIndexController(IAffinityIndexService affinityIndexService)
        {
            _affinityIndexService = affinityIndexService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Start()
        {
            return View();
        }

        public ActionResult Result(int id)
        {
            return View();
        }
    }
}