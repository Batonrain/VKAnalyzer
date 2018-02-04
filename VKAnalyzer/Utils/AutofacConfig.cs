using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Hangfire;
using VKAnalyzer.Controllers;
using VKAnalyzer.Controllers.Vk;
using VKAnalyzer.DBContexts;
using VKAnalyzer.Services.Interfaces;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Utils
{
    public class AutofacConfig
    {
        public static void ConfigureContainer()
        {
            // получаем экземпляр контейнера
            var builder = new ContainerBuilder();

            // регистрируем контроллер в текущей сборке
            //builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterType<HomeController>().InstancePerRequest();
            builder.RegisterType<MemologyController>().InstancePerRequest();
            builder.RegisterType<AffinityIndexController>().InstancePerRequest();
            builder.RegisterType<VkController>().InstancePerRequest();
            builder.RegisterType<AccountController>().InstancePerRequest();
            builder.RegisterType<ServiceController>().InstancePerRequest();

            // регистрируем споставление типов
            builder.RegisterType<BaseDb>().AsImplementedInterfaces();
            builder.RegisterType<VkBaseService>().InstancePerLifetimeScope();
            builder.RegisterType<VkDatabaseService>().InstancePerLifetimeScope();
            builder.RegisterType<VkAdsRequestService>().InstancePerLifetimeScope();
            builder.RegisterType<VkUrlService>().InstancePerLifetimeScope();
            builder.RegisterType<VkDbService>().InstancePerLifetimeScope();

            builder.RegisterType<AffinityIndexService>().InstancePerLifetimeScope();

            

            // создаем новый контейнер с теми зависимостями, которые определены выше
            var container = builder.Build();

            GlobalConfiguration.Configuration.UseAutofacActivator(container, false);
            
            // установка сопоставителя зависимостей
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}