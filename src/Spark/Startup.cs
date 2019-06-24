using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Owin;
using Spark.Engine.Extensions;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: OwinStartup(typeof(Spark.Startup))]
namespace Spark
{
    public class Startup
    {
        public void Configure(HttpConfiguration config)
        {
            UnityConfig.RegisterComponents(config);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            config.AddFhir(Settings.PermissiveParsing);
        }
        public void Configuration(IAppBuilder app)
        {
            // initialize logging
            Logging.Instance.SetUp();
            var appProperties = new AppProperties(app.Properties);
            var token = appProperties.OnAppDisposing;
            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    Logging.Instance.TearDown();
                });
            }
            app.MapSignalR();
            GlobalConfiguration.Configure(this.Configure);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}