﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using VKAnalyzer.Models.VKModels.JsonModels;

namespace VKAnalyzer.Services.VK
{
    public class VkAdsRequestService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private string UserId { get; set; }
        private const int SleepTime = 5000;
        private const int SleepTimeLong = 10000;
        private const int SleepTimeDay = 86400000;

        public XDocument Request(string requestString)
        {
            var tryingCount = 10;
            var floodControlTimer = 0;
            while (true)
            {
                Thread.Sleep(SleepTime);
                var result = XDocument.Load(requestString);

                if (!result.Descendants("error_code").Any())
                {
                    return result;
                }
                else
                {
                    var errorCode = result.Descendants("error_code").FirstOrDefault();
                    if (errorCode != null && errorCode.Value == "9")
                    {
                        floodControlTimer++;
                        Thread.Sleep(SleepTimeLong);
                    }
                    tryingCount--;
                    if (floodControlTimer == 7)
                    {
                        _logger.Error("Flood control. Request string is");
                        _logger.Error(requestString);
                        _logger.Error("Result is");
                        _logger.Error(result);
                        Thread.Sleep(SleepTimeDay);
                        return result;
                    }
                    if (tryingCount == 0)
                    {
                        return result;
                    }
                }
            }
        }

        public string RequestJs(string requestString)
        {
            var tryingCount = 10;
            while (true)
            {
                Thread.Sleep(SleepTime);
                using (var wc = new WebClient())
                {
                    var result = wc.DownloadData(requestString);
                    var json = Encoding.UTF8.GetString(result);

                    if (!json.Contains("error_code"))
                    {
                        return json;
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<List<Error>>(JObject.Parse(json)["response"].ToString()).FirstOrDefault();

                        if (error.error_code == 603)
                        {
                            return json;
                        }

                        if (error.error_code == 9)
                        {
                            Thread.Sleep(SleepTimeLong);
                        }

                        tryingCount--;

                        if (tryingCount == 0)
                        {
                            return json;
                        }
                    }
                    
                }
            }
        }

        public string RequestJson(string requestString)
        {
            while (true)
            {
                Thread.Sleep(SleepTime);
                using (var wc = new WebClient())
                {
                    var result = wc.DownloadData(requestString);
                    var json = Encoding.UTF8.GetString(result);

                    return json;
                }
            }
        }

        public string GetAccounts(string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("https://api.vk.com/api.php?oauth=1&method=ads.getAccounts&access_token={0}", accessToken);

                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);

                return json;
            }
        }

        public string GetClients(string accountId, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl =
                    String.Format(
                        "https://api.vk.com/api.php?oauth=1&method=ads.getClients&access_token={0}&account_id={1}",
                        accessToken, accountId);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }

        public string GetTargetsGroups(string accountId, string clientId, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.getTargetGroups&access_token={0}&account_id={1}&client_id={2}",
                    accessToken, accountId, clientId);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }

        public string GetTargetsGroups(string accountId, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var result = wc.DownloadData(String.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.getTargetGroups&access_token={0}&account_id={1}", accessToken, accountId));
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }

        public string DeleteCampaigns(string accountId, string campaignIds, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.deleteCampaigns&access_token={0}&account_id={1}&ids={2}",
                    accessToken, accountId, campaignIds);

                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }

        public string DeleteAds(string accountId, string adIds, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.deleteAds&access_token={0}&account_id={1}&ids={2}",
                    accessToken, accountId, adIds);

                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }

        public string DeleteTargetGroup(string accountId, string clientId, string targetGroup, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.deleteTargetGroup&access_token={0}&account_id={1}&client_id={2}&target_group_id={3}",
                    accessToken, accountId, clientId, targetGroup);

                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }


    }
}