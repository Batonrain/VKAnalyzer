using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VKAnalyzer.Models.VKModels.AffinityIndex;
using VKAnalyzer.Models.VKModels.JsonModels;

namespace VKAnalyzer.Services.VK
{
    public class AffinityIndexService
    {
        private VkDatabaseService _vkDatabaseService;
        private VkBaseService _vkBaseService;
        private VkAdsRequestService _vkAdsRequestService;
        private VkUrlService _vkUrlService;

        public AffinityIndexService(VkDatabaseService vkDatabaseService, VkBaseService vkBaseService, VkAdsRequestService vkAdsRequestService, VkUrlService vkUrlService)
        {
            _vkDatabaseService = vkDatabaseService;
            _vkBaseService = vkBaseService;
            _vkAdsRequestService = vkAdsRequestService;
            _vkUrlService = vkUrlService;
        }

        public void Start(IEnumerable<AffinityIndexOptionsAuditoryModel> audiencesUnderAnalysis, AffinityIndexOptionsAuditoryModel comparativeAudience, string accountId, string clientId, string accessToken, string userId)
        {
            var result = new AffinityIndexResult();

            var categories = GetCategories(accessToken);

            foreach (var audience in audiencesUnderAnalysis)
            {
                var commonCampaign = CreateCampaign(accountId, clientId, accessToken, "common");
                var commonAd = CreateAd(accountId, accessToken, commonCampaign.id,
                    audience.Gender, audience.AgesFrom, audience.AgesUpTo, audience.InterestGroupIds,
                    audience.ExcludeInterestGroupIds);

                var commonTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", commonAd.id), accessToken);
                if (commonTargeting.Error != null)
                {
                    result.ErrorMessage =
                        string.Format(
                            "Выбранные Вами параметры настроек для аудитории {0} содержат в себе слишком мало людей для анализа.", audience.Name);
                    continue;
                }
                
                foreach (var category in categories)
                {
                    var categoryCampaign = CreateCampaign(accountId, clientId, accessToken, string.Format("common_{0}", audience.Name));

                    var categoryAd = CreateAd(accountId, accessToken, categoryCampaign.id,
                    audience.Gender, audience.AgesFrom, audience.AgesUpTo, audience.InterestGroupIds,
                    audience.ExcludeInterestGroupIds, category.id.ToString());

                    if (categoryAd.Error != null)
                    {
                        result.ErrorMessage =
                            string.Format(
                                "Выбранные Вами параметры настроек для аудитории {0} содержат в себе слишком мало людей для анализа.", audience.Name);
                        continue;
                    }

                    var categoryTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", categoryAd.id), accessToken);
                    result.Results.Add(new AffinityIndexCounter
                    {
                        Category = category.name,
                        CategoryId = category.id,
                        Audience1Result = (decimal)categoryTargeting.count / (decimal)commonTargeting.count
                    });
                }
            }

            //Сравнительная аудитория

            var commonСomparativeCampaign = CreateCampaign(accountId, clientId, accessToken, "common");

            var commonСomparativeAd = CreateAd(accountId, accessToken, commonСomparativeCampaign.id,
                comparativeAudience.Gender, comparativeAudience.AgesFrom, comparativeAudience.AgesUpTo, comparativeAudience.InterestGroupIds,
                comparativeAudience.ExcludeInterestGroupIds);

            var commonСomparativeTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", commonСomparativeAd.id), accessToken);
            if (commonСomparativeTargeting.Error != null)
            {
                result.ErrorMessage =
                    string.Format(
                        "Выбранные Вами параметры настроек для аудитории {0} содержат в себе слишком мало людей для анализа.", comparativeAudience.Name);
            }

            foreach (var category in categories)
            {
                var categoryCampaign = CreateCampaign(accountId, clientId, accessToken, string.Format("common_{0}", comparativeAudience.Name));

                var categoryAd = CreateAd(accountId, accessToken, categoryCampaign.id,
                comparativeAudience.Gender, comparativeAudience.AgesFrom, comparativeAudience.AgesUpTo, comparativeAudience.InterestGroupIds,
                comparativeAudience.ExcludeInterestGroupIds, category.id.ToString());

                var categoryTargeting = GetAdTarget(accountId, clientId, string.Format("[{0}]", categoryAd.id), accessToken);

                if (result.Results.Any(f => f.CategoryId == category.id))
                {
                    result.Results.First(f => f.CategoryId == category.id).Audience2Result = (decimal)categoryTargeting.count / (decimal)commonСomparativeTargeting.count;
                }
            }


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
            var categories = _vkBaseService.GetJsonFromResponse(_vkDatabaseService.GetInterestsCategories(accessToken));
            var accsDeserialized = JsonConvert.DeserializeObject<List<VkInterestCategory>>(categories);
            return accsDeserialized;
        }
    }

    public class AffinityIndexCounter
    {
        public int CategoryId { get; set; }

        public string Category { get; set; }

        public decimal Audience1Result { get; set; }

        public decimal Audience2Result { get; set; }
    }

    public class AffinityIndexResult
    {
        public string ErrorMessage { get; set; }

        public List<AffinityIndexCounter> Results { get; set; }

        public AffinityIndexResult()
        {
            Results = new List<AffinityIndexCounter>();
        }
    }
}