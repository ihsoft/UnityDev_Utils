// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Collections.Generic;

namespace UnityDev.Utils.Configs {

/// <summary>Object to keep structured run-time data.</summary>
/// <seealso cref="PersistentNode"/>
public class ConfigNode {
  /// <summary>Name of this node.</summary>
  /// <remarks>When node is in hierarchy, then this name is a name of the property in the parent.</remarks>
  public string Name { get; private set; }

  /// <summary>Creates a named node with optional comment node.</summary>
  public ConfigNode(string name = "") {
    Name = name;
  }

  /// <summary>All node fields.</summary>
  readonly Dictionary<string, List<ConfigNode>> _nodeFields = new();

  /// <summary>All simple string fields.</summary>
  readonly Dictionary<string, List<string>> _valueFields = new();

  /// <summary>Tells if the node has no fields.</summary>
  public bool IsEmpty() {
    return _valueFields.Count == 0 && _nodeFields.Count == 0;
  }

  /// <summary>Removes the field and all its content.</summary>
  public bool ClearField(string key) {
    return _nodeFields.Remove(key) || _valueFields.Remove(key);
  }

  /// <summary>Clears all data in the node.</summary>
  public void ClearData() {
    _nodeFields.Clear();
    _valueFields.Clear();
  }

  /// <summary>Sets a string value to the field.</summary>
  /// <remarks>This method overwrites the old value regardless to its type.</remarks>
  public void SetValue(string key, string value, string comment = null) {
    _valueFields[key] = new List<string> { value };
    _nodeFields.Remove(key);
  }

  /// <summary>Returns string value of the field.</summary>
  /// <param name="key">
  /// The field name. If the field exists, but it's type is not string, then it's assumed the field doesn't exist. If
  /// field has multiple values, then the first value is returned.
  /// </param>
  /// <returns>String value of the field or <c>null</c> if the field is not found or the type is wrong.</returns>
  public string GetValue(string key) {
    if (!_valueFields.TryGetValue(key, out var valueList) || valueList.Count == 0) {
      return null;
    }
    return valueList[0];
  }

  /// <summary>Adds a string value into the repeated field.</summary>
  /// <remarks>If there are nodes at this key, then they will be dropped.</remarks>
  public void AddValue(string key, string value, string comment = null) {
    if (!_valueFields.TryGetValue(key, out var valueList)) {
      valueList = new List<string>();
      _valueFields[key] = valueList;
    }
    valueList.Add(value);
    _nodeFields.Remove(key);
  }

  /// <summary>Returns strings from the repeated field.</summary>
  /// <returns>Collection of strings or empty list if the field is not found or the type is wrong.</returns>
  public IEnumerable<string> GetValues(string key) {
    return !_valueFields.TryGetValue(key, out var valueList)
        ? Array.Empty<string>()
        : valueList;
  }

  /// <summary>Sets config node as a value of the field.</summary>
  /// <remarks>This method overwrites the old value regardless to its type. The node name will be set to key.</remarks>
  public void SetNode(string key, ConfigNode value) {
    value.Name = key;
    _nodeFields[key] = new List<ConfigNode> { value };
    _valueFields.Remove(key);
  }

  /// <summary>Returns config node from the field.</summary>
  /// <param name="key">
  /// The field name. If the field exists, but it's type is not config node, then it's assumed the field doesn't exist.
  /// </param>
  /// <param name="createIfMissing">
  /// Indicates that if the field is not found, then a new empty value should be created for it.
  /// </param>
  /// <returns>Config node or <c>null</c> if the field is not found or the type is wrong.</returns>
  public ConfigNode GetNode(string key, bool createIfMissing = false) {
    if ((!_nodeFields.TryGetValue(key, out var nodeList) || nodeList.Count == 0) && !createIfMissing) {
      return null;
    }
    if (nodeList == null) {
      nodeList = new List<ConfigNode>();
      _nodeFields[key] = nodeList;
      _valueFields.Remove(key);
    }
    if (nodeList.Count == 0) {
      nodeList.Add(new ConfigNode(key));
    }
    return nodeList[0];
  }

  /// <summary>Adds a config node to the repeated field.</summary>
  /// <remarks>
  /// If the requested field doesn't exist or has a non-repeated type, then it will be assigned with a new repeated
  /// config node type.
  /// </remarks>
  public void AddNode(string key, ConfigNode value) {
    if (!_nodeFields.TryGetValue(key, out var nodeList)) {
      nodeList = new List<ConfigNode>();
      _nodeFields[key] = nodeList;
      _valueFields.Remove(key);
    }
    value.Name = key;
    nodeList.Add(value);
  }

  /// <summary>Creates and returns an empty child node.</summary>
  public ConfigNode AddNode(string key) {
    var node = new ConfigNode(key);
    AddNode(key, node);
    return node;
  }

  /// <summary>Returns names of all fields that contain nodes.</summary>
  /// <remarks>The sorting order of the names can be unstable.</remarks>
  public IEnumerable<string> GetAllNodeFields() {
    return _nodeFields.Keys;
  }

  /// <summary>Returns names of all fields that contain nodes.</summary>
  /// <remarks>The sorting order of the names can be unstable.</remarks>
  public IEnumerable<string> GetAllValueFields() {
    return _valueFields.Keys;
  }

  /// <summary>Returns list of config nodes from the repeated field.</summary>
  /// <returns>Collection of config nodes or empty list the field is not found or the type is wrong.</returns>
  public IEnumerable<ConfigNode> GetNodes(string key) {
    return !_nodeFields.TryGetValue(key, out var nodeList) ? Array.Empty<ConfigNode>() : nodeList;
  }

  public override string ToString() {
    return $"{{ConfigNode '{Name}': fields={_valueFields.Count+_nodeFields.Count}}}";
  }
}

}
