namespace Chickensoft.GameTools.Displays;

using Godot;

/// <summary>
/// Information needed for a <see cref="Window"/> to properly support high DPI.
/// This is dependent on the theme scale, system scale, and other factors that
/// are examined during the process of analyzing the environment.
/// </summary>
/// <param name="SystemScale">What Godot believe the system scale factor is.
/// This is completely unreliable on macOS, and can break on Windows in
/// multi-monitor, mixed-DPI scenarios. In general, prefer
/// <see cref="DisplayScale"/> if you want to take into account the user's
/// desired display scaling.</param>
/// <param name="RetinaScale">The retina scale factor for macOS (2 or 3).
/// On other platforms, this is just 1.0.</param>
/// <param name="DisplayScale">The actual display scale for macOS and Windows,
/// as reported by CoreGraphics on macOS and Win32 API's on Windows. Requires
/// Windows 10+ as we need newer Win32 API's that work around the fact that
/// Godot does not support per-monitor DPI awareness.</param>
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
/// <param name="ProjectViewportSize">The viewport size as configured in the
/// Godot project. Provided for convenience.</param>
/// <param name="ProjectWindowSize">The window size as configured in the
/// Godot project. Provided for convenience.</param>
/// <param name="WindowSize">New window size.</param>
public record WindowScaleInfo(
  float SystemScale,
  float RetinaScale,
  float DisplayScale,
  float ContentScaleFactor,
  float CorrectionFactor,
  Vector2I ProjectViewportSize,
  Vector2I ProjectWindowSize,
  Vector2I NativeResolution,
  Vector2I WindowSize
) {
  /// <summary>
  /// String representation of the <see cref="WindowScaleInfo"/>.
  /// </summary>
  public override string ToString() => $"""
  SystemScale: {SystemScale}
  DisplayScale: {DisplayScale}
  ContentScaleFactor: {ContentScaleFactor}
  CorrectionFactor: {CorrectionFactor}
  ProjectViewportSize: {ProjectViewportSize}
  ProjectWindowSize: {ProjectWindowSize}
  NativeResolution: {NativeResolution}
  WindowSize: {WindowSize}
  """;
}
