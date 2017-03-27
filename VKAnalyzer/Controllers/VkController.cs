using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CohortAnalysis(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                var accessToken = string.Empty;
                using (var context = new BaseDb())
                {
                    var userid = User.Identity.GetUserId();
                    accessToken = context.UserAccessTokens.First(u => u.VkUserId == userid).AccessToken;
                }

                // const string groupId = "116839698"; //D&D в Лесу

                var posts = GetWallPosts(Convert.ToInt32(model.PostsCount), model.GroupId);

                var rawData = posts.Select(post => new CohortAnalysisModel()
                {
                    PostId = post.Id, 
                    PostDate = post.Date, 
                    LikedIds = GetListOfLikedUsers(model.GroupId, post.Id)
                }).ToList();

                var result = new CohortAnalysisResultModel
                {
                    ResultMatrix = BuildCohortAnalyseData(rawData),
                    TableLength = rawData.Count

                };

                //var doc = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=groups.getMembers.xml&group_id={0}", groupId)); // получить список людей, состоящих в сообществе

                ViewBag.Message = "Cohort analysis";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        private IEnumerable<PostDataModel> GetWallPosts(int count, string groupId)
        {
            var posts = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&count={0}&owner_id=-{1}", count, groupId)); // получить список всех постов на стене сообщества

            return posts.Descendants("post").Select(p => new PostDataModel()
            {
                Id = p.Element("id").Value,
                Date = UnixTimeStampToDateTime(Convert.ToDouble(p.Element("date").Value))
            }).ToList();
        }

        private IEnumerable<string> GetListOfLikedUsers(string groupId, string postId)
        {
            var users = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=likes.getList.xml&owner_id=-{0}&item_id={1}&type=post", groupId, postId)); // получить список людей лайкнувших пост

            var result = users.Descendants("users").Elements("uid").Select(p => p.Value).ToList();

            return result;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private string[,] BuildCohortAnalyseData(IEnumerable<CohortAnalysisModel> data)
        {
            var invertedData = data.OrderByDescending(d => d.PostDate).ToList();
            var invertedDataCpunt = invertedData.Count();
            var result = new string[invertedDataCpunt, invertedDataCpunt];

            for (var h = 0; h < invertedDataCpunt; h++)
            {
                for (var g = 0; g < invertedDataCpunt; g++)
                {
                    if (h == 0 && g == 0)
                    {
                        result[0, 0] = invertedData[0].LikedIds.Count().ToString();
                    }
                    if (h == g && g > 0 )
                    {
                        var currentCohort = invertedData[g].LikedIds.ToList();

                        var allPreviuosUsers = new List<string>();

                        for (var i = g; i > g; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни
                            allPreviuosUsers = allPreviuosUsers.Concat(invertedData[i - 1].LikedIds).ToList();
                        }
                        
                        //Новые уникальные пользователи
                        var newUsersCount = currentCohort.Count(q => !allPreviuosUsers.Contains(q));
                        result[g, g] = newUsersCount.ToString(); // запись в таблицу самых новых пользователей

                        //Пользователи за предыдущие дни
                        for (var i = g; i > 0; i--)
                        {
                            //конкатинация всех пользователей за предыдущие дни
                            var previusCohort = invertedData[i - 1].LikedIds;
                            var usersFromPreviousCohortCount = currentCohort.Count(previusCohort.Contains);
                            result[i-1, g] = usersFromPreviousCohortCount.ToString();
                        }
                    }
                }
            }

            return result;
        }

        public List<T> IntersectAll<T>(IEnumerable<IEnumerable<T>> lists)
        {
            HashSet<T> hashSet = null;
            foreach (var list in lists)
            {
                if (hashSet == null)
                {
                    hashSet = new HashSet<T>(list);
                }
                else
                {
                    hashSet.IntersectWith(list);
                }
            }
            return hashSet == null ? new List<T>() : hashSet.ToList();
        }
    }
}