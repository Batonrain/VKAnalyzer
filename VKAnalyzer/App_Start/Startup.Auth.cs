using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.VKontakte;
using Owin.Security.Providers.VKontakte.Provider;

namespace VKAnalyzer
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.UseVKontakteAuthentication("2203076", "VyDbxal6Wbs6zbt0xy7E");
            app.UseVKontakteAuthentication(new VKontakteAuthenticationOptions()
            {
                ClientId = "2203076",
                ClientSecret = "VyDbxal6Wbs6zbt0xy7E",
                Display = "page",
                CallbackPath = new PathString("/Vk/Result"),
                Scope = new []{"friends", "photo"},
                AuthenticationType = "Вконтакте",
                AuthenticationMode = AuthenticationMode.Passive,
                Provider = new VKontakteAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("VKAccessToken", context.AccessToken));
                        return Task.FromResult(0);
                    }

                }
            });
        }
    }
}