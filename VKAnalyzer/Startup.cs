using System.Web;
using Hangfire;
using Microsoft.Owin;
using Owin;
using VKAnalyzer.Filters;

[assembly: OwinStartupAttribute(typeof(VKAnalyzer.Startup))]
namespace VKAnalyzer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            var options = new DashboardOptions
            {
                AppPath = VirtualPathUtility.ToAbsolute("~"),
                Authorization = new[] { new DashboardAuthorizationFilter() }
            };
            app.UseHangfireDashboard("/hangfire", options);
            app.UseHangfireServer();
        }
    }
}
