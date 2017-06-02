using System;
using System.Web;
using Microsoft.Owin.Host.SystemWeb;

namespace VKAnalyzer.VKAuth
{
    public class VKontakteAuthenticationClient
    {
        public string appId;
        public string appSecret;

        public VKontakteAuthenticationClient(string appId, string appSecret)
        {
            this.appId = appId;
            this.appSecret = appSecret;
        }

        string ProviderName
        {
            get { return "vkontakte"; }
        }

        void RequestAuthentication(
                                HttpContextBase context, Uri returnUrl)
        {
            throw new NotImplementedException();
        }

        AuthenticationResult VerifyAuthentication(
                               HttpContextBase context)
        {
            throw new NotImplementedException();
        }
    }
}