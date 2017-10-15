using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels.Memas;

namespace VKAnalyzer.Services.VK
{
    public class VkMemasService
    {
        private VkRequestService RequestService { get; set; }
        private BaseDb _dbContext;

        public VkMemasService()
        {
            RequestService = new VkRequestService();
            _dbContext = new BaseDb();
        }

        public MemasAnalyzeResultModel Analyze(string accessToken)
        {
            var result = new MemasAnalyzeResultModel();
            var groups = GetListOfGroups();
            var rawPosts = new List<XDocument>();

            var memasPosts = new List<MemasPost>();

            foreach (var group in groups)
            {
                var toAdd = GetGroupPosts(group, accessToken);
                rawPosts.Add(toAdd);
            }

            foreach (var rawPost in rawPosts)
            {
                var toAdd = CreateModelPosts(rawPost);
                memasPosts.AddRange(toAdd);
            }

            result.TopByLikes = memasPosts.OrderByDescending(p => p.Likes).Take(10).ToList();
            result.TopByComments = memasPosts.OrderByDescending(p => p.Comments).Take(10).ToList();
            result.TopByReposts = memasPosts.OrderByDescending(p => p.Reposts).Take(10).ToList();
            result.TopByViews = memasPosts.OrderByDescending(p => p.Views).Take(10).ToList();

            return result;
        }

        private IEnumerable<string> GetListOfGroups()
        {
            var result = new List<string>();

            result = _dbContext.Groups.Take(3).Select(g => g.Link.Replace("https://vk.com/", "")).ToList();

            return result;
        }

        private XDocument GetGroupPosts(string groupId, string accessToken)
        {
            var posts = new XDocument();

            posts = RequestService.GetWallPostsByDomain(1, 50, groupId, accessToken);
            Thread.Sleep(1000);
            return posts;
        }

        private IEnumerable<MemasPost> CreateModelPosts(XDocument rawPosts)
        {
            var posts = new List<MemasPost>();

            var rawRes = rawPosts.Descendants("post");

            foreach (var xElement in rawRes)
            {
                var memasPost = new MemasPost();
                var elementId = xElement.Element("id");
                if (elementId != null)
                {
                    memasPost.Id = elementId.Value.Replace(";","");
                }

                var elementOwnerId = xElement.Element("to_id");
                if (elementOwnerId != null)
                {
                    memasPost.OwnerId = elementOwnerId.Value;
                }

                var commentsXml = xElement.Descendants("comments").FirstOrDefault();
                if (commentsXml != null)
                {
                    memasPost.Comments = Convert.ToInt32(commentsXml.Element("count").Value);
                }

                var likesXml = xElement.Descendants("likes").FirstOrDefault();
                if (likesXml != null)
                {
                    memasPost.Likes = Convert.ToInt32(likesXml.Element("count").Value);
                }

                var repostsXml = xElement.Descendants("reposts").FirstOrDefault();
                if (repostsXml != null)
                {
                    memasPost.Reposts = Convert.ToInt32(repostsXml.Element("count").Value);
                }

                var viewsXml = xElement.Element("views");
                if (viewsXml != null)
                {
                    memasPost.Views = Convert.ToInt32(viewsXml.Element("count").Value);
                }

                var attachements = xElement.Element("attachments");
                if (attachements != null)
                {
                    var photos = attachements.Descendants("photo").FirstOrDefault();
                    if (photos != null)
                    {
                        memasPost.MainPicture = photos.Element("src_big").Value;
                    }
                }

                posts.Add(memasPost);
            }

            return posts;
        }

    }

    [Serializable]
    public class MemasPost
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public int Comments { get; set; }
        public int Likes { get; set; }
        public int Reposts { get; set; }
        public int Views { get; set; }

        public string MainPicture { get; set; }
    }
}