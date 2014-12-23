namespace Keiser.MvxPlugins.SoftwareManager
{
    using System;
    using System.Diagnostics;
    using Cirrious.CrossCore.Platform;

    public class Trace
    {
        private const string Tag = "Keiser.MvxPlugins.SoftwareManager";

        public static void Info(string message, params object[] args)
        {
            MvxTrace.TaggedTrace(MvxTraceLevel.Diagnostic, Tag, message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            MvxTrace.TaggedTrace(MvxTraceLevel.Warning, Tag, message, args);
        }

        public static void Error(string message, params object[] args)
        {
            MvxTrace.TaggedTrace(MvxTraceLevel.Error, Tag, message, args);
        }
    }
}