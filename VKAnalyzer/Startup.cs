using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VKAnalyzer.Startup))]
namespace VKAnalyzer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
