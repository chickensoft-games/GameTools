namespace Chickensoft.GameTools.Demo;

using Chickensoft.GameTools.Displays;
using Godot;

public partial class Main : Control
{
  public Button Scenario1 { get; set; } = default!;
  public Button Scenario2 { get; set; } = default!;

  public static WindowInfo? WindowInfo { get; set; }

  public override void _Ready()
  {
    var window = GetWindow();

    window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
    window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;

    window.LookGood(WindowScaleBehavior.UIFixed, Display.UHD4k, isFullscreen: false);

    window.Title = "Display Scaling Demos";

    Scenario1 = GetNode<Button>("%Scenario1");
    Scenario2 = GetNode<Button>("%Scenario2");

    Scenario1.Pressed += () => CallDeferred("RunScene", "res://src/scenario1/Scenario1.tscn");
    Scenario2.Pressed += () => CallDeferred("RunScene", "res://src/scenario2/Scenario2.tscn");
  }

  private void RunScene(string scene)
    => GetTree().ChangeSceneToFile(scene);
}
