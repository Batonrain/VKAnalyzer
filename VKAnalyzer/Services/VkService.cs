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
        public string AccessToken { get; set; }

        public List<CohortAnalysisModel> GetPostsForAnalyze(string groupId, DateTime startDate, DateTime endDate)
        {
            // Создание модели с параметрами для запроса всех постов за определённую дату
            var parametersModel = PrepareGetParameters(groupId, startDate, endDate);

            //Получение всеъ постов, чтобы исключить ранее неактивных пользователей
            var excludeList = GetExcludeUserList(groupId, startDate);

            // Получение постов
            var postsXDoc = GetPosts(parametersModel);

            // Извлечение из постов ID и Date для последующей обработки
            var allRawPosts = GetRawPostsFromXml(postsXDoc);

            // Получение лайков по Id постов и по датам
            return GetLikesByIdAndDates(allRawPosts, startDate, endDate, groupId, excludeList);
        }

        private IEnumerable<string> GetExcludeUserList(string groupId, DateTime startDate)
        {
            var maxCount = GetPostsCount(groupId);
            var cc = (decimal)maxCount / 100;

            var allPostsParametersModel = new GetWallPostsParametersModel()
            {
                InnerStep = 100,
                OuterStep = 0,
                GroupId = groupId,
                CyclesCount = (int)Math.Ceiling((double)cc)
            };

            var allPostsXDoc = GetPosts(allPostsParametersModel); //все посты

            var allRawPosts = GetRawPostsFromXml(allPostsXDoc);

            var allActiveUsers = GetAlreadyActiveUsers(allRawPosts, startDate, groupId);

            return allActiveUsers.Distinct().ToList();
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
                var cc = (decimal)parametersModel.InnerStep / 100;
                parametersModel.CyclesCount = (int)Math.Ceiling((double)cc) * 3;
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
                    var currentIteration = XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset={0}&count={1}&owner_id=-{2}&access_token={3}", cycleNumber * count, count, model.GroupId, AccessToken));
                    posts.Add(currentIteration);
                }
                catch (Exception exception)
                {
                    _logger.Error("Error in GetPosts: {0}", exception.Message);
                    _logger.Error("Error in GetPosts: {0}", exception.InnerException);

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
                    _logger.Error("Error in GetRawPostsFromXml: {0}", exception.Message);
                    _logger.Error("Error in GetRawPostsFromXml", exception.InnerException);
                }

            }

            return allRawPosts;
        }

        private List<CohortAnalysisModel> GetLikesByIdAndDates(IEnumerable<PostDataModel> allRawPosts, DateTime dateOfTheBeginning, DateTime dateOfTheEnd, string groupId, IEnumerable<string> excludeUserList)
        {
            var analyzeModels = new List<CohortAnalysisModel>();

            foreach (var post in allRawPosts.Where(arp => arp.Date >= dateOfTheBeginning && arp.Date <= dateOfTheEnd))
            {
                analyzeModels.Add(new CohortAnalysisModel
                {
                    PostId = post.Id,
                    PostDate = post.Date,
                    LikedIds = GetListOfLikedUsers(groupId, post.Id).Except(excludeUserList).ToList()
                });
            }

            return analyzeModels;
        }

        private IEnumerable<string> GetListOfLikedUsers(string groupId, string postId)
        {
            var users = new XDocument();
            try
            {
                // получить список людей лайкнувших пост
                users = XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=likes.getList.xml&owner_id=-{0}&item_id={1}&type=post", groupId, postId));
            }
            catch (Exception exception)
            {
                _logger.Error("Error in GetListOfLikedUsers {0}: {1}", postId, exception.Message);
                _logger.Error("Error in GetListOfLikedUsers {0}: {1}", postId, exception.InnerException);

                //throw new HttpException(500, "Во время скачивания лайков произошла ошибка");
            }

            var result = users.Descendants("users").Elements("uid").Select(p => p.Value);

            return result;
        }

        private int GetPostsCount(string groupId)
        {
            var xml = new XDocument();

            // получить список всех постов на стене сообщества
            try
            {
                xml = XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset=0&count=1&owner_id=-{0}&access_token={1}", groupId, AccessToken));
                return Convert.ToInt32(xml.Document.Element("response").Element("count").Value);
            }
            catch (Exception exception)
            {
                _logger.Error("Error in GetPostsCount: {0}", exception.Message);
                _logger.Error("Error in GetPostsCount: {0}", exception.InnerException);
            }

            return 10000;
        }

        //Получение всех активных пользователей до даты сбора данных
        private IEnumerable<string> GetAlreadyActiveUsers(IEnumerable<PostDataModel> allRawPosts, DateTime dateOfTheBeginning, string groupId)
        {
            var result = new List<string>();

            foreach (var post in allRawPosts.Where(arp => arp.Date < dateOfTheBeginning))
            {
                result.AddRange(GetListOfLikedUsers(groupId, post.Id));
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
    }
}