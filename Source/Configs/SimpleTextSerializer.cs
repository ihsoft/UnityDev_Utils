// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityDev.LogUtils;

namespace UnityDev.Utils.Configs {

/// <summary>Implements serializer for simple text format that supports structure and comments.</summary>
/// <remarks>This format is simple and human friendly. However, it's not efficient performance wise.</remarks>
/// <example><code><![CDATA[
/// NodeName {
///   value : 123 // comment
///   SubNodeName {
///     value2 : 321 // element #0
///     value2 : 321 // element #1
///   }
///  ArrayNodeName {} // empty node element #0
///  ArrayNodeName {} // empty node element #1
/// }
/// ]]></code></example>
public static class SimpleTextSerializer {

  #region API
  /// <summary>Makes config node from it's serialized state.</summary>
  /// <returns>The node or <c>null</c> if the content cannot be parsed.</returns>
  public static ConfigNode Deserialize(string serializedNode) {
    return ParseFromLines(serializedNode.Split('\n'));
  }

  /// <summary>Loads config node from a file on disk.
  /// </summary>
  /// <param name="filename">The file name. It must exist.</param>
  /// <param name="ignoreMissing">
  /// If <c>false</c>, then in case of missing file an exception is thrown. Otherwise, a<c>null</c> value is returned.
  /// </param>
  /// <returns>Config node or <c>null</c> if the node cannot be made from the file content.</returns>
  public static ConfigNode LoadFromFile(string filename, bool ignoreMissing = false) {
    if (File.Exists(filename)) {
      return ParseFromLines(File.ReadAllLines(filename));
    }
    if (!ignoreMissing) {
      DebugEx.Error("Cannot find config file: {0}", filename);
    }
    return null;
  }

  /// <summary>Serializes config node into a multi-line string.</summary>
  /// <remarks></remarks>
  /// <param name="node">
  /// The node to persist. If the node has name, then its content is persisted with the node's name as a key name. If
  /// the node's name is empty, then this node is considered a "wrapper". All sub-nodes from the wrapper will be saved
  /// under their keys, and all values in the wrapper node will be ignored. Wrapper allows storing multiple nodes in
  /// one file without adding unnecessary brackets at the top level.
  /// </param>
  // ReSharper disable once MemberCanBePrivate.Global
  public static string Serialize(ConfigNode node) {
    var content = new StringBuilder();
    if (node.Name.Length == 0) {
      // Unwrap group container.
      if (node.GetAllValueFields().Any()) {
        DebugEx.Warning("Ignoring {0} value fields in a wrapper node", node.GetAllValueFields().Count());
      }
      foreach (var fieldName in node.GetAllNodeFields()) {
        foreach (var childNode in node.GetNodes(fieldName)) {
          SerializeNode(content, childNode, 0);
        }
      }
    } else {
      SerializeNode(content, node , 0);  // Write as is.
    }
    return content.ToString();
  }

  /// <summary>Saves the node into file.</summary>
  /// <remarks>The directory path will be created if missing. An existing file will be overwritten.</remarks>
  /// <param name="filename">File name to store the content into.</param>
  /// <param name="node">The node to store.</param>
  /// <param name="throwOnError">
  /// If set to <c>true</c>, then any exception during the node persistence will be rethrown.
  /// </param>
  /// <returns>
  /// <c>False</c> if exception happen during persistence and <paramref name="throwOnError"/> is disabled.
  /// </returns>
  public static bool SaveToFile(string filename, ConfigNode node, bool throwOnError = false) {
    try {
      var content = Serialize(node);
      var savePath = Path.GetDirectoryName(filename);
      if (!string.IsNullOrEmpty(savePath)) {
        Directory.CreateDirectory(savePath);
      }
      File.WriteAllText(filename, content);
    } catch (Exception e) {
      DebugEx.Error("Failed saving node '{0}' to file {1}: {2}", node.Name, filename, e.Message);
      if (throwOnError) {
        throw;
      }
      return false;
    }
    return true;
  }
  #endregion

  #region Local fields
  /// <summary>Parses a beginning of the multiline sub-node declaration.</summary>
  /// <remarks>
  /// It detects the staring of the block and returns the key (sub-node name) as <c>$2</c>.
  /// </remarks>
  /// <example>
  /// <code><![CDATA[
  /// MODULE
  /// {
  ///   foo = bar
  /// }
  /// ]]></code>
  /// </example>
  static readonly Regex NodeMultiLinePrefixDeclRe = new(@"^\s*(\W?)([a-zA-Z0-9]+)(\S*)\s*(.*?)\s*$");

  /// <summary>Parses a beginning sub-node declaration that starts on the same line.</summary>
  /// <remarks>
  /// It detects the staring of the block and returns the key (sub-node name) as <c>$2</c> and
  /// everything after the opening bracket as <c>$3</c>.
  /// </remarks>
  /// <example>
  /// <code><![CDATA[
  /// MODULE { foo = bar }
  /// foo {} bar {}
  /// ]]></code>
  /// </example>
  static readonly Regex NodeSameLineDeclRe = new(@"^\s*(\W?)([a-zA-Z0-9]*)(\S*)\s*{\s*(.*?)\s*$");

  /// <summary>Parses a simple key/value pair.</summary>
  /// <remarks>
  /// The any bracket symbol work as a stop symbol! The key is returned as <c>$2</c> and the value is returned as
  /// <c>$3</c>.
  /// </remarks>
  /// <example>
  /// <code><![CDATA[
  /// foo = bar
  /// ]]></code>
  /// </example>
  static readonly Regex KeyValueLineDeclRe = new(@"^\s*(\W?)(\S+)\s*(\W?)=\s*(.*?)\s*(//\s*(.*))?$");

  /// <summary>Parses a comment that takes the whole line.</summary>
  static readonly Regex CommentDeclRe = new(@"^\s*//\s*(.*?)\s*?$");
  #endregion

  #region Local methods
  static ConfigNode ParseFromLines(string[] lines) {
    for (int i = lines.Length - 1; i >= 0; i--) {
      lines[i] = lines[i].Trim();
    }

    var nodesStack = new List<ConfigNode> { new() };
    var node = nodesStack[0];
    var lineNum = 0;
    while (lineNum < lines.Length) {
      var line = lines[lineNum];

      // Check for the node section close.
      if (line.StartsWith("}", StringComparison.Ordinal)) {
        nodesStack.RemoveAt(nodesStack.Count - 1);
        if (nodesStack.Count == 0) {
          ReportParseError(line, lineNum, "Unexpected node close statement");
          return null;
        }
        node = nodesStack[nodesStack.Count - 1];

        line = line.Substring(1).TrimStart();  // Chop-off "}".
        if (line.Length == 0) {
          lineNum++;
          continue;
        }

        // Check if it's a closing bracket comment.
        var commentMatch = CommentDeclRe.Match(line);
        if (commentMatch.Success) {
          lineNum++;
        } else {
          lines[lineNum] = line;  // There's something left in the line, re-try it.
        }
        continue;
      }

      // CASE #1: Empty line.
      if (line.Length == 0) {
        lineNum++;
        continue;
      }

      // CASE #2: Line comment.
      var lineMatch = CommentDeclRe.Match(line);
      if (lineMatch.Success) {
        lineNum++;
        continue;
      }

      // CASE #3: Key value pair.
      lineMatch = KeyValueLineDeclRe.Match(line);
      if (lineMatch.Success) {
        // May have an in-line existingComment.
        var fieldName = lineMatch.Groups[2].Value;
        var fieldValue = lineMatch.Groups[4].Value;

        // Any bracket in the value is a stop symbol.
        var lineLeftOff = "";
        var brackets = "{}".ToCharArray();
        var stopIndex = fieldValue.IndexOfAny(brackets);
        if (stopIndex != -1) {
          lineLeftOff = fieldValue.Substring(stopIndex);
          fieldValue = fieldValue.Substring(0, stopIndex).TrimEnd();
        }

        node.AddValue(fieldName, fieldValue);

        if (lineLeftOff != "") {
          lines[lineNum] = lineLeftOff;
        } else {
          lineNum++;
        }
        continue;
      }

      // CASE #5: Node, that starts on the same line.
      lineMatch = NodeSameLineDeclRe.Match(line);
      if (lineMatch.Success) {
        // The node declaration starts on the same line. There can be more data in the same line!
        // Everything, which is not a existingComment, is processed as a next line.
        var nodeName = lineMatch.Groups[2].Value;
        var lineLeftOff = lineMatch.Groups[4].Value;
        node = node.AddNode(nodeName);
        nodesStack.Add(node);
        if (lineLeftOff.Length > 0) {
          lines[lineNum] = lineLeftOff;
        } else {
          lineNum++;
        }
        continue;
      }

      // CASE #6: Node, that starts on the next line(s).
      lineMatch = NodeMultiLinePrefixDeclRe.Match(line);
      if (lineMatch.Success) {
        var nodeName = lineMatch.Groups[2].Value;
        var lineLeftOff = lineMatch.Groups[4].Value;
        if (lineLeftOff != "") {
          if (!CommentDeclRe.Match(lineLeftOff).Success) {
            ReportParseError(line, lineNum, "Unexpected non-comment text");
            return null;
          }
        }
        var startLine = lineNum ;

        // Find the opening bracket in the following lines, capturing the possible comments.
        for (lineNum++; lineNum < lines.Length; lineNum++) {
          var skipLine = lines[lineNum];
          if (skipLine.Length == 0) {
            // Empty line before the opening bracket  cannot be preserved.
            DebugEx.Warning("Ignoring empty line before opening bracket at line {0}", lineNum + 1);
            continue;
          }
          var commentMatch = CommentDeclRe.Match(skipLine);
          if (commentMatch.Success) {
            // A comment before the opening bracket cannot be preserved.
            DebugEx.Warning("Ignoring a comment before opening bracket at line {0}", lineNum + 1);
            continue;
          }
          break;  // The open bracket line candidate found.
        }
        var bracketLine = lineNum < lines.Length ? lines[lineNum] : "";
        if (!bracketLine.StartsWith("{", StringComparison.Ordinal)) {
          DebugEx.Warning("Skipping node without body at line {1}: fieldName={0}", nodeName, startLine + 1);
          continue;
        }

        // Unwrap the data after the opening bracket.
        lineLeftOff = bracketLine.Substring(1).Trim();  // Chop off "{"
        if (lineLeftOff.Length > 0) {
          var commentMatch = CommentDeclRe.Match(lineLeftOff);
          if (commentMatch.Success) {
            lineNum++;  // This line is done.
          } else {
            // The left-off comment is a real data. Stay at the line, but resume parsing.
            lines[lineNum] = lineLeftOff;
          }
        } else {
          lineNum++;  // This line is done.
        }

        node = node.AddNode(nodeName);
        nodesStack.Add(node);
      }
    }

    if (nodesStack.Count > 1) {
      DebugEx.Error("Cannot properly parse content");
    }
    return nodesStack[0];
  }

  /// <summary>Recursively collects and serializes the fields in the nodes.</summary>
  static void SerializeNode(StringBuilder res, ConfigNode node, int indentation) {
    var indentSpaces = new string('\t', indentation);

    // Check for an empty block. Write it in a short form.
    if (node.IsEmpty()) {
      res.AppendLine(indentSpaces + node.Name + " {}");
      return;
    }

    res.AppendLine(indentSpaces + node.Name);
    res.AppendLine(indentSpaces + "{");
    indentation++;

    foreach (var valueFieldName in node.GetAllValueFields()) {
      var values = node.GetValues(valueFieldName);
      foreach (var value in values) {
        res.AppendLine(MakeConfigNodeLine(indentation, valueFieldName, value));
      }
    }
    foreach (var fieldNodeName in node.GetAllNodeFields()) {
      foreach (var childNode in node.GetNodes(fieldNodeName)) {
        SerializeNode(res, childNode, indentation);
      }
    }

    res.AppendLine(indentSpaces + "}");
  }

  /// <summary>Formats a config node key/value line.</summary>
  /// <param name="indentation">The indentation in tabs. Each tab is 8 spaces.</param>
  /// <param name="key">The key string.</param>
  /// <param name="value">
  /// The value string. It can contain multiple lines separated by a "\n" symbols.
  /// </param>
  /// <returns>A properly formatted line.</returns>
  static string MakeConfigNodeLine(int indentation, string key, string value) {
    return new string('\t', indentation) + key + " = " + EscapeValue(value);
  }

  /// <summary>reports a verbose error to the logs.</summary>
  static void ReportParseError(string lineContent, int lineNum, string message) {
    DebugEx.Error("Error parsing at line {0}. {1} in:\n{2}", lineNum, message, lineContent);
  }

  /// <summary>Escapes special symbols so that they don't break the formatting.</summary>
  static string EscapeValue(string value) {
    // Turn the leading and the trailing spaces into the unicode codes. Otherwise, they won't load.
    if (value.Length > 0) {
      value = EscapeChar(value[0]) + value.Substring(1);
    }
    if (value.Length > 1) {
      value = value.Substring(0, value.Length - 1) + EscapeChar(value[value.Length - 1]);
    }
    // Also, escape the linefeed character since it breaks the formatting.
    return value.Replace("\n", "\\n").Replace("\t", "\\t");
  }

  /// <summary>Escapes a whitespace character.</summary>
  /// <returns>The unicode encode (<c>\uXXXX</c>) character string, or the character itself.</returns>
  static string EscapeChar(char c) {
    return c is ' ' or '\u00a0' or '\t'
        ? "\\u" + ((int)c).ToString("x4")
        : "" + c;
  }
  #endregion
}

}
