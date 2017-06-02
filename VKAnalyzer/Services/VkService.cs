using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.BusinessLogic.VK.Models;

namespace VKAnalyzer.Services
{
    public class VkService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public List<CohortAnalysisModel> GetPostsForAnalyze(string groupId, DateTime startDate, DateTime endDate)
        {
            // Создание модели с параметрами для запроса всех постов за определённую дату
            var parametersModel = PrepareGetParameters(groupId, startDate, endDate);

            // Получение постов
            var postsXDoc = GetPosts(parametersModel);

            // Извлечение из постов ID и Date для последующей обработки
            var allRawPosts = GetRawPostsFromXml(postsXDoc);

            // Получение лайков по Id постов и по датам
            return GetLikesByIdAndDates(allRawPosts, startDate, endDate, groupId);
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
                var qq = (decimal)parametersModel.InnerStep / 100;
                parametersModel.CyclesCount = (int)Math.Ceiling((double)qq) * 3;
            }

            return parametersModel;
        }

        private IEnumerable<XDocument> GetPosts(GetWallPostsParametersModel model)
        {
            var posts = new List<XDocument>();
            var count = model.InnerStep;

            if (model.InnerStep > 100)
            {
                count = 100;
            }

            // получить список всех постов на стене сообщества
            for (var cycleNumber = 0; cycleNumber < model.CyclesCount; cycleNumber++)
            {
                try
                {
                    var currentIteration = XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset={0}&count={1}&owner_id=-{2}", cycleNumber * count, count, model.GroupId));
                    posts.Add(currentIteration);
                }
                catch (Exception exception)
                {
                    _logger.Error("Ошибка получения постов со стены группы: {0}", exception.Message);
                    _logger.Error("Ошибка получения постов со стены группы: {0}", exception.InnerException);

                    //throw new HttpException(500, "Во время скачивания постов произошла ошибка");
                }
            }

            return posts;
        }

        private IEnumerable<PostDataModel> GetRawPostsFromXml(IEnumerable<XDocument> postsXDoc)
        {
            var allRawPosts = new List<PostDataModel>();

            foreach (var xDocument in postsXDoc)
            {
                try
                {
                    var innerPosts = xDocument.Descendants("post").Select(p => new PostDataModel()
                    {
                        Id = p.Element("id").Value,
                        Date = UnixTimeStampToDateTime(Convert.ToDouble(p.Element("date").Value))
                    }).ToList();

                    allRawPosts.AddRange(innerPosts);
                }
                catch (Exception exception)
                {
                    _logger.Error("Ошибка обработки постов в GetRawPostsFromXml: {0}", exception.Message);
                    _logger.Error("Ошибка получения постов в GetRawPostsFromXml: {0}", exception.InnerException);
                }

            }

            return allRawPosts;
        }

        public List<CohortAnalysisModel> GetLikesByIdAndDates(IEnumerable<PostDataModel> allRawPosts, DateTime dateOfTheBeginning, DateTime dateOfTheEnd, string groupId)
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

        private List<string> GetListOfLikedUsers(string groupId, string postId)
        {
            var users = new XDocument();
            try
            {
                // получить список людей лайкнувших пост
                users = XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=likes.getList.xml&owner_id=-{0}&item_id={1}&type=post", groupId, postId));
            }
            catch (Exception exception)
            {
                _logger.Error("Ошибка получения списков лайка поста {0}: {1}", postId, exception.Message);
                _logger.Error("Ошибка получения списков лайка поста {0}: {1}", postId, exception.InnerException);

                //throw new HttpException(500, "Во время скачивания лайков произошла ошибка");
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