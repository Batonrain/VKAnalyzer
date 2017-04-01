using System.Data.Entity;
using VKAnalyzer.Models.EFModels;

namespace VKAnalyzer.DBContexts
{
    public class BaseDb : DbContext
    {
        public BaseDb()
            : base("DefaultConnection")
        {
            
        }

        public DbSet<UserAccessToken> UserAccessTokens { get; set; }
    }
}