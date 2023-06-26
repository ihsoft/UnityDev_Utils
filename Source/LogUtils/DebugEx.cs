// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Linq;
using UnityEngine;

namespace UnityDev.LogUtils {

/// <summary>An extended version of the logging capabilities in the game.</summary>
/// <remarks>
/// One of the benefit of this logging class is that it can better resolve the arguments of the certain types. E.g.
/// when logging out a value referring a <see cref="Transform"/> type, the resulted record will represent a full
/// hierarchy path instead of just the object name. See <see cref="DebugEx.ObjectToString"/> for the full list of the
/// supported types.
/// </remarks>
/// <seealso cref="HostedDebugLog"/>
public static class DebugEx {
  /// <summary>
  /// Logs a formatted INFO message giving a better context on the objects in the parameters.
  /// </summary>
  /// <remarks>
  /// The arguments are not just transformed into the strings by using their <c>ToString</c> method. Instead, this
  /// method tries to make a best guess of what the object is, and gives more context when possible. Read the full list
  /// of the supported objects in the <see cref="ObjectToString"/> method docs.
  /// </remarks>
  /// <param name="format">The format string for the log message.</param>
  /// <param name="args">The arguments for the format string.</param>
  /// <seealso cref="ObjectToString"/>
  /// <seealso cref="Log"/>
  public static void Info(string format, params object[] args) {
    Log(LogType.Log, format, args);
  }

  /// <summary>Logs a formatted INFO message when the <i>verbose</i> logging mode is enabled.</summary>
  /// <inheritdoc cref="Info"/>
  public static void Fine(string format, params object[] args) {
    if (LoggingSettings.VerbosityLevel > 0) {
      Log(LogType.Log, format, args);
    }
  }

  /// <summary>Logs a formatted WARNING message with a host identifier.</summary>
  /// <inheritdoc cref="Info"/>
  public static void Warning(string format, params object[] args) {
    Log(LogType.Warning, format, args);
  }

  /// <summary>Logs a formatted ERROR message with a host identifier.</summary>
  /// <inheritdoc cref="Info"/>
  public static void Error(string format, params object[] args) {
    Log(LogType.Error, format, args);
  }

  /// <summary>Generic method to emit a log record.</summary>
  /// <remarks>
  /// It also catches the improperly declared formatting strings, and reports the error instead of throwing.
  /// </remarks>
  /// <param name="type">The type of the log record.</param>
  /// <param name="format">The format string for the log message.</param>
  /// <param name="args">The arguments for the format string.</param>
  /// <seealso cref="ObjectToString"/>
  public static void Log(LogType type, string format, params object[] args) {
    try {
      Debug.unityLogger.LogFormat(type, format, args.Select(ObjectToString).ToArray());
    } catch (Exception e) {
      Debug.LogErrorFormat("Failed to format logging string: {0}.\n{1}", format, e.StackTrace);
    }
  }

  /// <summary>Helper method to make a user friendly object name for the logs.</summary>
  /// <remarks>
  /// This method is much more intelligent than a regular <c>ToString()</c>, it can detect some common types and give
  /// more context on them while keeping the output short.
  /// </remarks>
  /// <param name="obj">The object to stringify. It can be <c>null</c>.</param>
  /// <returns>A human friendly string or the original object.</returns>
  public static object ObjectToString(object obj) {
    if (obj == null) {
      return "[NULL]";
    }
    if (obj is string || obj.GetType().IsPrimitive) {
      return obj;  // Skip types don't override ToString() and don't have special representation.
    }
    switch (obj) {
      case Component componentHost:
        return "[" + componentHost.GetType().Name + ":" + DbgFormatter.TransformPath(componentHost.transform) + "]";
      case Vector3 vec:
        return $"[Vector3:{vec.x:0.0###},{vec.y:0.0###},{vec.z:0.0###}]";
      case Quaternion rot:
        return $"[Quaternion:{rot.x:0.0###}, {rot.y:0.0###}, {rot.z:0.0###}, {rot.w:0.0###}]";
      default:
        return obj.ToString();
    }
  }
}

}  // namespace
