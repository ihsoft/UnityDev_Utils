// Unity Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityDev.Utils.Configs;
using UnityDev.Utils.FSUtils;

namespace UnityDev.LogUtils {

/// <summary>Logging settings.</summary>
/// <remarks>
/// The settings are not loaded pro-actively. The config is supposed to be loaded when it's first time needed.
/// </remarks>
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class LoggingSettings {

  #region Settings group
  public class SettingsGroup : PersistentNode {
    public int verbosityLevel = 0;
  }

  static void LoadSettings() {
    _settings = new SettingsGroup();
    // ReSharper disable once StringLiteralTypo
    var configPath = ModPaths.MakeAbsPathForPlugin(typeof(LoggingSettings), "UnityDev_logsettings.cfg");
    if (File.Exists(configPath)) {
      //var config = ConfigNode.ParseFileAsNode(configPath);
      var config = SimpleTextSerializer.LoadFromFile(configPath);
      if (config != null) {
        _settings.LoadFromConfigNode(config);
        DebugEx.Fine("Loaded UnityDev settings. Logs verbosity: {0}", _settings.verbosityLevel);
      }
    }
  }
  #endregion

  /// <summary>Settings group.</summary>
  //public static SettingsGroup Settings => _settings ??= new SettingsGroup();
  public static SettingsGroup Settings {
    get {
      if (_settings == null) {
        LoadSettings();
      }
      return _settings;
    }
  }
  static SettingsGroup _settings;

  /// <summary>Level above 0 enables <see cref="DebugEx.Fine"/> logs.</summary>
  public static int VerbosityLevel => Settings.verbosityLevel;
}
}
