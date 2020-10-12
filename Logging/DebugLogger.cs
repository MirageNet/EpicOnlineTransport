#region Statements

using System;
using System.Diagnostics;
using Epic.OnlineServices.Logging;
using UnityEngine;
using Debug = UnityEngine.Debug;

#endregion

namespace Epic.Logging
{
    public static class DebugLogger
    {
        /// <summary>
        ///     Convert epic services logger to unity debug logger.
        /// </summary>
        /// <param name="message"></param>
        [Conditional("UNITY_EDITOR")]
        public static void EpicDebugLog(LogMessage message)
        {
            switch (message.Level)
            {
                case LogLevel.Info:
                    Debug.Log($"Epic Manager: Category - {message.Category} Message - {message.Message}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"Epic Manager: Category - {message.Category} Message - {message.Message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"Epic Manager: Category - {message.Category} Message - {message.Message}");
                    break;
                case LogLevel.Fatal:
                    Debug.LogException(
                        new Exception($"Epic Manager: Category - {message.Category} Message - {message.Message}"));
                    break;
                default:
                    Debug.Log(
                        $"Epic Manager: Unknown log processing. Category - {message.Category} Message - {message.Message}");
                    break;
            }
        }

        /// <summary>
        ///     Regular unity debug logger.
        /// </summary>
        /// <param name="message">The message we want to display to logger.</param>
        /// <param name="logType">The type of log to display.</param>
        [Conditional("UNITY_EDITOR")]
        public static void RegularDebugLog(string message, LogType logType = LogType.Log)
        {
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log($"<color=green> {message} </color>");
                    break;
                case LogType.Warning:
                    Debug.LogWarning($"<color=orange> {message} </color>");
                    break;
                case LogType.Error:
                    Debug.LogError($"<color=red> {message} </color>");
                    break;
                default:
                    Debug.LogException(new Exception($"<color=red> {message} </color>"));
                    break;

            }
        }
    }
}
