using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using Spark.Engine.Logging;
using Spark.Mongo;
using System.Diagnostics.Tracing;

namespace Spark
{
    public class Logging
    {
        private static Logging logger = new Logging();
        private Logging()
        {

        }
        private ObservableEventListener eventListener;
        public void SetUp()
        {
            eventListener = new ObservableEventListener();
            eventListener.EnableEvents(SparkEngineEventSource.Log, EventLevel.LogAlways,
                Keywords.All);
            eventListener.EnableEvents(SparkMongoEventSource.Log, EventLevel.LogAlways,
                Keywords.All);
            eventListener.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.LogAlways, Keywords.All);
            var formatter = new JsonEventTextFormatter(EventTextFormatting.Indented);
            eventListener.LogToFlatFile(@".\spark.log", formatter);
        }

        public void TearDown()
        {
            if (eventListener != null)
            {
                eventListener.DisableEvents(SemanticLoggingEventSource.Log);
                eventListener.DisableEvents(SparkMongoEventSource.Log);
                eventListener.DisableEvents(SparkEngineEventSource.Log);
                eventListener.Dispose();
            }
        }

        public static Logging Instance {
            get { return logger; }
        }
    }
}