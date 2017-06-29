using System;
using System.Data.Entity;
using System.Linq;
using NLog;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Models.EFModels;

namespace VKAnalyzer.Services
{
    public class VkAuthService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private BaseDb Context { get; set; }

        public VkAuthService()
        {
            Context = new BaseDb();
        }

        public void CreateOrUpdateUserAccessToken(string accessToken, string userId)
        {
            try
            {
                using (var context = new BaseDb())
                {
                    
                    if (Context.UserAccessTokens.Count(us => us.VkUserId == userId) > 0)
                    {
                        var userToken = Context.UserAccessTokens.FirstOrDefault(us => us.VkUserId == userId);
                        userToken.AccessToken = accessToken;
                        context.Entry(userToken).State = EntityState.Modified;
                    }
                    else
                    {
                        var userAccess = new UserAccessToken
                        {
                            AccessToken = accessToken,
                            VkUserId = userId
                        };
                        context.UserAccessTokens.Add(userAccess);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error during working with Entity: {0}", ex.Message);
                _logger.Error(string.Format("Error during working with Entity: {0}", ex.InnerException));
            }
        }
    }
}