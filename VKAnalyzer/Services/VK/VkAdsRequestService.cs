using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace VKAnalyzer.Services.VK
{
    public class VkAdsRequestService
    {
        private const int SleepTime = 1000;

        public XDocument Request(string requestString)
        {
            var tryingCount = 10;
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
                    tryingCount--;
                    if (tryingCount == 0)
                    {
                        return result;
                    }
                }
            }
        }

        public string GetAccounts(string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(String.Format("https://api.vk.com/api.php?oauth=1&method=ads.getAccounts&access_token={0}", accessToken));
                return json;
            }
        }

        public string GetClients(string accountId, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(String.Format("https://api.vk.com/api.php?oauth=1&method=ads.getClients&access_token={0}&account_id={1}", accessToken, accountId));
                return json;
            }
        }

        public string GetTargetsGroups(string accountId, string clientId, string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var result = wc.DownloadData(String.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.getTargetGroups&access_token={0}&account_id={1}&client_id={2}", accessToken, accountId, clientId));
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