// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UnityDev.LogUtils {

/// <summary>Helper class to log a record which is bound to a specific object.</summary>
/// <remarks>
/// <p>
/// It may be useful when there are situations that relate to a specific instance of a common Unity object. With the
/// hosted logging, there will be no need to manually designate for which object the record is being logged.
/// </p>
/// <p>
/// Another benefit of this logging class is that it can better resolve the arguments of the certain types. E.g. when
/// logging out a value referring a <see cref="Transform"/> type, the resulted record will represent a full hierarchy
/// path instead of just the object name. See <see cref="DebugEx.ObjectToString"/> for the full list of the
/// supported types.
/// </p>
/// </remarks>
/// <seealso cref="DebugEx"/>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class HostedDebugLog {
  /// <summary>Logs a formatted INFO message with a host identifier.</summary>
  public static void Info(Transform host, string format, params object[] args) {
    Log(LogType.Log, host, format, args);
  }

  /// <summary>
  /// Logs a formatted INFO message with a host identifier when the <i>verbose</i> logging mode is enabled.
  /// </summary>
  public static void Fine(Transform host, string format, params object[] args) {
    if (LoggingSettings.VerbosityLevel > 0) {
      Log(LogType.Log, host, format, args);
    }
  }

  /// <summary>Logs a formatted WARNING message with a host identifier.</summary>
  public static void Warning(Transform host, string format, params object[] args) {
    Log(LogType.Warning, host, format, args);
  }

  /// <summary>Logs a formatted ERROR message with a host identifier.</summary>
  public static void Error(Transform host, string format, params object[] args) {
    Log(LogType.Error, host, format, args);
  }

  /// <summary>Generic method to emit a hosted log record.</summary>
  /// <param name="type">The type of the log record.</param>
  /// <param name="host">The host object which is bound to the log record. It can be <c>null</c>.</param>
  /// <param name="format">The format string for the log message.</param>
  /// <param name="args">The arguments for the format string.</param>
  /// <seealso cref="DebugEx.ObjectToString"/>
  public static void Log(LogType type, object host, string format, params object[] args) {
    DebugEx.Log(type, DebugEx.ObjectToString(host) + " " + format, args);
  }
}

}  // namespace
