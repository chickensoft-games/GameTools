namespace Chickensoft.GameTools.Displays;

/// <summary>
/// Information about how a window should be sized and scaled.
/// </summary>
/// <param name="ScaleBehavior">The selected scale behavior for the window.
/// </param>
/// <param name="ScaleInfo">Window scale information.</param>
/// <param name="SizeInfo">Window size information.</param>
public record WindowInfo(
  WindowScaleBehavior ScaleBehavior,
  WindowScaleInfo ScaleInfo,
  WindowSizeInfo SizeInfo
) {
  /// <inheritdoc />
  public override string ToString() =>
    $"""
    ScaleBehavior: {ScaleBehavior}
    {ScaleInfo}
    {SizeInfo}
    """;
}
