using Epic.OnlineServices;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Sockets.EpicSocket
{
    public static class EpicLogger
    {
        // change default log level based on if we are in debug or release mode.
        // this is only default, if there are log settings they will be used instead of these
#if DEBUG
        const LogType DEFAULT_LOG = LogType.Warning;
#else
        const LogType DEFAULT_LOG = LogType.Error;
#endif
        internal static readonly ILogger logger = LogFactory.GetLogger(typeof(EpicSocket), DEFAULT_LOG);

        public static void WarnResult(string tag, Result result)
        {
            if (result == Result.Success) return;
            if (logger.WarnEnabled())
                logger.LogWarning($"{tag} failed with result: {result}");
        }

        internal static void Verbose(string log)
        {
#if UNITY_EDITOR
            Debug.Log(log);
#else
            Console.WriteLine(log);
#endif
        }
    }
}

