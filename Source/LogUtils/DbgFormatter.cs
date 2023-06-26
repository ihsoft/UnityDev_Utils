// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityDev.ModelUtils;

namespace UnityDev.LogUtils {

/// <summary>A set of tools to format various game entities for debugging purposes.</summary>
public static class DbgFormatter {
  /// <summary>Returns a string representation of a vector with more precision.</summary>
  /// <param name="vec">Vector to dump.</param>
  /// <returns>String representation.</returns>
  public static string Vector(Vector3 vec) {
    return $"({vec.x:0.0###}, {vec.y:0.0###}, {vec.z:0.0###})";
  }

  /// <summary>Returns a string representation of a quaternion with more precision.</summary>
  /// <param name="rot">Quaternion to dump.</param>
  /// <returns>String representation.</returns>
  public static string Quaternion(Quaternion rot) {
    return $"({rot.x:0.0###}, {rot.y:0.0###}, {rot.z:0.0###}, {rot.w:0.0###})";
  }

  /// <summary>Returns a full string path for the transform.</summary>
  /// <param name="obj">Object to make the path for.</param>
  /// <param name="parent">Optional parent to use a root.</param>
  /// <returns>Full string path to the root.</returns>
  public static string TransformPath(Transform obj, Transform parent = null) {
    return obj != null 
        ? Hierarchy.MakePath(Hierarchy.GetFullPath(obj, parent))
        : "Transform#NULL";
  }
  
  /// <summary>Returns a full string path for the game object.</summary>
  /// <param name="obj">Object to make the path for.</param>
  /// <param name="parent">Optional parent to use a root.</param>
  /// <returns>Full string path to the root.</returns>
  public static string TransformPath(GameObject obj, Transform parent = null) {
    return obj != null 
        ? TransformPath(obj.transform, parent)
        : "GameObject#NULL";
  }

  /// <summary>Flattens collection items into a comma separated string.</summary>
  /// <remarks>This method's name is a shorthand for "Collection-To-String". Given a collection (e.g. list, set, or
  /// anything else implementing <c>IEnumerable</c>) this method transforms it into a human readable string.
  /// </remarks>
  /// <param name="collection">A collection to represent as a string.</param>
  /// <param name="predicate">
  /// A predicate to use to extract string representation of an item. If <c>null</c> then standard <c>ToString()</c> is
  /// used.
  /// </param>
  /// <param name="separator">String to use to glue the parts.</param>
  /// <returns>Human readable form of the collection.</returns>
  /// <typeparam name="TSource">Collection's item type.</typeparam>
  public static string C2S<TSource>(IEnumerable<TSource> collection, Func<TSource, string> predicate = null,
                                    string separator = ",") {
    if (collection == null) {
      return "Collection#NULL";
    }
    var res = new StringBuilder();
    var firstItem = true;
    foreach (var item in collection) {
      if (firstItem) {
        firstItem = false;
      } else {
        res.Append(separator);
      }
      res.Append(predicate != null ? predicate(item) : item.ToString());
    }
    return res.ToString();
  }

  /// <summary>Prints out a content if the nullable type.</summary>
  /// <typeparam name="T">Type of the nullable value.</typeparam>
  /// <param name="value">The value to print.</param>
  /// <param name="nullStr">A string to present when the value is <c>null</c>.</param>
  /// <returns>The content of a non-null value or <paramref name="nullStr"/>.</returns>
  public static string Nullable<T>(T? value, string nullStr = "NULL") where T : struct, IConvertible {
    return value?.ToString() ?? nullStr;
  }
}

}  // namespace
