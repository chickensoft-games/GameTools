namespace Chickensoft.GameTools.Displays;

using Chickensoft.GameTools.Environment;
using Chickensoft.Platform;
using Godot;

/// <summary>
/// A collection of methods for working with displays and high DPI scaling.
/// </summary>
public static class Display {
  /// <summary>
  /// 1280x720 resolution — The original "HD" resolution, but not what people
  /// usually think of as HD these days. See
  /// https://en.wikipedia.org/wiki/720p
  /// </summary>
  public static Vector2I HD => new(1280, 720);
  /// <summary>
  /// 1920x1080 resolution, or "Full HD". This is the "HD" you're probably
  /// thinking of. See https://en.wikipedia.org/wiki/1080p
  /// </summary>
  public static Vector2I FullHD => new(1920, 1080);
  /// <summary>
  /// 2560x1440 resolution, or "Quad HD" since it is 4 x 720p See
  /// https://en.wikipedia.org/wiki/1440p
  /// </summary>
  public static Vector2I QHD => new(2560, 1440);
  /// <summary>
  /// 3840x2160 resolution, or "Ultra HD". Also commonly known as 4K. See
  /// https://en.wikipedia.org/wiki/2160p
  /// </summary>
  public static Vector2I UHD4k => new(3840, 2160);
  /// <summary>
  /// 5120x2880 resolution, or "Ultra HD 5K". This is the resolution of Apple's
  /// Retina 5K display. See https://en.wikipedia.org/wiki/5K_resolution
  /// </summary>
  public static Vector2I UHD5k => new(5120, 2880);
  /// <summary>
  /// 7680x4320 resolution, or "Ultra HD 8K". See
  /// https://en.wikipedia.org/wiki/8K_resolution
  /// </summary>
  public static Vector2I UHD8k => new(7680, 4320);

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

  /// <summary>
  /// Make the game window look good, regardless of the user's display settings.
  /// </summary>
  /// <param name="window">Godot window.</param>
  /// <param name="themeResolution">Theme design resolution.</param>
  /// <param name="info">Window scaling info. Omit if you'd like this to
  /// method to determine it for you (default).</param>
  /// <returns>Window scale info.</returns>
  public static WindowScaleInfo LookGood(
    this Window window,
    Vector2I themeResolution,
    WindowScaleInfo? info = null
  ) {
    info ??= window.GetWindowScaleInfo(themeResolution, isFullscreen: false);
    window.ApplyScaling(info, true);

    return info;
  }

  /// <summary>
  /// Make the game window look good fullscreen, regardless of the user's
  /// display settings.
  /// </summary>
  /// <param name="window">Godot window.</param>
  /// <param name="themeResolution">Theme design resolution.</param>
  /// <param name="useExclusiveFullScreen">True if the game should use
  /// exclusive fullscreen. Exclusive fullscreen is generally
  /// more performant. Default is true.</param>
  /// <param name="info">Window scaling info. Omit if you'd like this to
  /// method to determine it for you (default).</param>
  /// <returns>Window scale info.</returns>
  public static WindowScaleInfo LookGoodFullscreen(
    this Window window,
    Vector2I themeResolution,
    bool useExclusiveFullScreen = true,
    WindowScaleInfo? info = null
  ) {
    info ??= window.GetWindowScaleInfo(themeResolution, isFullscreen: true);
    window.ApplyScaling(info, true);

    var windowId = window.GetWindowId();
    var mode = DisplayServer.WindowGetMode(windowId);

    if (
      mode is
        DisplayServer.WindowMode.Fullscreen or
        DisplayServer.WindowMode.ExclusiveFullscreen
    ) {
      return info;
    }

    DisplayServer.WindowSetMode(
      useExclusiveFullScreen
        ? DisplayServer.WindowMode.ExclusiveFullscreen
        : DisplayServer.WindowMode.Fullscreen,
      windowId
    );

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
    var godotScale = ScreenGetScale(screen);
    var windowSize = window.Size;
    // We need a ratio to correct from system scale to actual monitor scale
    var correctionFactor = 1f / (
      Features.OperatingSystem == OSFamily.macOS
        ? (float)nativeResolution.Y / godotRes.Y // macos
        : displayScale / systemScale             // windows & linux
    );

    var themeScale = (float)nativeResolution.Y / themeResolution.Y;

    // This is the retina multiplier on macOS since the macos logical
    // backbuffer coordinate space is multiplied by this factor.
    //
    // On windows, this is simply the theme's scale factor.
    var windowScale = Features.OperatingSystem == OSFamily.macOS
      ? godotScale
      : themeScale;

    // This content scale factor accounts for the actual monitor scaling and
    // godot's scaling so that the UI takes up roughly the same amount of
    // physical pixels regardless of the scaling chosen for the monitor. For
    // games, this is typically desired as the UI should be designed for a
    // sufficiently large resolution (like 4k) and then scaled to fit the
    // monitor's actual resolution. Users can offer scaling options in their
    // game and multiply this factor by the scaling option to get the final
    // scale, but this at least gives them a common frame of reference.
    var contentScaleFactor = themeScale * correctionFactor;

    // Window frame positions can be translated from theme design coordinates
    // independent of the scale factor by multiplying by the correctionFactor
    // and windowScale. Project window size should always be specified in terms
    // of the theme design size.
    var newWindowSize = new Vector2I(
      (int)(ProjectWindowSize.X * correctionFactor * windowScale),
      (int)(ProjectWindowSize.Y * correctionFactor * windowScale)
    );

    return new WindowScaleInfo(
      SystemScale: systemScale,
      RetinaScale: windowScale,
      DisplayScale: displayScale,
      ThemeScale: themeScale,
      ContentScaleFactor: contentScaleFactor,
      CorrectionFactor: correctionFactor,
      ProjectViewportSize: ProjectViewportSize,
      ProjectWindowSize: ProjectWindowSize,
      NativeResolution: nativeResolution,
      WindowSize: newWindowSize
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

  private static void ApplyScaling(
    this Window window, WindowScaleInfo info, bool shouldCenter
  ) {
    var screen = window.CurrentScreen;
    window.ContentScaleFactor = info.ContentScaleFactor;
    window.Size = info.WindowSize;
    window.CurrentScreen = screen;

    if (shouldCenter) {
      window.Position = (ScreenGetSize(screen) - info.WindowSize) / 2;
    }

    // Required since the window sometimes ends up on another screen, possibly
    // due to virtual window coordinates.
    window.CurrentScreen = screen;
  }
}
