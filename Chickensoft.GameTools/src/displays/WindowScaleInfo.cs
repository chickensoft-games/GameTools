namespace Chickensoft.GameTools.Displays;

using Godot;

/// <summary>
/// Information a <see cref="Window"/> needs to properly support scaling on
/// a specific display. This is dependent on the theme scale, system scale,
/// and other factors that
/// are examined during the process of analyzing the environment.
/// </summary>
/// <param name="Screen">The Godot screen index for the display the window was
/// on at the time this information was determined.</param>
/// <param name="SystemScale">What Godot believe the system scale factor is.
/// This is completely unreliable on macOS, and can break on Windows in
/// multi-monitor, mixed-DPI scenarios. In general, prefer
/// <see cref="DisplayScale"/> if you want to take into account the user's
/// desired display scaling.</param>
/// <param name="WindowScale">A scale factor used in combination with
/// <see cref="CorrectionFactor" /> to determine window size independently
/// of the scale factor. On macOS, this is the retina multiplier which scales
/// all logical resolutions.</param>
/// <param name="DisplayScale">The actual display scale for macOS and Windows,
/// as reported by CoreGraphics on macOS and Win32 API's on Windows. Requires
/// Windows 10+ as we need newer Win32 API's that work around the fact that
/// Godot does not support per-monitor DPI awareness.</param>
/// <param name="ThemeScale">The native resolution height divided by the
/// theme's design resolution height.</param>
/// <param name="ContentScaleFactor">The new content scale factor
/// needed to scale the theme correctly based on the native resolution of the
/// display. Applying this scale factor will result in your application
/// appearing the same size regardless of the display's actual scale
/// factor.</param>
/// <param name="CorrectionFactor">Correction factor to convert from Godot's
/// understanding of the virtual window device-independent pixels (DIPs — which
/// can be completely wrong) to the actual amount needed to appear consistently.
/// </param>
/// <param name="NativeResolution">Native resolution of the monitor on macOS
/// and Windows, as determined by CoreGraphics on macOS and Win32 API's on
/// Windows.</param>
/// <param name="LogicalResolution">The resolution of the window, as determined
/// by Godot. This is often the logical resolution and can be in an unintuitive
/// coordinate space if other monitors are involved or you're on macOS.
/// </param>
/// <param name="ProjectViewportSize">The viewport size as configured in the
/// Godot project. Provided for convenience.</param>
/// <param name="ProjectWindowSize">The window size as configured in the
/// Godot project. Provided for convenience.</param>
public record WindowScaleInfo(
  int Screen,
  float SystemScale,
  float WindowScale,
  float DisplayScale,
  float ThemeScale,
  float ContentScaleFactor,
  float CorrectionFactor,
  Vector2I ProjectViewportSize,
  Vector2I ProjectWindowSize,
  Vector2I NativeResolution,
  Vector2I LogicalResolution,
  Vector2I ScreenPosition
)
{
  /// <inheritdoc />
  public override string ToString() => $"""
  {nameof(Screen)}: {Screen}
  {nameof(SystemScale)}: {SystemScale}
  {nameof(WindowScale)}: {WindowScale}
  {nameof(DisplayScale)}: {DisplayScale}
  {nameof(ThemeScale)}: {ThemeScale}
  {nameof(ContentScaleFactor)}: {ContentScaleFactor}
  {nameof(CorrectionFactor)}: {CorrectionFactor}
  {nameof(ProjectViewportSize)}: {ProjectViewportSize}
  {nameof(ProjectWindowSize)}: {ProjectWindowSize}
  {nameof(NativeResolution)}: {NativeResolution}
  {nameof(LogicalResolution)}: {LogicalResolution}
  {nameof(ScreenPosition)}: {ScreenPosition}
  """;
}
