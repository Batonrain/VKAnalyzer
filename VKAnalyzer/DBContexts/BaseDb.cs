using System.Data.Entity;
using VKAnalyzer.Models.EFModels;
using VKAnalyzer.Models.VKModels;

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
    }
}