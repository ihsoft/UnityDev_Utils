// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using UnityEngine;

namespace UnityDev.Utils.GUIUtils {

/// <summary>A utility class to render big disabled blocks of GUI.</summary>
public class GuiColorScope : IDisposable {
  readonly Color _oldColor;
  readonly Color _oldContentColor;
  readonly Color _oldBackgroundColor;

  /// <summary>Stores the old state and sets a new one.</summary>
  /// <param name="color">The new color for <c>GUI.color</c>.</param>
  /// <param name="contentColor">The new color for <c>GUI.contentColor</c>.</param>
  /// <param name="backgroundColor">The new color for <c>GUI.backgroundColor</c>.</param>
  public GuiColorScope(Color? color = null, Color? contentColor = null, Color? backgroundColor = null) {
    _oldColor = GUI.color;
    if (color.HasValue) {
      GUI.color = color.Value;
    }
    _oldContentColor = GUI.contentColor;
    if (contentColor.HasValue) {
      GUI.contentColor = contentColor.Value;
    }
    _oldBackgroundColor = GUI.backgroundColor;
    if (backgroundColor.HasValue) {
      GUI.backgroundColor = backgroundColor.Value;
    }
  }

  /// <summary>Restores the colors that were set before the scope started.</summary>
  public void Dispose() {
    GUI.color = _oldColor;
    GUI.contentColor = _oldContentColor;
    GUI.backgroundColor = _oldBackgroundColor;
  }
}

}  // namespace
