// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using UnityEngine;

namespace UnityDev.Utils.GUIUtils {

/// <summary>A utility class to render a big disabled block in GUI.</summary>
public class GuiEnabledStateScope : IDisposable {
  readonly bool _oldState;

  /// <summary>Stores the old state and sets a new one.</summary>
  /// <param name="newState">The new state to set.</param>
  public GuiEnabledStateScope(bool newState) {
    _oldState = GUI.enabled;
    GUI.enabled = newState;
  }

  /// <summary>Restores the enabled state that was set before the scope started.</summary>
  public void Dispose() {
    GUI.enabled = _oldState;
  }
}

}  // namespace
