namespace Chickensoft.GameTools.Displays;

using Godot;

/// <summary>
/// Represents how the game window scaling should behave.
/// </summary>
public enum WindowScaleBehavior {
  /// <summary>
  /// <para>
  /// UI size is calculated based on the theme design size and the monitor's
  /// native resolution (correcting for any misunderstandings of display
  /// scale on Godot's side) and then scaled proportionally to the game's
  /// window size. In fullscreen, the UI will be 100% of the computed size.
  /// This is useful for games which are expected to be run fullscreen,
  /// have very precise UI layout requirements, or are only intended to
  /// support a single resolution.
  /// </para>
  /// <para>
  /// UI scaling uses the
  /// <see cref="Window.ContentScaleModeEnum.CanvasItems"/> mode to ensure
  /// that the UI is scaled crisply with respect to the relevant mipmaps. Be
  /// sure to enable mipmaps on your UI textures when importing them.
  /// </para>
  /// </summary>
  UIProportional,

  /// <summary>
  /// <para>
  /// UI size is calculated based on the theme design size and the monitor's
  /// native resolution (correcting for any misunderstandings of display
  /// scale on Godot's side) but does not scale with the window size,
  /// remaining 100% of the computed size. This is useful for applications
  /// or games which want to run well when windowed. Be sure to set a minimum
  /// window size to avoid cutting off the UI.
  /// </para>
  /// <para>
  /// UI scaling uses the
  /// <see cref="Window.ContentScaleModeEnum.Disabled"/> mode to ensure that
  /// the UI is only scaled to the correct, computed size but does not change
  /// with the window size. Be sure to enable mipmaps on your UI textures
  /// when importing them.
  /// </para>
  /// </summary>
  UIFixed
}
