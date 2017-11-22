using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using NLog;
using VKAnalyzer.Models.VKModels.Memas;

namespace VKAnalyzer.Services.VK
{
    public class VkMemasService
    {
        private VkRequestService RequestService { get; set; }
        private VkDatabaseService DatabaseService { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public VkMemasService()
        {
            RequestService = new VkRequestService();
            DatabaseService = new VkDatabaseService();
        }

        public MemasAnalyzeResultModel Analyze(string accessToken)
        {
            var result = new MemasAnalyzeResultModel();
            var groups = DatabaseService.GetListOfGroups();
            var rawPosts = new List<XDocument>();

            var memasPosts = new List<MemasPost>();

            try
            {
                foreach (var group in groups)
                {
                    var toAdd = GetGroupPosts(group, accessToken);
                    rawPosts.Add(toAdd);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("Ошибка во время скачивания постов из групп: {0}", exception.InnerException));
            }

            try
            {
                foreach (var rawPost in rawPosts)
                {
                    var toAdd = CreateModelPosts(rawPost);
                    memasPosts.AddRange(toAdd);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("Ошибка во время создания моделей для групп: {0}", exception.InnerException));
            }

            result.TopByLikes = memasPosts.OrderByDescending(p => p.Likes).Take(10).ToList();

            try
            {
                foreach (var likes in result.TopByLikes)
                {
                    Thread.Sleep(1000);
                    var likeIds = GetUsersIds(likes.OwnerId, likes.Id, likes.Likes).ToList();
                    var path = string.Format(@"{0}\Results\Memas\{1}_{2}_{3}_{4}.txt", AppDomain.CurrentDomain.BaseDirectory, accessToken, DateTime.Now.ToString("yyyy.MM.dd.mm.ss"), likes.OwnerId, likes.Id);
                    using (var sw = File.AppendText(path))
                    {
                        foreach (var like in likeIds)
                        {
                            sw.WriteLine("{0};", like);
                        }
                    }

                    likes.ListOfLikeIds = path;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("Ошибка во время анализа поство и получения лайкнувших пользователей: {0}", exception.InnerException));
            }

            result.TopByComments = memasPosts.OrderByDescending(p => p.Comments).Take(10).ToList();

            result.TopByReposts = memasPosts.OrderByDescending(p => p.Reposts).Take(10).ToList();

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
                    memasPost.Id = elementId.Value.Replace(";", "");
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

        private IEnumerable<string> GetUsersIds(string groupId, string postId, int countOfLikes)
        {
            var users = new XDocument();
            var result = new List<string>();

            // получить список людей лайкнувших пост
            const int step = 1000;
            try
            {
                for (var offset = 0; offset * step < countOfLikes; offset++)
                {
                    Thread.Sleep(1000);
                    users = RequestService.GetListOfLikedUsers(groupId, postId, offset * step, 1000);
                    result.AddRange(users.Descendants("users").Elements("uid").Select(p => p.Value).ToList());
                }

            }
            catch (Exception exception)
            {
                Logger.Error("Error in GetListOfLikedUsers {0}: {1}", postId, exception.InnerException);

                throw new HttpException(500, "Во время скачивания лайков произошла ошибка");
            }

            return result;
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
        public string ListOfLikeIds { get; set; }
    }
}