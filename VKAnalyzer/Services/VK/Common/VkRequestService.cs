using System;
using System.Threading;
using System.Web.Configuration;
using System.Xml.Linq;

namespace VKAnalyzer.Services.VK
{
    public class VkRequestService
    {
        private string BaseUrl { get; set; }

        public VkRequestService()
        {
            BaseUrl = string.Format("{0}&{1}", WebConfigurationManager.AppSettings["VkApiBaseUrl"],
                WebConfigurationManager.AppSettings["VkApiActualVersion"]);
        }

        public XDocument GetWallPosts(int offset, int count, string groupId, string accessToken)
        {
            return XDocument.Load(String.Format("{0}&method=wall.get.xml&offset={1}&count={2}&owner_id=-{3}&access_token={4}", BaseUrl, offset, count, groupId, accessToken));
        }

        public XDocument GetWallPostsByDomain(int offset, int count, string domain, string accessToken)
        {
            return XDocument.Load(String.Format("{0}&method=wall.get.xml&offset={1}&count={2}&domain={3}&access_token={4}", BaseUrl, offset, count, domain, accessToken));
        }

        public XDocument GetListOfLikedUsers(string groupId, string postId)
        {
            return XDocument.Load(String.Format("{0}&method=likes.getList.xml&owner_id=-{1}&item_id={2}&type=post", BaseUrl, groupId, postId));
        }

        public XDocument GetListOfLikedUsers(string groupId, string postId, int offset, int count)
        {
            return XDocument.Load(string.Format("{0}&method=likes.getList.xml&owner_id={1}&item_id={2}&type=post&offset={3}&count={4}", BaseUrl, groupId, postId, offset, count));
        }

        public XDocument GetPostsCount(string groupId, string accessToken)
        {
            Thread.Sleep(3000);
            return XDocument.Load(String.Format("{0}&method=wall.get.xml&offset=0&count=1&owner_id=-{1}&access_token={2}", BaseUrl, groupId, accessToken));
        }

        public XDocument GetGroupUsersCount(string groupId, string accessToken)
        {
            return XDocument.Load(String.Format("{0}&method=groups.getMembers.xml&offset=0&count=1&group_id={1}&access_token={2}", BaseUrl, groupId, accessToken));
        }

        public XDocument GetGroupUsers(int offset, int count, string groupId, string accessToken)
        {
            return XDocument.Load(String.Format("{0}&method=groups.getMembers.xml&offset={1}&count={2}&group_id={3}&access_token={4}", BaseUrl, offset, count, groupId, accessToken));
        }

        public XDocument GetRandomUsers(string q, int count, string accessToken)
        {
            var result = XDocument.Load(String.Format("{0}&method=users.search.xml&q={1}&count={2}&access_token={3}", BaseUrl, q, count, accessToken));
            return result;
        }
    }
}