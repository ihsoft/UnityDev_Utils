﻿// UnityDev Utils
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using System.Reflection;
using UnityDev.LogUtils;

namespace UnityDev.Utils.ReflectionUtils {

/// <summary>Wrapper to implement efficient access to the class fields via reflection.</summary>
/// <remarks>It ignores access scope.</remarks>
/// <typeparam name="T">type of the class.</typeparam>
/// <typeparam name="TV">type of the field value.</typeparam>
public sealed class ReflectedField<T, TV> {
  readonly FieldInfo _fieldInfo;

  /// <summary>Creates the reflection for the field.</summary>
  public ReflectedField(string fieldName) {
    _fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (_fieldInfo == null) {
      DebugEx.Error("Cannot obtain field {0} from {1}", fieldName, typeof(T));
    }
  }

  /// <summary>Indicates if the target field was found and ready to use.</summary>
  public bool IsValid() {
    return _fieldInfo != null;
  }

  /// <summary>Gets the field value or returns a default value if the field is not found.</summary>
  public TV Get(T instance) {
    return _fieldInfo != null ? (TV)_fieldInfo.GetValue(instance) : default(TV);
  }

  /// <summary>Gets the field value or returns the provided default value if the field is not found.</summary>
  public TV Get(T instance, TV defaultValue) {
    return _fieldInfo != null ? (TV)_fieldInfo.GetValue(instance) : defaultValue;
  }

  /// <summary>Sets the field value or does nothing if the field is not found.</summary>
  public void Set(T instance, TV value) {
    if (_fieldInfo != null) {
      _fieldInfo.SetValue(instance, value);
    }
  }
}


/// <summary>Wrapper to implement efficient access to the class fields via reflection.</summary>
/// <remarks>It ignores access scope.</remarks>
/// <typeparam name="TV">type of the field value.</typeparam>
public sealed class ReflectedField<TV> {
  readonly FieldInfo _fieldInfo;

  /// <summary>Creates the reflection for the field.</summary>
  public ReflectedField(IReflect type, string fieldName) {
    _fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (_fieldInfo == null) {
      DebugEx.Error("Cannot obtain field {0} from {1}", fieldName, type);
    }
  }

  /// <summary>Indicates if the target field was found and ready to use.</summary>
  public bool IsValid() {
    return _fieldInfo != null;
  }

  /// <summary>Gets the field value or returns a default value if the field is not found.</summary>
  public TV Get(object instance) {
    return _fieldInfo != null ? (TV)_fieldInfo.GetValue(instance) : default;
  }

  /// <summary>Gets the field value or returns the provided default value if the field is not found.</summary>
  public TV Get(object instance, TV defaultValue) {
    return _fieldInfo != null ? (TV)_fieldInfo.GetValue(instance) : defaultValue;
  }

  /// <summary>Sets the field value or does nothing if the field is not found.</summary>
  public void Set(object instance, TV value) {
    if (_fieldInfo != null) {
      _fieldInfo.SetValue(instance, value);
    }
  }
}

}
