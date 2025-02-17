namespace Chickensoft.GameTools.Displays;

using Chickensoft.GameTools.Environment;
using Chickensoft.Platform;
using Godot;

/// <summary>
/// A collection of methods for working with displays and high DPI scaling.
/// </summary>
public static class Display {
  internal delegate float ScreenGetScaleDelegate(int screen);
  internal delegate Vector2I ScreenGetSizeDelegate(int screen);
  internal delegate int ScreenGetDpiDelegate(int screen);

  internal delegate float GetDisplayScaleFactorDelegate(Window window);
  internal delegate Vector2I GetDisplayNativeResolutionDelegate(Window window);

  internal static ScreenGetScaleDelegate ScreenGetScaleDefault { get; } =
    DisplayServer.ScreenGetScale;
  internal static ScreenGetScaleDelegate ScreenGetScale { get; set; } =
    ScreenGetScaleDefault;
  internal static ScreenGetSizeDelegate ScreenGetSizeDefault { get; } =
    DisplayServer.ScreenGetSize;
  internal static ScreenGetSizeDelegate ScreenGetSize { get; set; } =
    ScreenGetSizeDefault;
  internal static ScreenGetDpiDelegate ScreenGetDpiDefault { get; } =
    DisplayServer.ScreenGetDpi;
  internal static ScreenGetDpiDelegate ScreenGetDpi { get; set; } =
    ScreenGetDpiDefault;

  internal static GetDisplayScaleFactorDelegate
    GetDisplayScaleFactorDefault { get; } =
      Displays.Singleton.GetDisplayScaleFactor;
  internal static GetDisplayScaleFactorDelegate GetDisplayScaleFactor { get; set; } =
    GetDisplayScaleFactorDefault;

  internal static GetDisplayNativeResolutionDelegate
    GetDisplayNativeResolutionDefault { get; } =
      Displays.Singleton.GetNativeResolution;

  internal static GetDisplayNativeResolutionDelegate
    GetDisplayNativeResolution { get; set; } =
      GetDisplayNativeResolutionDefault;

  internal static Vector2I ProjectWindowSize => new(
    ProjectSettings.GetSetting(
      "display/window/size/window_width_override"
    ).AsInt32(),
    ProjectSettings.GetSetting(
      "display/window/size/window_height_override"
    ).AsInt32()
  );

  internal static Vector2I ProjectViewportSize => new(
    ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(),
    ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32()
  );

  /// <summary>
  /// Describes the screen index in a human-readable way.
  /// </summary>
  /// <param name="screen">Screen index</param>
  /// <returns>Screen description.</returns>
  public static string Describe(long screen) => screen switch {
    DisplayServer.ScreenOfMainWindow => "[Screen of Main Window]",
    DisplayServer.ScreenPrimary => "[Primary Screen]",
    DisplayServer.ScreenWithKeyboardFocus => "[Screen with Keyboard Focus]",
    DisplayServer.ScreenWithMouseFocus => "[Screen with Mouse Focus]",
    _ => $"[Screen {screen}]"
  };

  /// <summary>
  /// Describes the screen index in a human-readable way.
  /// </summary>
  /// <param name="screen">Screen index</param>
  /// <returns>Screen description.</returns>
  public static string Describe(int screen) =>
    Describe((long)screen);

  public static WindowScaleInfo LookGood(
    this Window window,
    Vector2I themeResolution,
    WindowScaleInfo? info = null
  ) {
    info ??= window.GetWindowScaleInfo(themeResolution, isFullscreen: false);
    ApplyScaling(window, info);

    return info;
  }

  public static WindowScaleInfo LookGoodFullscreen(
    this Window window,
    Vector2I themeResolution,
    WindowScaleInfo? info = null
  ) {
    info ??= window.GetWindowScaleInfo(themeResolution, isFullscreen: true);
    ApplyScaling(window, info);

    return info;
  }

  /// <summary>
  /// Work around limitations of Godot's display scaling and determine the
  /// actual scale factor of the display on Windows and macOS.
  /// </summary>
  /// <param name="window">Godot window.</param>
  /// <param name="themeResolution">Theme asset design resolution.</param>
  /// <param name="isFullscreen">True if the game is intended to be
  /// full screen.</param>
  /// <returns></returns>
  public static WindowScaleInfo GetWindowScaleInfo(
    this Window window, Vector2I themeResolution, bool isFullscreen = false
  ) {
    // The native resolution (true resolution of the monitor) and Godot's
    // understanding of the monitor resolution on Windows can be different,
    // since Godot does not have per-monitor DPI awareness on Windows (yet).
    // https://github.com/godotengine/godot/issues/56341
    //
    // On macOS, Godot is not able to get detailed scaling information
    // easily from the OS. We work around this by poking around CoreGraphics.

    var screen = window.CurrentScreen;

    // Use Win32 or CoreGraphics to get the display's actual resolution.
    var nativeResolution = GetDisplayNativeResolution(window);
    // Use Win32 or CoreGraphics to determine the display's actual scale factor.
    var displayScale = GetDisplayScaleFactor(window);
    // Godot reports Windows' system scale factor, which may be different from
    // the monitor's scale factor since Godot does not opt-in to per-monitor DPI
    // awareness on Windows.
    var systemDpi = DisplayServer.ScreenGetDpi(window.CurrentScreen);
    // Windows scale factor can be determined by dividing by 96.
    var systemScale = systemDpi / 96.0f;
    // Get the size of the window from Godot, which is going to be in the
    // system scale factor coordinate space.
    var godotRes = DisplayServer.ScreenGetSize(window.CurrentScreen);
    var windowSize = window.Size;
    // We need a ratio to correct from system scale to actual monitor scale
    var correctionFactor = Features.OperatingSystem == OSFamily.macOS
      ? (float)nativeResolution.Y / godotRes.Y // macos
      : displayScale / systemScale; // windows & linux

    var themeScale = (float)nativeResolution.Y / themeResolution.Y;

    // This content scale factor accounts for the actual monitor scaling and
    // godot's scaling so that the UI takes up roughly the same amount of
    // physical pixels regardless of the scaling chosen for the monitor. For
    // games, this is typically desired as the UI should be designed for a
    // sufficiently large resolution (like 4k) and then scaled to fit the
    // monitor's actual resolution. Users can offer scaling options in their
    // game and multiply this factor by the scaling option to get the final
    // scale, but this at least gives them a common frame of reference.
    var contentScaleFactor = themeScale / correctionFactor;

    var newWindowSize = new Vector2I(
      (int)(windowSize.X / correctionFactor),
      (int)(windowSize.Y / correctionFactor)
    );

    var newWindowPosition = (godotRes - newWindowSize) / 2;

    return new WindowScaleInfo(
      SystemScale: systemScale,
      DisplayScale: displayScale,
      ContentScaleFactor: contentScaleFactor,
      CorrectionFactor: correctionFactor,
      ProjectViewportSize: ProjectViewportSize,
      ProjectWindowSize: ProjectWindowSize,
      NativeResolution: nativeResolution,
      WindowSize: newWindowSize,
      WindowPosition: newWindowPosition
    );
  }

  /// <summary>
  /// <para>
  /// Guess the display scale the user might want the application to use based
  /// on a few heuristics involving the maximum known display scale,
  /// screen size, and screen DPI.
  /// </para>
  /// <para>
  /// This closely resembles the Godot Engine's
  /// <c>EditorSettings::get_auto_display_scale()</c> method. For the best
  /// experience, provide a setting in your application that allows the user to
  /// override the display scale.
  /// </para>
  /// </summary>
  /// <param name="screen">The screen to estimate the scale for. The default is
  /// the current screen (-1).</param>
  /// <returns>The best-guess scale factor, typically in the range of 1.0 to
  /// 2.0.</returns>
  public static float GetAutoDisplayScale(int screen = -1) {
    // Ported over from Godot's EditorSettings::get_auto_display_scale() method.
    // See editor/editor_settings.cpp in the Godot source code.
    // This uses various heuristics to "guess" the display scale the user might
    // want the application to use automatically.

    if (Features.OperatingSystem is OSFamily.macOS or OSFamily.Android) {
      return ScreenGetScale(screen);
    }

    var screenSize = ScreenGetSize(screen);

    if (screenSize == Vector2I.Zero) {
      // Invalid screen size, skip.
      return 1.0f;
    }

    // Use the smallest dimension to use a correct display scale on portrait
    // displays.
    var smallestDimension = Mathf.Min(screenSize.X, screenSize.Y);

    if (
      ScreenGetDpi(screen) >= 192 &&
      smallestDimension >= 1400
    ) {
      // hiDPI display.
      return 2.0f;
    }
    else if (smallestDimension >= 1700) {
      // Likely a hiDPI display, but we aren't certain due to the returned DPI.
      // Use an intermediate scale to handle this situation.
      return 1.5f;
    }

    return 1f;
  }

  private static void ApplyScaling(Window window, WindowScaleInfo info) {
    window.ContentScaleFactor = info.ContentScaleFactor;
    window.Size = info.WindowSize;
    window.Position = info.WindowPosition;
  }
}
