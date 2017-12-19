using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using VKAnalyzer.Controllers;

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
            builder.RegisterType<VkController>().InstancePerRequest();
            builder.RegisterType<AccountController>().InstancePerRequest();
            builder.RegisterType<ServiceController>().InstancePerRequest();

            //// регистрируем споставление типов
            //builder.RegisterType<BookRepository>().As<IRepository>();

            // создаем новый контейнер с теми зависимостями, которые определены выше
            var container = builder.Build();

            // установка сопоставителя зависимостей
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}