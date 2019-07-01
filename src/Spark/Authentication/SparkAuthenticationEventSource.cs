using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Web;

namespace Spark.Authentication
{
    [EventSource(Name = "Furore-Spark-Authentication")]
    public sealed class SparkAuthenticationEventSource: EventSource
    {
        public class Keywords
        {
            public const EventKeywords ServiceMethod = (EventKeywords)1;
            public const EventKeywords Invalid = (EventKeywords)2;
            public const EventKeywords Unsupported = (EventKeywords)4;
            public const EventKeywords Tracing = (EventKeywords)8;
        }

        public class Tasks
        {
            public const EventTask ServiceMethod = (EventTask)1;
        }

        private static readonly Lazy<SparkAuthenticationEventSource> Instance = new Lazy<SparkAuthenticationEventSource>(() => new SparkAuthenticationEventSource());

        internal SparkAuthenticationEventSource() { }

        public static SparkAuthenticationEventSource Log { get { return Instance.Value; } }

        [Event(1, Message = "Failed to validate token: {0}", Level = EventLevel.Error, Keywords = Keywords.Invalid)]
        internal void TokenValidationFailure(string message, string token)
        {
            this.WriteEvent(1, message, token);
        }

        [Event(2, Message = "Nu such file: {0}", Level = EventLevel.Warning, Keywords = Keywords.Invalid)]
        internal void NoSuchFile (string path)
        {
            this.WriteEvent(2, path);
        }

        [Event(3, Message = "JWT key added", Level = EventLevel.Verbose, Keywords = Keywords.Tracing)]
        internal void JwtKeyAdded (string type, string issuer, string name)
        {
            this.WriteEvent(3, type, issuer, name);
        }

    }
}