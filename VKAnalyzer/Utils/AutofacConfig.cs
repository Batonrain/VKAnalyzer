using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Hangfire;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Services.VK;
using VKAnalyzer.Services.VK.CohortAndSale;
using VKAnalyzer.Services.VK.Common;

namespace VKAnalyzer.Utils
{
    public class AutofacConfig
    {
        public static void ConfigureContainer()
        {
            // получаем экземпляр контейнера
            var builder = new ContainerBuilder();

            // регистрируем контроллер в текущей сборке
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // регистрируем споставление типов
            builder.RegisterType<BaseDb>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<VkBaseService>().InstancePerLifetimeScope();
            builder.RegisterType<VkDatabaseService>().InstancePerLifetimeScope();
            builder.RegisterType<VkAdsRequestService>().InstancePerLifetimeScope();
            builder.RegisterType<VkUrlService>().InstancePerLifetimeScope();
            builder.RegisterType<VkDbService>().InstancePerLifetimeScope();
            
            builder.RegisterType<VkRequestService>().InstancePerLifetimeScope();

            builder.RegisterType<VkService>().InstancePerLifetimeScope();

            builder.RegisterType<VkSalesAnalysisService>().InstancePerLifetimeScope();
            builder.RegisterType<AffinityIndexService>().InstancePerLifetimeScope();

            // создаем новый контейнер с теми зависимостями, которые определены выше
            var container = builder.Build();

            GlobalConfiguration.Configuration.UseAutofacActivator(container, false);
            
            // установка сопоставителя зависимостей
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}