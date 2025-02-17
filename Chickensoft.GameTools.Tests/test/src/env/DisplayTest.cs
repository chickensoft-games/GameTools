namespace Chickensoft.GameTools.Tests.Environment;

using Chickensoft.GameTools.Displays;
using Chickensoft.GameTools.Environment;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class DisplayTest(Node testScene) : TestClass(testScene) {
  public Vector2I ThemeResolution { get; } = new(3840, 2160);

  [Cleanup]
  public void Cleanup() {
    Features.Reset();
  }

  [Test]
  public void GetWindowDpiScaleInfo() {
    // This test should be run on macOS or Windows, but not linux
    // This actually computes the current screen's scale factor.
    var window = TestScene.GetWindow();

    var scaleInfo = window.GetWindowScaleInfo(ThemeResolution, false);

    GD.Print($"System scale: {scaleInfo.SystemScale}");

    scaleInfo.SystemScale.ShouldBeGreaterThan(0f);
  }

  [Test]
  public void GetWindowDpiScaleInfoWithReferenceSize() {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.macOS);
    Display.GetDisplayScaleFactor = window => 2f;

    var scaleInfo = window.GetWindowScaleInfo(ThemeResolution);

    scaleInfo.SystemScale.ShouldBeGreaterThan(0f);
    scaleInfo.WindowSize.ShouldBe(new Vector2I(1804, 1015));
  }

  [Test]
  public void DescribeScreen() {
    Display.Describe(DisplayServer.ScreenOfMainWindow)
      .ShouldBe("[Screen of Main Window]");
    Display.Describe(DisplayServer.ScreenPrimary)
      .ShouldBe("[Primary Screen]");
    Display.Describe(DisplayServer.ScreenWithKeyboardFocus)
      .ShouldBe("[Screen with Keyboard Focus]");
    Display.Describe(DisplayServer.ScreenWithMouseFocus)
      .ShouldBe("[Screen with Mouse Focus]");
    Display.Describe(3).ShouldBe("[Screen 3]");
  }

  [Test]
  public void GetAutoDisplayScaleMacOS() {
    Features.FakeOperatingSystem(OSFamily.macOS);

    Display.ScreenGetScale = screen => 2f;

    Display.GetAutoDisplayScale().ShouldBe(2f);
  }

  [Test]
  public void GetAutoDisplayScaleZeroSize() {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(0, 0);

    Display.GetAutoDisplayScale().ShouldBe(1f);
  }

  [Test]
  public void GetAutoDisplayScaleHighDpiLargeScreen() {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(3840, 2160);
    Display.ScreenGetDpi = screen => 200;

    Display.GetAutoDisplayScale().ShouldBe(2f);
  }

  [Test]
  public void GetAutoDisplayScaleLikelyHighDpiScreen() {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(3840, 2160);
    Display.ScreenGetDpi = screen => 180;

    Display.GetAutoDisplayScale().ShouldBe(1.5f);
  }

  [Test]
  public void GetAutoDisplayScaleUnknown() {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(1920, 1080);
    Display.ScreenGetDpi = screen => 96;

    Display.GetAutoDisplayScale().ShouldBe(1f);
  }
}
