namespace Chickensoft.GameTools.Displays;

using System;
using Chickensoft.GameTools.Environment;
using Chickensoft.Platform;
using Godot;

/// <summary>
/// A collection of methods for working with displays and high DPI scaling.
/// </summary>
public static class Display
{
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

  /// <summary>
  /// Maximum windowed size as a fraction of a screen dimension.
  /// </summary>
  public const float MAX_WINDOWED_SIZE = 0.75f;
  /// <summary>
  /// Minimum windowed size as a fraction of a screen dimension.
  /// </summary>
  public const float MIN_WINDOWED_SIZE = 0.25f;

  internal delegate float ScreenGetScaleDelegate(int screen);
  internal delegate Vector2I ScreenGetSizeDelegate(int screen);
  internal delegate int ScreenGetDpiDelegate(int screen);
  internal delegate Vector2I ScreenGetPositionDelegate(int screen);

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
  internal static ScreenGetPositionDelegate ScreenGetPositionDefault { get; } =
    DisplayServer.ScreenGetPosition;
  internal static ScreenGetPositionDelegate ScreenGetPosition { get; set; } =
    ScreenGetPositionDefault;

  internal static GetDisplayScaleFactorDelegate
    GetDisplayScaleFactorDefault
  { get; } =
      Displays.Singleton.GetDisplayScaleFactor;
  internal static GetDisplayScaleFactorDelegate
    GetDisplayScaleFactor
  { get; set; } = GetDisplayScaleFactorDefault;

  internal static GetDisplayNativeResolutionDelegate
    GetDisplayNativeResolutionDefault
  { get; } =
      Displays.Singleton.GetNativeResolution;

  internal static GetDisplayNativeResolutionDelegate
    GetDisplayNativeResolution
  { get; set; } =
      GetDisplayNativeResolutionDefault;

  internal static Vector2I ProjectWindowSize
  {
    get
    {
      var windowSize = new Vector2I(
        ProjectSettings.GetSetting(
          "display/window/size/window_width_override"
        ).AsInt32(),
        ProjectSettings.GetSetting(
          "display/window/size/window_height_override"
        ).AsInt32()
      );
      if (windowSize.X == 0 || windowSize.Y == 0)
      {
        var viewportSize = ProjectViewportSize;
        windowSize.X = windowSize.X == 0 ? viewportSize.X : windowSize.X;
        windowSize.Y = windowSize.Y == 0 ? viewportSize.Y : windowSize.Y;
      }
      return windowSize;
    }
  }


  internal static Vector2I ProjectViewportSize => new(
    ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(),
    ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32()
  );

  /// <summary>
  /// Describes the screen index in a human-readable way.
  /// </summary>
  /// <param name="screen">Screen index</param>
  /// <returns>Screen description.</returns>
  public static string Describe(long screen) => screen switch
  {
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
  /// Make a window "look good" by accounting for the scale of the display that
  /// the window is on, guessing good window size constraints, centering the
  /// window, and enabling the selected scaling behavior (fixed UI size or
  /// proportional UI size).
  /// </summary>
  /// <param name="window">Godot window.</param>
  /// <param name="scaleBehavior">High-level scaling strategy to apply.</param>
  /// <param name="themeResolution">The design resolution of your theme.
  /// You typically want a fairly high resolution for your theme that can be
  /// downscaled on most monitors (4K is usually plenty).</param>
  /// <param name="scaleInfo">Window scale info. If null, will be automatically
  /// determined on macOS and Windows via native platform methods (default).
  /// Generally, you would not supply this unless you are doing something
  /// highly custom.
  /// </param>
  /// <param name="sizeInfo">Window size info. If null, will be automatically
  /// computed by guessing the best size and size range for the window.
  /// </param>
  /// <param name="useProjectAspectRatio">Whether or not the aspect ratio
  /// of the window size set in the project settings should be used when
  /// selecting a window size. Default is true. Set to false if your game
  /// does not require a fixed aspect ratio.</param>
  /// <param name="isFullscreen">Whether or not the window should be fullscreen.
  /// You can pass in true/false to toggle fullscreen mode as needed.</param>
  /// <param name="useExclusiveFullscreen">True to use exclusive fullscreen
  /// (more performant). Default is true.</param>
  /// <param name="minWindowedSize">The minimum windowed size as a ratio of the
  /// minimum screen dimension between 0 and 1. The default is
  /// <see cref="MIN_WINDOWED_SIZE"/>.</param>
  /// <param name="maxWindowedSize">The maximum windowed size as a ratio of the
  /// maximum screen dimension between 0 and 1. The default is
  /// <see cref="MAX_WINDOWED_SIZE"/>.</param>
  /// <returns>Window scale and sizing information.</returns>
  public static WindowInfo LookGood(
    this Window window,
    WindowScaleBehavior scaleBehavior,
    Vector2I themeResolution,
    WindowScaleInfo? scaleInfo = null,
    WindowSizeInfo? sizeInfo = null,
    bool useProjectAspectRatio = true,
    bool isFullscreen = false,
    bool useExclusiveFullscreen = true,
    float minWindowedSize = MIN_WINDOWED_SIZE,
    float maxWindowedSize = MAX_WINDOWED_SIZE
  )
  {
    var screen = window.CurrentScreen;

    scaleInfo ??= window.GetWindowScaleInfo(themeResolution, isFullscreen);
    sizeInfo ??= GetWindowSizeInfo(
      scaleInfo.LogicalResolution,
      useProjectAspectRatio,
      minWindowedSize,
      maxWindowedSize
    );

    var fs = useExclusiveFullscreen
      ? Window.ModeEnum.ExclusiveFullscreen
      : Window.ModeEnum.Fullscreen;

    var targetMode = isFullscreen ? fs : Window.ModeEnum.Windowed;

    switch (scaleBehavior)
    {
      case WindowScaleBehavior.UIFixed:
        window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;

        break;

      default:
      case WindowScaleBehavior.UIProportional:
        if (isFullscreen)
        {
          window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
          window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
        }
        else
        {
          // We calculate the content scale size using the logical resolution
          // and the project window size aspect ratio.
          // The content scale size will be the same aspect ratio as the project window size,
          // regardless of the logical resolution's aspect ratio.
          window.ContentScaleSize = new Vector2I(
              Mathf.FloorToInt(scaleInfo.LogicalResolution.Y * ProjectWindowSize.Aspect()),
              scaleInfo.LogicalResolution.Y
            );
          window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
          window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
        }

        break;
    }

    window.ContentScaleFactor = scaleInfo.ContentScaleFactor;
    window.Size = sizeInfo.Size;
    window.MinSize = sizeInfo.MinSize;
    window.MaxSize = sizeInfo.MaxSize;

    // TODO(jolexxa): wayland screen position?
    // setting window screen and position won't work on Wayland, and
    // is a known issue that Godot can't even support as of 4.5.1:
    // https://github.com/godotengine/godot/blob/e47fb8b8989fd5589c65c4b0ac980de2e936c041/platform/linuxbsd/wayland/display_server_wayland.cpp#L1088-L1090
    //
    // On a wayland multi-monitor setup today, the window is adjusted for the
    // primary display and shown there -- not so bad. At least on my pc
    window.CurrentScreen = screen;

    window.Position = ((scaleInfo.LogicalResolution - window.Size) / 2) +
      scaleInfo.ScreenPosition;

    // Required since the window sometimes ends up on another screen, possibly
    // due to virtual window coordinates.
    window.CurrentScreen = screen;

    window.Mode = targetMode;

    return new(scaleBehavior, scaleInfo, sizeInfo);
  }

  /// <summary>
  /// Get the window size and minimum and maximum size constraints based on the
  /// project window size and actual screen size.
  /// </summary>
  /// <param name="screenSize">Godot's understanding of the screen size.</param>
  /// <param name="useProjectAspectRatio">True to use the aspect ratio defined
  /// by the project window size setting.</param>
  /// <param name="minWindowedSize">The minimum windowed size as a ratio of the
  /// minimum screen dimension between 0 and 1. The default is
  /// <see cref="MIN_WINDOWED_SIZE"/>.</param>
  /// <param name="maxWindowedSize">The maximum windowed size as a ratio of the
  /// maximum screen dimension between 0 and 1. The default is
  /// <see cref="MAX_WINDOWED_SIZE"/>.</param>
  /// <returns></returns>
  public static WindowSizeInfo GetWindowSizeInfo(
    Vector2I screenSize,
    bool useProjectAspectRatio = true,
    float minWindowedSize = MIN_WINDOWED_SIZE,
    float maxWindowedSize = MAX_WINDOWED_SIZE
  )
  {
    Vector2 screen = screenSize;
    // Figure out the minimum and maximum area of the screen we can use.
    var minSize = (screen * minWindowedSize).Round();
    var maxSize = (screen * maxWindowedSize).Round();

    var windowSize = maxSize;
    var windowMinSize = minSize;
    var windowMaxSize = maxSize;

    if (useProjectAspectRatio)
    {
      // Clamp screen down to the largest size that shares the same aspect
      // ratio as the project window size.
      var aspect = ProjectWindowSize.Aspect();
      windowSize = windowSize.Constrain(aspect, minSize, maxSize);
      windowMinSize = windowMinSize.Constrain(aspect, minSize, maxSize);
      windowMaxSize = windowMaxSize.Constrain(aspect, minSize, maxSize);
    }

    return new(
      (Vector2I)windowSize.Round(),
      (Vector2I)windowMinSize.Round(),
      (Vector2I)windowMaxSize.Round()
    );
  }

  /// <summary>
  /// Constrain a size to a given aspect ratio while keeping it inside the
  /// arbitrary min and max sizes (which may have different aspect ratios).
  /// </summary>
  /// <param name="size">Vector to constrain.</param>
  /// <param name="aspect">Aspect ratio to use.</param>
  /// <param name="minSize">Min size.</param>
  /// <param name="maxSize">Max size.</param>
  /// <returns></returns>
  public static Vector2 Constrain(
      this Vector2 size, float aspect, Vector2 minSize, Vector2 maxSize
  )
  {
    // First pass: try to match width
    var width = Mathf.Clamp(size.X, minSize.X, maxSize.X);
    var height = width / aspect;

    // If that height is out of range, clamp it and recompute width
    if (height < minSize.Y)
    {
      height = minSize.Y;
      width = (int)(height * aspect);
    }
    else if (height > maxSize.Y)
    {
      height = maxSize.Y;
      width = (int)(height * aspect);
    }

    // Clamp and resize again to try and avoid overflow in the other dimension.
    width = Mathf.Clamp(width, minSize.X, maxSize.X);

    return new(width, width / aspect);
  }

  /// <summary>
  /// Work around limitations of Godot's display scaling and determine the
  /// actual scale factor of the display on Windows and macOS.
  /// </summary>
  /// <param name="window">Godot window.</param>
  /// <param name="themeResolution">Theme asset design resolution.</param>
  /// <param name="isFullscreen">True if the game is intended to be
  /// full screen.</param>
  /// <returns>Window scale information.</returns>
  public static WindowScaleInfo GetWindowScaleInfo(
    this Window window, Vector2I themeResolution, bool isFullscreen = false
  )
  {
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
    // Use Win32, CoreGraphics, or linux drm to determine the display's actual
    // scale factor.
    var displayScale = GetDisplayScaleFactor(window);
    // Godot reports Windows' system scale factor, which may be different from
    // the monitor's scale factor since Godot does not opt-in to per-monitor DPI
    // awareness on Windows.
    var systemDpi = ScreenGetDpi(window.CurrentScreen);
    // Windows scale factor can be determined by dividing by 96.
    var systemScale = systemDpi / 96.0f;
    // Get the size of the window from Godot, which is going to be in the
    // logical resolution coordinate space.`
    var godotResolution = ScreenGetSize(window.CurrentScreen);
    var godotScale = ScreenGetScale(screen);
    // We need a ratio to correct from system scale to actual monitor scale
    var correctionFactor = 1f / Features.OperatingSystem switch
    {
      // on macOS and linux, we can simply use the ratio between the native
      // resolution and Godot's reported resolution to determine how much
      // Godot is off by (since Godot is reporting logical resolution).
      OSFamily.macOS => (float)nativeResolution.Y / godotResolution.Y,
      OSFamily.Linux => (float)nativeResolution.Y / godotResolution.Y,
      // windows is much more special -- Godot is not per-monitor DPI aware yet.
      OSFamily.Windows => displayScale / systemScale,

      // TODO(jolexxa): Support iOS / Android (and FreeBSD someday?)
      // don't make promises we can't keep -- users should implement this
      // themselves on these platforms and hopefully contribute back
      // to this and Chickensoft.Platform (if needed).
      OSFamily.FreeBSD => throw new NotSupportedException(
        "[Chickensoft.GameTools]: Display scaling for FreeBSD is not yet " +
        "supported."
      ),
      OSFamily.iOS => throw new NotSupportedException(
        "[Chickensoft.GameTools]: Display scaling for iOS is not yet supported."
      ),
      OSFamily.Android => throw new NotSupportedException(
        "[Chickensoft.GameTools]: Display scaling for Android is not yet " +
        "supported."
      ),
      _ => throw new NotSupportedException(
        "[Chickensoft.GameTools]: Display scaling can't detect the OS."
      )
    };

    var themeScale = (float)nativeResolution.Y / themeResolution.Y;

    // This is the retina multiplier on macOS since the macos logical
    // backbuffer coordinate space is multiplied by this factor.
    // TODO(jolexxa): This may need consideration on iOS too.
    //
    // For all the normal os's, this is simply the theme's scale factor.
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

    // This is the position of the screen in the virtual desktop with all of
    // the monitors considered.
    //
    // We use this to center the window on the screen, by offsetting the window
    // position by the screen position.
    var screenPosition = ScreenGetPosition(screen);

    return new WindowScaleInfo(
      Screen: screen,
      SystemScale: systemScale,
      WindowScale: windowScale,
      DisplayScale: displayScale,
      ThemeScale: themeScale,
      ContentScaleFactor: contentScaleFactor,
      CorrectionFactor: correctionFactor,
      ProjectViewportSize: ProjectViewportSize,
      ProjectWindowSize: ProjectWindowSize,
      NativeResolution: nativeResolution,
      LogicalResolution: godotResolution,
      ScreenPosition: screenPosition
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
  public static float GetAutoDisplayScale(int screen = -1)
  {
    // Ported over from Godot's EditorSettings::get_auto_display_scale() method.
    // See editor/editor_settings.cpp in the Godot source code.
    // This uses various heuristics to "guess" the display scale the user might
    // want the application to use automatically.

    if (Features.OperatingSystem is OSFamily.macOS or OSFamily.Android)
    {
      return ScreenGetScale(screen);
    }

    var screenSize = ScreenGetSize(screen);

    if (screenSize == Vector2I.Zero)
    {
      // Invalid screen size, skip.
      return 1.0f;
    }

    // Use the smallest dimension to use a correct display scale on portrait
    // displays.
    var smallestDimension = Mathf.Min(screenSize.X, screenSize.Y);

    if (
      ScreenGetDpi(screen) >= 192 &&
      smallestDimension >= 1400
    )
    {
      // hiDPI display.
      return 2.0f;
    }
    else if (smallestDimension >= 1700)
    {
      // Likely a hiDPI display, but we aren't certain due to the returned DPI.
      // Use an intermediate scale to handle this situation.
      return 1.5f;
    }

    return 1f;
  }
}
