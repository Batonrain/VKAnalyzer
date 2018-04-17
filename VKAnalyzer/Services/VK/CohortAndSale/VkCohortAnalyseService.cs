using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.JsonModels;
using VKAnalyzer.Services.VK.Common;

namespace VKAnalyzer.Services.VK.CohortAndSale
{
    public class VkCohortAnalyseService
    {
        private string AccessToken { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private VkUrlService _vkUrlService;
        private VkWallRequestService _vkWallRequestService;
        private VkBaseService _vkBaseService;

        public VkCohortAnalyseService(VkUrlService vkUrlService, VkWallRequestService vkWallRequestService, VkBaseService vkBaseService)
        {
            _vkUrlService = vkUrlService;
            _vkWallRequestService = vkWallRequestService;
            _vkBaseService = vkBaseService;
        }

        public void GetPosts(CohortAnalysysInputModel model)
        {
            // Создание модели с параметрами для запроса всех постов за определённую дату
            var parametersModel = PrepareParameters(model.GroupId, model.StartDate, model.EndDate);
        }

        private List<Post> GetPosts(VkWallParametersModel model)
        {
            var posts = new List<Post>();
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
                    var requestString = _vkUrlService.GetWallPosts(cycleNumber*count, count, model.GroupId, AccessToken);
                    var postsJson = _vkBaseService.GetJsonFromResponse(_vkWallRequestService.Request(requestString));
                    var currentIteration = JsonConvert.DeserializeObject<List<Post>>(postsJson); 

                    posts.AddRange(currentIteration);
                }
                catch (Exception exception)
                {
                    Logger.Error("Error in GetPosts: {0}", exception.Message);
                }
            }

            return posts;
        }

        private VkWallParametersModel PrepareParameters(string groupId, DateTime startDate, DateTime endDate)
        {
            var needToParseCount = (endDate - startDate).Days*4;

            var parametersModel = new VkWallParametersModel
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
    }
}