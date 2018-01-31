using System;
using System.Collections.Generic;
using Hangfire;
using Newtonsoft.Json;
using VKAnalyzer.Models.VKModels.AffinityIndex;
using VKAnalyzer.Models.VKModels.JsonModels;
using VKAnalyzer.Services.Interfaces;

namespace VKAnalyzer.Services.VK
{
    public class AffinityIndexService : IAffinityIndexService
    {
        private IVkDatabaseService _vkDatabaseService;
        private IVkBaseService _vkBaseService;

        public AffinityIndexService(IVkDatabaseService vkDatabaseService, IVkBaseService vkBaseService)
        {
            _vkDatabaseService = vkDatabaseService;
            _vkBaseService = vkBaseService;
        }

        public void Start(IEnumerable<AffinityIndexOptionsAuditoryModel> audiencesUnderAnalysis, IEnumerable<AffinityIndexOptionsAuditoryModel> comparativeAudience, string accessToken, string userId)
        {
            var categories = GetCategories(accessToken);
        }

        private List<VkInterestCategory> GetCategories(string accessToken)
        {
            var categories = _vkBaseService.GetJsonFromResponse(_vkDatabaseService.GetInterestsCategories(accessToken));
            var accsDeserialized = JsonConvert.DeserializeObject<List<VkInterestCategory>>(categories);

            return accsDeserialized;
        }
    }
}