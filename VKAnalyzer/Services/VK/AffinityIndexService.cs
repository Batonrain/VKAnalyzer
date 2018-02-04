using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using VKAnalyzer.Models.VKModels.AffinityIndex;
using VKAnalyzer.Models.VKModels.JsonModels;
using WebGrease.Css.Extensions;

namespace VKAnalyzer.Services.VK
{
    public class AffinityIndexService
    {
        private VkDatabaseService _vkDatabaseService;
        private VkBaseService _vkBaseService;
        private VkAdsRequestService _vkAdsRequestService;
        private VkUrlService _vkUrlService;
        private VkDbService _vkDbService;

        public AffinityIndexService(VkDatabaseService vkDatabaseService, VkBaseService vkBaseService, VkAdsRequestService vkAdsRequestService, VkUrlService vkUrlService, VkDbService vkDbService)
        {
            _vkDatabaseService = vkDatabaseService;
            _vkBaseService = vkBaseService;
            _vkAdsRequestService = vkAdsRequestService;
            _vkUrlService = vkUrlService;
            _vkDbService = vkDbService;
        }

        public void Start(IEnumerable<AffinityIndexOptionsAuditoryModel> audiencesUnderAnalysis, AffinityIndexOptionsAuditoryModel comparativeAudience, string accountId, string clientId, string accessToken, string userId, string name)
        {
            var result = new AffinityIndexResult
            {
                ComparativeAudience = comparativeAudience.Name
            };

            var categories = GetCategories(accessToken);

            categories.ForEach(cat => result.Results.Add(new AffinityIndexCounter
            {
                Category = cat.name,
                CategoryId = cat.id
            }));

            //Сравнительная аудитория
            //Общая кампания для запроса
            var affinityIndexCampaign = CreateCampaign(accountId, clientId, accessToken, "common");

            var allCategories = string.Join(",", categories.Select(c => c.id));

            var commonСomparativeAd = CreateAd(accountId, accessToken, affinityIndexCampaign.id,
                comparativeAudience.Gender, comparativeAudience.AgesFrom, comparativeAudience.AgesUpTo, comparativeAudience.InterestGroupIds,
                comparativeAudience.ExcludeInterestGroupIds, allCategories);

            var commonСomparativeTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", commonСomparativeAd.id), accessToken);
            if (!string.IsNullOrEmpty(commonСomparativeTargeting.error_desc))
            {
                result.ErrorMessage =
                    string.Format(
                        "Выбранные Вами параметры настроек для аудитории {0} содержат в себе слишком мало людей для анализа.", comparativeAudience.Name);
            }

            //Анализируемые аудтиории

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                foreach (var audience in audiencesUnderAnalysis)
                {
                    result.Audience = audience.Name;

                    var commonAd = CreateAd(accountId, accessToken, affinityIndexCampaign.id,
                        audience.Gender, audience.AgesFrom, audience.AgesUpTo, audience.InterestGroupIds,
                        audience.ExcludeInterestGroupIds, allCategories);

                    var commonTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", commonAd.id), accessToken);
                    if (!string.IsNullOrEmpty(commonTargeting.error_desc))
                    {
                        continue;
                    }

                    foreach (var category in categories)
                    {
                        var categoryAd = CreateAd(accountId, accessToken, affinityIndexCampaign.id,
                        audience.Gender, audience.AgesFrom, audience.AgesUpTo, audience.InterestGroupIds,
                        audience.ExcludeInterestGroupIds, category.id.ToString());

                        if (!string.IsNullOrEmpty(categoryAd.error_desc))
                        {
                            continue;
                        }

                        var categoryTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", categoryAd.id), accessToken);

                        result.Results.First(f => f.CategoryId == category.id).Audience1Result =
                            (decimal) categoryTargeting.count/(decimal) commonTargeting.count;
                    }
                }

                //Выгрузка категорий сравнительной аудитории
                foreach (var category in categories)
                {
                    if (result.Results.All(f => f.Audience1Result == 0))
                    {
                        continue;
                    }

                    var categoryAd = CreateAd(accountId, accessToken, affinityIndexCampaign.id,
                    comparativeAudience.Gender, comparativeAudience.AgesFrom, comparativeAudience.AgesUpTo, comparativeAudience.InterestGroupIds,
                    comparativeAudience.ExcludeInterestGroupIds, category.id.ToString());

                    if (!string.IsNullOrEmpty(categoryAd.error_desc))
                    {
                        continue;
                    }

                    var categoryTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", categoryAd.id), accessToken);

                    result.Results.First(f => f.CategoryId == category.id).Audience2Result = (decimal)categoryTargeting.count / (decimal)commonСomparativeTargeting.count;
                    
                }

                result.Results.Where(w => w.Audience1Result != 0 && w.Audience2Result != 0).ForEach(f => f.Index = f.Audience1Result / f.Audience2Result);
            }
            
            result.DateOfCollection = DateTime.Now;

            _vkDbService.SaveAddinityIndex(result, userId, name);
        }

        public List<AffinityIndexResultsViewModel> GetResults(string userId)
        {
            return _vkDbService.GetAffinityIndexResults(userId);
        }

        public AffinityIndexResult GetResult(int id)
        {
            var resultDb = _vkDbService.GetAffinityIndexResult(id);

            var result = new AffinityIndexResult();
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(resultDb.Result))
                {
                    result = (AffinityIndexResult)formatter.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private VkCampaignSuccess CreateCampaign(string accountId, string clientId, string accessToken, string name)
        {
            var reqString = _vkUrlService.CreateCampaignUrl(accountId, clientId,
                    string.Format("EvilMarketingAffinity_Campaign_{0}", name), accessToken);

            var campaign = _vkBaseService.GetJsonFromResponse(_vkAdsRequestService.RequestJs(reqString));
            var result = JsonConvert.DeserializeObject<List<VkCampaignSuccess>>(campaign).FirstOrDefault();

            return result;
        }

        private VkAdSuccess CreateAd(string accountId, string accessToken, int campaignId, string sex, int ageFrom, int ageUpTo, string groups, string excludedGroupds, string interestCategories = "")
        {
            var reqString = _vkUrlService.CreateAdUrl(accountId, campaignId, accessToken, "EvilMarketing_Affinity_Ad", sex, ageFrom, ageUpTo,
                groups, excludedGroupds, interestCategories: interestCategories);

            var ad = _vkBaseService.GetJsonFromResponse(_vkAdsRequestService.RequestJs(reqString));
            var result = JsonConvert.DeserializeObject<List<VkAdSuccess>>(ad).FirstOrDefault();

            return result;
        }

        private VkAdTargetInfo GetAdTarget(string accountId, string clientId, string adIds, string accessToken)
        {
            var reqString = _vkUrlService.CreateGetAdsTargetingUrl(accountId, clientId, adIds, accessToken);
            var ad = _vkBaseService.GetJsonFromResponse(_vkAdsRequestService.RequestJs(reqString));
            var result = JsonConvert.DeserializeObject<List<VkAdTargetInfo>>(ad).FirstOrDefault();

            return result;
        }

        private List<VkInterestCategory> GetCategories(string accessToken)
        {
            var categories = _vkDatabaseService.GetInterestsCategories(accessToken);

            var accsDeserialized = JsonConvert.DeserializeObject<List<VkInterestCategory>>(categories);

            return accsDeserialized;
        }
    }
}