using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.Models.VKModels;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CohortAnalysis(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {

                var posts = GetWallPosts(Convert.ToInt32(model.DaysCount + 30), model.GroupId);

                var rawData = PrepareDataForCohortAnalyse(posts, Convert.ToInt32(model.DaysCount), model.GroupId).OrderBy(d => d.PostDate).ToList();

                var analyser = new CohortAnalyser();

                var result = new CohortAnalysisResultModel
                {
                    ResultMatrix = analyser.BuildCohortAnalyseData(rawData),
                    TableLength = rawData.Count,
                    GroupId = model.GroupId
                };

                ViewBag.Message = "Cohort analysis";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        private IEnumerable<CohortAnalysisModel> PrepareDataForCohortAnalyse(IEnumerable<PostDataModel> posts, int daysCount, string groupId)
        {
            var result = new List<CohortAnalysisModel>();

            for (var i = 1; i <= daysCount; i++)
            {
                var pDate = DateTime.Now.AddDays(-i);
                if (posts.Count(p => p.Date.Date == pDate.Date) > 0)
                {
                    var postsForDay = posts.Where(p => p.Date.Date == pDate.Date).Select(p => p.Id);
                    var likesIds = new List<string>();

                    foreach (var post in postsForDay)
                    {
                        likesIds.AddRange(GetListOfLikedUsers(groupId, post));
                    }

                    result.Add(new CohortAnalysisModel
                    {
                        PostDate = pDate,
                        LikedIds = likesIds.Distinct()
                    });
                }
            }

            return result;
        }

        private void GetPosts(string groupId, AnalyseStep step, DateTime startDate, DateTime endDate)
        {
            // Внутренний шаг для поиска постов, параметр count
            var innerStep = (endDate - startDate).Days + 20;

            // Внешний шаг для пропуска ненужных постов, параметр offset
            var outerStep = (DateTime.Now - endDate).Days;

            double cycleCount = 1;
            if (innerStep > 100)
            {
                cycleCount = Math.Round((double) (innerStep/100), MidpointRounding.AwayFromZero);
            }
        }

        private IEnumerable<PostDataModel> GetWallPosts(int count, string groupId)
        {
            XDocument posts;
            try
            {
                // получить список всех постов на стене сообщества
                posts = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&count={0}&owner_id=-{1}", count, groupId));
            }
            catch (Exception exception)
            {
                _logger.Error("Ошибка получения постов со стены группы: {0}", exception.Message);
                _logger.Error("Ошибка получения постов со стены группы: {0}", exception.InnerException);

                throw new HttpException(500, "Во время скачивания постов произошла ошибка");
            }

            return posts.Descendants("post").Select(p => new PostDataModel()
            {
                Id = p.Element("id").Value,
                Date = UnixTimeStampToDateTime(Convert.ToDouble(p.Element("date").Value))
            }).ToList();
        }

        

        private IEnumerable<string> GetListOfLikedUsers(string groupId, string postId)
        {
            XDocument users;
            try
            {
                // получить список людей лайкнувших пост
                users = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=likes.getList.xml&owner_id=-{0}&item_id={1}&type=post", groupId, postId));
            }
            catch (Exception exception)
            {
                _logger.Error("Ошибка получения списков лайка поста {0}: {1}", postId, exception.Message);
                _logger.Error("Ошибка получения списков лайка поста {0}: {1}", postId, exception.InnerException);

                throw new HttpException(500, "Во время скачивания лайков произошла ошибка");
            }

            var result = users.Descendants("users").Elements("uid").Select(p => p.Value).ToList();

            return result;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }




    }
}