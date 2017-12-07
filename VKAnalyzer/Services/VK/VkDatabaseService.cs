using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.Memas;

namespace VKAnalyzer.Services.VK
{
    public class VkDatabaseService
    {
        private BaseDb _dbContext;

        public VkDatabaseService()
        {
            _dbContext = new BaseDb();
        }

        public IEnumerable<string> GetListOfGroups()
        {
            return _dbContext.Groups.Select(g => g.Link.Replace("https://vk.com/", "")).ToList();
        }

        public void SaveMemas(MemasAnalyzeResultModel result, string userId)
        {
            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkMemasAnalyzeResults.Add(new VkMemasAnalyzeResult
                {
                    UserId = userId,
                    Name = string.Format("Анализатор мемасов за {0}", DateTime.Now),
                    CollectionDate = DateTime.Now,
                    Result = rr
                });
                cntx.SaveChanges();
            }
        }

        public void SaveCohortAnalyze(CohortAnalysisResultModel result, string userId, string name, string groupId)
        {
            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkCohortAnalyseResults.Add(new VkCohortAnalyseResult
                {
                    UserId = userId,
                    Name = name,
                    CollectionDate = DateTime.Now,
                    GroupId = groupId,
                    Result = rr
                });
                cntx.SaveChanges();
            }
        }

        public void SaveAnalyzeOfSalesWithRetarget(CohortAnalysisResultModel result, string userId, string name, string groupId)
        {
            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkCohortAnalyseResults.Add(new VkCohortAnalyseResult
                {
                    UserId = userId,
                    Name = name,
                    CollectionDate = DateTime.Now,
                    GroupId = groupId,
                    Result = rr
                });
                cntx.SaveChanges();
            }
        }

        public void SaveAnalyzeOfSalesWithRetarget(SalesActivitiesRetargetResult result, string userId, string name, string groupId)
        {
            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkCohortSalesAnalyseResults.Add(new VkCohortSalesAnalyseResults
                {
                    UserId = userId,
                    Name = name,
                    CollectionDate = DateTime.Now,
                    GroupId = groupId,
                    Result = rr
                });
                cntx.SaveChanges();
            }
        }
    }
}