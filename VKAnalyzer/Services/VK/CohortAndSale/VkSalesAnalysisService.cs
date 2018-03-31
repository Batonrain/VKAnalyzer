using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.JsonModels;
using VKAnalyzer.Services.VK.Common;

namespace VKAnalyzer.Services.VK.CohortAndSale
{
    public class VkSalesAnalysisService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private VkAdsRequestService VkAdsRequestService { get; set; }
        private VkRequestService VkRequestService { get; set; }
        private VkUrlService VkUrlService { get; set; }
        private VkApiExecuteService VkApiExecuteService { get; set; }
        private const int DefaultPostsCountValue = 10;

        public VkSalesAnalysisService()
        {
            VkAdsRequestService = new VkAdsRequestService();
            VkRequestService = new VkRequestService();
            VkUrlService = new VkUrlService();
            VkApiExecuteService = new VkApiExecuteService();
        }

        public IEnumerable<VkAnalyseSalesResultModel> CreateRetargets(List<CohortAnalysisModel> posts, string accountId, string clientId, string excludeTargetGroupdId, string accessToken)
        {
            var results = new List<VkAnalyseSalesResultModel>();

            //Создаём рекламную кампанию
            var campaignJson = VkAdsRequestService.RequestJs(VkUrlService.CreateCampaignUrl(accountId, clientId, string.Format("EM-{0}", accountId), accessToken));
            var campaign = JsonConvert.DeserializeObject<List<VkCampaignSuccess>>(JObject.Parse(campaignJson)["response"].ToString());

            for (var i = 0; i < posts.Count(); i = i + DefaultPostsCountValue)
            {
                var items = posts.Skip(i).Take(DefaultPostsCountValue).ToList();

                
                int iterationCount = 0;

                foreach (var item in items)
                {
                    try
                    {
                        int fCount = 0;
                        int uCount = 0;

                        // Создание группы ретаргета
                        var retargetGroupJson = VkAdsRequestService.RequestJs(VkUrlService.CreateRetargetGroupUrl(accountId, clientId, accessToken));

                        var retargetGroup = JsonConvert.DeserializeObject<VkTargetGroupSuccess>(JObject.Parse(retargetGroupJson)["response"].ToString());
                        if (retargetGroup.Id == 0 || retargetGroup.ErrorCode != 0)
                        {
                            continue;
                        }

                        //Если при создании группы ретаргетинга в ней меньше 100 человек, мы добавляем совершенно левых, чтобы удовлетворять требуемым условиям
                        if (item.LikedIds.Count() < 100)
                        {
                            var needToAdd = 150 - item.LikedIds.Count();
                            item.LikedIds.AddRange(GetRandomUsers(needToAdd, accessToken));
                        }


                        var contacts = item.LikedIds.Aggregate(string.Empty, (current, id) => current + string.Format("{0},", id));

                        //Добавляем пользователей в группу ретаргета
                        var updatedGroup = VkAdsRequestService.RequestJs(VkUrlService.CreateImportRetargetContactsUrl(accountId, clientId, retargetGroup.Id.ToString(), contacts, accessToken));

                        //Создаём рекламное объявление
                        var adsJson = VkAdsRequestService.RequestJs(VkUrlService.CreateAdUrl(accountId, campaign.FirstOrDefault().Id.ToString(), retargetGroup.Id.ToString(), string.Format("EM_Ad_{0}", item.PostId), accessToken));
                        var newAds = JsonConvert.DeserializeObject<List<VkAdSuccess>>(JObject.Parse(adsJson)["response"].ToString());

                        var newAd = newAds.FirstOrDefault();
                        if (newAd.Id == 0 || newAd.ErrorCode != 0)
                        {
                            continue;
                        }

                        //Получаем охват текущей рекламной кампании
                        var adTargetInfoJson = VkAdsRequestService.RequestJs(VkUrlService.CreateGetAdsTargetingUrl(accountId, clientId, string.Format("[{0}]", newAd.Id), accessToken));
                        var adTargeting = JsonConvert.DeserializeObject<List<VkAdTargeting>>(JObject.Parse(adTargetInfoJson)["response"].ToString());

                        var adChk = adTargeting.FirstOrDefault();
                        if (adChk.Id == 0 || adChk.ErrorCode != 0)
                        {
                            continue;
                        }
                        fCount = adChk.Count;

                        //Добавляем в исключение выбранную пользователем группу ретаргета
                        var updatedInfoJson = VkAdsRequestService.RequestJs(VkUrlService.UpdateAd(accountId, newAd.Id.ToString(), excludeTargetGroupdId, accessToken));
                        var updatedInfo = JsonConvert.DeserializeObject<List<VkAdSuccess>>(JObject.Parse(updatedInfoJson)["response"].ToString());

                        var result = new VkAnalyseSalesResultModel
                        {
                            PostId = item.PostId,
                            Date = item.PostDate
                        };

                        if (updatedInfo.Any(x => x.ErrorCode != 0 || !string.IsNullOrEmpty(x.ErrorDesc)))
                        {
                            //Плохая группа ретаргета и невозможно провести обновление
                            result.Result = "<100";
                        }
                        else
                        {
                            var updatedAdsInfoJson = VkAdsRequestService.RequestJs(VkUrlService.CreateGetAdsTargetingUrl(accountId, clientId, string.Format("[{0}]", updatedInfo.FirstOrDefault().Id), accessToken));
                            var updatedAdsInfo = JsonConvert.DeserializeObject<List<VkAdTargeting>>(JObject.Parse(updatedAdsInfoJson)["response"].ToString());

                            if (updatedAdsInfo.Any(x => x.ErrorCode != 0 || !string.IsNullOrEmpty(x.ErrorDesc)))
                            {
                                continue;
                            }
                            uCount = updatedAdsInfo.FirstOrDefault().Count;
                            result.Result = (Math.Abs(Convert.ToInt32(fCount) - Convert.ToInt32(uCount))).ToString();
                        }

                        //var deleteAdsResult = VkAdsRequestService.RequestJs(VkUrlService.CreateDeleteAdsUrl(accountId, new[] {chk.Id}, accessToken));
                        //var deleteResult = VkAdsRequestService.RequestJs(VkUrlService.CreateDeleteRetargetGroupUrl(accountId, clientId, retargetGroup.Id.ToString(), accessToken));

                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error during foreach in CreateRetargets:");
                        _logger.Error(ex);
                        _logger.Error("Inner Exception:");
                        _logger.Error(ex.InnerException);
                    }

                    ++iterationCount;
                }
            }

            return results;
        }

        public string GetAccounts(string accessToken)
        {
            return VkAdsRequestService.GetAccounts(accessToken);
        }

        public string GetClients(string accountId, string accessToken)
        {
            return VkAdsRequestService.GetClients(accountId, accessToken);
        }

        private IEnumerable<string> GetRandomUsers(int count, string accessToken)
        {
            var users = VkRequestService.GetRandomUsers("Колян", count, accessToken);
            return users.Descendants("id").Select(x => x.Value).ToList();
        }
    }
}