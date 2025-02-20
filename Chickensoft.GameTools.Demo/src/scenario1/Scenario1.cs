namespace Chickensoft.GameTools.Demo;

using Chickensoft.GameTools.Displays;
using Godot;

public partial class Scenario1 : Control {
  private bool _isFullscreen;

  public Vector2I BaseResolution => Display.UHD4k;

  public override void _Ready() {
    var window = GetWindow();

    GetNode<Button>("%ToggleButton").Pressed += ToggleFullscreen;

    GetNode<Button>("%MainMenuButton").Pressed +=
      () => GetTree().ChangeSceneToFile("res://src/Main.tscn");

    window.Title = "Scenario 1";

    SetFullscreen(_isFullscreen);
  }

  public override void _Input(InputEvent @event) {
    if (Input.IsActionJustReleased("fullscreen_toggle")) {
      ToggleFullscreen();
    }
  }

  public void ToggleFullscreen() => SetFullscreen(!_isFullscreen);

  public void SetFullscreen(bool isFullscreen) {
    _isFullscreen = isFullscreen;

    var info = GetWindow().LookGood(
      WindowScaleBehavior.UIProportional,
      BaseResolution,
      isFullscreen: _isFullscreen
    );

    GD.Print(info);
  }
}
