namespace Chickensoft.GameTools.Tests.Displays;

using System;
using Chickensoft.GameTools.Displays;
using Chickensoft.GameTools.Environment;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class DisplayTest(Node testScene) : TestClass(testScene)
{
  public Vector2I ThemeResolution { get; } = new(3840, 2160);

  [Cleanup]
  public void Cleanup()
  {
    Display.GetDisplayNativeResolution =
      Display.GetDisplayNativeResolutionDefault;

    Display.GetDisplayScaleFactor = Display.GetDisplayScaleFactorDefault;
    Display.ScreenGetDpi = Display.ScreenGetDpiDefault;
    Display.ScreenGetSize = Display.ScreenGetSizeDefault;
    Display.ScreenGetScale = Display.ScreenGetScaleDefault;

    Features.Reset();
  }

  [Test]
  public void Constants()
  {
    Display.HD.ShouldBe(new Vector2I(1280, 720));
    Display.FullHD.ShouldBe(new Vector2I(1920, 1080));
    Display.QHD.ShouldBe(new Vector2I(2560, 1440));
    Display.UHD4k.ShouldBe(new Vector2I(3840, 2160));
    Display.UHD5k.ShouldBe(new Vector2I(5120, 2880));
    Display.UHD8k.ShouldBe(new Vector2I(7680, 4320));
  }

  [Test]
  public void GetWindowScaleInfoMacOS()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.macOS);

    var logicalResolution = new Vector2I(3840, 2160) * 2;
    Display.GetDisplayNativeResolution = window => Display.UHD4k;
    Display.GetDisplayScaleFactor = window => 1.5f;
    Display.ScreenGetDpi = window => 138;
    // Simulate logical resolution being twice as big (common w/ macOS retina)
    Display.ScreenGetSize = window => logicalResolution;
    Display.ScreenGetScale = window => 2f;

    var scaleInfo = window.GetWindowScaleInfo(ThemeResolution, false);

    // Verify the math does what it says it does.
    scaleInfo.Screen.ShouldBe(window.CurrentScreen);
    scaleInfo.SystemScale.ShouldBe(1.4375f);
    scaleInfo.WindowScale.ShouldBe(2f);
    scaleInfo.DisplayScale.ShouldBe(1.5f);
    scaleInfo.ThemeScale.ShouldBe(1f);
    scaleInfo.ContentScaleFactor.ShouldBe(2f);
    scaleInfo.CorrectionFactor.ShouldBe(2f);
    scaleInfo.ProjectViewportSize.ShouldBe(Display.ProjectViewportSize);
    scaleInfo.ProjectWindowSize.ShouldBe(Display.ProjectWindowSize);
    scaleInfo.NativeResolution.ShouldBe(Display.UHD4k);
    scaleInfo.LogicalResolution.ShouldBe(logicalResolution);
  }

  [Test]
  public void GetWindowScaleInfoWindows()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.Windows);

    // on windows, it's possible the logical resolution is just wildly
    // different as it will be in the coordinate space of the scale factor
    // of the primary monitor.
    var logicalResolution = (Vector2I)(new Vector2(3840, 2160) * 1.3f);
    Display.GetDisplayNativeResolution = window => Display.UHD4k;
    Display.GetDisplayScaleFactor = window => 1.5f;
    Display.ScreenGetDpi = window => 138;

    Display.ScreenGetSize = window => logicalResolution;
    Display.ScreenGetScale = window => 2f;

    var scaleInfo = window.GetWindowScaleInfo(ThemeResolution, false);

    // Verify the math does what it says it does.
    scaleInfo.Screen.ShouldBe(window.CurrentScreen);
    scaleInfo.SystemScale.ShouldBe(1.4375f);
    scaleInfo.WindowScale.ShouldBe(1f);
    scaleInfo.DisplayScale.ShouldBe(1.5f);
    scaleInfo.ThemeScale.ShouldBe(1f);
    scaleInfo.ContentScaleFactor.ShouldBe(0.9583333f);
    scaleInfo.CorrectionFactor.ShouldBe(0.9583333f);
    scaleInfo.ProjectViewportSize.ShouldBe(Display.ProjectViewportSize);
    scaleInfo.ProjectWindowSize.ShouldBe(Display.ProjectWindowSize);
    scaleInfo.NativeResolution.ShouldBe(Display.UHD4k);
    scaleInfo.LogicalResolution.ShouldBe(logicalResolution);
  }

  [Test]
  public void GetWindowScaleInfoLinux()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.Linux);

    var logicalResolution = new Vector2I(3840, 2160);
    Display.GetDisplayNativeResolution = window => Display.UHD4k;
    Display.GetDisplayScaleFactor = window => 1.5f;
    Display.ScreenGetDpi = window => 219;
    Display.ScreenGetSize = window => logicalResolution;
    Display.ScreenGetScale = window => 1f;

    var scaleInfo = window.GetWindowScaleInfo(ThemeResolution, false);

    // Verify the math does what it says it does.
    scaleInfo.Screen.ShouldBe(window.CurrentScreen);
    scaleInfo.SystemScale.ShouldBe(2.28125f);
    scaleInfo.WindowScale.ShouldBe(1f);
    scaleInfo.DisplayScale.ShouldBe(1.5f);
    scaleInfo.ThemeScale.ShouldBe(1f);
    scaleInfo.ContentScaleFactor.ShouldBe(1f);
    scaleInfo.CorrectionFactor.ShouldBe(1f);
    scaleInfo.ProjectViewportSize.ShouldBe(Display.ProjectViewportSize);
    scaleInfo.ProjectWindowSize.ShouldBe(Display.ProjectWindowSize);
    scaleInfo.NativeResolution.ShouldBe(Display.UHD4k);
    scaleInfo.LogicalResolution.ShouldBe(logicalResolution);
  }

  [Test]
  public void GetWindowScaleInfoFreeBSD()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.FreeBSD);

    Should.Throw<NotSupportedException>(
      () => window.GetWindowScaleInfo(ThemeResolution, false)
    );
  }

  [Test]
  public void GetWindowScaleInfoIOS()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.iOS);

    Should.Throw<NotSupportedException>(
      () => window.GetWindowScaleInfo(ThemeResolution, false)
    );
  }

  [Test]
  public void GetWindowScaleInfoAndroid()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.Android);

    Should.Throw<NotSupportedException>(
      () => window.GetWindowScaleInfo(ThemeResolution, false)
    );
  }

  [Test]
  public void GetWindowScaleInfoUnknownOS()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem((OSFamily)(-1));

    Should.Throw<NotSupportedException>(
      () => window.GetWindowScaleInfo(ThemeResolution, false)
    );
  }

  [Test]
  public void GetWindowScaleInfoWithReferenceSize()
  {
    var window = TestScene.GetWindow();

    Features.FakeOperatingSystem(OSFamily.macOS);
    Display.GetDisplayScaleFactor = window => 2f;

    var scaleInfo = window.GetWindowScaleInfo(ThemeResolution);

    scaleInfo.SystemScale.ShouldBeGreaterThan(0f);
  }

  [Test]
  public void ProjectWindowSizeWithWindowSize()
  {
    ProjectSettings.SetSetting(
      "display/window/size/window_width_override", 100
    );
    ProjectSettings.SetSetting(
      "display/window/size/window_height_override", 100
    );

    Display.ProjectWindowSize.ShouldBe(new Vector2I(100, 100));
  }

  [Test]
  public void ProjectWindowSizeWithWNoWindowHeight()
  {
    ProjectSettings.SetSetting(
      "display/window/size/window_width_override", 100
    );
    ProjectSettings.SetSetting(
      "display/window/size/window_height_override", 0
    );

    Display.ProjectWindowSize.ShouldBe(
      new Vector2I(100, Display.ProjectViewportSize.Y)
    );
  }

  [Test]
  public void ProjectWindowSizeWithNoWindowWidth()
  {
    ProjectSettings.SetSetting(
      "display/window/size/window_width_override", 0
    );
    ProjectSettings.SetSetting(
      "display/window/size/window_height_override", 100
    );

    Display.ProjectWindowSize.ShouldBe(
      new Vector2I(Display.ProjectViewportSize.X, 100)
    );
  }

  [Test]
  public void DescribeScreen()
  {
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
  public void GetAutoDisplayScaleMacOS()
  {
    Features.FakeOperatingSystem(OSFamily.macOS);

    Display.ScreenGetScale = screen => 2f;

    Display.GetAutoDisplayScale().ShouldBe(2f);
  }

  [Test]
  public void GetAutoDisplayScaleZeroSize()
  {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(0, 0);

    Display.GetAutoDisplayScale().ShouldBe(1f);
  }

  [Test]
  public void GetAutoDisplayScaleHighDpiLargeScreen()
  {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(3840, 2160);
    Display.ScreenGetDpi = screen => 200;

    Display.GetAutoDisplayScale().ShouldBe(2f);
  }

  [Test]
  public void GetAutoDisplayScaleLikelyHighDpiScreen()
  {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(3840, 2160);
    Display.ScreenGetDpi = screen => 180;

    Display.GetAutoDisplayScale().ShouldBe(1.5f);
  }

  [Test]
  public void GetAutoDisplayScaleUnknown()
  {
    Features.FakeOperatingSystem(OSFamily.Windows);

    Display.ScreenGetSize = screen => new Vector2I(1920, 1080);
    Display.ScreenGetDpi = screen => 96;

    Display.GetAutoDisplayScale().ShouldBe(1f);
  }

  [Test]
  public void ConstrainsVector()
  {
    Vector2 size = Display.FullHD;
    var aspect = size.Aspect();

    var minSize = new Vector2I(250, 250);
    var maxSize = new Vector2I(800, 600);

    var constrained = size.Constrain(aspect, minSize, maxSize);

    constrained.ShouldBe(new Vector2I(800, 450));
  }

  [Test]
  public void ConstrainsIfHeightIsBelowMinSize()
  {
    // A wide aspect ratio so that after clamping width,
    // the computed height will drop below the minimum.
    var size = new Vector2(1000, 100);
    var aspect = size.X / size.Y;

    var minSize = new Vector2I(250, 250);
    var maxSize = new Vector2I(800, 600);

    var constrained = size.Constrain(aspect, minSize, maxSize);

    constrained.ShouldBe(new Vector2I(800, 80));
  }

  [Test]
  public void ConstrainsIfHeightIsAboveMaxSize()
  {
    // A tall aspect ratio (width < height) so that the first pass
    // yields a height above the maximum.
    var size = new Vector2(400, 900);
    var aspect = size.X / size.Y;

    var minSize = new Vector2I(250, 250);
    var maxSize = new Vector2I(800, 600);

    var constrained = size.Constrain(aspect, minSize, maxSize);

    constrained.ShouldBe(new Vector2(266, 598.5f));
  }

  [Test]
  public void GetWindowSizeInfoUsingAspectRatio()
  {
    ProjectSettings.SetSetting(
      "display/window/size/window_width_override", Display.FullHD.X
    );
    ProjectSettings.SetSetting(
      "display/window/size/window_height_override", Display.FullHD.Y
    );

    var info = Display.GetWindowSizeInfo(
        Display.FullHD,
        useProjectAspectRatio: true,
        minWindowedSize: 0.25f,
        maxWindowedSize: 0.75f
    );

    info.Size.ShouldBe(new Vector2I(1440, 810));
    info.MinSize.ShouldBe(new Vector2I(480, 270));
    info.MaxSize.ShouldBe(new Vector2I(1440, 810));
  }

  [Test]
  public void GetWindowSizeInfoWithoutUsingAspectRatio()
  {
    var info = Display.GetWindowSizeInfo(
        Display.FullHD,
        useProjectAspectRatio: false,
        minWindowedSize: 0.25f,
        maxWindowedSize: 0.75f
    );

    info.Size.ShouldBe(new Vector2I(1440, 810));
    info.MinSize.ShouldBe(new Vector2I(480, 270));
    info.MaxSize.ShouldBe(new Vector2I(1440, 810));
  }

  [Test]
  public void LookGood()
  {
    var window = TestScene.GetWindow();

    window.LookGood(
      WindowScaleBehavior.UIProportional, Display.FullHD, isFullscreen: true, useExclusiveFullscreen: false
    );

    window.ContentScaleMode.ShouldBe(Window.ContentScaleModeEnum.Disabled);

    window.LookGood(
      WindowScaleBehavior.UIProportional, Display.FullHD, isFullscreen: false
    );

    window.ContentScaleMode.ShouldBe(Window.ContentScaleModeEnum.CanvasItems);

    window.LookGood(
      WindowScaleBehavior.UIFixed, Display.FullHD, isFullscreen: true
    );

    window.ContentScaleMode.ShouldBe(Window.ContentScaleModeEnum.Disabled);

    window.LookGood(
      WindowScaleBehavior.UIFixed, Display.FullHD, isFullscreen: false
    );

    window.ContentScaleMode.ShouldBe(Window.ContentScaleModeEnum.Disabled);
  }
}

