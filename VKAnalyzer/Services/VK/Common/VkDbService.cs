using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.AffinityIndex;
using VKAnalyzer.Models.VKModels.Memas;

namespace VKAnalyzer.Services.VK
{
    public class VkDbService
    {
        private BaseDb _dbContext;

        public VkDbService()
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

        public void SaveAnalyzeOfSalesWithList(CohortAnalysisResultModel result, string userId, string name, string groupId)
        {
            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkCohortSalesAnalyseWithListResults.Add(new VkCohortSalesAnalyseWithListResults
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

        public void SaveAffinityIndex(AffinityIndexResult result, string userId, string name)
        {
            using (var ms = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, result);
                byte[] rr = ms.GetBuffer();

                var cntx = new BaseDb();
                cntx.VkAffinityIndexResults.Add(new VkAffinityIndexResults
                {
                    UserId = userId,
                    Name = name,
                    CollectionDate = DateTime.Now,
                    Result = rr
                });
                cntx.SaveChanges();
            }
        }

        public List<AffinityIndexResultsViewModel> GetAffinityIndexResults(string userId)
        {
            return _dbContext
                   .VkAffinityIndexResults
                   .Where(x => x.UserId == userId)
                   .OrderByDescending(order => order.CollectionDate)
                   .Select(rest => new AffinityIndexResultsViewModel
                   {
                       Id = rest.Id,
                       Name = rest.Name,
                       DateOfCollection = rest.CollectionDate,
                       AnalyseType = "Аффинити Индекс"
                   })
                   .ToList();
        }

        public VkAffinityIndexResults GetAffinityIndexResult(int id)
        {
            return  _dbContext.VkAffinityIndexResults.FirstOrDefault(rest => rest.Id == id);
        }
    }
}