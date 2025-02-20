namespace Chickensoft.GameTools.Demo;

using Chickensoft.GameTools.Displays;
using Godot;

public partial class Main : Control {
  public Button Scenario1 { get; set; } = default!;
  public Button Scenario2 { get; set; } = default!;

  public static WindowInfo? WindowInfo { get; set; }

  public override void _Ready() {
    var window = GetWindow();
    var scaleInfo = window.GetWindowScaleInfo(Display.UHD4k);
    var sizeInfo = Display.GetWindowSizeInfo(scaleInfo.Resolution);

    window.ContentScaleFactor = scaleInfo.ContentScaleFactor;
    window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
    window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
    window.Title = "Display Scaling Demos";

    Scenario1 = GetNode<Button>("%Scenario1");
    Scenario2 = GetNode<Button>("%Scenario2");

    Scenario1.Pressed += () => CallDeferred("RunScene", "res://src/scenario1/Scenario1.tscn");
    Scenario2.Pressed += () => CallDeferred("RunScene", "res://src/scenario2/Scenario2.tscn");

    var windowInfo = WindowInfo ?? new(WindowScaleBehavior.UIFixed, scaleInfo, sizeInfo);

    window.MinSize = windowInfo.SizeInfo.MinSize;
    window.MaxSize = windowInfo.SizeInfo.MaxSize;
    window.Size = windowInfo.SizeInfo.Size;

    // center
    window.Position = (windowInfo.ScaleInfo.Resolution - window.Size) / 2;
    window.CurrentScreen = windowInfo.ScaleInfo.Screen;

    WindowInfo = windowInfo;

    window.Mode = Window.ModeEnum.Windowed;
  }

  private void RunScene(string scene)
    => GetTree().ChangeSceneToFile(scene);
}
