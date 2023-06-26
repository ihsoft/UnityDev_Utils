// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using UnityDev.Utils.Extensions;
using UnityDev.LogUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityDev.Utils.GUIUtils {

/// <summary>A utility class to deal with the GUI windows.</summary>
/// <remarks>
/// The stock window cancels dragging when the window layout is changed. It makes it useless when dealing with windows
/// that can change their layout depending on the position. This method doesn't have this drawback. Moreover, it can
/// tell if the window is being dragged, so that the code could postpone the layout update until the dragging is over. 
/// </remarks>
public static class GuiWindow {
  /// <summary>Latest mouse position to which the dragged window position has updated.</summary>
  /// <remarks>When this field is <c>null</c>, it means no window is being dragged.</remarks>
  static Vector2? _dragPosition;

  /// <summary>
  /// Makes the window movable. It's an improved version of the stock <c>GUI.DragWindow()</c>
  /// method.
  /// </summary>
  /// <remarks>
  /// The main difference from the stock method is that the dragging state is not reset by the GUI layout methods. Also,
  /// it reports the dragging state, so some updates to the dialog may be frozen to not interfere with the move
  /// operation.
  /// </remarks>
  /// <param name="windowRect">
  /// The window rectangle. It must be the same instance which is passed to the <c>GUILayout.Window</c> method.
  /// </param>
  /// <param name="dragArea">
  /// The rectangle in the local windows's space that defines the dragging area. In case of it's out of bounds of the
  /// window rectangle, it will be clipped.
  /// </param>
  /// <returns><c>true</c> if the window is being dragged.</returns>
  public static bool DragWindow(ref Rect windowRect, Rect dragArea) {
    var mousePosition = GetMousePos();
    var screenPosition = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
    if (_dragPosition.HasValue) {
      windowRect.position += screenPosition - _dragPosition.Value;
      _dragPosition = screenPosition;
    }
    var mouseEvent = Event.current;
    if (mouseEvent.isMouse && mouseEvent.button == 0) {
      if (mouseEvent.type == EventType.MouseDown) {
        dragArea.position += windowRect.position;
        dragArea = dragArea.Intersect(windowRect);
        if (dragArea.Contains(screenPosition)) {
          _dragPosition = screenPosition;
        }
      } else if (mouseEvent.type == EventType.MouseUp) {
        if (_dragPosition.HasValue) {
          _dragPosition = null;
        }
      }
    }
    return _dragPosition.HasValue;
  }

  /// <summary>Gets mouse position.</summary>
  /// <remarks>Detects if both old and new input systems installed into the system and avoids collision.</remarks>
  // ReSharper disable once MemberCanBePrivate.Global
  public static Vector2 GetMousePos() {
    if (!_inputSystemChecked) {
      try {
        var unused = Input.mousePosition;  // This will throw if the new input system is enabled.
        _useOldSystem = true;
      } catch (InvalidOperationException) {
        _useOldSystem = false;
        DebugEx.Info("Switching UnityDev Utils to the new input system");
      }
      _inputSystemChecked = true;
    }
    return _useOldSystem ? Input.mousePosition : Mouse.current.position.ReadValue();
  }
  static bool _inputSystemChecked;
  static bool _useOldSystem = true;
}

}  // namespace
