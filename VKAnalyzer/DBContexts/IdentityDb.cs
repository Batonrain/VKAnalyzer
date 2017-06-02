using Microsoft.AspNet.Identity.EntityFramework;
using VKAnalyzer.Models;

namespace VKAnalyzer.DBContexts
{
    public class IdentityDb : IdentityDbContext<ApplicationUser>
    {
        public IdentityDb()
            : base("DefaultConnection")
        {
        }
    }
}