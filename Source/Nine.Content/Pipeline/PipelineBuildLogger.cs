namespace Nine.Content.Pipeline
{
    using System.Diagnostics;
    using Microsoft.Xna.Framework.Content.Pipeline;

    class PipelineBuildLogger : ContentBuildLogger
    {
        public static PipelineBuildLogger Instance { get; private set; }

        static PipelineBuildLogger()
        {
            Instance = new PipelineBuildLogger();
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
#if PCL
            Debug.WriteLine(message, messageArgs);
#else
            Trace.TraceInformation(message, messageArgs);
#endif
        }

        public override void LogMessage(string message, params object[] messageArgs)
        {
#if PCL
            Debug.WriteLine(message, messageArgs);
#else
            Trace.WriteLine(string.Format(message, messageArgs));
#endif
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
#if PCL
            Debug.WriteLine(message, messageArgs);
#else
            Trace.TraceWarning(message, messageArgs);
#endif
        }
    }
}
