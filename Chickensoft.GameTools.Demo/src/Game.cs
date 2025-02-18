namespace Chickensoft.GameTools.Demo;

using Chickensoft.GameTools.Displays;
using Godot;

public partial class Game : Control {
  public Button TestButton { get; private set; } = default!;
  public int ButtonPresses { get; private set; }

  public override void _Ready() {
    var hd = new Vector2I(1920, 1080);
    var uhd = new Vector2I(3840, 2160);
    var info = Display.LookGood(GetWindow(), hd);
    GD.Print(info);
  }
}
