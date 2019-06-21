using Microsoft.Owin.BuilderProperties;
using Owin;
using System.Threading;

namespace Spark
{
    public class Startup
    {
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
        }
    }
}