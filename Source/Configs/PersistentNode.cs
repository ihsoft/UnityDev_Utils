// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityDev.LogUtils;
using UnityEngine;

namespace UnityDev.Utils.Configs {

/// <summary>Class that can save/restore its fields to/from <c>ConfigNode</c>.</summary>
/// <remarks>
/// <p>
/// Due to config node limitations a list of lists cannot be handled. The descendants can override
/// <see cref="GetPersistentFields"/> method to explicitly specify which fields to persist. By default, all public
/// non-static fields will be attempted to persist.
/// </p>
/// <p>
/// Collections must be descendants of <c>ICollection&lt;T&gt;</c>. The collection field can be read-only if it's not
/// not NULL. If the field is NULL, then the content will be created on load. If it's an existing collection, then it
/// will be cleared before reading new values.  
/// </p>
/// <p>
/// Nested nodes must be descendants of <c>PersistentNode</c>. If it's not readonly and NULL, then a new empty node will
/// be created on load. If it's an existing node, then the loaded content will be merged.
/// </p>
/// <p>Also supported some stock Unity types: <c>Vector2</c>, <c>Vector3</c>, <c>Vector4</c>, and <c>Color</c>.</p>
/// <p>
/// Extra custom types can be handled via overriding <see cref="TryParseCustomType"/> and
/// <see cref="CustomTypeToString"/>.
/// </p>
/// </remarks>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class PersistentNode {
  #region API
  /// <summary>Loads node from config.</summary>
  public void LoadFromConfigNode(ConfigNode node) {
    foreach (var fieldInfo in GetPersistentFields()) {
      var oldValue = fieldInfo.GetValue(this);
      var newValue = ParseFieldValue(oldValue, fieldInfo.FieldType, fieldInfo.Name, node);
      if (!fieldInfo.IsInitOnly) {
        fieldInfo.SetValue(this, newValue);
      } else if (oldValue == null && newValue != null) {
        DebugEx.Error("Cannot assign to readonly field: name={0}, type={1}", fieldInfo.Name, fieldInfo.FieldType);
      }
    }
  }

  /// <summary>Stores node state into an existing config node. Only non-null fields are stored.</summary>
  public void MergeToConfigNode(ConfigNode node) {
    foreach (var fieldInfo in GetPersistentFields()) {
      var fieldValue = fieldInfo.GetValue(this);
      if (fieldValue != null) {
        StoreFieldValue(fieldValue, fieldInfo.Name, node);
      }
    }
  }

  /// <summary>Captures node state into a new config node. Only non-null fields are captured.</summary>
  /// <returns>The captured state. It's never <c>null</c>.</returns>
  /// <seealso cref="ConfigNode.IsEmpty"/>
  public ConfigNode GetConfigNode(string name = null) {
    var node = new ConfigNode(name ?? GetType().FullName);
    MergeToConfigNode(node);
    return node;
  }
  #endregion

  #region Overrides
  /// <summary>Returns fields that should be persisted.</summary>
  /// <remarks>The default implementation will attempt all non-static public fields.</remarks>
  protected virtual IEnumerable<FieldInfo> GetPersistentFields() {
    return GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
  }

  /// <summary>Tries to parse a value that was not recognized by the implementation.</summary>
  /// <remarks>This method can only deal with a single string serialized value.</remarks>
  /// <param name="fieldValue">
  /// The current value from the target type. It can be <c>null</c>. Update this reference if the result must be
  /// accepted.
  /// </param>
  /// <param name="valueType">The value type.</param>
  /// <param name="serializedValue">String value from the related config node.</param>
  /// <returns><c>true</c> if the value should be accepted.</returns>
  protected virtual bool TryParseCustomType(ref object fieldValue, Type valueType, string serializedValue) {
    return false;
  }

  /// <summary>Returns string representation of the value.</summary>
  /// <param name="value">Object to serialize. It's never <c>null</c>.</param>
  /// <returns>The serialized string or <c>null</c> if the type is unexpected or unknown.</returns>
  protected virtual string CustomTypeToString(object value) {
    return null;
  }
  #endregion

  #region Utility methods
  /// <summary>Parses a vector from serialized string</summary>
  /// <seealso cref="VectorToString(Vector2)"/>
  protected static Vector2? ParseVector2(string serializedValue) {
    var parts = serializedValue.Split(',');
    if (parts.Length == 2) {
      try {
        return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
      } catch (Exception) {
        // ignored
      }
    }
    DebugEx.Error("Cannot parse as Vector2: {0}", serializedValue);
    return null;
  }

  /// <inheritdoc cref="ParseVector2"/>
  protected static Vector3? ParseVector3(string serializedValue) {
    var parts = serializedValue.Split(',');
    if (parts.Length == 3) {
      try {
        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
      } catch (Exception) {
        // ignored
      }
    }
    DebugEx.Error("Cannot parse as Vector3: {0}", serializedValue);
    return null;
  }

  /// <inheritdoc cref="ParseVector2"/>
  protected static Vector4? ParseVector4(string serializedValue) {
    var parts = serializedValue.Split(',');
    if (parts.Length == 4) {
      try {
        return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
      } catch (Exception) {
        // ignored
      }
    }
    DebugEx.Error("Cannot parse as Vector4: {0}", serializedValue);
    return null;
  }

  /// <summary>Returns serialized string for vector type.</summary>
  protected static string VectorToString(Vector2 value) {
    return $"{value.x},{value.y}";
  }

  /// <inheritdoc cref="VectorToString(Vector2)"/>
  protected static string VectorToString(Vector3 value) {
    return $"{value.x},{value.y},{value.z}";
  }

  /// <inheritdoc cref="VectorToString(Vector2)"/>
  protected static string VectorToString(Vector4 value) {
    return $"{value.x},{value.y},{value.z},{value.w}";
  }
  #endregion

  #region Implementation
  /// <summary>Parses serialized state from the node into an object of the specified type.</summary>
  /// <remarks>If config node doesn't have a value for teh field, then it stays unchanged.</remarks>
  /// <param name="fieldValue">The old value from the existing target class. It can be <c>null</c>.</param>
  /// <param name="valueType">Value type.</param>
  /// <param name="fieldName">Field name in the target class.</param>
  /// <param name="node">The persisted state for this level of the nested nodes.</param>
  /// <returns>The value to store to the target class.</returns>
  object ParseFieldValue(object fieldValue, Type valueType, string fieldName, ConfigNode node) {
    // Simple ordinary types that can be deserialized from a single string.
    if (valueType.IsPrimitive || valueType.IsEnum || valueType == typeof(string)) {
      var serializedValue = node.GetValue(fieldName);
      return serializedValue != null
          ? TypeDescriptor.GetConverter(valueType).ConvertFromString(serializedValue)
          : fieldValue;
    }
    if (valueType == typeof(Vector2)) {
      return ParseVector2(node.GetValue(fieldName) ?? "") ?? fieldValue;
    }
    if (valueType == typeof(Vector3)) {
      return ParseVector3(node.GetValue(fieldName) ?? "") ?? fieldValue;
    }
    if (valueType == typeof(Vector4)) {
      return ParseVector4(node.GetValue(fieldName) ?? "") ?? fieldValue;
    }
    if (valueType == typeof(Color)) {
      var res = ParseVector4(node.GetValue(fieldName) ?? "");
      if (res.HasValue) {
        return (Color) res.Value;
      }
      return fieldValue;
    }
    if (TryParseCustomType(ref fieldValue, valueType, node.GetValue(fieldName))) {
      return fieldValue;
    }

    // Handle nested persistent nodes.
    if (typeof(PersistentNode).IsAssignableFrom(valueType)) {
      var persistentNode = node.GetNode(fieldName);
      if (persistentNode == null) {
        return fieldValue;
      }
      if (fieldValue == null) {
        if (!TryCreateDefaultInstance(valueType, out fieldValue)) {
          DebugEx.Error("Cannot construct PersistentNode: field={0}, type={1}", fieldName, valueType);
          return fieldValue;
        }
      }
      (fieldValue as PersistentNode)!.LoadFromConfigNode(persistentNode);
      return fieldValue;
    }

    // Handle collections.
    if (ParseCollection(ref fieldValue, valueType, fieldName, node)) {
      return fieldValue;
    }

    DebugEx.Error("Don't know how to parse field: name={0}, type={1}", fieldName, valueType);
    return fieldValue;
  }

  /// <summary>Parses a field of type ICollection&lt;T&gt;.</summary>
  /// <remarks>The collection element type must be constructable via <c>Activator</c>.</remarks>
  bool ParseCollection(ref object fieldValue, Type valueType, string fieldName, ConfigNode node) {
    var collectionType = GetCollectionInterface(valueType);
    if (collectionType == null) {
      return false;
    }
    if (fieldValue == null) {
      if (!TryCreateDefaultInstance(valueType, out fieldValue)) {
        DebugEx.Error("Cannot construct collection: field={0}, type={1}", fieldName, valueType);
        return true;
      }
    }
    var addMethod = collectionType.GetMethod("Add");
    var clearMethod = collectionType.GetMethod("Clear");
    if (addMethod == null || clearMethod == null) {
      return false;  // Cannot happen, but just to calm down the static analyzers.
    }
    var elementType = collectionType.GetGenericArguments()[0];
    if (IsCollection(elementType)) {
      DebugEx.Error("Cannot handle collections of collections: field={0}, type={1}", fieldName, valueType);
      return true;
    }
    clearMethod.Invoke(fieldValue, Array.Empty<object>());
    if (elementType == typeof(PersistentNode)) {
      foreach (var elementNode in node.GetNodes(fieldName)) {
        if (!TryCreateDefaultInstance(elementType, out var elementValue)) {
          DebugEx.Error("Cannot create element of collection: field={0}, type={1}", fieldName, valueType);
          return true;
        }
        elementValue = ParseFieldValue(elementValue, elementType, fieldName, elementNode);
        addMethod.Invoke(fieldValue, new []{ elementValue });
      }
    } else {
      foreach (var elementPersistedValue in node.GetValues(fieldName)) {
        var elementNode = new ConfigNode();
        elementNode.SetValue(fieldName, elementPersistedValue);
        var elementValue = ParseFieldValue(null, elementType, fieldName, elementNode);
        addMethod.Invoke(fieldValue, new []{ elementValue });
      }
    }
    return true;
  }

  void StoreFieldValue(object fieldValue, string fieldName, ConfigNode node) {
    var valueType = fieldValue.GetType();

    // Handle nested persistent nodes.
    if (fieldValue is PersistentNode) {
      var childNode = new ConfigNode(fieldName);
      StoreFieldValue(fieldValue, fieldName, childNode);
      if (!childNode.IsEmpty()) {
        node.AddNode(fieldName, childNode);
      }
      return;
    }

    // Handle collections.
    if (IsCollection(valueType)) {
      if (IsCollection(valueType.GetGenericArguments()[0])) {
        DebugEx.Error("Cannot handle collections of collections: field={0}, type={1}", fieldName, valueType);
        return;
      }
      if (fieldValue is IEnumerable collection) {
        foreach (var element in collection) {
          if (element != null) {
            StoreFieldValue(element, fieldName, node);
          }
        }
      }
      return;
    }

    if (valueType.IsPrimitive || valueType.IsEnum || valueType == typeof(string)) {
      node.AddValue(fieldName, fieldValue.ToString());
      return;
    }
    var strValue = fieldValue switch {
        Vector2 value => VectorToString(value),
        Vector3 value => VectorToString(value),
        Vector4 value => VectorToString(value),
        Color color => VectorToString(color),
        _ => CustomTypeToString(fieldValue)
    };
    if (strValue != null) {
      node.AddValue(fieldName, strValue);
      return;
    }

    DebugEx.Error("Don't know how to serialize field: name={0}, type={1}", fieldName, valueType);
  }

  static bool TryCreateDefaultInstance(Type type, out object result) {
    try {
      result = Activator.CreateInstance(type);  // Not very efficient approach performance wise.
    } catch (Exception) {
      result = null;
      return false;
    }
    return true;
  }

  static Type GetCollectionInterface(Type type) {
    return type.GetInterfaces()
        .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
  }

  static bool IsCollection(Type type) {
    return GetCollectionInterface(type) != null;
  }
  #endregion
}

}
