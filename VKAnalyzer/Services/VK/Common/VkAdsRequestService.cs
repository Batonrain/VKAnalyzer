using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using VKAnalyzer.Models.VKModels.JsonModels;

namespace VKAnalyzer.Services.VK.Common
{
    public class VkAdsRequestService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string BaseUrl { get; set; }
        private const int SleepTime = 5000;
        private const int SleepTimeLong = 10000;
        private const int SleepTimeDay = 86400000;

        public VkAdsRequestService()
        {
            BaseUrl = string.Format("{0}&{1}", WebConfigurationManager.AppSettings["VkApiBaseUrl"],
                WebConfigurationManager.AppSettings["VkApiActualVersion"]);
        }

        public string RequestJs(string requestString, bool sleepLong = false)
        {
            var tryingCount = 10;
            while (true)
            {
                Thread.Sleep(sleepLong ? SleepTimeLong : SleepTime);

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

                        if (error.ErrorCode == 603)
                        {
                            return json;
                        }

                        if (error.ErrorCode == 9)
                        {
                            Logger.Error("Flood control. Request string is");
                            Logger.Error(requestString);
                            Logger.Error("Result is");
                            Logger.Error(result);
                            Thread.Sleep(SleepTimeLong);
                        }

                        tryingCount--;

                        if (tryingCount == 0)
                        {
                            Logger.Error("Flood control. TryingCount is 0");
                            Logger.Error(requestString);
                            Logger.Error("Result is");
                            Logger.Error(result);
                            return json;
                        }
                    }
                    
                }
            }
        }

        public string GetAccounts(string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("{0}&method=ads.getAccounts&access_token={1}",BaseUrl, accessToken);

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
                        "{0}&method=ads.getClients&access_token={1}&account_id={2}", BaseUrl, accessToken, accountId);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }

        public string DeleteCampaigns(string accountId, string campaignIds, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("{0}&method=ads.deleteCampaigns&access_token={1}&account_id={2}&ids={3}", BaseUrl,
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
                    "{0}&method=ads.deleteAds&access_token={0}&account_id={1}&ids={2}", BaseUrl, accessToken, accountId, adIds);

                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                return json;
            }
        }
    }
}