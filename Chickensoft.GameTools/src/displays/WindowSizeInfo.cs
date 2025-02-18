namespace Chickensoft.GameTools.Displays;

using Godot;

/// <summary>
/// Window size information.
/// </summary>
/// <param name="Size">Recommended window size.</param>
/// <param name="MinSize">Minimum window size.</param>
/// <param name="MaxSize">Maximum window size.</param>
public record WindowSizeInfo(Vector2I Size, Vector2I MinSize, Vector2I MaxSize);
