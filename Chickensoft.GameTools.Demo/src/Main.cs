namespace Chickensoft.GameTools.Demo;

using Chickensoft.GameTools.Displays;
using Godot;

public partial class Main : Control {
  public Button Scenario1 { get; set; } = default!;
  public Button Scenario2 { get; set; } = default!;

  public static Vector2I? WindowSize { get; set; }

  public override void _Ready() {
    var window = GetWindow();
    var info = Display.GetWindowScaleInfo(window, Display.UHD4k);
    window.ContentScaleFactor = info.ContentScaleFactor;
    window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
    window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
    window.Title = "Display Scaling Demos";

    Scenario1 = GetNode<Button>("%Scenario1");
    Scenario2 = GetNode<Button>("%Scenario2");

    Scenario1.Pressed += () => CallDeferred("RunScene", "res://src/scenario1/Scenario1.tscn");
    Scenario2.Pressed += () => CallDeferred("RunScene", "res://src/scenario2/Scenario2.tscn");

    if (WindowSize.HasValue) {
      window.Mode = Window.ModeEnum.Windowed;
      window.MinSize = default;
      window.Size = WindowSize.Value;

      // center
      window.Position = (info.Resolution - window.Size) / 2;
    }
    else {
      WindowSize = window.Size;
    }
  }

  private void RunScene(string scene)
    => GetTree().ChangeSceneToFile(scene);
}
