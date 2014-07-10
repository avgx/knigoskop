using System;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Knigoskop.Site.Code.Formatters;
using Knigoskop.Site.Common.Security;
using Knigoskop.Site.Models;
using Knigoskop.Site.Services;
using Knigoskop.Site.Services.Interface;
using OAuth2;
using OAuth2.Client;
using RestSharp;

namespace Knigoskop.Site
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Session_Start(Object sender, EventArgs e)
        {
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterConfigs();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            RegisterFormatters();
            SetDependencyResolver();
        }

        private void RegisterConfigs()
        {            
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void RegisterFormatters()
        {
            GlobalConfiguration.Configuration.Formatters.Add(new ProtobufMediaTypeFormatter());
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("json", "true", "application/json"));
        }

        private void SetDependencyResolver()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.RegisterWebApiModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());


            builder
                .RegisterAssemblyTypes(
                    Assembly.GetExecutingAssembly(),
                    Assembly.GetAssembly(typeof(DataService)),
                    Assembly.GetAssembly(typeof(WebApiDataService)),
                    Assembly.GetAssembly(typeof(OpdsDataService)),
                    Assembly.GetAssembly(typeof(ConvertService)),
                    Assembly.GetAssembly(typeof(SocialService)),
                    Assembly.GetAssembly(typeof(MailService)),                 
                    Assembly.GetAssembly(typeof(OAuth2Client)),
                    Assembly.GetAssembly(typeof(RestClient)),
                    Assembly.GetAssembly(typeof(SocialIdentityInjector))
                ).AsImplementedInterfaces().AsSelf();

            builder.RegisterType<SearchService>()
                    .AsImplementedInterfaces()
                    .OnActivating(e => e.Instance.Initialize(new SearchStorageModel
                    {
                        DirectoryName = HttpRuntime.AppDomainAppPath,
                        StorageType = SearchStorageTypeEnum.FileSystem
                    }));

            builder.RegisterType<AuthorizationRoot>()
                   .WithParameter(new NamedParameter("sectionName", "oauth2"));

            builder.RegisterType<SocialIdentityInjector>().PropertiesAutowired();            

            builder.RegisterFilterProvider();

            IContainer container = builder.Build();
            
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}