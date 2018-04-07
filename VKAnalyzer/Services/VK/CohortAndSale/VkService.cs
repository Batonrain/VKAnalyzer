using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.BusinessLogic.VK.Models;
using VKAnalyzer.Models.VKModels;

namespace VKAnalyzer.Services.VK.CohortAndSale
{
    public class VkService : VkBaseService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string AccessToken { get; set; }

        private VkRequestService RequestService { get; set; }
        private readonly CohortAnalyser _cohortAnalyser;
        private readonly VkMemasService _vkMemasService;
        private readonly VkSalesAnalysisService _vkSalesAnalysisService;

        private readonly VkDbService _vkDbService;

        public VkService()
        {
            _vkDbService = new VkDbService();
            _vkMemasService = new VkMemasService();
            _vkSalesAnalysisService = new VkSalesAnalysisService();
            RequestService = new VkRequestService();
            _cohortAnalyser = new CohortAnalyser();
        }

        //public VkService(VkDbService vkDbService, VkMemasService vkMemasService, VkSalesAnalysisService vkSalesAnalysisService, VkRequestService vkRequestService, CohortAnalyser cohortAnalyser)
        //{
        //    _vkDbService = vkDbService;
        //    _vkMemasService = vkMemasService;
        //    _vkSalesAnalysisService = vkSalesAnalysisService;
        //    RequestService = vkRequestService;
        //    _cohortAnalyser = cohortAnalyser;
        //}

        public void AnalyzeActivities(CohortAnalysysInputModel model, string accessToken, string userId)
        {
            AccessToken = accessToken;
            var analyzeModels = GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate, null, false);

            var result = _cohortAnalyser.Analyze(analyzeModels, model.Step, model.StartDate,
                model.EndDate, model.GroupId);

            _vkDbService.SaveCohortAnalyze(result, userId, model.Name, model.GroupId);
        }

        public void AnalyzeSales(VkAnalyseSalesModel model, string accessToken, string userId)
        {
            AccessToken = accessToken;

            var listOfBuyers = model.ListOfBuyers != null ? ConvertstringToList(model.ListOfBuyers).ToList() : null;

            //Список постов для анализа
            var analyzeModels = GetPostsForAnalyze(model.GroupId, model.StartDate, model.EndDate, listOfBuyers, false);

            if (listOfBuyers != null)
            {
                var result = _cohortAnalyser.Analyze(analyzeModels, model.Step, model.StartDate,
                model.EndDate, model.GroupId);

                _vkDbService.SaveAnalyzeOfSalesWithList(result, userId, model.Name, model.GroupId);
            }
            else
            {
                //Создание групп ретаргета для каждого поста
                if (analyzeModels.Any())
                {
                    var retargetsInfo = _vkSalesAnalysisService.CreateRetargets(analyzeModels, model.AccountId, model.ClientId, model.ExcludeTargetGroup, AccessToken).ToList();
                    var result = _cohortAnalyser.AnalyzeAcitivitySalesWithRetargetsInfo(retargetsInfo, model.Step, model.StartDate, model.EndDate, model.GroupId);

                    _vkDbService.SaveAnalyzeOfSalesWithRetarget(result, userId, model.Name, model.GroupId);
                }
                }
        }

        public void AnalyzeMemas(string accessToken, string userId)
        {
            AccessToken = accessToken;
            var result = _vkMemasService.Analyze(accessToken);

            _vkDbService.SaveMemas(result, userId);
        }

        

        private List<CohortAnalysisModel> GetPostsForAnalyze(string groupId, DateTime startDate, DateTime endDate,IEnumerable<string> buyers = null, bool excludeUsers = false)
        {
            // Создание модели с параметрами для запроса всех постов за определённую дату
            var parametersModel = PrepareGetParameters(groupId, startDate, endDate);

            //Получение всеъ постов, чтобы исключить ранее неактивных пользователей
            List<string> excludeList;

            if (excludeUsers)
            {
                excludeList = GetExcludeUserList(groupId, startDate).ToList();

                var allUsers = GetAllGroupUsers(parametersModel).ToList();

                var notFromGroup = excludeList.Where(i => !allUsers.Any(i.Contains)).ToList();
                
                if (notFromGroup.Any())
                {
                    excludeList = excludeList.Where(i => !notFromGroup.Any(i.Contains)).ToList();
                }
            }
            else
            {
                excludeList = GetExcludeUserList(groupId, startDate).ToList();
            }

            // Получение постов
            var postsXDoc = GetPosts(parametersModel);

            // Извлечение из постов ID и Date для последующей обработки
            var allRawPosts = GetRawPostsFromXml(postsXDoc);

            // Получение лайков по Id постов и по датам
            if (buyers != null)
            {
                return GetLikesByIdAndDatesForSales(allRawPosts, startDate, endDate, groupId, buyers, excludeList);
            }

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

            var allActiveUsers = GetAlreadyActiveUsers(allRawPosts, startDate, groupId).Distinct().ToList();

            return allActiveUsers;
        }

        private GetWallPostsParametersModel PrepareGetParameters(string groupId, DateTime startDate, DateTime endDate)
        {
            var needToParseCount = (endDate - startDate).Days*4;

            var parametersModel = new GetWallPostsParametersModel
            {
                // Внутренний шаг для поиска постов, параметр count
                InnerStep = 100,
                // Внешний шаг для пропуска ненужных постов, параметр offset
                OuterStep = (DateTime.Now - endDate).Days,
                GroupId = groupId,
                CyclesCount = 1
            };

            if (needToParseCount > 100)
            {
                var cc = (decimal)needToParseCount / 100;
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
                    var currentIteration = RequestService.GetWallPosts(cycleNumber * count, count, model.GroupId, AccessToken);
                    posts.Add(currentIteration);
                }
                catch (Exception exception)
                {
                    Logger.Error("Error in GetPosts: {0}", exception.Message);
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
                    Logger.Error("Error in GetRawPostsFromXml: {0}", exception.Message);
                    Logger.Error("Error in GetRawPostsFromXml", exception.InnerException);
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

        private List<CohortAnalysisModel> GetLikesByIdAndDatesForSales(IEnumerable<PostDataModel> allRawPosts, DateTime dateOfTheBeginning, DateTime dateOfTheEnd, string groupId, IEnumerable<string> buyers, IEnumerable<string> excludeUserList)
        {
            var analyzeModels = new List<CohortAnalysisModel>();

            foreach (var post in allRawPosts.Where(arp => arp.Date >= dateOfTheBeginning && arp.Date <= dateOfTheEnd))
            {
                var listOfLiked = GetListOfLikedUsers(groupId, post.Id).Except(excludeUserList).ToList();
                var result = listOfLiked.Where(buyers.Contains).ToList();
                analyzeModels.Add(new CohortAnalysisModel
                {
                    PostId = post.Id,
                    PostDate = post.Date,
                    LikedIds = result
                });
            }

            return analyzeModels;
        }

        private IEnumerable<string> GetListOfLikedUsers(string groupId, string postId)
        {
            var users = new XDocument();
            var result = new List<string>();
            try
            {
                // получить список людей лайкнувших пост
                users = RequestService.GetListOfLikedUsers(groupId, postId);
                result = users.Descendants("items").Elements("item").Select(p => p.Value).ToList();
            }
            catch (Exception exception)
            {
                Logger.Error("Error in GetListOfLikedUsers {0}: {1}", postId, exception.Message);
                Logger.Error("Error in GetListOfLikedUsers {0}: {1}", postId, exception.InnerException);

                throw new HttpException(500, "Во время скачивания лайков произошла ошибка");
            }

            return result;
        }

        private int GetPostsCount(string groupId)
        {
            var xml = new XDocument();

            // получить список всех постов на стене сообщества
            try
            {
                xml = RequestService.GetPostsCount(groupId, AccessToken);
                return Convert.ToInt32(xml.Document.Element("response").Element("count").Value);
            }
            catch (Exception exception)
            {
                Logger.Error("Error in GetPostsCount: {0}", exception.Message);
                Logger.Error(string.Format("Error in GetPostsCount: {0}", exception.InnerException));
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

        private int GetAllGroupUsersCount(string groupId)
        {
            var xml = new XDocument();

            // получить список всех постов на стене сообщества
            try
            {
                xml = RequestService.GetGroupUsersCount(groupId, AccessToken);
                return Convert.ToInt32(xml.Document.Element("response").Element("count").Value);
            }
            catch (Exception exception)
            {
                Logger.Error("Error in GetAllGroupUsersCount: {0}", exception.Message);
                Logger.Error(string.Format("Error in GetAllGroupUsersCount: {0}", exception.InnerException));
            }

            return 500000;
        }

        private IEnumerable<string> GetAllGroupUsers(GetWallPostsParametersModel model)
        {
            var users = new List<string>();
            var userCounts = GetAllGroupUsersCount(model.GroupId);

            var cyclesCount = userCounts / 1000 + 1;

            // получить список всех постов на стене сообщества
            for (var cycleNumber = 0; cycleNumber < cyclesCount; cycleNumber++)
            {
                var tryAgain = true;
                while (tryAgain)
                {
                    try
                    {
                        var currentIteration = RequestService.GetGroupUsers(cycleNumber * 1000, 1000, model.GroupId, AccessToken);
                        var usersList =
                            currentIteration.Document.Element("response")
                                .Element("users")
                                .Elements("uid")
                                .Select(element => element.Value);
                        users.AddRange(usersList);
                        tryAgain = false;
                        Thread.Sleep(1000);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error("Error in GetAllGroupUsers: {0}", exception.Message);

                    }
                }
            }

            return users;
        }

        public string GetAccounts(string accessToken)
        {
            return _vkSalesAnalysisService.GetAccounts(accessToken);
        }

        public string GetClients(string accountId, string accessToken)
        {
            return _vkSalesAnalysisService.GetClients(accountId, accessToken);
        }
    }
}