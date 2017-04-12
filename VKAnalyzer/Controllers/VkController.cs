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
                // Создание модели с параметрами для запроса всех постов за определённую дату
                var parametersModel = PrepareGetParameters(model.GroupId, AnalyseStep.Day, model.DateOfTheBeginning, model.DateOfTheEnd);

                // Получение постов
                var postsXDoc = GetPosts(parametersModel);

                // Извлечение из постов ID и Date для последующей обработки
                var allRawPosts = new List<PostDataModel>();
                foreach (var xDocument in postsXDoc)
                {
                    var innerPosts = xDocument.Descendants("post").Select(p => new PostDataModel()
                     {
                         Id = p.Element("id").Value,
                         Date = UnixTimeStampToDateTime(Convert.ToDouble(p.Element("date").Value))
                     }).ToList();

                    allRawPosts.AddRange(innerPosts);
                }

                // Получение лайков по Id постов и по датам
                var analyzeModels = new List<CohortAnalysisModel>();
                foreach (var post in allRawPosts.Where(arp => arp.Date >= model.DateOfTheBeginning && arp.Date <= model.DateOfTheEnd))
                {
                    analyzeModels.Add(new CohortAnalysisModel
                    {
                        PostId = post.Id,
                        PostDate = post.Date,
                        LikedIds = GetListOfLikedUsers(model.GroupId, post.Id)
                    });
                }

                // Сортировка данных для последующей обработки когортного анализатора
                var preparedData = PrepareDataForCohortAnalyse(analyzeModels, AnalyseStep.Day).OrderBy(d => d.PostDate).ToList();

                var analyser = new CohortAnalyser();
                var result = new CohortAnalysisResultModel
                {
                    ResultMatrix = analyser.BuildCohortAnalyseData(preparedData),
                    TableLength = preparedData.Count,
                    GroupId = model.GroupId
                };

                ViewBag.Message = "Cohort analysis";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        private List<CohortAnalysisModel> PrepareDataForCohortAnalyse(List<CohortAnalysisModel> posts, AnalyseStep step)
        {
            var result = new List<CohortAnalysisModel>();

            if (step == AnalyseStep.Day)
            {
                var getAllDays = posts.Select(p => p.PostDate.Date).Distinct();
                foreach (var day in getAllDays)
                {
                    var res = new CohortAnalysisModel()
                    {
                        PostDate = day,
                        LikedIds = posts.Where(p => p.PostDate.Date == day).SelectMany(s => s.LikedIds).Distinct().ToList()
                    };

                    result.Add(res);
                }
            }

            return result;
        }

        private GetWallPostsParametersModel PrepareGetParameters(string groupId, AnalyseStep step, DateTime startDate, DateTime endDate)
        {
            var parametersModel = new GetWallPostsParametersModel
            {
                // Внутренний шаг для поиска постов, параметр count
                InnerStep = (endDate - startDate).Days + 20,
                // Внешний шаг для пропуска ненужных постов, параметр offset
                OuterStep = (DateTime.Now - endDate).Days,
                GroupId = groupId,
                CyclesCount = 1
            };

            if (parametersModel.InnerStep > 100)
            {
                parametersModel.CyclesCount = (int)Math.Round((double)(parametersModel.InnerStep / 100), MidpointRounding.AwayFromZero);
            }

            return parametersModel;
        }

        private IEnumerable<XDocument> GetPosts(GetWallPostsParametersModel model)
        {
            var posts = new List<XDocument>();

            // получить список всех постов на стене сообщества
            for (var cycleNumber = 0; cycleNumber < model.CyclesCount; cycleNumber++)
            {
                try
                {
                    var currentIteration = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset={0}&count={1}&owner_id=-{2}", model.OuterStep, model.InnerStep, model.GroupId));
                    posts.Add(currentIteration);
                }
                catch (Exception exception)
                {
                    _logger.Error("Ошибка получения постов со стены группы: {0}", exception.Message);
                    _logger.Error("Ошибка получения постов со стены группы: {0}", exception.InnerException);

                    throw new HttpException(500, "Во время скачивания постов произошла ошибка");
                }
            }

            return posts;
        }


        private List<string> GetListOfLikedUsers(string groupId, string postId)
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
                        LikedIds = likesIds.Distinct().ToList()
                    });
                }
            }

            return result;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        //var posts = GetWallPosts(model.GroupId, Convert.ToInt32(30));

        //var rawData = PrepareDataForCohortAnalyse(posts, Convert.ToInt32(30), model.GroupId).OrderBy(d => d.PostDate).ToList();

        //var analyser = new CohortAnalyser();

        //var result = new CohortAnalysisResultModel
        //{
        //    ResultMatrix = analyser.BuildCohortAnalyseData(rawData),
        //    TableLength = rawData.Count,
        //    GroupId = model.GroupId
        //};

    }
}