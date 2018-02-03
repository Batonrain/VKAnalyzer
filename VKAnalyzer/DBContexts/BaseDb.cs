using System.Data.Entity;
using VKAnalyzer.Models.EFModels;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.AffinityIndex;

namespace VKAnalyzer.DBContexts
{
    public class BaseDb : DbContext
    {
        public BaseDb()
            : base("DefaultConnection")
        {
            
        }

        public DbSet<UserAccessToken> UserAccessTokens { get; set; }
        public DbSet<VkCohortAnalyseResult> VkCohortAnalyseResults { get; set; }
        public DbSet<VkCohortSalesAnalyseResults> VkCohortSalesAnalyseResults { get; set; }
        public DbSet<VkMemasAnalyzeResult> VkMemasAnalyzeResults { get; set; }
        public DbSet<VkAffinityIndexResults> VkAffinityIndexResults { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}