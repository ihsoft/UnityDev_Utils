﻿// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Collections.Generic;
using UnityDev.LogUtils;
using UnityEngine;

namespace UnityDev.Utils.GUIUtils {

/// <summary>A helper to accumulate GUI actions.</summary>
/// <remarks>
/// Unity may issue multiple GUI passes during a frame, and it requires the number of UI elements not to change between
/// the passes. Unity expects the number of UI controls in every pass to be exactly the same as in the very first one:
/// <see href="http://docs.unity3d.com/ScriptReference/EventType.Layout.html">EventType.Layout</see>. When the UI
/// interactions affect the representation, all the changes must be postponed till the frame rendering is ended. This
/// helper can be used to store the actions that will be executed at the beginning of the next frame.
/// </remarks>
/// <example>
/// <code>
/// public class MyUI : MonoBehaviour {
///   private readonly GuiActionsList guiActions = new GuiActionsList();
///   private bool showLabel = false;
///
///   void OnGUI() {
///     if (guiActions.ExecutePendingGuiActions()) {
///       // ...do other stuff that affects UI... 
///     }
///
///     if (GUILayout.Button(new GUIContent("Test Button"))) {
///       // If "showLabel" is changed right here then Unity GUI will complain saying the number
///       // of UI controls has changed. So, postpone the change until current frame is ended.
///       guiActions.Add(() => {
///         showLabel = !showLabel;  // This will be done at the beginning of the next frame.
///       });
///     }
///     
///     if (showLabel) {
///       GUILayout.Label("Test label");
///     }
///   }
/// }
/// </code>
/// <p>
/// If you were using simple approach and updated <c>showLabel</c> right away Unity would likely thrown an error like this:
/// </p>
/// <p>
/// <c>[EXCEPTION] ArgumentException: Getting control 1's position in a group with only 1
/// controls when doing Repaint</c>
/// </p>
/// </example>
public class GuiActionsList {
  /// <summary>A list of pending actions.</summary>
  readonly List<Action> _guiActions = new List<Action>();

  /// <summary>Adds an action to the pending list.</summary>
  /// <param name="actionFn">An action callback.</param>
  public void Add(Action actionFn) {
    // In the layout phase the controls are not supposed to trigger anything.
    if (Event.current.type != EventType.Layout) {
      _guiActions.Add(actionFn);
    }
  }

  /// <summary>Executes actions when it's safe to do the changes.</summary>
  /// <remarks>
  /// It's safe to call this method in every pass. It will detect when it's safe to apply the changes and apply the
  /// changes only once per a frame.
  /// </remarks>
  /// <returns><c>true</c> if actions have been applied.</returns>
  public bool ExecutePendingGuiActions() {
    if (Event.current.type == EventType.Layout) {
      foreach (var actionFn in _guiActions) {
        try {
          actionFn();
        } catch (Exception ex) {
          DebugEx.Error("GUI action failed: {0}", ex);
        }
      }
      _guiActions.Clear();
      return true;
    }
    return false;
  }
}

}  // namespace
