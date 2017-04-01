using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;

namespace VKAnalyzer.Controllers
{
    public class VkController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CohortAnalysis(CohortAnalysysInputModel model)
        {
            if (ModelState.IsValid)
            {
                // const string groupId = "77390912";

                var posts = GetWallPosts(Convert.ToInt32(model.DaysCount + 30), model.GroupId);

                var rawData = PrepareDataForCohortAnalyse(posts, Convert.ToInt32(model.DaysCount), model.GroupId);

                var result = new CohortAnalysisResultModel
                {
                    ResultMatrix = BuildCohortAnalyseData(rawData),
                    TableLength = rawData.Count,
                    GroupId = model.GroupId
                };

                ViewBag.Message = "Cohort analysis";

                return View(result);
            }

            return RedirectToAction("Index");
        }

        private List<CohortAnalysisModel> PrepareDataForCohortAnalyse(IEnumerable<PostDataModel> posts, int daysCount, string groupId)
        {
            var result = new List<CohortAnalysisModel>();

            for (var i = 1; i <= daysCount; i++)
            {
                var pDate = DateTime.Now.AddDays(-i);
                if (posts.Count(p => p.Date.Date == pDate.Date) > 0)
                {
                    var postsForDay = posts.Where(p => p.Date.Date == pDate.Date).Select(p => p.Id);
                    var likesIds = new List<string>();

                    foreach (var post in postsForDay)
                    {
                        likesIds.AddRange(GetListOfLikedUsers(groupId, post));
                    }

                    result.Add(new CohortAnalysisModel
                    {
                        PostDate = pDate,
                        LikedIds = likesIds.Distinct()
                    });
                }
            }

            return result;
        }

        private IEnumerable<PostDataModel> GetWallPosts(int count, string groupId)
        {
            XDocument posts;
            try
            {
                // получить список всех постов на стене сообщества
                posts = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=wall.get.xml&count={0}&owner_id=-{1}", count, groupId)); 
            }
            catch (Exception exception)
            {
                _logger.Error("Ошибка получения постов со стены группы: {0}", exception.Message);
                _logger.Error("Ошибка получения постов со стены группы: {0}", exception.InnerException);

                throw new HttpException(500, "Во время скачивания постов произошла ошибка");
            }


            return posts.Descendants("post").Select(p => new PostDataModel()
            {
                Id = p.Element("id").Value,
                Date = UnixTimeStampToDateTime(Convert.ToDouble(p.Element("date").Value))
            }).ToList();
        }

        private IEnumerable<string> GetListOfLikedUsers(string groupId, string postId)
        {
            XDocument users;
            try
            {
                // получить список людей лайкнувших пост
                users = XDocument.Load(string.Format("https://api.vk.com/api.php?oauth=1&method=likes.getList.xml&owner_id=-{0}&item_id={1}&type=post", groupId, postId)); 
            }
            catch (Exception exception)
            {
                _logger.Error("Ошибка получения списков лайка поста {0}: {1}", postId, exception.Message);
                _logger.Error("Ошибка получения списков лайка поста {0}: {1}", postId, exception.InnerException);

                throw new HttpException(500, "Во время скачивания лайков произошла ошибка");
            }

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
            var invertedData = data.OrderBy(d => d.PostDate).ToList();
            var invertedDataCpunt = invertedData.Count();
            var result = new string[invertedDataCpunt, invertedDataCpunt];

            for (var h = 0; h < invertedDataCpunt; h++)
            {
                for (var g = 0; g < invertedDataCpunt; g++)
                {
                    if (h == 0 && g == 0)
                    {
                        //Первая запись в матрице
                        result[0, 0] = invertedData[0].LikedIds.Count().ToString();
                    }
                    if (h == g && g > 0)
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
                            result[i - 1, g] = usersFromPreviousCohortCount.ToString();
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