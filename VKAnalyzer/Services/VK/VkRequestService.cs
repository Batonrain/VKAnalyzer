using System;
using System.Xml.Linq;

namespace VKAnalyzer.Services.VK
{
    public class VkRequestService
    {
        public XDocument GetWallPosts(int offset, int count, string groupId, string accessToken)
        {
            return XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset={0}&count={1}&owner_id=-{2}&access_token={3}", offset, count, groupId, accessToken));
        }

        public XDocument GetWallPostsByDomain(int offset, int count, string domain, string accessToken)
        {
            return XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset={0}&count={1}&domain={2}&access_token={3}", offset, count, domain, accessToken));
        }

        public XDocument GetListOfLikedUsers(string groupId, string postId)
        {
            return XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=likes.getList.xml&owner_id=-{0}&item_id={1}&type=post", groupId, postId));
        }

        public XDocument GetPostsCount(string groupId, string accessToken)
        {
           return XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&offset=0&count=1&owner_id=-{0}&access_token={1}", groupId, accessToken));
        }

        public XDocument GetGroupUsersCount(string groupId, string accessToken)
        {
            return XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=groups.getMembers.xml&offset=0&count=1&group_id={0}&access_token={1}", groupId, accessToken));
        }

        public XDocument GetGroupUsers(int offset, int count, string groupId, string accessToken)
        {
            return XDocument.Load(String.Format("https://api.vk.com/api.php?oauth=1&method=groups.getMembers.xml&offset={0}&count={1}&group_id={2}&access_token={3}", offset, count, groupId, accessToken));
        }
        
    }
}