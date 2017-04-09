using Autofac;
using FinancialAdvisor.Entity;
using FinancialAdvisor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using TranslatorService;

namespace FinancialAdvisor
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            var builder = new ContainerBuilder();
            builder.RegisterType<RequestLimiter>().As<IRequestLimiter>();
            builder.RegisterType<TranslatorServiceClient>().As<ITranslatorServiceClient>();
            var container = builder.Build();
            ServiceResolver.Container = container;
        }
    }
}
