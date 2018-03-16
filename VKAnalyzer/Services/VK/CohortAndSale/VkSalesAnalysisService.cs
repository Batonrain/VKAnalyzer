﻿using System;
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

        public VkSalesAnalysisService()
        {
            VkAdsRequestService = new VkAdsRequestService();
            VkRequestService = new VkRequestService();
            VkUrlService = new VkUrlService();
        }

        public IEnumerable<VkAnalyseSalesResultModel> CreateRetargets(List<CohortAnalysisModel> posts, string accountId, string clientId, string excludeTargetGroupdId, string accessToken)
        {
            var results = new List<VkAnalyseSalesResultModel>();

            // Создание группы ретаргета
            var retargetJson = VkAdsRequestService.RequestJs(VkUrlService.CreateRetargetGroupUrl(accountId, clientId, accessToken));
            var retargetGroup = JsonConvert.DeserializeObject<VkTargetGroupSuccess>(JObject.Parse(retargetJson)["response"].ToString());

            //Создаём рекламную кампанию
            var campaignJson = VkAdsRequestService.RequestJs(VkUrlService.CreateCampaignUrl(accountId, clientId, string.Format("EM-{0}", accountId), accessToken));
            var campaign = JsonConvert.DeserializeObject<List<VkCampaignSuccess>>(JObject.Parse(campaignJson)["response"].ToString());

            for (var i = 0; i < posts.Count(); i = i + 30)
            {
                var items = posts.Skip(i).Take(30).ToList();
                var adIds = new List<string>();

                foreach (var item in items)
                {
                    try
                    {
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
                        var ad = JsonConvert.DeserializeObject<List<VkAdSuccess>>(JObject.Parse(adsJson)["response"].ToString());

                        var chk = ad.FirstOrDefault();
                        if (chk.Id == 0 || chk.ErrorCode != 0)
                        {
                            continue;
                        }

                        //Получаем охват текущей рекламной кампании
                        var adTargetInfoJson = VkAdsRequestService.RequestJs(VkUrlService.CreateGetAdsTargetingUrl(accountId, clientId, string.Format("[{0}]", chk.Id), accessToken));
                        var adTargeting = JsonConvert.DeserializeObject<List<VkAdTargeting>>(JObject.Parse(adTargetInfoJson)["response"].ToString());

                        var adChk = adTargeting.FirstOrDefault();
                        if (adChk.Id == 0 || adChk.ErrorCode != 0)
                        {
                            continue;
                        }
                        var fCount = adChk.Count;

                        //Добавляем в исключение выбранную пользователем группу ретаргета
                        var updatedInfoJson = VkAdsRequestService.RequestJs(VkUrlService.UpdateAd(accountId, chk.Id.ToString(), excludeTargetGroupdId, accessToken));
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
                            var uCount = updatedAdsInfo.FirstOrDefault().Count;
                            result.Result = (Math.Abs(Convert.ToInt32(fCount) - Convert.ToInt32(uCount))).ToString();
                        }

                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error during foreach in CreateRetargets:");
                        _logger.Error(ex);
                        _logger.Error("Inner Exception:");
                        _logger.Error(ex.InnerException);
                    }
                }

                //DeleteAds(accountId, adIds, accessToken);
                //DeleteCampaigns(accountId, campaignIds, accessToken);
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

        private void DeleteCampaigns(string accountId, IEnumerable<string> campaignIds, string accessToken)
        {
            VkAdsRequestService.DeleteCampaigns(accountId, GenerateJsonArray(campaignIds), accessToken);
        }

        private void DeleteAds(string accountId, IEnumerable<string> adsId, string accessToken)
        {
            VkAdsRequestService.DeleteAds(accountId, GenerateJsonArray(adsId), accessToken);
        }

        private string GenerateJsonArray(IEnumerable<string> values)
        {
            var array = new JArray();
            foreach (var value in values)
            {
                array.Add(new JValue(value));
            }
            return array.ToString();
        }

        private string GetJsonFromResponse(string json)
        {
            var parsed = JObject.Parse(json);
            return parsed["response"].ToString();
        }

        private IEnumerable<string> GetRandomUsers(int count, string accessToken)
        {
            var users = VkRequestService.GetRandomUsers("Колян", count, accessToken);
            return users.Descendants("id").Select(x => x.Value).ToList();
        }
    }
}