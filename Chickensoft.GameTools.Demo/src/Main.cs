namespace Chickensoft.GameTools.Demo;

using Godot;

public partial class Main : Node2D {
  public override void _Ready() {
    // If we don't need to run tests, we can just switch to the game scene.
    CallDeferred("RunScene");
  }

  private void RunScene()
    => GetTree().ChangeSceneToFile("res://src/Game.tscn");
}
