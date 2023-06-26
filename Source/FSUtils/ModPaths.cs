// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.IO;

namespace UnityDev.Utils.FSUtils {

/// <summary>A helper class to deal with plugins file structure.</summary>
public static class ModPaths {
  /// <summary>Returns full path to the game's root folder.</summary>
  public static string ApplicationRootPath => throw new NotImplementedException("Implement for the target game");

  /// <summary>Returns full path to the plugins root folder.</summary>
  public static string PluginsRootPath => throw new NotImplementedException("Implement for the target game");

  /// <summary>Makes full absolute path from the provided relative path in the type's DLL location folder.</summary>
  /// <remarks>
  /// If joining of all the provided parts gives a full path then it's only normalized. In case of path is relative it's
  /// resolved against plugin root folder.
  /// </remarks>
  /// <param name="type">Type to detect the root folder from.</param>
  /// <param name="pathParts">Path parts for an absolute or relative path.</param>
  /// <returns>
  /// Absolute path. All relative casts (e.g. '..') will be resolved, and all directory separators will be translated to
  /// the platform format (e.g. '/' will become '\' on Windows). 
  /// </returns>
  public static string MakeAbsPathForPlugin(Type type, params string[] pathParts) {
    var path = Path.Combine(
        Path.GetDirectoryName(type.Assembly.Location)!, string.Join("" + Path.DirectorySeparatorChar, pathParts)); 
    return Path.GetFullPath(new Uri(path).LocalPath);
  }
}

}  // namespace
