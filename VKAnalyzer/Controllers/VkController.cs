using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using NLog;
using NLog.Fluent;
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
                var parametersModel = PrepareGetParameters(model.GroupId, model.DateOfTheBeginning, model.DateOfTheEnd);

                // Получение постов
                var postsXDoc = GetPosts(parametersModel);

                // Извлечение из постов ID и Date для последующей обработки
                var allRawPosts = GetRawPostsFromXml(postsXDoc);

                // Получение лайков по Id постов и по датам
                var analyzeModels = GetLikesByIdAndDates(allRawPosts, model.DateOfTheBeginning, model.DateOfTheEnd, model.GroupId);

                // Сортировка данных для последующей обработки когортного анализатора
                var preparedData = PrepareDataForCohortAnalyse(analyzeModels, model.Step, model.DateOfTheBeginning, model.DateOfTheEnd).OrderBy(d => d.PostDate).ToList();

                var result = Analyse(preparedData, model.GroupId);

                result.Dates = new List<string>();
                if (model.Step == 1)
                {
                    for (var dt = model.DateOfTheBeginning; dt < model.DateOfTheEnd; dt = dt.AddDays(1))
                    {
                        result.Dates.Add(dt.ToShortDateString());
                    }
                }
                if (model.Step == 2)
                {
                    var allDays = (model.DateOfTheEnd - model.DateOfTheBeginning).TotalDays;
                    var st = 7;
                    var countOfSteps = Math.Ceiling(allDays / st);
                    for (var i = 0; i < countOfSteps; i++)
                    {
                        var startDate = model.DateOfTheBeginning.AddDays(i * st);
                        var endDate = model.DateOfTheBeginning.AddDays((i + 1) * st);
                        if (endDate > model.DateOfTheEnd)
                        {
                            endDate = model.DateOfTheEnd;
                        }

                        result.Dates.Add(string.Format("{0} - {1}", startDate.ToShortDateString(), endDate.ToShortDateString()));
                    }
                }
                if (model.Step == 3)
                {
                    var allDays = (model.DateOfTheEnd - model.DateOfTheBeginning).TotalDays;
                    var st = 30;
                    var countOfSteps = Math.Ceiling(allDays / st);
                    for (var i = 0; i < countOfSteps; i++)
                    {
                        var startDate = model.DateOfTheBeginning.AddDays(i * st);
                        var endDate = model.DateOfTheBeginning.AddDays((i + 1) * st) ;
                        if (endDate > model.DateOfTheEnd)
                        {
                            endDate = model.DateOfTheEnd;
                        }

                        result.Dates.Add(string.Format("{0} - {1}", startDate.ToShortDateString(), endDate.ToShortDateString()));
                    }
                }


                ViewBag.Message = "Cohort analysis";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        private IEnumerable<PostDataModel> GetRawPostsFromXml(IEnumerable<XDocument> postsXDoc)
        {
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

            return allRawPosts;
        }

        private List<CohortAnalysisModel> GetLikesByIdAndDates(IEnumerable<PostDataModel> allRawPosts, DateTime dateOfTheBeginning, DateTime dateOfTheEnd, string groupId)
        {

            var analyzeModels = new List<CohortAnalysisModel>();

            foreach (var post in allRawPosts.Where(arp => arp.Date >= dateOfTheBeginning && arp.Date <= dateOfTheEnd))
            {
                analyzeModels.Add(new CohortAnalysisModel
                {
                    PostId = post.Id,
                    PostDate = post.Date,
                    LikedIds = GetListOfLikedUsers(groupId, post.Id)
                });
            }

            return analyzeModels;
        }

        private CohortAnalysisResultModel Analyse(List<CohortAnalysisModel> preparedData, string groupId, int step = 1)
        {
            var analyser = new CohortAnalyser();

            var result = new CohortAnalysisResultModel
            {
                ResultMatrix = analyser.BuildCohortAnalyseData(preparedData),
                TableLength = preparedData.Count,
                GroupId = groupId
            };

            result.TotalHorizontal = CountTotalHorizontal(result.ResultMatrix);
            result.TotalVertical = CountTotalVertical(result.ResultMatrix);

            return result;
        }

        private List<int> CountTotalHorizontal(List<string>[,] resultMatrix)
        {
            var result = new List<int>();

            for (var i = 0; i < resultMatrix.GetLength(0); i++) // for 1
            {
                var summ = 0;

                for (var j = 0; j < resultMatrix.GetLength(0); j++) // for 2
                {
                    if (resultMatrix[i, j] != null && resultMatrix[i, j].Count > 0)
                        summ += resultMatrix[i, j].Count;
                } // for 2

                result.Add(summ);

            } // for 1

            return result;
        }

        private List<int> CountTotalVertical(List<string>[,] resultMatrix)
        {
            var result = new List<int>();

            // Обсчитываем массив
            for (var i = 0; i < resultMatrix.GetLength(0); i++) // for 1
            {
                var summ = 0;

                for (var j = 0; j < resultMatrix.GetLength(0); j++) // for 2
                {
                    if (resultMatrix[j, i] != null && resultMatrix[j, i].Count > 0)
                        summ += resultMatrix[j, i].Count;
                } // for 2

                result.Add(summ);

            } // for 1

            return result;
        }

        private IEnumerable<CohortAnalysisModel> PrepareDataForCohortAnalyse(List<CohortAnalysisModel> posts, int step, DateTime dateOfTheBeginning, DateTime dateOfTheEnd)
        {
            var result = new List<CohortAnalysisModel>();

            if (step == 1)
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
            if (step == 2)
            {
                var allDays = (dateOfTheEnd - dateOfTheBeginning).TotalDays;
                var st = 7;
                var countOfSteps = Math.Ceiling(allDays / st);
                for (var i = 0; i < countOfSteps; i++)
                {
                    var startDate = dateOfTheBeginning.AddDays(i * st);
                    var endDate = dateOfTheBeginning.AddDays((i + 1) * st);

                    var res = new CohortAnalysisModel
                    {
                        PostDate = dateOfTheBeginning.AddDays(i),
                        LikedIds = posts.Where(p => startDate <= p.PostDate.Date && p.PostDate.Date < endDate).SelectMany(s => s.LikedIds).Distinct().ToList()
                    };

                    result.Add(res);
                }
            }

            if (step == 3)
            {
                var allDays = (dateOfTheEnd - dateOfTheBeginning).TotalDays;
                var st = 30;
                var countOfSteps = Math.Ceiling(allDays / st);
                for (var i = 0; i < countOfSteps; i++)
                {
                    var startDate = dateOfTheBeginning.AddDays(i * st);
                    var endDate = dateOfTheBeginning.AddDays((i + 1) * st);

                    var res = new CohortAnalysisModel
                    {
                        PostDate = dateOfTheBeginning.AddDays(i),
                        LikedIds = posts.Where(p => startDate <= p.PostDate.Date && p.PostDate.Date < endDate).SelectMany(s => s.LikedIds).Distinct().ToList()
                    };

                    result.Add(res);
                }
            }

            return result;
        }

        private GetWallPostsParametersModel PrepareGetParameters(string groupId, DateTime startDate, DateTime endDate)
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

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}