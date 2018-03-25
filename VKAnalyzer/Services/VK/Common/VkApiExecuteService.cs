using System.Collections.Generic;
using System.Web.Configuration;

namespace VKAnalyzer.Services.VK.Common
{
    public class VkApiExecuteService
    {
        private string BaseUrl { get; set; }

        public VkApiExecuteService()
        {
            BaseUrl = string.Format("{0}&{1}", WebConfigurationManager.AppSettings["VkApiBaseUrl"],
                WebConfigurationManager.AppSettings["VkApiActualVersion"]);
        }

        public string CreateRetargets(string accountId, string clientId, string accessToken, int count = 20)
        {
            var operations = new List<string>();

            for (var c = 0; c < count; c++)
            {
                var singleOperation =
                string.Format(
                    @"API.ads.createTargetGroup({{""account_id"":{0},""client_id"":{1}, ""name"":""EM_SA_EX_RG_{2}"" }})",
                    accountId, clientId, c);

                operations.Add(singleOperation);
            }
            var code = string.Format(@"return [[{0}]];", string.Join(",", operations));

            return BuildUrl(accessToken, code);
        }

        private string BuildUrl(string accessToken,string code)
        {
            return string.Format("{0}&method=execute&access_token={1}&code={2}",
                                 BaseUrl, accessToken, code); 
        }
    }
}